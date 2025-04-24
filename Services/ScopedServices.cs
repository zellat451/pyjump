using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace pyjump.Services
{
    public static class ScopedServices
    {
        public static DriveService DriveService { get; private set; }
        public static SheetsService SheetsService { get; private set; }
        public static Spreadsheet ActiveSpreadsheet { get; set; }
        public static Sheet SheetHolder_J { get; set; }
        public static Sheet SheetHolder_S { get; set; }
        public static Sheet SheetHolder_J_1 { get; set; }
        public static Sheet SheetHolder_S_1 { get; set; }

        public static void Clear()
        {
            DriveService = null;
            SheetsService = null;
            ActiveSpreadsheet = null;
            SheetHolder_J = null;
            SheetHolder_S = null;
            SheetHolder_J_1 = null;
            SheetHolder_S_1 = null;
        }

        public static void Initialize()
        {
            InitializeServices();

            var spreadsheet = SheetsService.Spreadsheets.Get(SingletonServices.SpreadsheetId).Execute();
            ActiveSpreadsheet = spreadsheet;

            SheetHolder_J = EnsureSheetCreated(Statics.Sheet.SHEET_J);
            SheetHolder_S = EnsureSheetCreated(Statics.Sheet.SHEET_S);
            SheetHolder_J_1 = EnsureSheetCreated(Statics.Sheet.SHEET_J_1);
            SheetHolder_S_1 = EnsureSheetCreated(Statics.Sheet.SHEET_S_1);
        }

        private static void InitializeServices()
        {
            string[] scopes = [
                DriveService.Scope.Drive,
                SheetsService.Scope.Spreadsheets
            ];

            UserCredential credential;
            var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "credentials.json");
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                string tokPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokPath, true)).Result;
            }

            DriveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MyGoogleDriveApp",
            });

            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MyGoogleSheetsApp",
            });

            // Optional: Log or show message
            Debug.WriteLine("Google services initialized.");
        }

        private static Sheet EnsureSheetCreated(string sheetName)
        {
            if (sheetName == Statics.Sheet.SHEET_J && SheetHolder_J != null) { return SheetHolder_J; }
            if (sheetName == Statics.Sheet.SHEET_S && SheetHolder_S != null) { return SheetHolder_S; }
            if (sheetName == Statics.Sheet.SHEET_J_1 && SheetHolder_J_1 != null) { return SheetHolder_J_1; }
            if (sheetName == Statics.Sheet.SHEET_S_1 && SheetHolder_S_1 != null) { return SheetHolder_S_1; }

            var activeSpreadsheet = ActiveSpreadsheet;
            var existingSheet = activeSpreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            if (existingSheet == null)
            {
                var addSheetRequest = new AddSheetRequest
                {
                    Properties = new SheetProperties { Title = sheetName }
                };

                var batchRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests =
                    [
                        new Request { AddSheet = addSheetRequest }
                    ]
                };

                var batchResponse = SheetsService.Spreadsheets.BatchUpdate(batchRequest, SingletonServices.SpreadsheetId).Execute();
                var newSheetId = batchResponse.Replies.First().AddSheet.Properties.SheetId;

                var updateDimension = new UpdateDimensionPropertiesRequest
                {
                    Range = new DimensionRange
                    {
                        SheetId = newSheetId,
                        Dimension = "COLUMNS",
                        StartIndex = 0,
                        EndIndex = 1
                    },
                    Properties = new DimensionProperties
                    {
                        PixelSize = 500
                    },
                    Fields = "pixelSize"
                };

                var updateDimBatch = new BatchUpdateSpreadsheetRequest
                {
                    Requests =
                    [
                        new() { UpdateDimensionProperties = updateDimension }
                    ]
                };

                SheetsService.Spreadsheets.BatchUpdate(updateDimBatch, SingletonServices.SpreadsheetId).Execute();

                var row = new List<object>(new string[Statics.Sheet.SHEET_COLS]);
                row[Statics.Sheet.SHEET_NAMECOL - 1] = "Name";
                row[Statics.Sheet.SHEET_LOCATIONCOL - 1] = "Location";
                row[Statics.Sheet.SHEET_DATECOL - 1] = "Last Updated";
                row[Statics.Sheet.SHEET_CREATORCOL - 1] = "Owner";

                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Range = $"{sheetName}!A1",
                    Values = [row]
                };

                var updateRequest = SheetsService.Spreadsheets.Values.Update(
                    valueRange,
                    SingletonServices.SpreadsheetId,
                    $"{sheetName}!A1"
                );

                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                updateRequest.Execute();

                // Set font bold for first row
                var formatRequest = new Request
                {
                    RepeatCell = new RepeatCellRequest
                    {
                        Range = new GridRange
                        {
                            SheetId = newSheetId,
                            StartRowIndex = 0,
                            EndRowIndex = 1,
                            StartColumnIndex = 0,
                            EndColumnIndex = Statics.Sheet.SHEET_COLS
                        },
                        Cell = new CellData
                        {
                            UserEnteredFormat = new CellFormat
                            {
                                TextFormat = new TextFormat { Bold = true }
                            }
                        },
                        Fields = "userEnteredFormat.textFormat.bold"
                    }
                };

                SheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
                {
                    Requests = [formatRequest]
                }, SingletonServices.SpreadsheetId).Execute();

                Debug.WriteLine($"Created new sheet: {sheetName}");

                // Reload the spreadsheet to get the new sheet reference
                ActiveSpreadsheet = SheetsService.Spreadsheets.Get(SingletonServices.SpreadsheetId).Execute();
                existingSheet = ActiveSpreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            }

            if (sheetName == Statics.Sheet.SHEET_J) { SheetHolder_J = existingSheet; }
            if (sheetName == Statics.Sheet.SHEET_S) { SheetHolder_S = existingSheet; }
            if (sheetName == Statics.Sheet.SHEET_J_1) { SheetHolder_J_1 = existingSheet; }
            if (sheetName == Statics.Sheet.SHEET_S_1) { SheetHolder_S_1 = existingSheet; }

            return existingSheet;
        }


    }
}
