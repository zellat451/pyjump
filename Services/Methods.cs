using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Sheets.v4.Data;
using pyjump.Entities;
using pyjump.Entities.Data;
using pyjump.Infrastructure;
using pyjump.Interfaces;

namespace pyjump.Services
{
    public static class Methods
    {
        #region whitelist methods
        /// <summary>
        /// Scans all drives and folders in the registered main drives and updates the database with the new whitelist entries.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ScanWhitelist(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                #region get all folder entries for whitelist
                var drives = SingletonServices.MainDrives;

                cancellationToken.ThrowIfCancellationRequested();

                var scanner = new DriveScanner();
                var allWhitelistEntries = await scanner.GetAllFolderNamesRecursiveAsync(drives.Data, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (allWhitelistEntries == null || allWhitelistEntries.Count == 0)
                {
                    SingletonServices.LogForm.Log("❌ No folders found in the main drives.");
                    return;
                }
                ConcurrentBag<WhitelistEntry> allWhitelistEntriesBag = [.. allWhitelistEntries];
                #endregion

                // check existing whitelist entries
                using (var db = new AppDbContext())
                {
                    var existingEntries = db.Whitelist.ToList();

                    #region add new whitelist entries
                    // 1. entries existing in the new list but not in the database > add the entries to the database
                    var toAdd = allWhitelistEntries.Where(x => !existingEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                    db.Whitelist.AddRange(toAdd);
                    SingletonServices.LogForm.Log($"Added {toAdd.Count} entries to the database.");
                    Debug.WriteLine($"Added {toAdd.Count} entries to the database.");
                    #endregion

                    #region update existing whitelist entries
                    // 2. entries existing in both lists > update the entries in the database if the name | url | resource key is different
                    var toUpdate = new List<WhitelistEntry>();
                    foreach (var entry in existingEntries)
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        catch (Exception)
                        {
                            SingletonServices.LogForm.Log("❌ Scan cancelled.");
                            db.Dispose();
                            return;
                        }

                        var newEntry = allWhitelistEntries.FirstOrDefault(x => x.Id == entry.Id);
                        if (newEntry != null)
                        {
                            if (entry.Name != newEntry.Name
                                || entry.Url != newEntry.Url
                                || entry.ResourceKey != newEntry.ResourceKey
                                || entry.DriveId != newEntry.DriveId)
                            {
                                entry.Name = newEntry.Name;
                                entry.Url = newEntry.Url;
                                entry.ResourceKey = newEntry.ResourceKey;
                                entry.DriveId = newEntry.DriveId;
                                entry.LastChecked = null;

                                toUpdate.Add(entry);
                            }
                        }
                    }
                    db.Whitelist.UpdateRange(toUpdate);
                    SingletonServices.LogForm.Log($"Updated {toUpdate.Count} entries in the database.");
                    Debug.WriteLine($"Updated {toUpdate.Count} entries in the database.");
                    #endregion

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    catch (Exception)
                    {
                        SingletonServices.LogForm.Log("❌ Scan cancelled.");
                        db.Dispose();
                        return;
                    }

                    // 3. commit the changes to the database
                    await SafeSaveAsync(db, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error scanning whitelist: {e}");
                throw;
            }
        }

        /// <summary>
        /// Matches the whitelist entries to the drives in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task MatchWhitelistToDrives(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                // get all drives
                var driveNames = SingletonServices.MainDrives?.Data?.Select(x => x.Name).ToList();
                if (driveNames == null || driveNames.Count == 0)
                {
                    SingletonServices.LogForm.Log("❌ No drives found.");
                    return;
                }

                // delete all the whitelist entries that are not in the drives
                using (var db = new AppDbContext())
                {
                    var allWhitelistEntries = db.Whitelist.ToList();
                    var entriesToDelete = allWhitelistEntries.Where(x => !driveNames.Contains(x.Name)).ToList();
                    db.Whitelist.RemoveRange(entriesToDelete);

                    await SafeSaveAsync(db, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error matching whitelist to drives: {e}");
                throw;
            }
        }
        #endregion

        #region file methods
        /// <summary>
        /// Scans all files in the whitelist entries and updates the database with the new file entries.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ScanFiles(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                #region get whitelist all entries
                // get all whitelist entries from the database
                List<WhitelistEntry> whitelistEntries;
                using (var db = new AppDbContext())
                {
                    whitelistEntries = [.. db.Whitelist];
                }
                #endregion

                ScopedServices.LoadingForm.PrepareLoadingBar("Scanning files", whitelistEntries.Count);

                cancellationToken.ThrowIfCancellationRequested();

                // get all files from the whitelist entries
                var scanner = new DriveScanner();

                if (!SingletonServices.AllowThreading)
                {
                    await ScanFilesSingleThread(whitelistEntries, cancellationToken);
                }
                else
                {
                    await ScanFilesMultiThread(whitelistEntries, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error scanning files: {e}");
                throw;
            }
        }

        /// <summary>
        /// Scans all files in the whitelist entries and updates the database with the new file entries.
        /// Uses a single thread to scan the files.
        /// </summary>
        /// <param name="whitelistEntries"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task ScanFilesSingleThread(List<WhitelistEntry> whitelistEntries, CancellationToken cancellationToken = default)
        {
            try
            {
                if (whitelistEntries == null || whitelistEntries.Count == 0)
                {
                    SingletonServices.LogForm.Log("❌ No whitelist entries to scan.");
                    return;
                }

                var scanner = new DriveScanner();

                foreach (var w in whitelistEntries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // get all files for one entry
                    var scannedFileEntries = await scanner.GetAllFilesInWhitelistAsync(w, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    await ScannedFilesToDb([(w, scannedFileEntries)], cancellationToken: cancellationToken);

                    ScopedServices.LoadingForm.IncrementProgress();
                }

            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error scanning files: {e}");
                throw;
            }
        }

        /// <summary>
        /// Scans all files in the whitelist entries and updates the database with the new file entries.
        /// Uses multiple threads to scan the files.
        /// </summary>
        /// <param name="whitelistEntries"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ScanFilesMultiThread(List<WhitelistEntry> whitelistEntries, CancellationToken cancellationToken = default)
        {
            try
            {
                if (whitelistEntries == null || whitelistEntries.Count == 0)
                {
                    SingletonServices.LogForm.Log("❌ No whitelist entries to scan.");
                    return;
                }
                var maxThreads = SingletonServices.MaxThreads;
                var entryQueue = new BlockingCollection<WhitelistEntry>(new ConcurrentQueue<WhitelistEntry>(whitelistEntries));
                entryQueue.CompleteAdding();

                var fileResults = new ConcurrentBag<FileEntry>();
                var whitelistUpdates = new ConcurrentBag<WhitelistEntry>();
                var threads = new List<Thread>();
                var countdown = new CountdownEvent(maxThreads);

                var scanner = new DriveScanner();

                for (int i = 0; i < maxThreads; i++)
                {
                    var thread = new Thread(async () =>
                    {
                        try
                        {
                            foreach (var w in entryQueue.GetConsumingEnumerable(cancellationToken))
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    break;

                                try
                                {
                                    var scannedFiles = await scanner.GetAllFilesInWhitelistAsync(w, cancellationToken);
                                    if (cancellationToken.IsCancellationRequested) break;

                                    foreach (var f in scannedFiles)
                                        fileResults.Add(f);

                                    if (scannedFiles.Count > 0)
                                    {
                                        whitelistUpdates.Add(w);
                                    }

                                    ScopedServices.LoadingForm.IncrementProgress();
                                }
                                catch (OperationCanceledException)
                                {
                                    SingletonServices.LogForm.Log("❌ File scan cancelled.");
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    SingletonServices.LogForm.Log($"❌ Error scanning entry: {ex.Message}");
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            SingletonServices.LogForm.Log("❌ Scan cancelled.");
                            return;
                        }
                        catch (Exception ex)
                        {
                            SingletonServices.LogForm.Log($"❌ Error in thread: {ex.Message}");
                        }
                        finally
                        {
                            countdown.Signal();
                        }
                    })
                    {
                        IsBackground = true
                    };
                    threads.Add(thread);
                    thread.Start();
                }

                // Wait for all threads to finish
                await Task.Run(countdown.Wait, cancellationToken);

                // Final DB write
                await ScannedFilesToDb([.. whitelistUpdates.Select(w => (w, fileResults.Where(f => f.FolderId == w.Id).ToList()))], cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error scanning files: {e}");
                throw;
            }
        }

        /// <summary>
        /// Saves the scanned files to the database.
        /// </summary>
        /// <param name="sets"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task ScannedFilesToDb(List<(WhitelistEntry whitelist, List<FileEntry> scannedFileEntries)> sets = null, CancellationToken cancellationToken = default)
        {
            try
            {
                ScopedServices.LoadingForm?.PrepareLoadingBar("Saving files to database", sets.Count);
                foreach (var (whitelist, scannedFileEntries) in sets)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // check existing file entries
                    using (var db = new AppDbContext())
                    {
                        var currentFileEntries = db.Files.ToList();

                        #region add new file entries
                        // 1. entries existing in the new list but not in the database > add the entries to the database
                        var toAdd = scannedFileEntries.Where(x => !currentFileEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                        try
                        {
                            await db.Files.AddRangeAsync(toAdd, cancellationToken);
                        }
                        catch (Exception e)
                        {
                            SingletonServices.LogForm.Log($"Error adding file entries: {e}");
                            db.Dispose();
                            throw;
                        }
                        SingletonServices.LogForm.Log($"Added {toAdd.Count} file entries.");
                        Debug.WriteLine($"Added {toAdd.Count} file entries.");
                        #endregion

                        #region update existing file entries
                        // 2. entries existing in both lists > update the entries in the database if info is different
                        // we don't check the Type because it might have been changed manually
                        var toUpdate = new List<FileEntry>();
                        foreach (var entry in currentFileEntries)
                        {
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            catch (Exception)
                            {
                                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                                db.Dispose();
                                return;
                            }

                            var newEntry = scannedFileEntries.FirstOrDefault(x => x.Id == entry.Id);
                            if (newEntry != null)
                            {
                                if (entry.Name != newEntry.Name
                                    || entry.Url != newEntry.Url
                                    || entry.ResourceKey != newEntry.ResourceKey
                                    || entry.DriveId != newEntry.DriveId
                                    || entry.LastModified != newEntry.LastModified
                                    || entry.Owner != newEntry.Owner
                                    || entry.FolderId != newEntry.FolderId
                                    || entry.FolderName != newEntry.FolderName
                                    || entry.FolderUrl != newEntry.FolderUrl)
                                {
                                    entry.Name = newEntry.Name;
                                    entry.Url = newEntry.Url;
                                    entry.ResourceKey = newEntry.ResourceKey;
                                    entry.DriveId = newEntry.DriveId;
                                    entry.LastModified = newEntry.LastModified;
                                    entry.Owner = newEntry.Owner;
                                    entry.FolderId = newEntry.FolderId;
                                    entry.FolderName = newEntry.FolderName;
                                    entry.FolderUrl = newEntry.FolderUrl;

                                    toUpdate.Add(entry);
                                }
                            }
                        }
                        try
                        {
                            db.Files.UpdateRange(toUpdate);
                        }
                        catch (Exception e)
                        {
                            SingletonServices.LogForm.Log($"Error updating file entries: {e}");
                            db.Dispose();
                            throw;
                        }
                        SingletonServices.LogForm.Log($"Updated {toUpdate.Count} file entries.");
                        Debug.WriteLine($"Updated {toUpdate.Count} file entries.");
                        #endregion

                        #region update the lastchecked date of the whitelist entry
                        if (scannedFileEntries.Count > 0)
                        {
                            // update the last checked date of the whitelist entry to today, midnight Utc
                            whitelist.LastChecked = DateTime.UtcNow.Date;
                            try
                            {
                                db.Whitelist.Update(whitelist);
                            }
                            catch (Exception e)
                            {
                                SingletonServices.LogForm.Log($"Error updating whitelist entry: {e}");
                                db.Dispose();
                                throw;
                            }
                        }
                        #endregion

                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        catch (Exception)
                        {
                            SingletonServices.LogForm.Log("❌ Scan cancelled.");
                            db.Dispose();
                            return;
                        }

                        await SafeSaveAsync(db, cancellationToken);
                    }

                    ScopedServices.LoadingForm?.IncrementProgress();
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Scan cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error saving files to database: {e}");
                throw;
            }
        }

        /// <summary>
        /// Forces the file type to match the folder type for all files in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ForceMatchType(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. get all files from the database
                List<FileEntry> allFiles;
                List<WhitelistEntry> allWhitelistEntries;
                using (var db = new AppDbContext())
                {
                    allFiles = [.. db.Files];
                    allWhitelistEntries = [.. db.Whitelist];
                }

                var filesToCheck = allFiles.Where(x => !string.IsNullOrWhiteSpace(x.FolderId)).ToList();

                ScopedServices.LoadingForm.PrepareLoadingBar("Forcing files to match type with folder", filesToCheck.Count);

                foreach (var file in filesToCheck)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // get the corresponding whitelist entry
                    var whitelistEntry = allWhitelistEntries.FirstOrDefault(x => x.Id == file.FolderId);
                    if (whitelistEntry == null)
                    {
                        SingletonServices.LogForm.Log($"❌ Whitelist entry not found for file {file.Name} ({file.Id})");
                        ScopedServices.LoadingForm.IncrementProgress();
                        continue;
                    }
                    else
                    {
                        // check if the file type is different from the whitelist entry type
                        if (file.Type != whitelistEntry.Type)
                        {
                            // update the file type to match the whitelist entry type
                            file.Type = whitelistEntry.Type;
                            using (var db = new AppDbContext())
                            {
                                try
                                {
                                    db.Files.Update(file);
                                }
                                catch (Exception e)
                                {
                                    SingletonServices.LogForm.Log($"Error updating file type: {e}");
                                    ScopedServices.LoadingForm.IncrementProgress();
                                    continue;
                                }
                                await SafeSaveAsync(db, cancellationToken);
                            }
                            SingletonServices.LogForm.Log($"✅ File {file.Name} ({file.Id}) updated to match folder type {whitelistEntry.Type}.");
                        }

                        ScopedServices.LoadingForm.IncrementProgress();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Forcing match type cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error forcing match type: {e}");
                throw;
            }
        }
        #endregion

        #region Common methods
        /// <summary>
        /// Uploads the entries to the specified sheet in the Google Sheets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <param name="sheetName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task UploadToSheetAsync<T>(List<T> entries, string sheetName, CancellationToken cancellationToken = default)
            where T : ISheetDataEntity
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (entries.Count == 0)
                {
                    SingletonServices.LogForm.Log($"No entries to upload to sheet '{sheetName}'.");
                    return;
                }

                var service = ScopedServices.SheetsService;

                // Step 1: Get the sheet ID and current grid size
                var spreadsheet = await service.Spreadsheets.Get(SingletonServices.SpreadsheetId).ExecuteAsync(cancellationToken);
                var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
                if (sheet == null)
                {
                    SingletonServices.LogForm.Log($"❌ Sheet '{sheetName}' not found in spreadsheet.");
                    throw new Exception($"❌ Sheet '{sheetName}' not found in spreadsheet.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                int sheetId = (int)sheet.Properties.SheetId;
                #region resize the sheet
                var currentRowCount = sheet.Properties.GridProperties.RowCount ?? 1;
                var currentColCount = sheet.Properties.GridProperties.ColumnCount ?? 1;

                int rowsNeeded = entries.Count + 1; // +1 for header row
                int colsNeeded = T.GetSheetColumnsNumber();

                // Step 2: Resize the sheet
                var resizeRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests =
                    [
                        new() {
                        UpdateSheetProperties = new UpdateSheetPropertiesRequest
                        {
                            Properties = new SheetProperties
                            {
                                SheetId = sheetId,
                                GridProperties = new GridProperties
                                {
                                    RowCount = rowsNeeded,
                                    ColumnCount = colsNeeded
                                }
                            },
                            Fields = "gridProperties(rowCount,columnCount)"
                        }
                    }
                    ]
                };

                await service.Spreadsheets.BatchUpdate(resizeRequest, SingletonServices.SpreadsheetId).ExecuteAsync(cancellationToken);
                #endregion

                cancellationToken.ThrowIfCancellationRequested();

                // Step 3: Build data and clearing requests
                var dataRequests = new List<Request>();

                #region clear sheet data
                // 3.1: Clear rows (but keep the header)
                dataRequests.Add(new Request
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Range = new GridRange
                        {
                            SheetId = sheetId,
                            StartRowIndex = 1 // keep header (row 0)
                        },
                        Fields = "userEnteredValue"
                    }
                });
                #endregion

                #region upload the new data
                // 3.2: Build row data
                var cellData = new List<RowData>();
                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var data = entry.GetRowData();
                    cellData.Add(new RowData
                    {
                        Values = data
                    });

                    ScopedServices.LoadingForm.IncrementProgress();
                }

                // 3.3: Write data starting from row 1 (avoiding header)
                dataRequests.Add(new Request
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Start = new GridCoordinate { SheetId = sheetId, RowIndex = 1, ColumnIndex = 0 },
                        Rows = cellData,
                        Fields = "userEnteredValue"
                    }
                });
                #endregion

                // Step 4: Execute the data update batch
                await service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = dataRequests }, SingletonServices.SpreadsheetId).ExecuteAsync(cancellationToken);

                SingletonServices.LogForm.Log($"✅ Uploaded {entries.Count} entries to sheet '{sheetName}'.");
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Sheet building cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"❌ Error uploading to sheet '{sheetName}': {e}");
                throw;
            }
        }

        /// <summary>
        /// Builds the sheets for all the data in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task BuildSheets(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                ScopedServices.LoadingForm.PrepareLoadingBar("Building sheets - Initialization", 2);

                // 0. initialize by getting all file entries from the database
                List<FileEntry> allFiles;
                using (var db = new AppDbContext())
                {
                    allFiles = [.. db.Files];
                }

                List<WhitelistEntry> allWhitelistEntries;
                using (var db = new AppDbContext())
                {
                    allWhitelistEntries = [.. db.Whitelist];
                }

                ScopedServices.LoadingForm.IncrementProgress();

                cancellationToken.ThrowIfCancellationRequested();

                // 1. filter similar results for jumps / stories
                // we want to group all files by name / owner.
                // we want to skip files that are marked Accessible = false.
                // we want to get the most recent file of each set.
                var sheetJumpFiles = allFiles
                    .Where(x => x.Type == Statics.FolderType.Jump && x.Accessible)
                    .GroupBy(x => new { x.Name, x.Owner })
                    .Select(g => g.OrderByDescending(x => x.LastModified).First())
                    .ToList();

                var sheetStoryFiles = allFiles
                    .Where(x => x.Type == Statics.FolderType.Story && x.Accessible)
                    .GroupBy(x => new { x.Name, x.Owner })
                    .Select(g => g.OrderByDescending(x => x.LastModified).First())
                    .ToList();

                // 3. separate the Jump / Story / Other / Blacklisted files (must join on Folder)
                // we want 5 sheets: Jumps, Stories, Others, Jumps (Unfiltered) & Stories (Unfiltered)
                // order them by descending date modified

                // Filter and group files by type
                var dataSheetJump = sheetJumpFiles
                    .Where(x => x.Type == Statics.FolderType.Jump)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetStory = sheetStoryFiles
                    .Where(x => x.Type == Statics.FolderType.Story)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetOther = allFiles
                    .Where(x => x.Type == Statics.FolderType.Other)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetJumpUnfiltered = allFiles
                    .Where(x => x.Type == Statics.FolderType.Jump)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetStoryUnfiltered = allFiles
                    .Where(x => x.Type == Statics.FolderType.Story)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                cancellationToken.ThrowIfCancellationRequested();

                // 4. upload the data to the sheets
                ScopedServices.LoadingForm.PrepareLoadingBar("Building Jumps sheet", dataSheetJump.Count);
                await UploadToSheetAsync(dataSheetJump, Statics.Sheet.File.SHEET_J, cancellationToken);

                ScopedServices.LoadingForm.PrepareLoadingBar("Building Stories sheet", dataSheetStory.Count);
                await UploadToSheetAsync(dataSheetStory, Statics.Sheet.File.SHEET_S, cancellationToken);

                ScopedServices.LoadingForm.PrepareLoadingBar("Building Others sheet", dataSheetOther.Count);
                await UploadToSheetAsync(dataSheetOther, Statics.Sheet.File.SHEET_O, cancellationToken);

                ScopedServices.LoadingForm.PrepareLoadingBar("Building Jumps (Unfiltered) sheet", dataSheetJumpUnfiltered.Count);
                await UploadToSheetAsync(dataSheetJumpUnfiltered, Statics.Sheet.File.SHEET_J_1, cancellationToken);

                ScopedServices.LoadingForm.PrepareLoadingBar("Building Stories (Unfiltered) sheet", dataSheetStoryUnfiltered.Count);
                await UploadToSheetAsync(dataSheetStoryUnfiltered, Statics.Sheet.File.SHEET_S_1, cancellationToken);

                ScopedServices.LoadingForm.PrepareLoadingBar("Building Whitelist sheet", allWhitelistEntries.Count);
                await UploadToSheetAsync(allWhitelistEntries.OrderBy(x => x.Name).ToList(), Statics.Sheet.Whitelist.SHEET_W, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Sheet building cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error building sheets: {e}");
                throw;
            }
        }

        /// <summary>
        /// Escapes a string for use in a Google Sheets formula.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EscapeForFormula(string input) => input?.Replace("\"", "\"\"") ?? string.Empty;

        /// <summary>
        /// Opens the registered Google Spreadsheet in the default browser.
        /// </summary>
        public static void GoToSheet()
        {
            var url = $"https://docs.google.com/spreadsheets/d/{SingletonServices.SpreadsheetId}/edit#gid={Statics.Sheet.File.SHEET_J}";
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            try
            {
                Process.Start(psi);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error opening sheet: {e}");
            }
        }

        public static async Task BatchSetInaccessible(string filePath, CancellationToken cancellationToken = default)
        {
            var ids = new List<string>();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. get the ids from the file
                string content = await File.ReadAllTextAsync(filePath, cancellationToken);
                string[] lines = content.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

                var regex = new Regex(@"(?:https:\/\/drive\.google\.com\/file\/d\/|^)([a-zA-Z0-9_-]{25,})(?=\/|$)", RegexOptions.Compiled);

                foreach (var line in lines)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        ids.Add(match.Groups[1].Value);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                // 2. for all ids in the FileEntry database, set Accessible = false
                List<FileEntry> entriesToUpdate;
                using (var db = new AppDbContext())
                {
                    entriesToUpdate = db.Files.Where(x => ids.Contains(x.Id)).ToList();
                }

                if (entriesToUpdate == null || entriesToUpdate.Count == 0)
                {
                    SingletonServices.LogForm.Log("❌ No entries found to update.");
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                using (var db = new AppDbContext())
                {
                    foreach (var entry in entriesToUpdate)
                    {
                        entry.Accessible = false;
                    }
                    db.Files.UpdateRange(entriesToUpdate);
                    await SafeSaveAsync(db, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Data modification cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error while modifying data: {e}");
                throw;
            }
        }

        /// <summary>
        /// Clears a range of files and folders from the database.
        /// If 'null' is passed, all files or folders will be deleted.
        /// Empty lists will delete nothing.
        /// Base behavior is to delete all files and folders.
        /// </summary>
        /// <param name="filesToDelete"></param>
        /// <param name="foldersToDelete"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ClearAllData(List<FileEntry> filesToDelete = null, List<WhitelistEntry> foldersToDelete = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (filesToDelete != null && filesToDelete.Count == 0 && foldersToDelete != null && foldersToDelete.Count == 0)
                {
                    SingletonServices.LogForm.Log("❌ No files or folders to delete.");
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                using (var db = new AppDbContext())
                {
                    if (filesToDelete != null && filesToDelete.Count > 0)
                    {
                        db.Files.RemoveRange(filesToDelete);
                    }
                    else if (filesToDelete == null)
                    {
                        db.Files.RemoveRange(db.Files);
                    }

                    if (foldersToDelete != null && foldersToDelete.Count > 0)
                    {
                        db.Whitelist.RemoveRange(foldersToDelete);
                    }
                    else if (foldersToDelete == null)
                    {
                        db.Whitelist.RemoveRange(db.Whitelist);
                    }

                    await SafeSaveAsync(db, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Data clear cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error clearing data: {e}");
                throw;
            }
        }

        /// <summary>
        /// Delete all broken entries from the database.
        /// </summary>
        /// <param name="deleteFiles"></param>
        /// <param name="deleteWhitelist"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task DeleteBrokenEntries(bool deleteFiles, bool deleteWhitelist, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                #region get all data
                List<FileEntry> allFileEntries = null;
                List<WhitelistEntry> allWhitelistEntries = null;
                using (var db = new AppDbContext())
                {
                    if (deleteFiles)
                        allFileEntries = [.. db.Files];
                    if (deleteWhitelist)
                        allWhitelistEntries = [.. db.Whitelist];
                }
                #endregion

                var brokenFiles = await DriveScanner.GetInaccessibleEntries(allFileEntries, cancellationToken);
                var brokenFolders = await DriveScanner.GetInaccessibleEntries(allWhitelistEntries, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                // delete the broken data from the database
                await ClearAllData(brokenFiles, brokenFolders, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Data clear cancelled.");
                return;
            }
            catch (Exception)
            {
                SingletonServices.LogForm.Log("❌ Error deleting broken entries.");
                throw;
            }
        }

        /// <summary>
        /// import data from a json file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ImportData(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. read the data from the file
                if (!File.Exists(filePath))
                {
                    SingletonServices.LogForm.Log($"❌ File not found: {filePath}");
                    return;
                }

                var datajson = await File.ReadAllTextAsync(filePath, cancellationToken);
                if (datajson == null || datajson.Length == 0)
                {
                    SingletonServices.LogForm.Log("❌ No data found in the file.");
                    return;
                }

                var data = JsonSerializer.Deserialize<DataSet>(datajson, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                var dbData = data?.Data;
                var preferences = data?.Preferences;

                // 2. load the db data into the database (erase exisintg data)
                if (dbData == null || ((dbData.FileEntries == null || dbData.FileEntries.Count == 0) && (dbData.WhitelistEntries == null || dbData.WhitelistEntries.Count == 0)))
                {
                    SingletonServices.LogForm.Log("❌ No db data found in the file.");
                }
                else
                {
                    using (var db = new AppDbContext())
                    {
                        // 2.b. add the data to the database
                        if (dbData.FileEntries != null && dbData.FileEntries.Count > 0)
                        {
                            // remove the existing file entries
                            db.Files.RemoveRange(db.Files);
                            await db.Files.AddRangeAsync(dbData.FileEntries, cancellationToken);
                        }
                        if (dbData.WhitelistEntries != null && dbData.WhitelistEntries.Count > 0)
                        {
                            // remove the existing whitelist entries
                            db.Whitelist.RemoveRange(db.Whitelist);
                            await db.Whitelist.AddRangeAsync(dbData.WhitelistEntries, cancellationToken);
                        }
                        // 2.c. save the changes to the database
                        await SafeSaveAsync(db, cancellationToken);
                    }
                }

                // 3. load the preferences into the preferences files
                if (preferences == null ||
                    ((preferences.CheckboxPreferences == null || preferences.CheckboxPreferences.Count == 0)
                    && preferences.OtherPreferences == null)
                )
                {
                    SingletonServices.LogForm.Log("❌ No preferences found in the file.");
                }
                else
                {
                    if (preferences.CheckboxPreferences != null && preferences.CheckboxPreferences.Count != 0)
                    {
                        // overwrite the preferences
                        PreferenceService.OverwriteCheckboxPreferences(preferences.CheckboxPreferences);
                    }

                    if (preferences.OtherPreferences != null)
                    {
                        // overwrite the preferences
                        PreferenceService.OverwriteOtherPreferences(preferences.OtherPreferences);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Data import cancelled.");
                return;
            }

            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error importing data: {e}");
                throw;
            }
        }

        /// <summary>
        /// Export data to a json file (name is generated from the current date)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task ExportData(string folderPath, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. get the current date
                var date = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

                // 2. create the file name
                var fileName = $"PyJump_DataExport_{date}.json";
                var filePath = Path.Combine(folderPath, fileName);

                // 3. get all data from the database
                DBDataSet dbData;
                using (var db = new AppDbContext())
                {
                    dbData = new DBDataSet
                    {
                        FileEntries = [.. db.Files],
                        WhitelistEntries = [.. db.Whitelist]
                    };
                }

                // 4. get the preferences
                PrefDataSet preferences = new();
                var cbPref = PreferenceService.GetCheckboxPreferences();
                var otherPref = PreferenceService.GetOtherPreferences();
                preferences.CheckboxPreferences = cbPref;
                preferences.OtherPreferences = otherPref;

                // 5. serialize the data to json
                var data = new DataSet
                {
                    Data = dbData,
                    Preferences = preferences
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });

                // 6. write the json to the file
                await File.WriteAllTextAsync(filePath, json, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("❌ Data export cancelled.");
                return;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error exporting data: {e}");
                throw;
            }
        }

        private static async Task SafeSaveAsync(AppDbContext db, CancellationToken cancellationToken = default)
        {
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"Error deleting whitelist entries: {e}");
                db.Dispose();
                throw;
            }
        }

        #endregion
    }
}
