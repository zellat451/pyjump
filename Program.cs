using Microsoft.EntityFrameworkCore;
using pyjump.Forms;
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
                db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            }

            SingletonServices.Initialize(); // Initialize singleton services

            ScopedServices.Initialize(); // Initialize scoped services. Required to get user token at start.
            ScopedServices.ClearServices();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            var containerForm = new ContainerForm();
            SingletonServices.RegisterForm(containerForm);
            Application.Run(containerForm);
        }
    }
}