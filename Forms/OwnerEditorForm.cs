using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using pyjump.Entities;
using pyjump.Infrastructure;

namespace pyjump.Forms
{
    public partial class OwnerEditorForm : Form
    {
        private BindingList<LNKOwner> _entryBinding;
        private bool _entriesUpdated;
        private readonly AppDbContext _context;
        private readonly List<int> _searchMatchIndexes;
        private int _currentSearchIndex;
        private readonly Label _lblSearchResults;

        public OwnerEditorForm()
        {
            InitializeComponent();
            this.FormClosed += OwnerEditorForm_FormClosed;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(FilesEditorForm_KeyDown);

            _context = new AppDbContext();
            _lblSearchResults = lblSearchResults;
            _searchMatchIndexes = [];
            _currentSearchIndex = -1;

            this.EntityEditorForm_Load();

        }

        private void OwnerEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _context?.Dispose();
        }


        #region data listing / editing
        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            this.EditSaveChanges();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var cancel = this.EditCancel();
            if (cancel)
                Close();
        }

        private void dataGridViewEntries_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.ClickCellContent(e);
        }

        private void dataGridViewEntries_CellContentEdit(object sender, DataGridViewCellEventArgs e)
        {
            this.EditCellContent(e);
        }
        #endregion

        #region data searching
        private void btnNext_Click(object sender, EventArgs e)
        {
            this.NextClick();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            this.PrevClick();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            this.PerformSearch(txtSearch.Text);
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

        #region buttons & grid
        public void EntityEditorForm_Load()
        {
            var entries = _context.Set<LNKOwner>().OrderBy(x => x.Name1).ToList();

            _entryBinding = new BindingList<LNKOwner>(entries);
            _entryBinding.AllowNew = true;
            _entryBinding.AllowRemove = true;

            _entryBinding.ListChanged += EntryBinding_ListChanged;

            this.dataGridViewEntries.DataSource = _entryBinding;

            if (!this.dataGridViewEntries.Columns.Contains("Delete"))
            {
                var deleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                this.dataGridViewEntries.Columns.Add(deleteButtonColumn);
            }

            this.countBox.Text = $"Total entries: {_entryBinding.Count}";
        }
        private void EntryBinding_ListChanged(object sender, ListChangedEventArgs e) => _entriesUpdated = true;

        public void EditSaveChanges()
        {
            if (_entriesUpdated)
            {
                // Update existing entries
                // Warning, LNKOwner is considered the same in both orders of Name1 and Name2
                var existingEntries = _context.Set<LNKOwner>().ToList();
                var reversedExistingEntries = existingEntries.Select(x => new LNKOwner { Name1 = x.Name2, Name2 = x.Name1 }).ToList();

                var allExistingEntries = existingEntries.Concat(reversedExistingEntries).ToList();

                var addedEntries = _entryBinding.Where(x => !allExistingEntries.Contains(x)).ToList();
                if (addedEntries.Count > 0)
                {
                    _context.AddRange(addedEntries);
                }
            }
            _context.SaveChanges();

            MessageBox.Show("Changes saved.");
        }

        public bool EditCancel()
        {
            if (_entriesUpdated)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to discard them?", "Confirm Discard", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return false;
            }

            _context.Dispose(); // discard context changes
            return true;
        }

        public void EditCellContent(DataGridViewCellEventArgs e) => _entriesUpdated = true;
        public void ClickCellContent(DataGridViewCellEventArgs e)
        {
            if (this.dataGridViewEntries.Columns[e.ColumnIndex].Name != "Delete" || e.RowIndex < 0)
            {
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;

            var entry = (LNKOwner)this.dataGridViewEntries.Rows[e.RowIndex].DataBoundItem;

            var entryState = _context.Entry(entry).State;

            if (entryState == EntityState.Added)
            {
                // If it's newly added and not yet saved, just detach it
                _context.Entry(entry).State = EntityState.Detached;
            }
            else
            {
                // If it's already in the database, mark it for deletion
                _context.Set<LNKOwner>().Remove(entry);
            }

            _entryBinding.Remove(entry); // Update UI
            _entriesUpdated = true;
        }
        #endregion

        #region data seaching
        private static List<Func<LNKOwner, string, bool>> SearchPredicatesLNKOwner =>
        [
            (entry, searchStr) => entry.Name1?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
            (entry, searchStr) => entry.Name2?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
        ];

        private static bool HasAnyMatch(LNKOwner entry, string searchStr)
        {
            return SearchPredicatesLNKOwner.Any(x => x.Invoke(entry, searchStr));

        }

        public void PerformSearch(string searchText)
        {
            _searchMatchIndexes.Clear();
            _currentSearchIndex = -1;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _lblSearchResults.Text = "";
                dataGridViewEntries.ClearSelection();
                foreach (DataGridViewRow row in dataGridViewEntries.Rows)
                    row.DefaultCellStyle.BackColor = dataGridViewEntries.DefaultCellStyle.BackColor;
                return;
            }

            for (int i = 0; i < _entryBinding.Count; i++)
            {
                if (HasAnyMatch(_entryBinding[i], searchText))
                    _searchMatchIndexes.Add(i);
            }

            if (_searchMatchIndexes.Count > 0)
            {
                _currentSearchIndex = 0;
                HighlightCurrentMatch();
            }
            else
            {
                _lblSearchResults.Text = "0 / 0";
                dataGridViewEntries.ClearSelection();
            }
        }
        public void NextClick()
        {
            if (_searchMatchIndexes.Count == 0) return;
            _currentSearchIndex = (_currentSearchIndex + 1) % _searchMatchIndexes.Count;
            HighlightCurrentMatch();
        }

        public void PrevClick()
        {
            if (_searchMatchIndexes.Count == 0) return;
            _currentSearchIndex = (_currentSearchIndex - 1 + _searchMatchIndexes.Count) % _searchMatchIndexes.Count;
            HighlightCurrentMatch();
        }

        private void HighlightCurrentMatch()
        {
            dataGridViewEntries.ClearSelection();

            foreach (DataGridViewRow row in dataGridViewEntries.Rows)
                row.DefaultCellStyle.BackColor = dataGridViewEntries.DefaultCellStyle.BackColor;

            if (_currentSearchIndex < 0 || _searchMatchIndexes.Count == 0)
            {
                _lblSearchResults.Text = "0 / 0";
                return;
            }

            int rowIndex = _searchMatchIndexes[_currentSearchIndex];
            var rowToHighlight = dataGridViewEntries.Rows[rowIndex];

            rowToHighlight.Selected = true;
            rowToHighlight.DefaultCellStyle.BackColor = Color.LightYellow;

            _lblSearchResults.Text = $"{_currentSearchIndex + 1} / {_searchMatchIndexes.Count}";

            dataGridViewEntries.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - 2);

        }
        #endregion
    }
}
