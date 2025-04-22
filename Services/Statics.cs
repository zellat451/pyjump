using System.Text.Json.Nodes;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using pyjump.Entities;

namespace pyjump.Services
{
    public static class Statics
    {
        public static JsonNode AppsettingsJson { get; set; }
        public static MainDrives MainDrives { get; set; }

        public static JsonNode GetJsonAppsettings()
        {
            var settPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "appsettings.json");
            var stream = new FileStream(settPath, FileMode.Open, FileAccess.Read);
            JsonNode json = JsonNode.Parse(stream);
            return json;
        }

        public static void Initialize()
        {
            // open the appsettings.json file as json, and get the value 'spreadsheet_id'
            var key = AppsettingsJson.AsObject().SingleOrDefault(x => x.Key.Equals("spreadsheet_id", StringComparison.CurrentCultureIgnoreCase)).Key;
            var spreadsheetId = AppsettingsJson[key].ToString();

            var spreadsheet = GoogleServiceManager.SheetsService.Spreadsheets.Get(spreadsheetId).Execute();
            SpreadsheetId = spreadsheetId;
            ActiveSpreadsheet = spreadsheet;
        }

        public static void Clear()
        {
            SpreadsheetId = null;
            ActiveSpreadsheet = null;
            SheetHolder_J = null;
            SheetHolder_S = null;
        }
        public static string SpreadsheetId { get; set; }
        public static Spreadsheet ActiveSpreadsheet { get; set; }

        public static Sheet SheetHolder_J { get; set; }
        public static Sheet SheetHolder_S { get; set; }

        public const string SHEET_J = "Jumps";
        public const string SHEET_S = "Stories";

        public const string CROSSMARK = "x";
        public const string TICKMARK = "-";

        public const int SHEET_J_NAMECOL = 1;
        public const int SHEET_J_LOCATIONCOL = 2;
        public const int SHEET_J_CREATORCOL = 3;
        public const int SHEET_J_DATECOL = 4;
        public const int SHEET_J_COLS = 4;

        public const int NB_THREAD = 15;

        public const string ENTRY_ADDED = "Added";
        public const string ENTRY_UPDATED = "Updated";
    }
}
