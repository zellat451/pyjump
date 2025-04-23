using System.Collections.Concurrent;
using System.Drawing.Interop;
using System.Text.Json;
using System.Text.RegularExpressions;
using Google.Apis.Drive.v3.Data;
using pyjump.Entities;
using pyjump.Forms;
using static pyjump.Services.Statics;

namespace pyjump.Services
{
    public class DriveScanner
    {
        public DriveScanner()
        {
            _visitedFolderIds = [];
            _foundFolderNames = [];
            var jsonKeywords = SingletonServices.GetAppsettingsValue("stories_keywords");
            if (string.IsNullOrEmpty(jsonKeywords))
                _storiesKeywords = [];
            else
                _storiesKeywords = JsonSerializer.Deserialize<List<string>>(jsonKeywords, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        private readonly HashSet<string> _visitedFolderIds;
        private readonly ConcurrentBag<WhitelistEntry> _foundFolderNames;
        private readonly List<string> _storiesKeywords;

        private SemaphoreSlim _throttle;

        public async Task<List<WhitelistEntry>> GetAllFolderNamesRecursiveAsync(List<string> folderUrls, LogForm logForm)
        {
            _throttle = new SemaphoreSlim(10);

            // find whitelist entries from maind drives
            var rootFolderIds = folderUrls.Select(ExtractFolderIdFromUrl).Where(id => id != null).Distinct();

            var throttledTasks = new List<Task>();
            foreach (var id in rootFolderIds)
            {
                throttledTasks.Add(SafeTraverse(id, logForm));
            }
            await Task.WhenAll(throttledTasks);

            _throttle.Dispose();

            return _foundFolderNames.ToList();
        }
        private async Task SafeTraverse(string id, LogForm logForm)
        {
            await _throttle.WaitAsync();
            try { await TraverseFolderAsync(id, string.Empty, logForm); }
            finally { _throttle.Release(); }
        }

        private async Task TraverseFolderAsync(string folderId, string parentName, LogForm logForm)
        {
            if (_visitedFolderIds.Contains(folderId))
                return;

            _visitedFolderIds.Add(folderId);

            Google.Apis.Drive.v3.Data.File folderMetadata;
            try
            {
                await _throttle.WaitAsync();
                folderMetadata = await ScopedServices.DriveService.Files.Get(folderId).ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                logForm.Log($"❌ API error getting metadata for {folderId}: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                logForm.Log($"❌ Unexpected error getting metadata for {folderId}: {ex.Message}");
                return;
            }
            finally
            {
                _throttle.Release();
            }

            if (folderMetadata.MimeType != GoogleMimeTypes.Folder)
                return;

            var whitelistEntry = new WhitelistEntry()
            {
                Id = folderMetadata.Id,
                ResourceKey = folderMetadata.ResourceKey ?? string.Empty,
                Name = parentName == string.Empty
                    ? folderMetadata.Name
                    : Path.Combine(parentName, folderMetadata.Name),
                Url = string.IsNullOrEmpty(folderMetadata.ResourceKey)
                    ? $"https://drive.google.com/drive/folders/{folderMetadata.Id}"
                    : $"https://drive.google.com/drive/folders/{folderMetadata.Id}?resourcekey={Uri.EscapeDataString(folderMetadata.ResourceKey)}",
            };
            whitelistEntry.Type = (_storiesKeywords.Any(s => whitelistEntry.Name.Contains(s, StringComparison.OrdinalIgnoreCase)))
                ? FolderType.Story
                : FolderType.Jump;
            _foundFolderNames.Add(whitelistEntry);
            logForm.Log($"✅ Found folder: {whitelistEntry.Name} ({folderMetadata.Id})");

            var subTasks = new List<Task>();
            string query = $"'{folderId}' in parents and trashed = false";
            string pageToken = null;

            do
            {
                FileList result;
                try
                {
                    await _throttle.WaitAsync();
                    var request = ScopedServices.DriveService.Files.List();
                    request.Q = query;
                    request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails)";
                    request.PageToken = pageToken;

                    result = await request.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    logForm.Log($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                    return;
                }
                finally
                {
                    _throttle.Release();
                }

                foreach (var file in result.Files)
                {
                    if (file.MimeType == GoogleMimeTypes.Folder)
                    {
                        subTasks.Add(TraverseFolderAsync(file.Id, whitelistEntry.Name, logForm));
                    }
                    else if (file.MimeType == GoogleMimeTypes.Shortcut &&
                             file.ShortcutDetails?.TargetMimeType == GoogleMimeTypes.Folder &&
                             file.ShortcutDetails?.TargetId != null)
                    {
                        subTasks.Add(TraverseFolderAsync(file.ShortcutDetails.TargetId, whitelistEntry.Name, logForm));
                    }
                }

                pageToken = result.NextPageToken;

            } while (pageToken != null);

            await Task.WhenAll(subTasks);
        }

        private string ExtractFolderIdFromUrl(string url)
        {
            // Matches /folders/<ID> or open?id=<ID>
            var match = Regex.Match(url, @"(?:folders/|id=)([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        public async Task<List<FileEntry>> GetAllFilesInWhitelistAsync(List<WhitelistEntry> whitelist, LogForm logForm)
        {
            var files = new List<FileEntry>();

            foreach (var entry in whitelist)
            {
                if (entry.Type == FolderType.Blacklisted)
                    continue;

                string folderId = entry.Id;

                string query;
                if (entry.LastChecked != null && entry.LastChecked != DateTime.MinValue)
                    query = $"'{folderId}' in parents and trashed = false and (modifiedTime > '{entry.LastChecked:yyyy-MM-ddTHH:mm:ssZ}' or createdTime > '{entry.LastChecked:yyyy-MM-ddTHH:mm:ssZ}')";
                else
                    query = $"'{folderId}' in parents and trashed = false";
                string pageToken = null;

                do
                {
                    FileList result;
                    try
                    {
                        var request = ScopedServices.DriveService.Files.List();
                        request.Q = query;
                        request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails, modifiedTime, createdTime, owners, resourceKey)";
                        request.PageToken = pageToken;

                        result = await request.ExecuteAsync();
                    }
                    catch (Exception ex)
                    {
                        logForm.Log($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                        break;
                    }

                    if (result.Files == null || result.Files.Count == 0)
                    {
                        logForm.Log($"✅ No files found in folder {entry.Name}");
                        break;
                    }

                    foreach (var file in result.Files)
                    {
                        Google.Apis.Drive.v3.Data.File actualFile = file;

                        if (file.MimeType == GoogleMimeTypes.Folder)
                            continue;

                        if (file.MimeType == GoogleMimeTypes.Shortcut)
                        {
                            var targetId = file.ShortcutDetails?.TargetId;
                            if (targetId == null) continue;

                            try
                            {
                                actualFile = await ScopedServices.DriveService.Files.Get(targetId)
                                    .ExecuteAsync();
                            }
                            catch (Exception ex)
                            {
                                logForm.Log($"❌ Failed to resolve shortcut {file.Id}: {ex.Message}");
                                continue;
                            }

                            if (actualFile.MimeType == GoogleMimeTypes.Folder)
                                continue;
                        }

                        var modifiedTime = actualFile.ModifiedTimeDateTimeOffset ?? actualFile.CreatedTimeDateTimeOffset;

                        var fileEntry = new FileEntry
                        {
                            Id = actualFile.Id,
                            ResourceKey = actualFile.ResourceKey ?? string.Empty,
                            Url = string.IsNullOrEmpty(actualFile.ResourceKey)
                                ? $"https://drive.google.com/file/d/{actualFile.Id}/view"
                                : $"https://drive.google.com/file/d/{actualFile.Id}/view?resourcekey={Uri.EscapeDataString(actualFile.ResourceKey)}",
                            Name = actualFile.Name,
                            LastModified = modifiedTime?.UtcDateTime ?? DateTime.MinValue,
                            Owner = actualFile.Owners?.FirstOrDefault()?.DisplayName ?? "Unknown",
                            FolderId = entry.Id,
                        };
                        entry.LastChecked = DateTime.UtcNow;

                        files.Add(fileEntry);
                        logForm.Log($"📄 Found file: {fileEntry.Name} in {entry.Name}");
                    }

                    pageToken = result.NextPageToken;

                } while (pageToken != null);
            }

            return files;
        }
    }

}
