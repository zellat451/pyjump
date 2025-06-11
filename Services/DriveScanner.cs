using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Apis.Drive.v3.Data;
using pyjump.Entities;
using pyjump.Forms;
using pyjump.Interfaces;
using static pyjump.Services.Statics;

namespace pyjump.Services
{
    public class DriveScanner
    {
        public DriveScanner()
        {
            _visitedFolderIds = [];
            _foundFolderNames = [];
            var jsonKeywords = SingletonServices.GetAppsettingsValue("keywords");
            if (string.IsNullOrEmpty(jsonKeywords))
                _keywords = [];
            else
            {
                var keywords = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonKeywords, options: new() { PropertyNameCaseInsensitive = true });
                _keywords = new ConcurrentDictionary<string, string>(keywords);
                foreach (var k in _keywords.Where(x => !FolderType.AllTypes.Contains(x.Value)))
                    _keywords.Remove(k.Key, out _);
            }

            _folderQueue = [];
        }

        private readonly ConcurrentDictionary<string, bool> _visitedFolderIds;
        private readonly ConcurrentBag<WhitelistEntry> _foundFolderNames;
        private readonly ConcurrentDictionary<string, string> _keywords;

        private readonly BlockingCollection<(string folderId, string resourceKey, string driveId, string parentName)> _folderQueue
            = [.. new ConcurrentQueue<(string, string, string, string)>()];

        private int _activeWorkers = 0;

        /// <summary>
        /// Get all folder names recursively from the given folder URLs.
        /// </summary>
        /// <param name="folderUrls"></param>
        /// <returns></returns>
        public async Task<List<WhitelistEntry>> GetAllFolderNamesRecursiveAsync(List<Entities.Drive> drives, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rootFolderIds = drives.Select(drive => (drive.Name, ExtractFolderInfosFromUrl(drive.Url))).DistinctBy(x => x.Item2.folderId);

                foreach (var (driveName, (folderId, resourceKey, driveId)) in rootFolderIds)
                {
                    if (string.IsNullOrEmpty(folderId))
                        continue;
                    var folderName = string.IsNullOrEmpty(driveName) ? folderId : driveName;
                    _folderQueue.Add((folderId, resourceKey, driveId, folderName), cancellationToken);
                }

                if (!SingletonServices.AllowThreading)
                {
                    // Single-threaded mode
                    while (_folderQueue.TryTake(out var folder))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await TraverseFolderAsync(folder.folderId, folder.resourceKey, folder.driveId, folder.parentName, cancellationToken);
                    }
                }
                else
                {
                    // Multi-threaded mode with dedicated threads
                    var threads = new List<Thread>();
                    int threadCount = SingletonServices.MaxThreads;

                    for (int i = 0; i < threadCount; i++)
                    {
                        var thread = new Thread(async () =>
                        {
                            try
                            {
                                while (!_folderQueue.IsCompleted)
                                {
                                    if (_folderQueue.TryTake(out var folder, Timeout.Infinite, cancellationToken))
                                    {
                                        try
                                        {
                                            Interlocked.Increment(ref _activeWorkers);
                                            await TraverseFolderAsync(folder.folderId, folder.resourceKey, folder.driveId, folder.parentName, cancellationToken);
                                            Interlocked.Decrement(ref _activeWorkers);
                                        }
                                        catch (OperationCanceledException)
                                        {
                                            // Graceful thread exit on cancellation
                                        }
                                        catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OperationCanceledException))
                                        {
                                            // Also OK if the task was canceled internally
                                        }
                                        catch (Exception ex)
                                        {
                                            SingletonServices.LogForm.Log($"⚠️ Worker thread error: {ex.Message}");
                                        }

                                    }
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                // Thread ends gracefully
                            }
                            catch (Exception ex)
                            {
                                SingletonServices.LogForm.Log($"⚠️ Worker thread error: {ex.Message}");
                            }
                        })
                        {
                            IsBackground = true
                        };
                        thread.Start();
                        threads.Add(thread);
                    }

                    // Monitor for cancellation
                    while (!_folderQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(200, cancellationToken); // avoid tight polling
                        if (_folderQueue.Count == 0 && Interlocked.CompareExchange(ref _activeWorkers, 0, 0) == 0)
                        {
                            _folderQueue.CompleteAdding();
                            break;
                        }
                    }

                    foreach (var thread in threads)
                        thread.Join();
                }

                cancellationToken.ThrowIfCancellationRequested();

                return [.. _foundFolderNames.DistinctBy(x => x.Id)];
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("🔍 Folder scanning cancelled.");
                return [];
            }
            catch (Exception ex)
            {
                SingletonServices.LogForm.Log($"❌ Error during folder scanning: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Recursively traverse a folder and its subfolders to find all folder names.
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="resourceKey"></param>
        /// <param name="driveId"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        private async Task TraverseFolderAsync(string folderId, string resourceKey, string driveId, string parentName, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_visitedFolderIds.ContainsKey(folderId))
                    return;

                _visitedFolderIds.TryAdd(folderId, true);

                Google.Apis.Drive.v3.Data.File folderMetadata = null;
                try
                {
                    var request = ScopedServices.DriveService.Files.Get(folderId);
                    request.SupportsAllDrives = true;
                    if (!string.IsNullOrEmpty(resourceKey))
                        AddRequestParameter(request, "resourceKey", resourceKey);
                    if (!string.IsNullOrEmpty(driveId))
                        AddRequestParameter(request, "driveId", driveId);

                    folderMetadata = await request.ExecuteAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    SingletonServices.LogForm.Log("🔍 Folder scanning cancelled.");
                    throw;
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    var url = BuildFolderUrl(folderId, resourceKey, driveId);

                    if (!string.IsNullOrEmpty(resourceKey))
                        SingletonServices.LogForm.Log($"⚠️ Folder: {folderId}, ResourceKey: {resourceKey} not found or inaccessible (404). Skipping.");
                    else
                        SingletonServices.LogForm.Log($"⚠️ Folder {folderId} not found or inaccessible (404). Skipping.");
                    SingletonServices.LogForm.Log($"🔗 Folder {folderId} URL: {url}");
                }
                catch (Google.GoogleApiException ex)
                {
                    SingletonServices.LogForm.Log($"❌ API error getting metadata for {folderId}: {ex.Message}");
                    return;
                }
                catch (Exception ex)
                {
                    SingletonServices.LogForm.Log($"❌ Unexpected error getting metadata for {folderId}: {ex.Message}");
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (folderMetadata?.MimeType != GoogleMimeTypes.Folder)
                    return;

                var whitelistEntry = new WhitelistEntry()
                {
                    Id = folderMetadata.Id,
                    ResourceKey = folderMetadata.ResourceKey ?? string.Empty,
                    DriveId = folderMetadata.DriveId ?? string.Empty,
                    Name = parentName == string.Empty
                        ? folderMetadata.Name
                        : Path.Combine(parentName, folderMetadata.Name),
                    Url = BuildFolderUrl(folderMetadata.Id, folderMetadata.ResourceKey, folderMetadata.DriveId),
                };
                whitelistEntry.Type = GetCorrespondingType(whitelistEntry);
                _foundFolderNames.Add(whitelistEntry);
                SingletonServices.LogForm.Log($"✅ Found folder: {whitelistEntry.Name} ({folderMetadata.Id})");
                if (_foundFolderNames.Count % 50 == 0)
                    SingletonServices.LogForm.Log($">>> Folder count: {_foundFolderNames.Count}");

                string query = $"'{folderId}' in parents and trashed = false";
                string pageToken = null;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    FileList result;
                    try
                    {
                        var request = ScopedServices.DriveService.Files.List();
                        request.Q = query;
                        request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails)";
                        request.PageToken = pageToken;
                        request.SupportsAllDrives = true;

                        result = await request.ExecuteAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        SingletonServices.LogForm.Log("🔍 Folder scanning cancelled.");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        SingletonServices.LogForm.Log($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                        return;
                    }

                    foreach (var file in result.Files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        try
                        {
                            if (file.MimeType == GoogleMimeTypes.Folder)
                            {
                                _folderQueue.Add((file.Id, file.ResourceKey, file.DriveId, whitelistEntry.Name), cancellationToken);
                            }
                            else if (file.MimeType == GoogleMimeTypes.Shortcut &&
                                     file.ShortcutDetails?.TargetMimeType == GoogleMimeTypes.Folder &&
                                     file.ShortcutDetails?.TargetId != null)
                            {
                                _folderQueue.Add((file.ShortcutDetails.TargetId, file.ShortcutDetails.TargetResourceKey, file.DriveId, whitelistEntry.Name), cancellationToken);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            SingletonServices.LogForm.Log("🔍 Folder scanning cancelled.");
                            throw;
                        }
                    }

                    pageToken = result.NextPageToken;

                } while (pageToken != null);
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("🔍 Folder scanning cancelled.");
                throw;
            }
            catch (Google.GoogleApiException ex)
            {
                SingletonServices.LogForm.Log($"❌ API error: {ex.Message}");
                throw;
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"❌ Unexpected error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all files in a given whitelist entry.
        /// </summary>
        /// <param name="whitelist"></param>
        /// <returns></returns>
        public async Task<List<FileEntry>> GetAllFilesInWhitelistAsync(WhitelistEntry whitelist, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var files = new List<FileEntry>();

                if (whitelist.Type == FolderType.Blacklisted)
                    return files;

                string folderId = whitelist.Id;

                string query;
                if (whitelist.LastChecked != null && whitelist.LastChecked != DateTime.MinValue)
                    query = $"'{folderId}' in parents and trashed = false and (modifiedTime > '{whitelist.LastChecked:yyyy-MM-ddTHH:mm:ssZ}' or createdTime > '{whitelist.LastChecked:yyyy-MM-ddTHH:mm:ssZ}')";
                else
                    query = $"'{folderId}' in parents and trashed = false";
                string pageToken = null;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    FileList result;
                    try
                    {
                        var request = ScopedServices.DriveService.Files.List();
                        request.Q = query;
                        request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails, modifiedTime, createdTime, owners, resourceKey)";
                        request.PageToken = pageToken;
                        request.SupportsAllDrives = true;

                        result = await request.ExecuteAsync(cancellationToken);
                    }
                    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                    {
                        SingletonServices.LogForm.Log($"⚠️ Folder {folderId} not found or inaccessible (404). Skipping.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        SingletonServices.LogForm.Log($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (result.Files == null || result.Files.Count == 0)
                    {
                        SingletonServices.LogForm.Log($"✅ No files found in folder {whitelist.Name}");
                        break;
                    }

                    foreach (var file in result.Files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Google.Apis.Drive.v3.Data.File actualFile = file;

                        if (file.MimeType == GoogleMimeTypes.Folder)
                            continue;

                        if (file.MimeType == GoogleMimeTypes.Shortcut)
                        {
                            var targetId = file.ShortcutDetails?.TargetId;
                            if (targetId == null) continue;

                            try
                            {
                                var request = ScopedServices.DriveService.Files.Get(targetId);
                                request.SupportsAllDrives = true;
                                if (!string.IsNullOrEmpty(file.ShortcutDetails?.TargetResourceKey))
                                    AddRequestParameter(request, "resourceKey", file.ShortcutDetails?.TargetResourceKey);
                                if (!string.IsNullOrEmpty(file.DriveId))
                                    AddRequestParameter(request, "driveId", file.DriveId);

                                actualFile = await request.ExecuteAsync(cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                SingletonServices.LogForm.Log("🔍 File scanning cancelled.");
                                throw;
                            }
                            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                            {
                                var url = BuildFileUrl(targetId, file.ShortcutDetails?.TargetResourceKey, file.DriveId);

                                if (!string.IsNullOrEmpty(file.ShortcutDetails?.TargetResourceKey))
                                    SingletonServices.LogForm.Log($"⚠️ Shortcut target {targetId}, ResourceKey: {file.ShortcutDetails?.TargetResourceKey} not found or inaccessible (404). Skipping.");
                                else
                                    SingletonServices.LogForm.Log($"⚠️ Shortcut target {targetId} not found or inaccessible (404). Skipping.");
                                SingletonServices.LogForm.Log($"🔗 Shortcut {targetId} URL: {url}");
                                continue;
                            }
                            catch (Exception ex)
                            {
                                SingletonServices.LogForm.Log($"❌ Failed to resolve shortcut {file.Id}: {ex.Message}");
                                continue;
                            }

                            if (actualFile.MimeType == GoogleMimeTypes.Folder)
                                continue;

                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        var lowestTime = DateTime.Parse("1981-01-01T00:00:00Z").ToUniversalTime();
                        var modifiedTime = (actualFile.ModifiedTimeDateTimeOffset ?? actualFile.CreatedTimeDateTimeOffset)?.UtcDateTime;
                        if (modifiedTime == null || modifiedTime < lowestTime)
                        {
                            if (!string.IsNullOrEmpty(actualFile.ModifiedTimeRaw) && DateTime.TryParse(actualFile.ModifiedTimeRaw, out var d) && d.ToUniversalTime() > lowestTime)
                                modifiedTime = DateTime.Parse(actualFile.ModifiedTimeRaw).ToUniversalTime();
                            else if (!string.IsNullOrEmpty(actualFile.CreatedTimeRaw) && DateTime.TryParse(actualFile.CreatedTimeRaw, out d) && d.ToUniversalTime() > lowestTime)
                                modifiedTime = DateTime.Parse(actualFile.CreatedTimeRaw).ToUniversalTime();
                            else if (file.MimeType == GoogleMimeTypes.Shortcut)
                            {
                                if (!string.IsNullOrEmpty(file.ModifiedTimeRaw) && DateTime.TryParse(file.ModifiedTimeRaw, out var d2) && d2.ToUniversalTime() > lowestTime)
                                    modifiedTime = DateTime.Parse(file.ModifiedTimeRaw).ToUniversalTime();
                                else if (!string.IsNullOrEmpty(file.CreatedTimeRaw) && DateTime.TryParse(file.CreatedTimeRaw, out d2) && d2.ToUniversalTime() > lowestTime)
                                    modifiedTime = DateTime.Parse(file.CreatedTimeRaw).ToUniversalTime();
                            }
                        }

                        var owner = actualFile.Owners?.FirstOrDefault()?.DisplayName;
                        if (owner == null && file.MimeType == GoogleMimeTypes.Shortcut)
                        {
                            owner = file.Owners?.FirstOrDefault()?.DisplayName;
                        }

                        var fileEntry = new FileEntry
                        {
                            Id = actualFile.Id,
                            ResourceKey = actualFile.ResourceKey ?? string.Empty,
                            DriveId = actualFile.DriveId ?? string.Empty,
                            Url = BuildFileUrl(actualFile.Id, actualFile.ResourceKey, actualFile.DriveId),
                            Name = actualFile.Name,
                            LastModified = modifiedTime ?? DateTime.MinValue,
                            Owner = owner ?? "Unknown",
                            FolderId = whitelist.Id,
                            FolderName = whitelist.Name,
                            FolderUrl = whitelist.Url,
                        };
                        fileEntry.Type = this.GetCorrespondingType(fileEntry, whitelist);

                        files.Add(fileEntry);
                        SingletonServices.LogForm.Log($"📄 Found file: {fileEntry.Name} in {whitelist.Name}");
                    }

                    pageToken = result.NextPageToken;

                } while (pageToken != null);

                cancellationToken.ThrowIfCancellationRequested();

                return [.. files.DistinctBy(x => x.Id)];
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("🔍 File scanning cancelled.");
                return [];
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"❌ Error getting files in folder {whitelist.Name}: {e.Message}");
                throw;
            }
        }

        public static async Task<List<T>> GetInaccessibleEntries<T>(List<T> entries, CancellationToken cancellationToken = default)
            where T : ISheetDataEntity
        {
            try
            {
                if (entries == null || entries.Count == 0) return [];

                cancellationToken.ThrowIfCancellationRequested();

                var isFolderCheck = typeof(T) == typeof(WhitelistEntry);

                SingletonServices.LoadingForm.PrepareLoadingBar($"Checking {(isFolderCheck ? "folders" : "files")}", entries.Count);

                if (SingletonServices.AllowThreading)
                {
                    // Multi-threaded mode
                    return await GetInaccessibleEntriesMultiThread(entries, isFolderCheck, cancellationToken);
                }
                else
                {
                    // Single-threaded mode
                    return await GetInaccessibleEntriesSingleThread(entries, isFolderCheck, cancellationToken);
                }

            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("🔍 Entry checking cancelled.");
                return [];
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"❌ Error checking entries: {e.Message}");
                throw;
            }
        }

        private static async Task<List<T>> GetInaccessibleEntriesSingleThread<T>(List<T> entries, bool isFolderCheck, CancellationToken cancellationToken = default)
            where T : ISheetDataEntity
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var brokenEntries = new List<T>();

                SingletonServices.LoadingForm.PrepareLoadingBar($"Checking {(isFolderCheck ? "folders" : "files")}", entries.Count);

                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var request = ScopedServices.DriveService.Files.Get(entry.Id);
                        request.SupportsAllDrives = true;
                        if (!string.IsNullOrEmpty(entry.ResourceKey))
                            AddRequestParameter(request, "resourceKey", entry.ResourceKey);
                        if (!string.IsNullOrEmpty(entry.DriveId))
                            AddRequestParameter(request, "driveId", entry.DriveId);

                        var file = await request.ExecuteAsync(cancellationToken);

                        if (file.Trashed.HasValue && file.Trashed.Value)
                        {
                            SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) is in trash. Adding to deletion list.");
                            brokenEntries.Add(entry);
                        }
                        else if (isFolderCheck && file.MimeType != GoogleMimeTypes.Folder)
                        {
                            SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) is not a folder anymore. Adding to deletion list.");
                            brokenEntries.Add(entry);
                        }
                        else if (!isFolderCheck && file.MimeType == GoogleMimeTypes.Folder)
                        {
                            SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) is now a folder, for some reason. Adding to deletion list.");
                            brokenEntries.Add(entry);
                        }
                        else
                        {
                            SingletonServices.LogForm.Log($"✅ Entry {entry.Name} (id: {entry.Id}, url: {entry.Url}) is accessible.");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        SingletonServices.LogForm.Log("🔍 Scanning cancelled.");
                        throw;
                    }
                    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) not found (404). May be unauthorized to check status. Skipping it.");
                    }
                    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) inaccessible (403). Adding to deletion list.");
                        brokenEntries.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        SingletonServices.LogForm.Log($"❌ Unexpected error checking Entry {entry.Name} (id: {entry.Id}, url: {entry.Url}). Skipping it: {ex.Message}");
                    }
                    finally
                    {
                        SingletonServices.LoadingForm.IncrementProgress();
                    }
                }

                return brokenEntries;
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("🔍 Entry checking cancelled.");
                return [];
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"❌ Error checking entries: {e.Message}");
                throw;
            }
        }

        private static async Task<List<T>> GetInaccessibleEntriesMultiThread<T>(List<T> entries, bool isFolderCheck, CancellationToken cancellationToken = default)
            where T : ISheetDataEntity
        {
            try
            {
                var maxThreads = SingletonServices.MaxThreads;
                var entryQueue = new BlockingCollection<T>(new ConcurrentQueue<T>(entries));
                entryQueue.CompleteAdding();
                var brokenEntries = new ConcurrentBag<T>();
                var threads = new List<Thread>();
                var countdown = new CountdownEvent(maxThreads);

                for (int i = 0; i < maxThreads; i++)
                {
                    var thread = new Thread(async () =>
                    {
                        try
                        {
                            foreach (var entry in entryQueue.GetConsumingEnumerable(cancellationToken))
                            {
                                if (cancellationToken.IsCancellationRequested)
                                    break;

                                try
                                {
                                    try
                                    {
                                        var request = ScopedServices.DriveService.Files.Get(entry.Id);
                                        request.SupportsAllDrives = true;
                                        if (!string.IsNullOrEmpty(entry.ResourceKey))
                                            AddRequestParameter(request, "resourceKey", entry.ResourceKey);
                                        if (!string.IsNullOrEmpty(entry.DriveId))
                                            AddRequestParameter(request, "driveId", entry.DriveId);

                                        var file = await request.ExecuteAsync();

                                        if (file.Trashed.HasValue && file.Trashed.Value)
                                        {
                                            SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) is in trash. Adding to deletion list.");
                                            brokenEntries.Add(entry);
                                        }
                                        else if (isFolderCheck && file.MimeType != GoogleMimeTypes.Folder)
                                        {
                                            SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) is not a folder anymore. Adding to deletion list.");
                                            brokenEntries.Add(entry);
                                        }
                                        else if (!isFolderCheck && file.MimeType == GoogleMimeTypes.Folder)
                                        {
                                            SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) is now a folder, for some reason. Adding to deletion list.");
                                            brokenEntries.Add(entry);
                                        }
                                        else
                                        {
                                            SingletonServices.LogForm.Log($"✅ Entry {entry.Name} (id: {entry.Id}, url: {entry.Url}) is accessible.");
                                        }
                                    }
                                    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                                    {
                                        SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) not found (404). May be unauthorized to check status. Skipping it.");
                                    }
                                    catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                                    {
                                        SingletonServices.LogForm.Log($"⚠️ Entry {entry.Name} ({entry.Url}) inaccessible (403). Adding to deletion list.");
                                        brokenEntries.Add(entry);
                                    }
                                    catch (Exception ex)
                                    {
                                        SingletonServices.LogForm.Log($"❌ Unexpected error checking Entry {entry.Name} (id: {entry.Id}, url: {entry.Url}). Skipping it: {ex.Message}");
                                    }
                                    finally
                                    {
                                        SingletonServices.LoadingForm.IncrementProgress();
                                    }
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
                            SingletonServices.LogForm.Log("🔍 Entry checking cancelled.");
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

                return [.. brokenEntries];
            }
            catch (OperationCanceledException)
            {
                SingletonServices.LogForm.Log("🔍 Entry checking cancelled.");
                return [];
            }
            catch (Exception e)
            {
                SingletonServices.LogForm.Log($"❌ Error checking entries: {e.Message}");
                throw;
            }
        }

        #region private common methods
        private static void AddRequestParameter(Google.Apis.Drive.v3.FilesResource.GetRequest request, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                request.RequestParameters.Add(name, new Google.Apis.Discovery.Parameter
                {
                    Name = name,
                    IsRequired = false,
                    ParameterType = "query",
                    DefaultValue = value,
                    Pattern = null
                });
            }
        }

        /// <summary>
        /// Extracts folderId, resourceKey, and driveId from a given URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private (string folderId, string resourceKey, string driveId) ExtractFolderInfosFromUrl(string url)
        {
            // Initialize default values
            string folderId = null;
            string resourceKey = null;
            string driveId = null;

            // Matches for folderId, resourceKey, and driveId in the URL
            var folderMatch = Regex.Match(url, @"(?:folders/|id=)([a-zA-Z0-9_-]+)");
            if (folderMatch.Success)
            {
                folderId = folderMatch.Groups[1].Value;
            }

            // Check for resourceKey in the URL
            var resourceKeyMatch = Regex.Match(url, @"(?:resourcekey=)([a-zA-Z0-9_-]+)");
            if (resourceKeyMatch.Success)
            {
                resourceKey = resourceKeyMatch.Groups[1].Value;
            }

            // Check for driveId (tid) in the URL (Shared Drives)
            var driveIdMatch = Regex.Match(url, @"(?:tid=)([a-zA-Z0-9_-]+)");
            if (driveIdMatch.Success)
            {
                driveId = driveIdMatch.Groups[1].Value;
            }

            // Return tuple containing folderId, resourceKey, and driveId
            return (folderId, resourceKey, driveId);
        }

        /// <summary>
        /// Builds a Google Drive folder URL using the provided folderId, resourceKey, and tid (Shared Drive ID).
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="resourceKey"></param>
        /// <param name="tid"></param>
        /// <returns></returns>
        private static string BuildFolderUrl(string folderId, string resourceKey, string tid)
        {
            // Construct the base URL for the folder
            var url = $"https://drive.google.com/drive/folders/{folderId}";

            // If a resourceKey is provided, add it to the URL
            if (!string.IsNullOrEmpty(resourceKey))
            {
                url += $"?resourcekey={Uri.EscapeDataString(resourceKey)}";
            }

            // If a tid (Shared Drive ID) is provided, append it to the URL as a query parameter
            if (!string.IsNullOrEmpty(tid))
            {
                // If there's already a query parameter (resourcekey), append the tid as another parameter
                if (url.Contains('?'))
                {
                    url += $"&tid={Uri.EscapeDataString(tid)}";
                }
                else
                {
                    // Otherwise, add tid as the first query parameter
                    url += $"?tid={Uri.EscapeDataString(tid)}";
                }
            }

            return url;
        }

        /// <summary>
        /// Builds a Google Drive file URL using the provided fileId, resourceKey, and tid (Shared Drive ID).
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="resourceKey"></param>
        /// <param name="tid"></param>
        /// <returns></returns>
        private static string BuildFileUrl(string fileId, string resourceKey, string tid)
        {
            // Construct the base URL for the file
            var url = $"https://drive.google.com/file/d/{fileId}/view";

            // If a resourceKey is provided, add it to the URL
            if (!string.IsNullOrEmpty(resourceKey))
            {
                url += $"?resourcekey={Uri.EscapeDataString(resourceKey)}";
            }

            // If a tid (Shared Drive ID) is provided, append it to the URL as a query parameter
            if (!string.IsNullOrEmpty(tid))
            {
                // If there's already a query parameter (resourcekey), append the tid as another parameter
                if (url.Contains('?'))
                {
                    url += $"&tid={Uri.EscapeDataString(tid)}";
                }
                else
                {
                    // Otherwise, add tid as the first query parameter
                    url += $"?tid={Uri.EscapeDataString(tid)}";
                }
            }

            return url;
        }

        /// <summary>
        /// Return jump type, or the corresponding type if the entry name contains a keyword.
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        private string GetCorrespondingType(WhitelistEntry whitelist)
        {
            foreach (var keyword in _keywords)
            {
                if (whitelist.Name.Contains(keyword.Key, StringComparison.OrdinalIgnoreCase))
                    return keyword.Value;
            }
            return FolderType.Jump;
        }

        /// <summary>
        /// Get the corresponding type if the file name contains a keyword, or the default type (whitelist type).
        /// </summary>
        /// <param name="file"></param>
        /// <param name="whitelist"></param>
        /// <returns></returns>
        private string GetCorrespondingType(FileEntry file, WhitelistEntry whitelist)
        {
            foreach (var keyword in _keywords)
            {
                if (file.Name.Contains(keyword.Key, StringComparison.OrdinalIgnoreCase))
                    return keyword.Value;
            }
            // If no keywords match, return the whitelist type
            return whitelist.Type;
        }
        #endregion

    }

}
