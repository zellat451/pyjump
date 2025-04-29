using System.ComponentModel;
using System.Data;
using Microsoft.EntityFrameworkCore;
using pyjump.Entities;
using pyjump.Infrastructure;

namespace pyjump.Forms
{
    public partial class FilesEditorForm : Form
    {
        private AppDbContext _context;
        private BindingList<FileEntry> _filesBinding;
        private bool _entriesUpdated = false;
        private List<int> _searchMatchIndexes;
        private int _currentSearchIndex;
        public FilesEditorForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _searchMatchIndexes = new List<int>();
            _currentSearchIndex = -1;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(FilesEditorForm_KeyDown);
        }

        #region data listing / editing
        public void FilesEditorForm_Load(object sender, EventArgs e)
        {
            var entries = _context.Files.AsTracking().OrderBy(x => x.Name).ToList();
            _filesBinding = new BindingList<FileEntry>(entries);
            _filesBinding.AllowNew = true;
            _filesBinding.AllowRemove = true;

            _filesBinding.ListChanged += FilesBinding_ListChanged;

            dataGridViewFiles.DataSource = _filesBinding;

            if (!dataGridViewFiles.Columns.Contains("Delete"))
            {
                var deleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                dataGridViewFiles.Columns.Add(deleteButtonColumn);
            }

            countBox.Text = $"Total files: {_filesBinding.Count}";
        }

        private void FilesBinding_ListChanged(object sender, ListChangedEventArgs e) => _entriesUpdated = true;

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (_entriesUpdated)
            {
                var addedEntries = _filesBinding.Where(x => !_context.Files.Contains(x)).ToList();
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

        private void dataGridViewFiles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewFiles.Columns[e.ColumnIndex].Name != "Delete" || e.RowIndex < 0)
                return;

            var result = MessageBox.Show("Are you sure you want to delete this entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;

            var entry = (FileEntry)dataGridViewFiles.Rows[e.RowIndex].DataBoundItem;

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

            _filesBinding.Remove(entry); // Update UI
            _entriesUpdated = true;
        }
        #endregion

        #region data searching
        private static List<Func<FileEntry, string, bool>> SearchPredicates => [
            (entry, searchStr) => entry.Name?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
            (entry, searchStr) => entry.FolderName?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
            (entry, searchStr) => entry.Owner?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
        ];
        private static bool HasAnyMatch(FileEntry entry, string searchStr) => SearchPredicates.Any(x => x.Invoke(entry, searchStr));
        private void PerformSearch(string searchText)
        {
            _searchMatchIndexes.Clear();
            _currentSearchIndex = -1;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                lblSearchResults.Text = "";
                dataGridViewFiles.ClearSelection();
                foreach (DataGridViewRow row in dataGridViewFiles.Rows)
                    row.DefaultCellStyle.BackColor = dataGridViewFiles.DefaultCellStyle.BackColor;
                return;
            }

            string lowerSearch = searchText.ToLower();

            for (int i = 0; i < _filesBinding.Count; i++)
            {
                if (HasAnyMatch(_filesBinding[i], lowerSearch))
                    _searchMatchIndexes.Add(i);
            }

            if (_searchMatchIndexes.Count > 0)
            {
                _currentSearchIndex = 0;
                HighlightCurrentMatch();
            }
            else
            {
                lblSearchResults.Text = "0 / 0";
                dataGridViewFiles.ClearSelection();
            }
        }
        private void HighlightCurrentMatch()
        {
            dataGridViewFiles.ClearSelection();

            foreach (DataGridViewRow row in dataGridViewFiles.Rows)
                row.DefaultCellStyle.BackColor = dataGridViewFiles.DefaultCellStyle.BackColor;

            if (_currentSearchIndex < 0 || _searchMatchIndexes.Count == 0)
            {
                lblSearchResults.Text = "0 / 0";
                return;
            }

            int rowIndex = _searchMatchIndexes[_currentSearchIndex];
            var rowToHighlight = dataGridViewFiles.Rows[rowIndex];

            rowToHighlight.Selected = true;
            rowToHighlight.DefaultCellStyle.BackColor = Color.LightYellow;
            dataGridViewFiles.FirstDisplayedScrollingRowIndex = rowIndex;

            lblSearchResults.Text = $"{_currentSearchIndex + 1} / {_searchMatchIndexes.Count}";

            dataGridViewFiles.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - 2);

        }


        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_searchMatchIndexes.Count == 0) return;
            _currentSearchIndex = (_currentSearchIndex + 1) % _searchMatchIndexes.Count;
            HighlightCurrentMatch();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_searchMatchIndexes.Count == 0) return;
            _currentSearchIndex = (_currentSearchIndex - 1 + _searchMatchIndexes.Count) % _searchMatchIndexes.Count;
            HighlightCurrentMatch();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            PerformSearch(txtSearch.Text);
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
