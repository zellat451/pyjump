using pyjump.Interfaces;
using pyjump.Services;

namespace pyjump.Entities
{
    public class FileEntry : ISheetDataEntity
    {
        public string Id { get; set; }
        public string ResourceKey { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public DateTime? LastModified { get; set; }
        public string Owner { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public string FolderUrl { get; set; }
        public string Type { get; set; }

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

        public List<string> GetRowData()
        {
            string locationName = this?.FolderName ?? "Unknown";
            string locationUrl = this?.FolderUrl ?? "";
            string escapedEntryName = Methods.EscapeForFormula(this.Name);
            string escapedLocationName = Methods.EscapeForFormula(locationName);

            var colNumber = GetSheetColumnsNumber();

            var data = new List<string>(new string[colNumber]);
            data[Statics.Sheet.File.SHEET_NAMECOL] = $"=HYPERLINK(\"{this.Url}\", \"{escapedEntryName}\")";
            data[Statics.Sheet.File.SHEET_LOCATIONCOL] = $"=HYPERLINK(\"{locationUrl}\", \"{escapedLocationName}\")";
            data[Statics.Sheet.File.SHEET_CREATORCOL] = this.Owner;
            data[Statics.Sheet.File.SHEET_DATECOL] = $"{this.LastModified:yyyy-MM-dd HH:mm:ss}";
            return data;
        }
    }

}
