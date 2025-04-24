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

        public FilesEditorForm()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

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

        private void FilesBinding_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                var newEntry = _filesBinding[e.NewIndex];
                _context.Files.Add(newEntry);
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
        }
    }
}
