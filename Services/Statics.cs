namespace pyjump.Services
{
    public static class Statics
    {
        public static class Sheet
        {

            public const string SHEET_J = "Jumps";
            public const string SHEET_S = "Stories";

            public const string SHEET_J_1 = "Jumps (Unfiltered)";
            public const string SHEET_S_1 = "Stories (Unfiltered)";

            public const string SHEET_O = "Others";

            public const string SHEET_W = "Whitelist";

            public const int SHEET_NAMECOL = 1;
            public const int SHEET_LOCATIONCOL = 2;
            public const int SHEET_CREATORCOL = 3;
            public const int SHEET_DATECOL = 4;
            public const int SHEET_COLS = 4;
        }

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
