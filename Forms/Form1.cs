using pyjump.Forms;
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

        private async void btnScanFiles_Click(object sender, EventArgs e)
        {
            InitializeEverything();
            try
            {
                await Methods.ScanFiles(_logForm);
                // update UI or show results here
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
            InitializeEverything();
            try
            {
                await Methods.ScanWhitelist(_logForm);
                // update UI or show results here
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

            _logForm.Hide();
            _logForm = null;

            this.Enabled = true;
            Cursor.Current = Cursors.Default;
        }
    }
}
