namespace pyjump.Services
{
    public static class Statics
    {
        public static class Sheet
        {
            public static class File
            {
                public const string SHEET_J = "Jumps";
                public const string SHEET_S = "Stories";
                public const string SHEET_J_1 = "Jumps (Unfiltered)";
                public const string SHEET_S_1 = "Stories (Unfiltered)";
                public const string SHEET_O = "Others";

                public const int SHEET_COLS = 4;

                public const int SHEET_NAMECOL = 0;
                public const string COL_NAME = "Name";
                public const int SHEET_LOCATIONCOL = 1;
                public const string COL_LOCATION = "Location";
                public const int SHEET_CREATORCOL = 2;
                public const string COL_CREATOR = "Owner";
                public const int SHEET_DATECOL = 3;
                public const string COL_LASTUPDATED = "Last Updated";
            }

            public static class Whitelist
            {
                public const string SHEET_W = "Whitelist";

                public const int SHEET_COLS = 1;

                public const int SHEET_NAMECOL = 0;
                public const string COL_NAME = "Whitelist";

            }
        }

        public static class FolderType
        {
            public const string Jump = "j";
            public const string Story = "s";
            public const string Other = "o";
            public const string Blacklisted = "-";

            public static readonly List<string> AllTypes =
            [
                Jump,
                Story,
                Other,
                Blacklisted
            ];
        }

        public static class GoogleMimeTypes
        {
            public const string Folder = "application/vnd.google-apps.folder";
            public const string Shortcut = "application/vnd.google-apps.shortcut";
            public const string Spreadsheet = "application/vnd.google-apps.spreadsheet";
        }
    }
}
