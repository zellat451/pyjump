using pyjump.Entities;
using pyjump.Interfaces;

namespace pyjump.Services
{
    public class SearchService<T> where T : ISheetDataEntity
    {
        public SearchService(DataGridView dataGridViewFiles, Label lblSearchResults)
        {
            _dataGridViewFiles = dataGridViewFiles;
            _lblSearchResults = lblSearchResults;
            _searchMatchIndexes = [];
            _currentSearchIndex = -1;
        }

        private readonly List<int> _searchMatchIndexes;
        private int _currentSearchIndex;
        private readonly DataGridView _dataGridViewFiles;
        private readonly Label _lblSearchResults;

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

        public void PerformSearch(string searchText, IEnumerable<T> filesBinding)
        {
            _searchMatchIndexes.Clear();
            _currentSearchIndex = -1;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _lblSearchResults.Text = "";
                _dataGridViewFiles.ClearSelection();
                foreach (DataGridViewRow row in _dataGridViewFiles.Rows)
                    row.DefaultCellStyle.BackColor = _dataGridViewFiles.DefaultCellStyle.BackColor;
                return;
            }

            string lowerSearch = searchText.ToLower();

            for (int i = 0; i < filesBinding.Count(); i++)
            {
                if (HasAnyMatch(filesBinding.ElementAt(i), lowerSearch))
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
                _dataGridViewFiles.ClearSelection();
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
            _dataGridViewFiles.ClearSelection();

            foreach (DataGridViewRow row in _dataGridViewFiles.Rows)
                row.DefaultCellStyle.BackColor = _dataGridViewFiles.DefaultCellStyle.BackColor;

            if (_currentSearchIndex < 0 || _searchMatchIndexes.Count == 0)
            {
                _lblSearchResults.Text = "0 / 0";
                return;
            }

            int rowIndex = _searchMatchIndexes[_currentSearchIndex];
            var rowToHighlight = _dataGridViewFiles.Rows[rowIndex];

            rowToHighlight.Selected = true;
            rowToHighlight.DefaultCellStyle.BackColor = Color.LightYellow;
            _dataGridViewFiles.FirstDisplayedScrollingRowIndex = rowIndex;

            _lblSearchResults.Text = $"{_currentSearchIndex + 1} / {_searchMatchIndexes.Count}";

            _dataGridViewFiles.FirstDisplayedScrollingRowIndex = Math.Max(0, rowIndex - 2);

        }
    }
}
