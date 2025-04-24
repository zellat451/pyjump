using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Interop;
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
                // 3. entries existing in both lists > update the entries in the database if the name | url | resource key | yype is different
                var toUpdate = new ConcurrentBag<WhitelistEntry>();
                var tasks = existingEntries.Select(entry =>
                {
                    var newEntry = allWhitelistEntries.FirstOrDefault(x => x.Id == entry.Id);
                    if (newEntry != null)
                    {
                        if (entry.Name != newEntry.Name || entry.Url != newEntry.Url
                            || entry.ResourceKey != newEntry.ResourceKey || entry.Type != newEntry.Type)
                        {
                            entry.Name = newEntry.Name;
                            entry.Url = newEntry.Url;
                            entry.ResourceKey = newEntry.ResourceKey;
                            entry.LastChecked = null;
                            entry.Type = newEntry.Type;

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
                        logForm.Log($"Error adding file entries: {e.Message}");
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
                        logForm.Log($"Error updating file entries: {e.Message}");
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
                            logForm.Log($"Error updating whitelist entry: {e.Message}");
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
                        logForm.Log($"Error saving changes to the database: {e.Message}");
                        throw;
                    }
                }

                loadingForm.SetProgress(loadingForm.GetProgress() + 1);
            }

            List<FileEntry> allFiles;
            using (var db = new AppDbContext())
            {
                allFiles = db.Files.ToList();
            }

            loadingForm.SetLabel("Treating sets for files");
            loadingForm.SetProgress(0);
            loadingForm.SetMax(allFiles.Count);

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
                        loadingForm.SetProgress(loadingForm.GetProgress() + 1);
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
                                logForm.Log($"Error adding similar set: {e.Message}");
                                throw;
                            }
                            // 3.a.2. save the set to the database (this will generate the Id for the set)
                            try
                            {
                                await db.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                logForm.Log($"Error saving changes to the database: {e.Message}");
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
                                    logForm.Log($"Error adding file to similar set: {e.Message}");
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
                                logForm.Log($"Error saving changes to the database: {e.Message}");
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
                                    logForm.Log($"Error adding file to similar set: {e.Message}");
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
                                logForm.Log($"Error saving changes to the database: {e.Message}");
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
                                    db.SimilarSets.Update(set);
                                }
                                catch (Exception e)
                                {
                                    logForm.Log($"Error updating similar set: {e.Message}");
                                    throw;
                                }
                            }

                            // 3.b.5. save the changes to the database
                            try
                            {
                                await db.SaveChangesAsync();
                            }
                            catch (Exception e)
                            {
                                logForm.Log($"Error saving changes to the database: {e.Message}");
                                throw;
                            }
                        }
                    }

                    loadingForm.SetProgress(loadingForm.GetProgress() + 1);
                }
            }
            catch (Exception e)
            {
                logForm.Log($"Error treating sets for files: {e.Message}");
                throw;
            }
        }
    }
}
