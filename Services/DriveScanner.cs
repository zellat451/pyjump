using System.Collections.Concurrent;
using System.Net;
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
            {
                var storiesK = JsonSerializer.Deserialize<List<string>>(jsonKeywords, options: new() { PropertyNameCaseInsensitive = true });
                _storiesKeywords = [.. storiesK];
            }

            _folderQueue = [];
        }

        private readonly ConcurrentDictionary<string, bool> _visitedFolderIds;
        private readonly ConcurrentBag<WhitelistEntry> _foundFolderNames;
        private readonly ConcurrentBag<string> _storiesKeywords;

        private readonly ConcurrentBag<(string folderId, string resourceKey, string parentName)> _folderQueue;

        public async Task<List<WhitelistEntry>> GetAllFolderNamesRecursiveAsync(List<string> folderUrls, LogForm logForm)
        {
            // find whitelist entries from maind drives
            var rootFolderIds = folderUrls.Select(ExtractFolderIdFromUrl).Where(id => id != null).Distinct();

            foreach (var id in folderUrls.Select(ExtractFolderIdFromUrl).Where(id => id != null))
                _folderQueue.Add((id, string.Empty, string.Empty));


            while (!_folderQueue.IsEmpty)
            {
                var success = _folderQueue.TryTake(out var folder);
                if (!success)
                    continue;
                await TraverseFolderAsync(folder.folderId, folder.resourceKey, folder.parentName, logForm);
            }

            return _foundFolderNames.DistinctBy(x => x.Id).ToList();
        }

        private async Task TraverseFolderAsync(string folderId, string resourceKey, string parentName, LogForm logForm)
        {
            if (_visitedFolderIds.ContainsKey(folderId))
                return;

            _visitedFolderIds.TryAdd(folderId, true);

            Google.Apis.Drive.v3.Data.File folderMetadata = null;
            try
            {
                var request = ScopedServices.DriveService.Files.Get(folderId);
                request.SupportsAllDrives = true;
                if(!string.IsNullOrEmpty(resourceKey))
                    request.RequestParameters.Add("resourceKey", new Google.Apis.Discovery.Parameter
                    {
                        Name = "resourceKey",
                        IsRequired = false,
                        ParameterType = "query",
                        DefaultValue = resourceKey,
                        Pattern = null
                    });
                folderMetadata = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                logForm.Log($"⚠️ Folder {folderId} not found or inaccessible (404). Skipping.");
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

            if (folderMetadata?.MimeType != GoogleMimeTypes.Folder)
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
            if (_foundFolderNames.Count % 50 == 0)
                logForm.Log($">>> Folder count: {_foundFolderNames.Count}");

            string query = $"'{folderId}' in parents and trashed = false";
            string pageToken = null;

            do
            {
                FileList result;
                try
                {
                    var request = ScopedServices.DriveService.Files.List();
                    request.Q = query;
                    request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails)";
                    request.PageToken = pageToken;
                    request.SupportsAllDrives = true;

                    result = await request.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    logForm.Log($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                    return;
                }

                foreach (var file in result.Files)
                {
                    if (file.MimeType == GoogleMimeTypes.Folder)
                    {
                        _folderQueue.Add((file.Id, file.ResourceKey, whitelistEntry.Name));
                    }
                    else if (file.MimeType == GoogleMimeTypes.Shortcut &&
                             file.ShortcutDetails?.TargetMimeType == GoogleMimeTypes.Folder &&
                             file.ShortcutDetails?.TargetId != null)
                    {
                        _folderQueue.Add((file.ShortcutDetails.TargetId, file.ShortcutDetails.TargetResourceKey, whitelistEntry.Name));
                    }
                }

                pageToken = result.NextPageToken;

            } while (pageToken != null);

        }

        private string ExtractFolderIdFromUrl(string url)
        {
            // Matches /folders/<ID> or open?id=<ID>
            var match = Regex.Match(url, @"(?:folders/|id=)([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        public async Task<List<FileEntry>> GetAllFilesInWhitelistAsync(WhitelistEntry whitelist, LogForm logForm)
        {
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
                FileList result;
                try
                {
                    var request = ScopedServices.DriveService.Files.List();
                    request.Q = query;
                    request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails, modifiedTime, createdTime, owners, resourceKey)";
                    request.PageToken = pageToken;
                    request.SupportsAllDrives = true;

                    result = await request.ExecuteAsync();
                }
                catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    logForm.Log($"⚠️ Folder {folderId} not found or inaccessible (404). Skipping.");
                    break;
                }
                catch (Exception ex)
                {
                    logForm.Log($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                    break;
                }

                if (result.Files == null || result.Files.Count == 0)
                {
                    logForm.Log($"✅ No files found in folder {whitelist.Name}");
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
                            var request = ScopedServices.DriveService.Files.Get(targetId);
                            request.SupportsAllDrives = true;
                            if (!string.IsNullOrEmpty(file.ShortcutDetails?.TargetResourceKey))
                                request.RequestParameters.Add("resourceKey", new Google.Apis.Discovery.Parameter
                                {
                                    Name = "resourceKey",
                                    IsRequired = false,
                                    ParameterType = "query",
                                    DefaultValue = file.ShortcutDetails?.TargetResourceKey,
                                    Pattern = null
                                });
                            actualFile = await request.ExecuteAsync();
                        }
                        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                        {
                            logForm.Log($"⚠️ Shortcut target {targetId} not found or inaccessible (404). Skipping.");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            logForm.Log($"❌ Failed to resolve shortcut {file.Id}: {ex.Message}");
                            continue;
                        }

                        if (actualFile.MimeType == GoogleMimeTypes.Folder)
                            continue;
                    }

                    var modifiedTime = (actualFile.ModifiedTimeDateTimeOffset ?? actualFile.CreatedTimeDateTimeOffset)?.UtcDateTime;
                    if(modifiedTime == null)
                    {
                        if(!string.IsNullOrEmpty(actualFile.ModifiedTimeRaw) && actualFile.ModifiedTimeRaw != "1970-01-01T00:00:00Z")
                            modifiedTime = DateTime.Parse(actualFile.ModifiedTimeRaw).ToUniversalTime();
                        else if (!string.IsNullOrEmpty(actualFile.CreatedTimeRaw) && actualFile.CreatedTimeRaw != "1970-01-01T00:00:00Z")
                            modifiedTime = DateTime.Parse(actualFile.CreatedTimeRaw).ToUniversalTime();
                        else if (file.MimeType == GoogleMimeTypes.Shortcut)
                        {
                            if (!string.IsNullOrEmpty(file.ModifiedTimeRaw) && file.ModifiedTimeRaw != "1970-01-01T00:00:00Z")
                                modifiedTime = DateTime.Parse(file.ModifiedTimeRaw).ToUniversalTime();
                            else if (!string.IsNullOrEmpty(file.CreatedTimeRaw) && file.CreatedTimeRaw != "1970-01-01T00:00:00Z")
                                modifiedTime = DateTime.Parse(file.CreatedTimeRaw).ToUniversalTime();
                        }
                    }

                    var fileEntry = new FileEntry
                    {
                        Id = actualFile.Id,
                        ResourceKey = actualFile.ResourceKey ?? string.Empty,
                        Url = string.IsNullOrEmpty(actualFile.ResourceKey)
                            ? $"https://drive.google.com/file/d/{actualFile.Id}/view"
                            : $"https://drive.google.com/file/d/{actualFile.Id}/view?resourcekey={Uri.EscapeDataString(actualFile.ResourceKey)}",
                        Name = actualFile.Name,
                        LastModified = modifiedTime ?? DateTime.MinValue,
                        Owner = actualFile.Owners?.FirstOrDefault()?.DisplayName ?? "Unknown",
                        FolderId = whitelist.Id,
                    };

                    files.Add(fileEntry);
                    logForm.Log($"📄 Found file: {fileEntry.Name} in {whitelist.Name}");
                }

                pageToken = result.NextPageToken;

            } while (pageToken != null);


            return files.DistinctBy(x => x.Id).ToList();
        }
    }

}
