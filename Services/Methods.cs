using System.Collections.Concurrent;
using System.Diagnostics;
using Google.Apis.Sheets.v4.Data;
using pyjump.Entities;
using pyjump.Forms;
using pyjump.Infrastructure;

namespace pyjump.Services
{
    public static class Methods
    {

        public static async Task ScanWhitelist(LogForm logForm)
        {
            #region get all folder entries for whitelist
            var drives = SingletonServices.MainDrives;

            var scanner = new DriveScanner();
            var allWhitelistEntries = await scanner.GetAllFolderNamesRecursiveAsync(drives.Data.Select(x => x.Url).ToList(), logForm);
            ConcurrentBag<WhitelistEntry> allWhitelistEntriesBag = [.. allWhitelistEntries];
            #endregion

            // check existing whitelist entries
            using (var db = new AppDbContext())
            {
                var existingEntries = db.Whitelist.ToList();

                #region delete missing whitelist entries
                // 1. entries existing in the database but not in the new list > remove the entries from the database
                var toRemove = existingEntries.Where(x => !allWhitelistEntriesBag.Select(y => y.Id).Contains(x.Id)).ToList();
                db.Whitelist.RemoveRange(toRemove);
                logForm.Log($"Removed {toRemove.Count} entries from the database.");
                Debug.WriteLine($"Removed {toRemove.Count} entries from the database.");
                #endregion

                #region add new whitelist entries
                // 2. entries existing in the new list but not in the database > add the entries to the database
                var toAdd = allWhitelistEntries.Where(x => !existingEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                db.Whitelist.AddRange(toAdd);
                logForm.Log($"Added {toAdd.Count} entries to the database.");
                Debug.WriteLine($"Added {toAdd.Count} entries to the database.");
                #endregion

                #region update existing whitelist entries
                // 3. entries existing in both lists > update the entries in the database if the name | url | resource key is different
                var toUpdate = new ConcurrentBag<WhitelistEntry>();
                var tasks = existingEntries.Select(entry =>
                {
                    var newEntry = allWhitelistEntries.FirstOrDefault(x => x.Id == entry.Id);
                    if (newEntry != null)
                    {
                        if (entry.Name != newEntry.Name || entry.Url != newEntry.Url
                            || entry.ResourceKey != newEntry.ResourceKey)
                        {
                            entry.Name = newEntry.Name;
                            entry.Url = newEntry.Url;
                            entry.ResourceKey = newEntry.ResourceKey;
                            entry.LastChecked = null;

                            toUpdate.Add(entry);
                        }
                    }
                    return Task.CompletedTask;
                });
                await Task.WhenAll(tasks);
                db.Whitelist.UpdateRange(toUpdate);
                logForm.Log($"Updated {toUpdate.Count} entries in the database.");
                Debug.WriteLine($"Updated {toUpdate.Count} entries in the database.");
                #endregion

                // commit the changes to the database
                db.SaveChanges();
            }
        }

        public static async Task ScanFiles(LogForm logForm, LoadingForm loadingForm)
        {
            #region get whitelist all entries
            // get all whitelist entries from the database
            List<WhitelistEntry> whitelistEntries;
            using (var db = new AppDbContext())
            {
                whitelistEntries = db.Whitelist.ToList();
            }
            #endregion

            loadingForm.PrepareLoadingBar("Scanning files", whitelistEntries.Count);

            // get all files from the whitelist entries
            var scanner = new DriveScanner();
            foreach (var w in whitelistEntries)
            {
                using (var db = new AppDbContext())
                {
                    #region get all files for one entry
                    var scannedFileEntries = await scanner.GetAllFilesInWhitelistAsync(w, logForm);
                    ConcurrentBag<FileEntry> scannedFileEntriesBag = [.. scannedFileEntries];

                    // check existing file entries
                    var currentFileEntries = db.Files.ToList();
                    #endregion

                    #region add new file entries
                    // 1. entries existing in the new list but not in the database > add the entries to the database
                    var toAdd = scannedFileEntriesBag.Where(x => !currentFileEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                    try
                    {
                        db.Files.AddRange(toAdd);
                    }
                    catch (Exception e)
                    {
                        logForm.Log($"Error adding file entries: {e}");
                        throw;
                    }
                    logForm.Log($"Added {toAdd.Count} file entries.");
                    Debug.WriteLine($"Added {toAdd.Count} file entries.");
                    #endregion

                    #region update existing file entries
                    // 2. entries existing in both lists > update the entries in the database if
                    // the name | url | resource key | last modified date | owner | folder id is different
                    var toUpdate = new ConcurrentBag<FileEntry>();
                    var tasks = currentFileEntries.Select(entry =>
                    {
                        var newEntry = scannedFileEntries.FirstOrDefault(x => x.Id == entry.Id);
                        if (newEntry != null)
                        {
                            if (entry.Name != newEntry.Name
                                || entry.Url != newEntry.Url
                                || entry.ResourceKey != newEntry.ResourceKey
                                || entry.LastModified != newEntry.LastModified
                                || entry.Owner != newEntry.Owner
                                || entry.FolderId != newEntry.FolderId)
                            {
                                entry.Name = newEntry.Name;
                                entry.Url = newEntry.Url;
                                entry.ResourceKey = newEntry.ResourceKey;
                                entry.LastModified = newEntry.LastModified;
                                entry.Owner = newEntry.Owner;
                                entry.FolderId = newEntry.FolderId;

                                toUpdate.Add(entry);
                            }
                        }
                        return Task.CompletedTask;
                    });
                    await Task.WhenAll(tasks);
                    try
                    {
                        db.Files.UpdateRange(toUpdate);
                    }
                    catch (Exception e)
                    {
                        logForm.Log($"Error updating file entries: {e}");
                        throw;
                    }
                    logForm.Log($"Updated {toUpdate.Count} file entries.");
                    Debug.WriteLine($"Updated {toUpdate.Count} file entries.");
                    #endregion

                    #region update the lastchecked date of the whitelist entry
                    if (scannedFileEntries.Count > 0)
                    {
                        // update the last checked date of the whitelist entry to today, midnight Utc
                        w.LastChecked = DateTime.UtcNow.Date;
                        try
                        {
                            db.Whitelist.Update(w);
                        }
                        catch (Exception e)
                        {
                            logForm.Log($"Error updating whitelist entry: {e}");
                            throw;
                        }
                    }
                    #endregion

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        logForm.Log($"Error saving changes to the database: {e}");
                        throw;
                    }
                }

                loadingForm.IncrementProgress();
            }

            List<FileEntry> allFiles;
            using (var db = new AppDbContext())
            {
                allFiles = db.Files.ToList();
            }

            loadingForm.PrepareLoadingBar("Treating sets for files", allFiles.Count);

            await TreatSetsForFiles(allFiles, logForm, loadingForm);
        }

        private static async Task TreatSetsForFiles(List<FileEntry> fileEntries, LogForm logForm, LoadingForm loadingForm)
        {
            try
            {
                // regroup the files under sets
                List<FileEntry> allFiles;
                using (var db = new AppDbContext())
                {
                    allFiles = db.Files.ToList();
                }

                List<string> treatedIds = [];
                foreach (var file in fileEntries)
                {
                    // 1. check if the file is already treated
                    if (treatedIds.Contains(file.Id))
                    {
                        loadingForm.IncrementProgress();
                        continue;
                    }

                    // 2. find all files with the same name and owner (contains the current file)
                    var similarFiles = allFiles.Where(x => x.Name == file.Name && x.Owner == file.Owner).ToList();
                    similarFiles.AddRange(fileEntries.Where(x => x.Name == file.Name && x.Owner == file.Owner));
                    similarFiles = similarFiles.DistinctBy(x => x.Id).ToList();

                    // 3. check if a set already exists for the files
                    LNKSimilarSetFile existingSet;
                    using (var db = new AppDbContext())
                    {
                        var similarIds = similarFiles.Select(x => x.Id).ToList();
                        existingSet = db.LNKSimilarSetFiles.FirstOrDefault(x => similarIds.Contains(x.FileEntryId));
                    }
                    if (existingSet == null)
                    {
                        // There is no set created for that file, or it would be registered in the LNKSimilarSetFile table
                        // 3.a.1. create a new set for the similar files (OwnerFileEntryId is the file with the most recent 'LastModified' date)
                        var similarSet = new SimilarSet
                        {
                            OwnerFileEntryId = similarFiles.OrderByDescending(x => x.LastModified).FirstOrDefault()?.Id
                        };
                        using (var db = new AppDbContext())
                        {
                            try
                            {
                                db.SimilarSets.Add(similarSet);
                            }
                            catch (Exception e)
                            {
                                logForm.Log($"Error adding similar set: {e}");
                                throw;
                            }
                            // 3.a.2. save the set to the database (this will generate the Id for the set)
                            try
                            {
                                await db.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                logForm.Log($"Error saving changes to the database: {e}");
                                throw;
                            }
                        }
                        using (var db = new AppDbContext())
                        {
                            // 3.a.3. add the similar files to the set
                            foreach (var similarFile in similarFiles)
                            {
                                var lnkSimilarSetFile = new LNKSimilarSetFile
                                {
                                    SimilarSetId = similarSet.Id,
                                    FileEntryId = similarFile.Id
                                };
                                try
                                {
                                    db.LNKSimilarSetFiles.Add(lnkSimilarSetFile);
                                }
                                catch (Exception e)
                                {
                                    logForm.Log($"Error adding file to similar set: {e}");
                                    throw;
                                }
                                treatedIds.Add(similarFile.Id);
                            }
                            // 3.a.4. save the changes to the database
                            try
                            {
                                await db.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                logForm.Log($"Error saving changes to the database: {e}");
                                throw;
                            }
                        }
                    }
                    else
                    {
                        // There is already a set created for that file
                        // 3.b.1. check in similarFiles all the files that are not in a set
                        var similarIds = similarFiles.Select(x => x.Id).ToList();
                        var filesNotInSet = similarFiles.Where(x => !existingSet.FileEntryId.Contains(x.Id)).Distinct().ToList();

                        // 3.b.2. add the files to the set
                        using (var db = new AppDbContext())
                        {
                            foreach (var similarFile in filesNotInSet)
                            {
                                var lnkSimilarSetFile = new LNKSimilarSetFile
                                {
                                    SimilarSetId = existingSet.SimilarSetId,
                                    FileEntryId = similarFile.Id
                                };
                                try
                                {
                                    if (!db.LNKSimilarSetFiles.Contains(lnkSimilarSetFile))
                                        db.LNKSimilarSetFiles.Add(lnkSimilarSetFile);
                                }
                                catch (Exception e)
                                {
                                    logForm.Log($"Error adding file to similar set: {e}");
                                    throw;
                                }
                                treatedIds.Add(similarFile.Id);
                            }

                            // 3.b.3. save the changes to the database
                            try
                            {
                                await db.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                logForm.Log($"Error saving changes to the database: {e}");
                                throw;
                            }
                        }

                        // 3.b.4. update the owner file entry id of the set
                        using (var db = new AppDbContext())
                        {
                            var set = db.SimilarSets.FirstOrDefault(x => x.Id == existingSet.SimilarSetId);
                            if (set != null)
                            {
                                set.OwnerFileEntryId = similarFiles.OrderByDescending(x => x.LastModified).FirstOrDefault()?.Id;
                                try
                                {
                                    if(!db.SimilarSets.Contains(set))
                                        db.SimilarSets.Update(set);
                                }
                                catch (Exception e)
                                {
                                    logForm.Log($"Error updating similar set: {e}");
                                    throw;
                                }

                                // 3.b.5. save the changes to the database
                                try
                                {
                                    await db.SaveChangesAsync();
                                }
                                catch (Exception e)
                                {
                                    logForm.Log($"Error saving changes to the database: {e}");
                                    throw;
                                }
                            }
                        }
                    }

                    logForm.Log($"✅ Set created/updated for {similarFiles.Count} files.");
                    loadingForm.IncrementProgress();
                }
            }
            catch (Exception e)
            {
                logForm.Log($"Error treating sets for files: {e}");
                throw;
            }
        }

        public static async Task BuildSheets(LogForm logForm, LoadingForm loadingForm)
        {
            try
            {
                loadingForm.PrepareLoadingBar("Building sheets - Initialization", 2);

                // 0. initialize by getting all files & whitelist entries from the database
                List<FileEntry> allFiles;
                using (var db = new AppDbContext())
                {
                    allFiles = db.Files.ToList();
                }
                List<WhitelistEntry> allWhitelistEntries;
                using (var db = new AppDbContext())
                {
                    allWhitelistEntries = db.Whitelist.ToList();
                }

                loadingForm.IncrementProgress();

                // 1. get all files which are not registered in the LNKSimilarSetFile table
                List<LNKSimilarSetFile> lnkSimilarSetFiles;
                using (var db = new AppDbContext())
                {
                    lnkSimilarSetFiles = db.LNKSimilarSetFiles.ToList();
                }

                var filesNotInSet = allFiles.Where(x => !lnkSimilarSetFiles.Select(y => y.FileEntryId).Contains(x.Id)).ToList();

                loadingForm.IncrementProgress();

                // 2. generate sets for the files not in a set
                if (filesNotInSet.Count > 0)
                {
                    logForm.Log($"Found {filesNotInSet.Count} files not in a set.");
                    loadingForm.PrepareLoadingBar("Treating sets for files", filesNotInSet.Count);
                    await TreatSetsForFiles(filesNotInSet, logForm, loadingForm);
                }

                // 3. get all sets from the database
                List<SimilarSet> allSets;
                using (var db = new AppDbContext())
                {
                    allSets = db.SimilarSets.ToList();
                }

                // 4. generate sheets data
                // get all owner file entries in sets
                var ownerFileEntriesIds = allSets.Select(x => x.OwnerFileEntryId).Distinct().ToList();
                var ownerFileEntries = allFiles.Where(x => ownerFileEntriesIds.Contains(x.Id)).ToList();

                // separate the Jump / Story / Other / Blacklisted files (must join on Folder)
                // we want 5 sheets: Jumps, Stories, Others, Jumps (Unfiltered) & Stories (Unfiltered)
                // order them by descending date modified

                // Create a lookup from folder ID to whitelist entry type
                var folderTypeLookup = allWhitelistEntries.ToDictionary(w => w.Id, w => w.Type);

                // Filter and group files by folder type
                var dataSheetJump = ownerFileEntries
                    .Where(x => folderTypeLookup.TryGetValue(x.FolderId, out var type) && type == Statics.FolderType.Jump)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetStory = ownerFileEntries
                    .Where(x => folderTypeLookup.TryGetValue(x.FolderId, out var type) && type == Statics.FolderType.Story)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetOther = allFiles
                    .Where(x => folderTypeLookup.TryGetValue(x.FolderId, out var type) && type == Statics.FolderType.Other)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetJumpUnfiltered = allFiles
                    .Where(x => folderTypeLookup.TryGetValue(x.FolderId, out var type) && type == Statics.FolderType.Jump)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var dataSheetStoryUnfiltered = allFiles
                    .Where(x => folderTypeLookup.TryGetValue(x.FolderId, out var type) && type == Statics.FolderType.Story)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                // 5. upload the data to the sheets
                loadingForm.PrepareLoadingBar("Building Jumps sheet", dataSheetJump.Count);
                await UploadToSheetAsync(dataSheetJump, Statics.Sheet.SHEET_J, allWhitelistEntries, logForm, loadingForm);

                loadingForm.PrepareLoadingBar("Building Stories sheet", dataSheetStory.Count);
                await UploadToSheetAsync(dataSheetStory, Statics.Sheet.SHEET_S, allWhitelistEntries, logForm, loadingForm);

                loadingForm.PrepareLoadingBar("Building Others sheet", dataSheetOther.Count);
                await UploadToSheetAsync(dataSheetOther, Statics.Sheet.SHEET_O, allWhitelistEntries, logForm, loadingForm);

                loadingForm.PrepareLoadingBar("Building Jumps (Unfiltered) sheet", dataSheetJumpUnfiltered.Count);
                await UploadToSheetAsync(dataSheetJumpUnfiltered, Statics.Sheet.SHEET_J_1, allWhitelistEntries, logForm, loadingForm);

                loadingForm.PrepareLoadingBar("Building Stories (Unfiltered) sheet", dataSheetStoryUnfiltered.Count);
                await UploadToSheetAsync(dataSheetStoryUnfiltered, Statics.Sheet.SHEET_S_1, allWhitelistEntries, logForm, loadingForm);
            }
            catch (Exception e)
            {
                logForm.Log($"Error building sheets: {e}");
                throw;
            }
        }
        
        private static string EscapeForFormula(string input) => input?.Replace("\"", "\"\"") ?? string.Empty;

        private static async Task UploadToSheetAsync(List<FileEntry> entries, string sheetName, List<WhitelistEntry> whitelist, LogForm logForm, LoadingForm loadingForm)
        {
            if (entries.Count == 0)
            {
                logForm.Log($"No entries to upload to sheet '{sheetName}'.");
                return;
            }

            var service = ScopedServices.SheetsService;

            // Step 1: Get the sheet ID and current grid size
            var spreadsheet = await service.Spreadsheets.Get(SingletonServices.SpreadsheetId).ExecuteAsync();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            if (sheet == null)
            {
                logForm.Log($"❌ Sheet '{sheetName}' not found in spreadsheet.");
                throw new Exception($"❌ Sheet '{sheetName}' not found in spreadsheet.");
            }

            int sheetId = (int)sheet.Properties.SheetId;
            var currentRowCount = sheet.Properties.GridProperties.RowCount ?? 1;
            var currentColCount = sheet.Properties.GridProperties.ColumnCount ?? 1;

            int rowsNeeded = entries.Count + 1; // +1 for header row
            int colsNeeded = Statics.Sheet.SHEET_COLS;

            // Step 2: Resize the sheet
            var resizeRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
            {
                new Request
                {
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
            }
            };

            await service.Spreadsheets.BatchUpdate(resizeRequest, SingletonServices.SpreadsheetId).ExecuteAsync();


            // Step 3: Build data and clearing requests
            var dataRequests = new List<Request>();

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

            // 3.2: Build row data
            var cellData = new List<RowData>();
            foreach (var entry in entries)
            {
                var whitelistEntry = whitelist.FirstOrDefault(w => w.Id == entry.FolderId);
                string locationName = whitelistEntry?.Name ?? "Unknown";
                string locationUrl = whitelistEntry?.Url ?? "";
                string escapedEntryName = EscapeForFormula(entry.Name);
                string escapedLocationName = EscapeForFormula(locationName);

                cellData.Add(new RowData
                {
                    Values =
                    [
                        new CellData
                        {
                            UserEnteredValue = new ExtendedValue
                            {
                                FormulaValue = $"=HYPERLINK(\"{entry.Url}\", \"{escapedEntryName}\")"
                            }
                        },
                        new CellData
                        {
                            UserEnteredValue = new ExtendedValue
                            {
                                FormulaValue = $"=HYPERLINK(\"{locationUrl}\", \"{escapedLocationName}\")"
                            }
                        },
                        new CellData
                        {
                            UserEnteredValue = new ExtendedValue
                            {
                                StringValue = entry.Owner
                            }
                        },
                        new CellData
                        {
                            UserEnteredValue = new ExtendedValue
                            {
                                StringValue = $"{entry.LastModified:yyyy-MM-dd HH:mm:ss}"
                            }
                        }
                    ]
                });

                loadingForm.IncrementProgress();
            }

            // 3.3: Write data starting from row 1
            dataRequests.Add(new Request
            {
                UpdateCells = new UpdateCellsRequest
                {
                    Start = new GridCoordinate { SheetId = sheetId, RowIndex = 1, ColumnIndex = 0 },
                    Rows = cellData,
                    Fields = "userEnteredValue"
                }
            });

            // Step 4: Execute the data update batch
            await service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = dataRequests }, SingletonServices.SpreadsheetId).ExecuteAsync();

            logForm.Log($"✅ Uploaded {entries.Count} entries to sheet '{sheetName}'.");
        }

        public static void GoToSheet()
        {
            var url = $"https://docs.google.com/spreadsheets/d/{SingletonServices.SpreadsheetId}/edit#gid={Statics.Sheet.SHEET_J}";
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
    }
}
