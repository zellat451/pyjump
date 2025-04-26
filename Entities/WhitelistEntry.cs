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

        public List<string> GetRowData()
        {
            string escapedEntryName = Methods.EscapeForFormula(this.Name);
            string escapedUrl = Methods.EscapeForFormula(this.Url);

            var colNumber = GetSheetColumnsNumber();

            var data = new List<string>(new string[colNumber]);
            data[Statics.Sheet.Whitelist.SHEET_NAMECOL] = $"=HYPERLINK(\"{escapedUrl}\", \"{escapedEntryName}\")";

            return data;
        }
    }
}
