using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using pyjump.Entities;
using pyjump.Forms;

namespace pyjump.Services
{
    public static class SingletonServices
    {
        public static JsonNode AppsettingsJson { get; private set; }
        public static MainDrives MainDrives { get; private set; }
        public static string SpreadsheetId { get; private set; }
        public static bool AllowLogFile { get; private set; }
        public static LogForm LogForm { get; private set; }
        public static bool AllowThreading { get; private set; }
        public static int MaxThreads { get; private set; }

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
                // open the appsettings.json file as json, and get the main drives
                var drives = System.Text.Json.JsonSerializer.Deserialize<List<Drive>>(GetAppsettingsValue("maindrives"), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                MainDrives = new() { Data = drives };
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error deserializing maindrives from appsettings: " + e.Message);
                throw;
            }

            try
            {
                // open the appsettings.json file as json, and get the value 'allowLogFile'
                var allowLogFile = GetAppsettingsValue("allowLogFile");
                AllowLogFile = bool.Parse(allowLogFile);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading allowLogFile from appsettings: " + e.Message);
                throw;
            }

            try
            {
                // open the appsettings.json file as json, and get the value 'allowThreading'
                var allowThreading = GetAppsettingsValue("allowThreading");
                AllowThreading = bool.Parse(allowThreading);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading allowThreading from appsettings: " + e.Message);
                throw;
            }

            try
            {
                // open the appsettings.json file as json, and get the value 'maxThreads'
                var maxThread = GetAppsettingsValue("maxThreads");
                MaxThreads = int.Parse(maxThread);
                if (MaxThreads < 1)
                {
                    MaxThreads = 1;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading maxThreads from appsettings: " + e.Message);
                throw;
            }
        }

        public static void RegisterForm(LogForm form)
        {
            LogForm = form;
            LogForm.Hide();
        }

        public static void InvertPermissionFileLogging() => AllowLogFile = !AllowLogFile;

        public static void InvertPermissionThreading() => AllowThreading = !AllowThreading;

        public static void SetMaxThreads(int max)
        {
            if (max < 1)
            {
                MaxThreads = 1;
            }
            else
            {
                MaxThreads = max;
            }
        }

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
            if (string.IsNullOrEmpty(trueKey)) return null;
            return AppsettingsJson[trueKey]?.ToString();
        }

        private static string ExtractSpreadsheetId(string input)
        {
            // Check if the input is a full URL, otherwise assume it's just the ID
            var match = Regex.Match(input, @"docs\.google\.com\/spreadsheets\/d\/([a-zA-Z0-9-_]+)");
            return match.Success ? match.Groups[1].Value : input;
        }
    }
}
