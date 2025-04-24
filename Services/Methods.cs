using System.Collections.Concurrent;
using System.Diagnostics;
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

        public static async Task ScanFiles(LogForm logForm)
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
                        w.LastChecked = scannedFileEntries.Select(x => x.LastModified).Where(x => x.HasValue).OrderDescending().FirstOrDefault();
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
            }

            // regroup the files under sets
            using (var db = new AppDbContext())
            {
                var allFiles = db.Files.ToList();

                List<string> treatedIds = [];
                foreach (var file in allFiles)
                {
                    // 1. check if the file is already treated
                    if (treatedIds.Contains(file.Id)) { continue; }

                    // 2. find all files with the same name and owner (contains the current file)
                    var similarFiles = allFiles.Where(x => x.Name == file.Name && x.Owner == file.Owner).ToList();

                    // 3. create a new set for the similar files (OwnerFileEntryId is the file with the most recent 'LastModified' date)
                    var similarSet = new SimilarSet
                    {
                        OwnerFileEntryId = similarFiles.OrderByDescending(x => x.LastModified).FirstOrDefault()?.Id
                    };
                    try
                    {
                        db.SimilarSets.Add(similarSet);
                    }
                    catch (Exception e)
                    {
                        logForm.Log($"Error adding similar set: {e.Message}");
                        throw;
                    }

                    // 4. add the similar files to the set
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
                }

                // 5. save the changes to the database
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
    }
}
