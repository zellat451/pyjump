using System.Collections.Concurrent;
using System.Diagnostics;
using pyjump.Entities;
using pyjump.Forms;
using pyjump.Infrastructure;
using static pyjump.Services.Statics;

namespace pyjump.Services
{
    public static class Methods
    {
        public static void FullScan()
        {


        }

        public static async Task ScanWhitelist(LogForm logForm)
        {
            var drives = SingletonServices.MainDrives;

            var scanner = new DriveScanner();
            var allWhitelistEntries = await scanner.GetAllFolderNamesRecursiveAsync(drives.Data.Select(x => x.Url).ToList(), logForm);
            ConcurrentBag<WhitelistEntry> allWhitelistEntriesBag = [.. allWhitelistEntries];

            // check existing whitelist entries
            using (var db = new AppDbContext())
            {
                var existingEntries = db.Whitelist.ToList();

                // 1. entries existing in the database but not in the new list > remove the entries from the database
                var toRemove = existingEntries.Where(x => !allWhitelistEntriesBag.Select(y => y.Id).Contains(x.Id)).ToList();
                db.Whitelist.RemoveRange(toRemove);
                logForm.Log($"Removed {toRemove.Count} entries from the database.");
                Debug.WriteLine($"Removed {toRemove.Count} entries from the database.");

                // 2. entries existing in the new list but not in the database > add the entries to the database
                var toAdd = allWhitelistEntries.Where(x => !existingEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                db.Whitelist.AddRange(toAdd);
                logForm.Log($"Added {toAdd.Count} entries to the database.");
                Debug.WriteLine($"Added {toAdd.Count} entries to the database.");

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

                // commit the changes to the database
                db.SaveChanges();
            }
        }

        public static async Task ScanFiles(LogForm logForm)
        {
            // get all whitelist entries from the database
            using (var db = new AppDbContext())
            {
                var whitelistEntries = db.Whitelist.ToList();

                // get all files from the whitelist entries
                var scanner = new DriveScanner();
                var allFileEntries = await scanner.GetAllFilesInWhitelistAsync(whitelistEntries, logForm);
                ConcurrentBag<FileEntry> allFileEntriesBag = [.. allFileEntries];

                // check existing file entries
                var existingEntries = db.Files.ToList();

                // 1. entries existing in the new list but not in the database > add the entries to the database
                var toAdd = allFileEntriesBag.Where(x => !existingEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                db.Files.AddRange(toAdd);
                logForm.Log($"Added {toAdd.Count} file entries.");
                Debug.WriteLine($"Added {toAdd.Count} file entries.");

                // 2. entries existing in both lists > update the entries in the database if
                // the name | url | resource key | last modified date | owner | folder id is different
                var toUpdate = new ConcurrentBag<FileEntry>();
                var tasks = existingEntries.Select(entry =>
                {
                    var newEntry = allFileEntries.FirstOrDefault(x => x.Id == entry.Id);
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
                db.Files.UpdateRange(toUpdate);
                logForm.Log($"Updated {toUpdate.Count} file entries.");
                Debug.WriteLine($"Updated {toUpdate.Count} file entries.");

                await db.SaveChangesAsync();
            }
        }
    }
}
