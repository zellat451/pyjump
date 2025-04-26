using Google.Apis.Sheets.v4.Data;

namespace pyjump.Interfaces
{
    public interface ISheetDataEntity
    {
        static abstract (int colNumber, List<object> cols) GetSheetColumns();
        static abstract int GetSheetColumnsNumber();
        List<CellData> GetRowData();
    }
}
