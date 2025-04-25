using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using pyjump.Entities;

namespace pyjump.Services
{
    public static class SingletonServices
    {
        public static JsonNode AppsettingsJson { get; private set; }
        public static MainDrives MainDrives { get; private set; }
        public static string SpreadsheetId { get; private set; }

        public static JsonNode GetJsonAppsettings()
        {
            var settPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "appsettings.json");
            var stream = new FileStream(settPath, FileMode.Open, FileAccess.Read);
            JsonNode json = JsonNode.Parse(stream, documentOptions: new JsonDocumentOptions() { CommentHandling = JsonCommentHandling.Skip });
            return json;
        }

        public static string GetAppsettingsValue(string key)
        {
            var trueKey = AppsettingsJson.AsObject().SingleOrDefault(x => x.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Key;
            return AppsettingsJson[trueKey]?.ToString();
        }

        private static string ExtractSpreadsheetId(string input)
        {
            // Check if the input is a full URL, otherwise assume it's just the ID
            var match = Regex.Match(input, @"docs\.google\.com\/spreadsheets\/d\/([a-zA-Z0-9-_]+)");
            return match.Success ? match.Groups[1].Value : input;
        }

        public static void Initialize()
        {
            try
            {
                AppsettingsJson = GetJsonAppsettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading appsettings.json file: " + e.Message);
                throw;
            }

            try
            {
                // open the appsettings.json file as json, and get the value 'spreadsheet_id'
                var spreadsheetId = GetAppsettingsValue("spreadsheet_id");
                SpreadsheetId = ExtractSpreadsheetId(spreadsheetId);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading spreadsheet_id from appsettings: " + e.Message);
                throw;
            }

            try
            {
                var drives = System.Text.Json.JsonSerializer.Deserialize<List<Drive>>(GetAppsettingsValue("maindrives"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                MainDrives = new() { Data = drives };
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error deserializing maindrives from appsettings: " + e.Message);
                throw;
            }
        }
    }
}
