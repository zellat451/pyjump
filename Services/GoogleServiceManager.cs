using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace pyjump.Services
{
    public static class GoogleServiceManager
    {
        public static DriveService DriveService { get; private set; }
        public static SheetsService SheetsService { get; private set; }
        public static void Clear()
        {
            DriveService = null;
            SheetsService = null;
        }

        public static void Initialize()
        {
            string[] scopes = [
                DriveService.Scope.Drive,
                SheetsService.Scope.Spreadsheets
            ];

            UserCredential credential;
            var credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "credentials.json");
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                string tokPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokPath, true)).Result;
            }

            DriveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MyGoogleDriveApp",
            });

            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MyGoogleSheetsApp",
            });

            // Optional: Log or show message
            Debug.WriteLine("Google services initialized.");
        }
    }
}
