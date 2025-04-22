using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using pyjump.Entities;
using pyjump.Infrastructure;
using pyjump.Services;

namespace pyjump
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var db = new AppDbContext())
            {
                db.Database.Migrate(); // Applies any pending migrations
            }

            try
            {
                Statics.AppsettingsJson = Statics.GetJsonAppsettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading appsettings.json file: " + e.Message);
                return;
            }
            try
            {
                var key = Statics.AppsettingsJson.AsObject().SingleOrDefault(x => x.Key.Equals("maindrives", StringComparison.CurrentCultureIgnoreCase)).Key;
                var drives = Statics.AppsettingsJson[key].Deserialize<List<Drive>>(new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                Statics.MainDrives = new() { Data = drives };
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error deserializing maindrives from appsettings: " + e.Message);
                return;
            }
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}