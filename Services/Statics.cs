namespace pyjump.Services
{
    public static class Statics
    {
        public const string SHEET_J = "Jumps";
        public const string SHEET_S = "Stories";

        public const string CROSSMARK = "x";
        public const string TICKMARK = "-";

        public const int SHEET_J_NAMECOL = 1;
        public const int SHEET_J_LOCATIONCOL = 2;
        public const int SHEET_J_CREATORCOL = 3;
        public const int SHEET_J_DATECOL = 4;
        public const int SHEET_J_COLS = 4;

        public const string ENTRY_ADDED = "Added";
        public const string ENTRY_UPDATED = "Updated";

        public static class FolderType
        {
            public const string Jump = "j";
            public const string Story = "s";
            public const string Other = "o";
            public const string Blacklisted = "-";
        }

        public static class GoogleMimeTypes
        {
            public const string Folder = "application/vnd.google-apps.folder";
            public const string Shortcut = "application/vnd.google-apps.shortcut";
            public const string Spreadsheet = "application/vnd.google-apps.spreadsheet";
        }
    }
}
