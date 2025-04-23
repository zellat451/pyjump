using System.Text.RegularExpressions;
using Google.Apis.Drive.v3.Data;
using pyjump.Entities;
using pyjump.Forms;

namespace pyjump.Services
{
    public class DriveScanner
    {
        private readonly HashSet<string> _visitedFolderIds = new();
        private readonly List<WhitelistEntry> _foundFolderNames = new();

        public async Task<List<WhitelistEntry>> GetAllFolderNamesRecursiveAsync(List<string> folderUrls, LogForm logForm)
        {
            // find whitelist entries from maind drives
            var rootFolderIds = folderUrls.Select(ExtractFolderIdFromUrl).Where(id => id != null).Distinct();

            foreach (var folderId in rootFolderIds)
            {
                await TraverseFolderAsync(folderId, string.Empty, logForm);
            }

            return _foundFolderNames;
        }

        private async Task TraverseFolderAsync(string folderId, string parentName, LogForm logForm)
        {
            if (_visitedFolderIds.Contains(folderId))
                return;

            _visitedFolderIds.Add(folderId);

            Google.Apis.Drive.v3.Data.File folderMetadata;
            try
            {
                folderMetadata = await GoogleServiceManager.DriveService.Files.Get(folderId).ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine($"❌ API error getting metadata for {folderId}: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error getting metadata for {folderId}: {ex.Message}");
                return;
            }

            if (folderMetadata.MimeType != "application/vnd.google-apps.folder")
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
                    : $"https://drive.google.com/drive/folders/{folderMetadata.Id}?resourcekey={Uri.EscapeDataString(folderMetadata.ResourceKey)}"
            };
            _foundFolderNames.Add(whitelistEntry);
            logForm.Log($"✅ Found folder: {whitelistEntry.Name} ({folderMetadata.Id})");

            string query = $"'{folderId}' in parents and trashed = false";
            string pageToken = null;

            do
            {
                FileList result;
                try
                {
                    var request = GoogleServiceManager.DriveService.Files.List();
                    request.Q = query;
                    request.Fields = "nextPageToken, files(id, name, mimeType, shortcutDetails)";
                    request.PageToken = pageToken;

                    result = await request.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to list contents of folder {folderId}: {ex.Message}");
                    return;
                }

                foreach (var file in result.Files)
                {
                    if (file.MimeType == "application/vnd.google-apps.folder")
                    {
                        await TraverseFolderAsync(file.Id, whitelistEntry.Name, logForm);
                    }
                    else if (file.MimeType == "application/vnd.google-apps.shortcut")
                    {
                        var targetMime = file.ShortcutDetails?.TargetMimeType;
                        var targetId = file.ShortcutDetails?.TargetId;

                        if (targetMime == "application/vnd.google-apps.folder" && targetId != null)
                        {
                            await TraverseFolderAsync(targetId, whitelistEntry.Name, logForm);
                        }
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
    }

}
