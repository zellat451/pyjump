using Google.Apis.Sheets.v4.Data;
using pyjump.Interfaces;
using pyjump.Services;

namespace pyjump.Entities
{
    public class FileEntry : ISheetDataEntity
    {
        public string Id { get; set; }
        public string ResourceKey { get; set; }
        public string DriveId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public DateTime? LastModified { get; set; }
        public string Owner { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public string FolderUrl { get; set; }
        public string Type { get; set; }
        public bool Accessible { get; set; } = true;

        public static (int colNumber, List<object> cols) GetSheetColumns()
        {
            var colNumber = GetSheetColumnsNumber();

            var cols = new List<object>(new string[colNumber]);
            cols[Statics.Sheet.File.SHEET_NAMECOL] = "Name";
            cols[Statics.Sheet.File.SHEET_LOCATIONCOL] = "Location";
            cols[Statics.Sheet.File.SHEET_CREATORCOL] = "Owner";
            cols[Statics.Sheet.File.SHEET_DATECOL] = "Last Updated";

            return (colNumber, cols);
        }

        public static int GetSheetColumnsNumber()
        {
            return Statics.Sheet.File.SHEET_COLS;
        }

        public List<CellData> GetRowData()
        {
            string locationName = this?.FolderName ?? "Unknown";
            string locationUrl = this?.FolderUrl ?? "";
            string escapedEntryName = Methods.EscapeForFormula(this.Name);
            string escapedLocationName = Methods.EscapeForFormula(locationName);

            var colNumber = GetSheetColumnsNumber();

            var data = new List<CellData>()
            {
                new() {
                        UserEnteredValue = new ExtendedValue
                        {
                            FormulaValue = $"=HYPERLINK(\"{this.Url}\", \"{escapedEntryName}\")"
                        }
                    },
                new() {
                        UserEnteredValue = new ExtendedValue
                        {
                            FormulaValue = $"=HYPERLINK(\"{locationUrl}\", \"{escapedLocationName}\")"
                        }
                    },
                new() {
                        UserEnteredValue = new ExtendedValue
                        {
                            StringValue = this.Owner
                        }
                    },
                new() {
                        UserEnteredValue = new ExtendedValue
                        {
                            StringValue = this.LastModified?.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
            };

            if (colNumber != data.Count)
            {
                throw new Exception($"Column count mismatch: expected {colNumber}, got {data.Count} data");
            }

            return data;
        }
    }

}
