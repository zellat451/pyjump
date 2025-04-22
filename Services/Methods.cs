using System.Diagnostics;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using pyjump.Forms;
using pyjump.Infrastructure;

namespace pyjump.Services
{
    public static class Methods
    {
        public static void FullScan()
        {


        }

        public static async Task LoadWhitelist(LogForm logForm)
        {
            var drives = Statics.MainDrives;

            var scanner = new DriveFolderScanner();
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

                // 3. entries existing in both lists > update the entries in the database if the name | url | resource key is different
                foreach (var entry in existingEntries)
                {
                    var newEntry = allWhitelistEntries.FirstOrDefault(x => x.Id == entry.Id);
                    if (newEntry != null)
                    {
                        if (entry.Name != newEntry.Name || entry.Url != newEntry.Url || entry.ResourceKey != newEntry.ResourceKey)
                        {
                            entry.Name = newEntry.Name;
                            entry.Url = newEntry.Url;
                            entry.ResourceKey = newEntry.ResourceKey;
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


        private static Sheet EnsureSheetCreated(string sheetName)
        {
            if (sheetName == Statics.SHEET_J && Statics.SheetHolder_J != null) { return Statics.SheetHolder_J; }
            if (sheetName == Statics.SHEET_S && Statics.SheetHolder_S != null) { return Statics.SheetHolder_S; }

            var activeSpreadsheet = Statics.ActiveSpreadsheet;
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

                var batchResponse = GoogleServiceManager.SheetsService.Spreadsheets.BatchUpdate(batchRequest, Statics.SpreadsheetId).Execute();
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

                GoogleServiceManager.SheetsService.Spreadsheets.BatchUpdate(updateDimBatch, Statics.SpreadsheetId).Execute();

                var row = new List<object>(new string[Statics.SHEET_J_COLS]);
                row[Statics.SHEET_J_NAMECOL - 1] = "Name";
                row[Statics.SHEET_J_LOCATIONCOL - 1] = "Location";
                row[Statics.SHEET_J_DATECOL - 1] = "Last Updated";
                row[Statics.SHEET_J_CREATORCOL - 1] = "Owner";

                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Range = $"{sheetName}!A1",
                    Values = [row]
                };

                var updateRequest = GoogleServiceManager.SheetsService.Spreadsheets.Values.Update(
                    valueRange,
                    Statics.SpreadsheetId,
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
                            EndColumnIndex = Statics.SHEET_J_COLS
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

                GoogleServiceManager.SheetsService.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
                {
                    Requests = [formatRequest]
                }, Statics.SpreadsheetId).Execute();

                Debug.WriteLine($"Created new sheet: {sheetName}");

                // Reload the spreadsheet to get the new sheet reference
                Statics.ActiveSpreadsheet = GoogleServiceManager.SheetsService.Spreadsheets.Get(Statics.SpreadsheetId).Execute();
                existingSheet = Statics.ActiveSpreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            }

            if (sheetName == Statics.SHEET_J) { Statics.SheetHolder_J = existingSheet; }
            if (sheetName == Statics.SHEET_S) { Statics.SheetHolder_S = existingSheet; }

            return existingSheet;
        }

        private static void SetCellValue(this Sheet sheet, int col, int row, string valueToSet)
        {
            string columnLetter = ColumnIndexToLetter(col); // Converts column to letter
            string cellRange = $"{sheet.Properties.Title}!{columnLetter}{row}";

            var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
            {
                Range = cellRange,
                Values = [[valueToSet]]
            };

            // Prepare the update request
            var updateRequest = GoogleServiceManager.SheetsService.Spreadsheets.Values.Update(
                valueRange,
                Statics.SpreadsheetId,
                cellRange
            );

            // Specify how the value should be inputted (RAW means no formatting)
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            // Execute the update request
            updateRequest.Execute();
        }

        private static object GetCellValue(this Sheet sheet, int col, int row)
        {
            string columnLetter = ColumnIndexToLetter(col); // Converts column to letter
            string cellRange = $"{sheet.Properties.Title}!{columnLetter}{row}";

            var request = GoogleServiceManager.SheetsService.Spreadsheets.Values.Get(
                Statics.SpreadsheetId,
                cellRange
            );

            var response = request.Execute();
            return response.Values?.FirstOrDefault()?.FirstOrDefault();
        }

        private static string ColumnIndexToLetter(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = (char)(modulo + 65) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}
