using System.Threading.Tasks;
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
            try
            {
                InitializeEverything();
                _logForm.Log("Starting file scan...");

                var loadingForm = InitProgressBar();
 
                await Methods.ScanFiles(_logForm, loadingForm);
                _logForm.Log("File scan completed.");
                MessageBox.Show("File scan completed.");

                loadingForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex.Message}");
            }
            finally
            {
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
                MessageBox.Show($"Something went wrong: {ex.Message}");
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

                MessageBox.Show("All whitelist entries last checked times have been reset.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex.Message}");
            }
            finally
            {
                ClearEverything();
            }
        }

        private async void btnBuildSheets_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();
                _logForm.Log("Building sheets...");

                var loadingForm = InitProgressBar();

                await Methods.BuildSheets(_logForm, loadingForm);

                _logForm.Log("Sheets built successfully.");
                MessageBox.Show("Sheets built successfully.");

                loadingForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex.Message}");
            }
            finally
            {
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
    }
}
