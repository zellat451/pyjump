using System.Windows.Forms;
using pyjump.Entities;
using pyjump.Services;

namespace pyjump.Forms
{
    public partial class FilesEditorForm : Form
    {
        private readonly DataEditorFormService<FileEntry> _searchService;
        public FilesEditorForm()
        {
            InitializeComponent();
            this.FormClosed += FileEditorForm_FormClosed;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(FilesEditorForm_KeyDown);
            _searchService = new DataEditorFormService<FileEntry>(dataGridViewEntries, countBox, lblSearchResults);
            _searchService.EntityEditorForm_Load();
        }

        private void FileEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _searchService?.Dispose();

            SingletonServices.ContainerForm.LoadChildForm(SingletonServices.MainForm);
        }

        #region data listing / editing
        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            _searchService.EditSaveChanges();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var cancel = _searchService.EditCancel();
            if (cancel)
                Close();
        }

        private void dataGridViewEntries_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _searchService.ClickCellContent(e);
        }

        private void dataGridViewEntries_CellContentEdit(object sender, DataGridViewCellEventArgs e)
        {
            _searchService.EditCellContent(e);
        }
        #endregion

        #region data searching
        private void btnNext_Click(object sender, EventArgs e)
        {
            _searchService.NextClick();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            _searchService.PrevClick();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchService.PerformSearch(txtSearch.Text);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (e.Shift)
                    btnPrev_Click(sender, EventArgs.Empty);
                else
                    btnNext_Click(sender, EventArgs.Empty);
            }

        }
        private void FilesEditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                e.Handled = true;
                txtSearch.Focus();
                txtSearch.SelectAll(); // highlight all text for quick replacement
            }
        }

        #endregion
    }
}
