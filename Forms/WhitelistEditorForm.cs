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

        public WhitelistEditorForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        public void WhitelistEditorForm_Load(object sender, EventArgs e)
        {
            var entries = _context.Whitelist.OrderBy(x => x.Name).ToList();
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
        }

        private void WhitelistBinding_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                var newEntry = _whitelistBinding[e.NewIndex];
                _context.Whitelist.Add(newEntry);
            }
            else if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                // We can’t get the item directly, so we handle this during SaveChanges
                // if needed, or use a custom delete button instead
            }
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            _context.SaveChanges();
            MessageBox.Show("Changes saved.");
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
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
        }


    }
}
