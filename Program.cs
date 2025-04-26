using Microsoft.EntityFrameworkCore;
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

            SingletonServices.Initialize(); // Initialize singleton services

            ScopedServices.Initialize(); // Initialize scoped services. Required to get user token at start.
            ScopedServices.Clear();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}