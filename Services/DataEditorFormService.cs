using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using pyjump.Entities;
using pyjump.Infrastructure;
using pyjump.Interfaces;

namespace pyjump.Services
{
    public class DataEditorFormService<T> where T : class, ISheetDataEntity, new()
    {
        public DataEditorFormService(DataGridView dataGridViewFiles, TextBox countBox, Label lblSearchResults)
        {
            _dataGridViewEntries = dataGridViewFiles;
            _countBox = countBox;
            _lblSearchResults = lblSearchResults;
            _searchMatchIndexes = [];
            _currentSearchIndex = -1;
            _context = new AppDbContext();
            _entriesUpdated = false;
        }

        private readonly List<int> _searchMatchIndexes;
        private int _currentSearchIndex;
        private readonly DataGridView _dataGridViewEntries;
        private readonly TextBox _countBox;
        private readonly Label _lblSearchResults;
        private readonly AppDbContext _context;
        private BindingList<T> _entryBinding;
        private bool _entriesUpdated;

        #region buttons & grid
        public void EntityEditorForm_Load()
        {
            var entries = _context.Set<T>().AsTracking().OrderBy(x => x.Name).ToList();
            _entryBinding = new BindingList<T>(entries);
            _entryBinding.AllowNew = true;
            _entryBinding.AllowRemove = true;

            _entryBinding.ListChanged += EntryBinding_ListChanged;

            _dataGridViewEntries.DataSource = _entryBinding;

            if (!_dataGridViewEntries.Columns.Contains("Delete"))
            {
                var deleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                _dataGridViewEntries.Columns.Add(deleteButtonColumn);
            }

            _countBox.Text = $"Total entries: {_entryBinding.Count}";
        }
        private void EntryBinding_ListChanged(object sender, ListChangedEventArgs e) => _entriesUpdated = true;

        public void EditSaveChanges()
        {
            if (_entriesUpdated)
            {
                var addedEntries = _entryBinding.Where(x => !_context.Set<T>().Contains(x)).ToList();
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
            if (_dataGridViewEntries.Columns[e.ColumnIndex].Name != "Delete" || e.RowIndex < 0)
            {
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;

            var entry = (T)_dataGridViewEntries.Rows[e.RowIndex].DataBoundItem;

            var entryState = _context.Entry(entry).State;

            if (entryState == EntityState.Added)
            {
                // If it's newly added and not yet saved, just detach it
                _context.Entry(entry).State = EntityState.Detached;
            }
            else
            {
                // If it's already in the database, mark it for deletion
                _context.Set<T>().Remove(entry);
            }

            _entryBinding.Remove(entry); // Update UI
            _entriesUpdated = true;
        }
        #endregion

        #region data seaching
        private static List<Func<FileEntry, string, bool>> SearchPredicatesFile => [
          (entry, searchStr) => entry.Name?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
            (entry, searchStr) => entry.FolderName?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
            (entry, searchStr) => entry.Owner?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
        ];
        private static List<Func<WhitelistEntry, string, bool>> SearchPredicatesWhitelist => [
            (entry, searchStr) => entry.Name?.Contains(searchStr, StringComparison.CurrentCultureIgnoreCase) == true,
        ];
        private static bool HasAnyMatch(T entry, string searchStr)
        {
            if (typeof(T) == typeof(FileEntry))
                return SearchPredicatesFile.Any(x => x.Invoke(entry as FileEntry, searchStr));
            else if (typeof(T) == typeof(WhitelistEntry))
                return SearchPredicatesWhitelist.Any(x => x.Invoke(entry as WhitelistEntry, searchStr));
            else
                return false;
        }

        public void PerformSearch(string searchText)
        {
            _searchMatchIndexes.Clear();
            _currentSearchIndex = -1;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _lblSearchResults.Text = "";
                _dataGridViewEntries.ClearSelection();
                foreach (DataGridViewRow row in _dataGridViewEntries.Rows)
                    row.DefaultCellStyle.BackColor = _dataGridViewEntries.DefaultCellStyle.BackColor;
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
                _dataGridViewEntries.ClearSelection();
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
            _dataGridViewEntries.ClearSelection();

            foreach (DataGridViewRow row in _dataGridViewEntries.Rows)
                row.DefaultCellStyle.BackColor = _dataGridViewEntries.DefaultCellStyle.BackColor;

            if (_currentSearchIndex < 0 || _searchMatchIndexes.Count == 0)
            {
                _lblSearchResults.Text = "0 / 0";
                return;
            }

            int rowIndex = _searchMatchIndexes[_currentSearchIndex];
            var rowToHighlight = _dataGridViewEntries.Rows[rowIndex];

            rowToHighlight.Selected = true;
            rowToHighlight.DefaultCellStyle.BackColor = Color.LightYellow;

            _lblSearchResults.Text = $"{_currentSearchIndex + 1} / {_searchMatchIndexes.Count}";

            _dataGridViewEntries.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - 2);

        }
        #endregion
    }
}
