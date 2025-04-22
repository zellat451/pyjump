using System.Security.Policy;
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

        private void btnFullScan_Click(object sender, EventArgs e)
        {
            InitializeEverything();
            Console.Write("Full Scan started...");
            Methods.FullScan();
            ClearEverything();
        }

        private void btnFullScanComplete_Click(object sender, EventArgs e)
        {
            InitializeEverything();
            Console.Write("Full Scan complete started...");
            ClearEverything();
        }

        private void btnRemoveBrokenJumps_Click(object sender, EventArgs e)
        {
            InitializeEverything();
            Console.Write("Remove Broken Jumps started...");
            ClearEverything();
        }

        private void btnRemoveBrokenWhitelist_Click(object sender, EventArgs e)
        {
            InitializeEverything();
            Console.Write("Remove Broken Whitelist started...");
            ClearEverything();
        }

        private void btnEditWhitelist_Click(object sender, EventArgs e)
        {
            using (var form = new WhitelistEditorForm())
            {
                form.WhitelistEditorForm_Load(sender, e);
                form.ShowDialog();
            }
        }

        private async void btnLoadWhitelist_Click(object sender, EventArgs e)
        {
            InitializeEverything();
            try
            {
                await Methods.LoadWhitelist(_logForm);
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

        private void InitializeEverything()
        {
            this.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            _logForm = new LogForm();
            _logForm.Show();

            GoogleServiceManager.Initialize();
            Statics.Initialize();
        }

        private void ClearEverything()
        {
            GoogleServiceManager.Clear();
            Statics.Clear();

            _logForm.Hide();
            _logForm = null;

            this.Enabled = true;
            Cursor.Current = Cursors.Default;
        }
    }
}
