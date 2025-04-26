namespace pyjump.Interfaces
{
    public interface ISheetDataEntity
    {
        static abstract (int colNumber, List<object> cols) GetSheetColumns();
        static abstract int GetSheetColumnsNumber();
        List<string> GetRowData();
    }
}
