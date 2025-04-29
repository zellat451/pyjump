using System.ComponentModel;
using System.Data;
using Microsoft.EntityFrameworkCore;
using pyjump.Entities;
using pyjump.Infrastructure;
using pyjump.Services;

namespace pyjump.Forms
{
    public partial class FilesEditorForm : Form
    {
        private readonly AppDbContext _context;
        private BindingList<FileEntry> _entryBinding;
        private bool _entriesUpdated = false;
        private readonly SearchService<FileEntry> _searchService;
        public FilesEditorForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(FilesEditorForm_KeyDown);
            _searchService = new SearchService<FileEntry>(dataGridViewEntries, lblSearchResults);
        }

        #region data listing / editing
        public void EntityEditorForm_Load(object sender, EventArgs e)
        {
            var entries = _context.Files.AsTracking().OrderBy(x => x.Name).ToList();
            _entryBinding = new BindingList<FileEntry>(entries);
            _entryBinding.AllowNew = true;
            _entryBinding.AllowRemove = true;

            _entryBinding.ListChanged += EntryBinding_ListChanged;

            dataGridViewEntries.DataSource = _entryBinding;

            if (!dataGridViewEntries.Columns.Contains("Delete"))
            {
                var deleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                dataGridViewEntries.Columns.Add(deleteButtonColumn);
            }

            countBox.Text = $"Total entries: {_entryBinding.Count}";
        }

        private void EntryBinding_ListChanged(object sender, ListChangedEventArgs e) => _entriesUpdated = true;

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (_entriesUpdated)
            {
                var addedEntries = _entryBinding.Where(x => !_context.Files.Contains(x)).ToList();
                if (addedEntries.Count > 0)
                {
                    _context.AddRange(addedEntries);
                }
            }
            _context.SaveChanges();
            MessageBox.Show("Changes saved.");
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_entriesUpdated)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to discard them?", "Confirm Discard", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return;
            }

            _context.Dispose(); // discard context changes
            Close();
        }

        private void dataGridViewEntries_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewEntries.Columns[e.ColumnIndex].Name != "Delete" || e.RowIndex < 0)
                return;

            var result = MessageBox.Show("Are you sure you want to delete this entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;

            var entry = (FileEntry)dataGridViewEntries.Rows[e.RowIndex].DataBoundItem;

            var entryState = _context.Entry(entry).State;

            if (entryState == EntityState.Added)
            {
                // If it's newly added and not yet saved, just detach it
                _context.Entry(entry).State = EntityState.Detached;
            }
            else
            {
                // If it's already in the database, mark it for deletion
                _context.Files.Remove(entry);
            }

            _entryBinding.Remove(entry); // Update UI
            _entriesUpdated = true;
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
            _searchService.PerformSearch(txtSearch.Text, _entryBinding);
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
