using pyjump.Forms;
using pyjump.Infrastructure;
using pyjump.Services;

namespace pyjump
{
    public partial class PyJumpForm : Form
    {
        public PyJumpForm()
        {
            InitializeComponent();
        }

        #region button methods
        private async void btnScanFiles_Click(object sender, EventArgs e)
        {
            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Starting file scan...");

                loadingForm = InitProgressBar();

                await Methods.ScanFiles(loadingForm);
                SingletonServices.LogForm.Log("File scan completed.");
                MessageBox.Show("File scan completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {

                loadingForm?.Close();
                ClearEverything();
            }
        }

        private async void btnScanWhitelist_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Starting whitelist scan...");
                await Methods.ScanWhitelist();
                SingletonServices.LogForm.Log("Whitelist scan completed.");
                MessageBox.Show("Whitelist scan completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                ClearEverything();
            }
        }
        private void btnResetWhitelistTimes_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Resetting all whitelist entries last checked times...");
                using (var context = new AppDbContext())
                {
                    var entries = context.Whitelist.ToList();
                    foreach (var entry in entries)
                    {
                        entry.LastChecked = null;
                    }
                    context.Whitelist.UpdateRange(entries);
                    context.SaveChanges();
                }
                SingletonServices.LogForm.Log("All whitelist entries last checked times have been reset.");
                MessageBox.Show("All whitelist entries last checked times have been reset.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                ClearEverything();
            }
        }

        private async void btnBuildSheets_Click(object sender, EventArgs e)
        {
            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Building sheets...");

                loadingForm = InitProgressBar();

                await Methods.BuildSheets(loadingForm);

                SingletonServices.LogForm.Log("Sheets built successfully.");
                MessageBox.Show("Sheets built successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                loadingForm?.Close();
                ClearEverything();
            }
        }

        private void btnEditWhitelist_Click(object sender, EventArgs e)
        {
            using (var form = new WhitelistEditorForm())
            {
                form.EntityEditorForm_Load(sender, e);
                form.ShowDialog();
            }
        }

        private void btnEditFiles_Click(object sender, EventArgs e)
        {
            using (var form = new FilesEditorForm())
            {
                form.EntityEditorForm_Load(sender, e);
                form.ShowDialog();
            }
        }

        private void btnGoToSheet_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Opening Google Sheets...");

                Methods.GoToSheet();

                SingletonServices.LogForm.Log("Google Sheets opened successfully.");
                MessageBox.Show("Google Sheets opened successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                ClearEverything();
            }
        }

        private async void btnFren_Click(object sender, EventArgs e)
        {
            // Fren resets all scoped services after every method call just in case, because Fren is a good fren.
            // And also, the two times I tried to use the same scoped service, it broke the database.
            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("I Fren. Fren says 'hi' :)");

                // 1. scan whitelist
                ClearEverything(true);
                InitializeEverything();
                await Methods.ScanWhitelist();

                // 2. scan files
                ClearEverything(true);
                InitializeEverything();
                loadingForm = InitProgressBar();
                await Methods.ScanFiles(loadingForm);

                // 3. build sheets
                ClearEverything(true);
                InitializeEverything();
                await Methods.BuildSheets(loadingForm);

                // 4. go to spreadsheet
                ClearEverything(true);
                InitializeEverything();
                Methods.GoToSheet();

                SingletonServices.LogForm.Log("Fren done. Fren does good work. Enjoy Fren's work, Fren's fren. :)");
                MessageBox.Show("Your friend worked hard. We all thank Fren for their automation efforts. bye-bye Fren!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                loadingForm?.Close();
                ClearEverything();
            }
        }

        private async void btnForceMatch_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to force the file Types to match the whitelist Types? This will overwrite any existing value you may have modified manually.",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return; // If user says No, exit the function early
            }

            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Starting force match...");

                loadingForm = InitProgressBar();

                await Methods.ForceMatchType(loadingForm);

                SingletonServices.LogForm.Log("Force match completed.");
                MessageBox.Show("Force match completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                loadingForm?.Close();
                ClearEverything();
            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all the data? This will completely remove the saved Whitelist & Files.",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return; // If user says No, exit the function early
            }

            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Clearing all data...");

                Methods.ClearAllData();

                SingletonServices.LogForm.Log("All data cleared successfully.");
                MessageBox.Show("All data cleared successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                ClearEverything();
            }
        }

        private async void btnDeleteBroken_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete all broken entries? This will take a while.",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                return; // If user says No, exit the function early
            }

            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Deleting broken entries...");

                loadingForm = InitProgressBar();

                await Methods.DeleteBrokenEntries(loadingForm);

                SingletonServices.LogForm.Log("Broken entries deleted successfully.");
                MessageBox.Show("Broken entries deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                loadingForm?.Close();
                ClearEverything();
            }
        }

        private void btnLogging_Click(object sender, EventArgs e)
        {
            try
            {
                SingletonServices.InvertPermissionFileLogging();
                this.btnLogging.Text = GetCurrentLoggingButtonText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }
        #endregion

        #region private methods
        private static string GetCurrentLoggingButtonText()
        {
            if (SingletonServices.AllowLogFile)
            {
                return "Disable Logging";
            }
            else
            {
                return "Enable Logging";
            }
        }

        private void InitializeEverything()
        {
            this.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            SingletonServices.LogForm.Show();
            ScopedServices.Initialize();
        }

        private void ClearEverything(bool keepLog = false)
        {
            ScopedServices.Clear();
            if (!keepLog) SingletonServices.LogForm.Hide();
            this.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private static LoadingForm InitProgressBar()
        {
            var loadingForm = new LoadingForm();
            loadingForm.Show();
            return loadingForm;
        }
        #endregion
    }
}
