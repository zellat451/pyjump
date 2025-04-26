using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using pyjump.Entities;
using pyjump.Infrastructure;

namespace pyjump.Forms
{
    public partial class WhitelistEditorForm : Form
    {
        private AppDbContext _context;
        private BindingList<WhitelistEntry> _whitelistBinding;
        private bool _entriesUpdated = false;

        public WhitelistEditorForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        public void WhitelistEditorForm_Load(object sender, EventArgs e)
        {
            var entries = _context.Whitelist.AsTracking().OrderBy(x => x.Name).ToList();
            _whitelistBinding = new BindingList<WhitelistEntry>(entries);
            _whitelistBinding.AllowNew = true;
            _whitelistBinding.AllowRemove = true;

            _whitelistBinding.ListChanged += WhitelistBinding_ListChanged;

            dataGridViewWhitelist.DataSource = _whitelistBinding;

            if (!dataGridViewWhitelist.Columns.Contains("Delete"))
            {
                var deleteButtonColumn = new DataGridViewButtonColumn
                {
                    Name = "Delete",
                    HeaderText = "",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };

                dataGridViewWhitelist.Columns.Add(deleteButtonColumn);
            }

            countBox.Text = $"Total entries: {_whitelistBinding.Count}";
        }

        private void WhitelistBinding_ListChanged(object sender, ListChangedEventArgs e) => _entriesUpdated = true;

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (_entriesUpdated)
            {
                var addedEntries = _whitelistBinding.Where(x => !_context.Whitelist.Contains(x)).ToList();
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

        private void dataGridViewWhitelist_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewWhitelist.Columns[e.ColumnIndex].Name != "Delete" || e.RowIndex < 0)
                return;

            var result = MessageBox.Show("Are you sure you want to delete this entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;

            var entry = (WhitelistEntry)dataGridViewWhitelist.Rows[e.RowIndex].DataBoundItem;

            var entryState = _context.Entry(entry).State;

            if (entryState == EntityState.Added)
            {
                // If it's newly added and not yet saved, just detach it
                _context.Entry(entry).State = EntityState.Detached;
            }
            else
            {
                // If it's already in the database, mark it for deletion
                _context.Whitelist.Remove(entry);
            }

            _whitelistBinding.Remove(entry); // Update UI

            _entriesUpdated = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
