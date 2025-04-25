using pyjump.Forms;
using pyjump.Infrastructure;
using pyjump.Services;

namespace pyjump
{
    public partial class Form1 : Form
    {
        private LogForm _logForm;

        public Form1()
        {
            InitializeComponent();
        }

        private LoadingForm InitProgressBar()
        {
            var loadingForm = new LoadingForm();
            loadingForm.Show();
            return loadingForm;
        }

        private async void btnScanFiles_Click(object sender, EventArgs e)
        {
            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                _logForm.Log("Starting file scan...");

                loadingForm = InitProgressBar();

                await Methods.ScanFiles(_logForm, loadingForm);
                _logForm.Log("File scan completed.");
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
                _logForm.Log("Starting whitelist scan...");
                await Methods.ScanWhitelist(_logForm);
                _logForm.Log("Whitelist scan completed.");
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
                _logForm.Log("Resetting all whitelist entries last checked times...");
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
                _logForm.Log("All whitelist entries last checked times have been reset.");
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
                _logForm.Log("Building sheets...");

                loadingForm = InitProgressBar();

                await Methods.BuildSheets(_logForm, loadingForm);

                _logForm.Log("Sheets built successfully.");
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
                form.WhitelistEditorForm_Load(sender, e);
                form.ShowDialog();
            }
        }

        private void btnEditFiles_Click(object sender, EventArgs e)
        {
            using (var form = new FilesEditorForm())
            {
                form.FilesEditorForm_Load(sender, e);
                form.ShowDialog();
            }
        }

        private void btnGoToSheet_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();
                _logForm.Log("Opening Google Sheets...");

                Methods.GoToSheet();

                _logForm.Log("Google Sheets opened successfully.");
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

        private void InitializeEverything()
        {
            this.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            _logForm = new LogForm();
            _logForm.Show();

            ScopedServices.Initialize();
        }

        private void ClearEverything()
        {
            ScopedServices.Clear();

            _logForm.Close();
            this.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private async void btnFren_Click(object sender, EventArgs e)
        {
            // Resetting all scoped services after a single mthod just in case.
            // I think it crashed the database once, I am not finding out if it was a freak accident or not
            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                _logForm.Log("I Fren. Fren says 'hi' :)");

                // 1. scan whitelist
                await Methods.ScanWhitelist(_logForm);
                ClearEverything();
                InitializeEverything();

                // 2. scan files
                loadingForm = InitProgressBar();
                await Methods.ScanFiles(_logForm, loadingForm);
                loadingForm?.Close();
                ClearEverything();
                InitializeEverything();

                // 3. build sheets
                loadingForm = InitProgressBar();
                await Methods.BuildSheets(_logForm, loadingForm);
                loadingForm?.Close();
                ClearEverything();
                InitializeEverything();

                // 4. go to spreadsheet
                Methods.GoToSheet();

                _logForm.Log("Fren done. Fren does good work. Enjoy Fren's work, Fren's fren. :)");
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
            LoadingForm loadingForm = null;
            try
            {
                InitializeEverything();
                _logForm.Log("Starting force match...");

                loadingForm = InitProgressBar();

                await Methods.ForceMatchType(_logForm, loadingForm);

                _logForm.Log("Force match completed.");
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
            try
            {
                InitializeEverything();
                _logForm.Log("Clearing all data...");

                Methods.ClearAllData();

                _logForm.Log("All data cleared successfully.");
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
    }
}
