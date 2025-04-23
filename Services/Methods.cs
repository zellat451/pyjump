using System.Diagnostics;
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

            // check existing whitelist entries
            using (var db = new AppDbContext())
            {
                var existingEntries = db.Whitelist.ToList();

                // 1. entries existing in the database but not in the new list > remove the entries from the database
                var toRemove = existingEntries.Where(x => !allWhitelistEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                foreach (var entry in toRemove)
                {
                    db.Whitelist.Remove(entry);
                    logForm.Log($"Removed entry: {entry.Name}");
                    Debug.WriteLine($"Removed entry: {entry.Name}");
                }

                // 2. entries existing in the new list but not in the database > add the entries to the database
                var toAdd = allWhitelistEntries.Where(x => !existingEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                foreach (var entry in toAdd)
                {
                    db.Whitelist.Add(entry);
                    logForm.Log($"Added entry: {entry.Name}");
                    Debug.WriteLine($"Added entry: {entry.Name}");
                }

                // 3. entries existing in both lists > update the entries in the database if the name | url | resource key | yype is different
                foreach (var entry in existingEntries)
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
                            db.Whitelist.Update(entry);
                            logForm.Log($"Updated entry: {entry.Name}");
                            Debug.WriteLine($"Updated entry: {entry.Name}");
                        }
                    }
                }

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

                // check existing file entries
                var existingEntries = db.Files.ToList();

                // 1. entries existing in the new list but not in the database > add the entries to the database
                var toAdd = allFileEntries.Where(x => !existingEntries.Select(y => y.Id).Contains(x.Id)).ToList();
                foreach (var entry in toAdd)
                {
                    db.Files.Add(entry);
                    logForm.Log($"Added file entry: {entry.Name}");
                    Debug.WriteLine($"Added file entry: {entry.Name}");
                }

                // 2. entries existing in both lists > update the entries in the database if
                // the name | url | resource key | last modified date | owner | folder id is different
                foreach (var entry in existingEntries)
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
                            db.Files.Update(entry);
                            logForm.Log($"Updated file entry: {entry.Name}");
                            Debug.WriteLine($"Updated file entry: {entry.Name}");
                        }
                    }
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
