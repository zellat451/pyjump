using Google.Apis.Sheets.v4.Data;
using pyjump.Interfaces;
using pyjump.Services;

namespace pyjump.Entities
{
    public class WhitelistEntry : ISheetDataEntity
    {
        public string Id { get; set; }
        public string ResourceKey { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime? LastChecked { get; set; }
        public string Type { get; set; }

        public static (int colNumber, List<object> cols) GetSheetColumns()
        {
            var colNumber = GetSheetColumnsNumber();

            var cols = new List<object>(new string[colNumber]);
            cols[Statics.Sheet.Whitelist.SHEET_NAMECOL] = Statics.Sheet.Whitelist.COL_NAME;

            return (colNumber, cols);
        }

        public static int GetSheetColumnsNumber()
        {
            return Statics.Sheet.Whitelist.SHEET_COLS;
        }

        public List<CellData> GetRowData()
        {
            string escapedEntryName = Methods.EscapeForFormula(this.Name);
            string escapedUrl = Methods.EscapeForFormula(this.Url);

            var colNumber = GetSheetColumnsNumber();

            var data = new List<CellData>()
            {
                new() {
                        UserEnteredValue = new ExtendedValue
                        {
                            FormulaValue = $"=HYPERLINK(\"{escapedUrl}\", \"{escapedEntryName}\")"
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
