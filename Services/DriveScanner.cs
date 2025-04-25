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

        private readonly ConcurrentBag<(string folderId, string resourceKey, string driveId, string parentName)> _folderQueue;

        public async Task<List<WhitelistEntry>> GetAllFolderNamesRecursiveAsync(List<string> folderUrls, LogForm logForm)
        {
            // find whitelist entries from maind drives
            var rootFolderIds = folderUrls.Select(ExtractFolderInfosFromUrl).Where(inf => !string.IsNullOrEmpty(inf.folderId)).Distinct();

            foreach (var inf in rootFolderIds)
                _folderQueue.Add((inf.folderId, inf.resourceKey, inf.driveId, string.Empty));


            while (!_folderQueue.IsEmpty)
            {
                var success = _folderQueue.TryTake(out var folder);
                if (!success)
                    continue;
                await TraverseFolderAsync(folder.folderId, folder.resourceKey, folder.driveId, folder.parentName, logForm);
            }

            return _foundFolderNames.DistinctBy(x => x.Id).ToList();
        }

        private async Task TraverseFolderAsync(string folderId, string resourceKey, string driveId, string parentName, LogForm logForm)
        {
            if (_visitedFolderIds.ContainsKey(folderId))
                return;

            _visitedFolderIds.TryAdd(folderId, true);

            Google.Apis.Drive.v3.Data.File folderMetadata = null;
            try
            {
                var request = ScopedServices.DriveService.Files.Get(folderId);
                request.SupportsAllDrives = true;
                if (!string.IsNullOrEmpty(resourceKey))
                    request.RequestParameters.Add("resourceKey", new Google.Apis.Discovery.Parameter
                    {
                        Name = "resourceKey",
                        IsRequired = false,
                        ParameterType = "query",
                        DefaultValue = resourceKey,
                        Pattern = null
                    });
                if (!string.IsNullOrEmpty(driveId))
                {
                    request.RequestParameters.Add("driveId", new Google.Apis.Discovery.Parameter
                    {
                        Name = "driveId",
                        IsRequired = false,
                        ParameterType = "query",
                        DefaultValue = driveId,
                        Pattern = null
                    });
                }
                folderMetadata = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                var url = BuildFolderUrl(folderId, resourceKey, driveId);

                if (!string.IsNullOrEmpty(resourceKey))
                    logForm.Log($"⚠️ Folder: {folderId}, ResourceKey: {resourceKey} not found or inaccessible (404). Skipping.");
                else
                    logForm.Log($"⚠️ Folder {folderId} not found or inaccessible (404). Skipping.");
                logForm.Log($"🔗 Folder {folderId} URL: {url}");
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
                Url = BuildFolderUrl(folderMetadata.Id, folderMetadata.ResourceKey, folderMetadata.DriveId),
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
                        _folderQueue.Add((file.Id, file.ResourceKey, file.DriveId, whitelistEntry.Name));
                    }
                    else if (file.MimeType == GoogleMimeTypes.Shortcut &&
                             file.ShortcutDetails?.TargetMimeType == GoogleMimeTypes.Folder &&
                             file.ShortcutDetails?.TargetId != null)
                    {
                        _folderQueue.Add((file.ShortcutDetails.TargetId, file.ShortcutDetails.TargetResourceKey, file.DriveId, whitelistEntry.Name));
                    }
                }

                pageToken = result.NextPageToken;

            } while (pageToken != null);

        }

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

        private string BuildFolderUrl(string folderId, string resourceKey, string tid)
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
                if (url.Contains("?"))
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

        private string BuildFileUrl(string fileId, string resourceKey, string tid)
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
                if (url.Contains("?"))
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
                            if (!string.IsNullOrEmpty(file.DriveId))
                            {
                                request.RequestParameters.Add("driveId", new Google.Apis.Discovery.Parameter
                                {
                                    Name = "driveId",
                                    IsRequired = false,
                                    ParameterType = "query",
                                    DefaultValue = file.DriveId,
                                    Pattern = null
                                });
                            }
                            actualFile = await request.ExecuteAsync();
                        }
                        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                        {
                            var url = BuildFileUrl(targetId, file.ShortcutDetails?.TargetResourceKey, file.DriveId);

                            if (!string.IsNullOrEmpty(file.ShortcutDetails?.TargetResourceKey))
                                logForm.Log($"⚠️ Shortcut target {targetId}, ResourceKey: {file.ShortcutDetails?.TargetResourceKey} not found or inaccessible (404). Skipping.");
                            else
                                logForm.Log($"⚠️ Shortcut target {targetId} not found or inaccessible (404). Skipping.");
                            logForm.Log($"🔗 Shortcut {targetId} URL: {url}");
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
                        Url = BuildFileUrl(actualFile.Id, actualFile.ResourceKey, actualFile.DriveId),
                        Name = actualFile.Name,
                        LastModified = modifiedTime ?? DateTime.MinValue,
                        Owner = owner ?? "Unknown",
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
