using Google.Apis.Sheets.v4.Data;

namespace pyjump.Interfaces
{
    public interface ISheetDataEntity
    {
        string Id { get; }
        string ResourceKey { get; }
        string DriveId { get; }
        string Name { get; }
        string Url { get; }
        static abstract (int colNumber, List<object> cols) GetSheetColumns();
        static abstract int GetSheetColumnsNumber();
        List<CellData> GetRowData();
    }
}
