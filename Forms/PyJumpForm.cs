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
            this.btnLogging.Text = GetCurrentLoggingButtonText();
            this.btnThreading.Text = GetCurrentThreadingButtonText();
            this.UpdateThreadCountInfos();
            SingletonServices.RegisterForm(new Forms.LogForm());
            this.textBoxThreadCountLoad.KeyDown += new KeyEventHandler(ThreadCountBox_KeyDown);
        }

        #region button methods
        private async void btnScanFiles_Click(object sender, EventArgs e)
        {
            
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Starting file scan...");                

                await Methods.ScanFiles(ScopedServices.CancellationTokenSource.Token);

                SingletonServices.LogForm.Log("File scan completed.");
                MessageBox.Show("File scan completed.");
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

        private async void btnScanWhitelist_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Starting whitelist scan...");

                await Methods.ScanWhitelist(ScopedServices.CancellationTokenSource.Token);

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
            
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Building sheets...");                

                await Methods.BuildSheets(ScopedServices.CancellationTokenSource.Token);

                SingletonServices.LogForm.Log("Sheets built successfully.");
                MessageBox.Show("Sheets built successfully.");
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

        private void btnEditWhitelist_Click(object sender, EventArgs e)
        {
            using (var form = new WhitelistEditorForm())
            {
                form.ShowDialog();
            }
        }

        private void btnEditFiles_Click(object sender, EventArgs e)
        {
            using (var form = new FilesEditorForm())
            {
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
            
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("I Fren. Fren says 'hi' :)");

                if (cbFrenScanW.Checked)
                {
                    // scan whitelist
                    ClearEverything(true);
                    InitializeEverything();
                    await Methods.ScanWhitelist(ScopedServices.CancellationTokenSource.Token); 
                }

                if (cbFrenScanF.Checked)
                {
                    // scan files
                    ClearEverything(true);
                    InitializeEverything();                    
                    
                    await Methods.ScanFiles(ScopedServices.CancellationTokenSource.Token); 
                }

                if (cbFrenDelLinksW.Checked || cbFrenDelLinksF.Checked)
                {
                    var isDeleteWhitelist = cbFrenDelLinksW.Checked;
                    var isDeleteFiles = cbFrenDelLinksF.Checked;

                    // delete links
                    ClearEverything(true);
                    InitializeEverything();                    
                    
                    await Methods.DeleteBrokenEntries(
                        isDeleteFiles,
                        isDeleteWhitelist,
                        ScopedServices.CancellationTokenSource.Token);
                }

                if (cbFrenBuildSheets.Checked)
                {
                    // build sheets
                    ClearEverything(true);
                    InitializeEverything();                    
                    
                    await Methods.BuildSheets(ScopedServices.CancellationTokenSource.Token); 
                }

                if (cbFrenOpenSheet.Checked)
                {
                    // go to spreadsheet
                    ClearEverything(true);
                    InitializeEverything();
                    Methods.GoToSheet(); 
                }

                SingletonServices.LogForm.Log("Fren done. Fren does good work. Enjoy Fren's work, Fren's fren. :)");
                MessageBox.Show("Your friend worked hard. We all thank Fren for their automation efforts. bye-bye Fren!");
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
            
            try
            {
                InitializeEverything();
                SingletonServices.LogForm.Log("Starting force match...");                

                await Methods.ForceMatchType(ScopedServices.CancellationTokenSource.Token);

                SingletonServices.LogForm.Log("Force match completed.");
                MessageBox.Show("Force match completed.");
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

        private async void btnClearAll_Click(object sender, EventArgs e)
        {
            var isClearWhitelist = cbClearW.Checked;
            var isClearFiles = cbClearF.Checked;

            if (!isClearWhitelist && !isClearFiles)
            {
                MessageBox.Show("Please select at least one option to clear.");
                return;
            }

            var msgStart = "Are you sure you want to clear all the data? This will completely remove the saved";
            var msgEnd = "permanently.";
            var msg = isClearWhitelist && isClearFiles
                ? $"{msgStart} Whitelist and Files {msgEnd}"
                : $"{msgStart} {(isClearWhitelist ? "Whitelist" : "Files")} {msgEnd}";

            var result = MessageBox.Show(
                msg,
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

                await Methods.ClearAllData(
                    isClearFiles ? null : [],
                    isClearWhitelist ? null : [],
                    cancellationToken: ScopedServices.CancellationTokenSource.Token
                );

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
            var isDeleteWhitelist = cbDelW.Checked;
            var isDeleteFiles = cbDelF.Checked;

            if (!isDeleteWhitelist && !isDeleteFiles)
            {
                MessageBox.Show("Please select at least one option to delete.");
                return;
            }

            var msgStart = "Are you sure you want to delete all broken";
            var msgEnd = "entries? This will take a while.";
            var msg = isDeleteWhitelist && isDeleteFiles
                ? $"{msgStart} Whitelist and Files {msgEnd}"
                : $"{msgStart} {(isDeleteWhitelist ? "Whitelist" : "File")} {msgEnd}";
            var result = MessageBox.Show(
                msg,
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
                SingletonServices.LogForm.Log("Deleting broken entries...");

                await Methods.DeleteBrokenEntries(
                    isDeleteFiles,
                    isDeleteWhitelist,
                    ScopedServices.CancellationTokenSource.Token);

                SingletonServices.LogForm.Log("Broken entries deleted successfully.");
                MessageBox.Show("Broken entries deleted successfully.");
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
        private void btnThreading_Click(object sender, EventArgs e)
        {
            try
            {
                SingletonServices.InvertPermissionThreading();
                this.btnThreading.Text = GetCurrentThreadingButtonText();
                this.UpdateThreadCountInfos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
        }

        private void btnThreadCount_Click(object sender, EventArgs e)
        {
            try
            {
                var success = int.TryParse(textBoxThreadCountLoad.Text, out var value);
                if (!success)
                {
                    MessageBox.Show("Please enter a valid thread number.");
                    return;
                }

                SingletonServices.SetMaxThreads(value);
                if (value < 1)
                {
                    MessageBox.Show("Thread count lower than 1: resetting to 1. Warning, this will be slow.");
                }
                else if (value > 10)
                {
                    MessageBox.Show($"Thread count set to {value}. Warning, Google apis have a limit on request number per second.");
                }
                else if (value > 100)
                {
                    value = 100;
                    MessageBox.Show($"Thread count higer than 100: set to {value}. Warning, Google apis have a limit on request number per second.");
                }
                else MessageBox.Show($"Thread count set to {value}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong: {ex}");
            }
            finally
            {
                this.UpdateThreadCountInfos();
            }
        }

        private void ThreadCountBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                this.btnThreadCount_Click(sender, EventArgs.Empty);
            }
        }

        private async void btnDataImport_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();

                SingletonServices.LogForm.Log("Importing data...");

                string filePath;
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Select a file to import from";
                    openFileDialog.Filter = "All files (*.*)|*.json";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Get the path of the selected file
                        filePath = openFileDialog.FileName;
                    }
                    else
                    {
                        SingletonServices.LogForm.Log("Operation cancelled by user.");
                        return;
                    }
                }

                await Methods.ImportData(filePath, ScopedServices.CancellationTokenSource.Token);

                SingletonServices.LogForm.Log("Data imported successfully.");
                MessageBox.Show("Data imported successfully.");
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

        private async void btnDataExport_Click(object sender, EventArgs e)
        {
            try
            {
                InitializeEverything();

                SingletonServices.LogForm.Log("Exporting data...");

                string folderPath;
                using (FolderBrowserDialog saveFileDialog = new FolderBrowserDialog())
                {
                    saveFileDialog.Description = "Select a folder to export to";
                    saveFileDialog.ShowNewFolderButton = true;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Get the path of the selected folder
                        folderPath = saveFileDialog.SelectedPath;
                    }
                    else
                    {
                        SingletonServices.LogForm.Log("Operation cancelled by user.");
                        return;
                    }
                }

                await Methods.ExportData(folderPath, ScopedServices.CancellationTokenSource.Token);

                SingletonServices.LogForm.Log("Data exported successfully.");
                MessageBox.Show("Data exported successfully.");
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

        private static string GetCurrentThreadingButtonText()
        {
            if (SingletonServices.AllowThreading)
            {
                return "Disable Threading";
            }
            else
            {
                return "Enable Threading";
            }
        }

        private void UpdateThreadCountInfos()
        {
            if (SingletonServices.AllowThreading)
            {
                this.btnThreadCount.Enabled = true;
                this.textBoxThreadCountLoad.Enabled = true;
                this.textBoxThreadCountLoad.Text = $"{SingletonServices.MaxThreads}";
            }
            else
            {
                this.btnThreadCount.Enabled = false;
                this.textBoxThreadCountLoad.Enabled = false;
                this.textBoxThreadCountLoad.Text = "N/A";
            }
        }

        private void InitializeEverything()
        {
            this.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            SingletonServices.LogForm.Show();
            ScopedServices.Initialize();
            ScopedServices.ResetProgressBar();
        }

        private void ClearEverything(bool keepLog = false)
        {
            ScopedServices.Clear();
            if (!keepLog) SingletonServices.LogForm.Hide();
            this.Enabled = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion
    }
}
