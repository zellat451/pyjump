namespace pyjump.Forms
{
    partial class FilesEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSaveChanges = new Button();
            btnCancel = new Button();
            dataGridViewFiles = new DataGridView();
            countBox = new TextBox();
            txtSearch = new TextBox();
            lblSearchResults = new Label();
            btnPrev = new Button();
            searchPanel = new Panel();
            btnNext = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewFiles).BeginInit();
            searchPanel.SuspendLayout();
            SuspendLayout();
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnSaveChanges.Location = new Point(312, 361);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(152, 74);
            btnSaveChanges.TabIndex = 0;
            btnSaveChanges.Text = "Save changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(655, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(99, 36);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // dataGridViewFiles
            // 
            dataGridViewFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewFiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewFiles.Location = new Point(31, 54);
            dataGridViewFiles.Name = "dataGridViewFiles";
            dataGridViewFiles.Size = new Size(702, 301);
            dataGridViewFiles.TabIndex = 2;
            dataGridViewFiles.CellContentClick += dataGridViewFiles_CellContentClick;
            // 
            // countBox
            // 
            countBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            countBox.Location = new Point(31, 364);
            countBox.Name = "countBox";
            countBox.ReadOnly = true;
            countBox.Size = new Size(100, 23);
            countBox.TabIndex = 3;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(38, 17);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(100, 23);
            txtSearch.TabIndex = 4;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // lblSearchResults
            // 
            lblSearchResults.AutoSize = true;
            lblSearchResults.Location = new Point(71, 43);
            lblSearchResults.Name = "lblSearchResults";
            lblSearchResults.Size = new Size(38, 15);
            lblSearchResults.TabIndex = 5;
            lblSearchResults.Text = "label1";
            // 
            // btnPrev
            // 
            btnPrev.Location = new Point(144, 7);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(26, 21);
            btnPrev.TabIndex = 6;
            btnPrev.Text = "↑";
            btnPrev.UseVisualStyleBackColor = true;
            btnPrev.Click += btnPrev_Click;
            // 
            // searchPanel
            // 
            searchPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            searchPanel.Controls.Add(btnNext);
            searchPanel.Controls.Add(lblSearchResults);
            searchPanel.Controls.Add(btnPrev);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Location = new Point(541, 361);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(182, 78);
            searchPanel.TabIndex = 8;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(144, 34);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(26, 24);
            btnNext.TabIndex = 7;
            btnNext.Text = "↓";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // FilesEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(766, 447);
            Controls.Add(searchPanel);
            Controls.Add(countBox);
            Controls.Add(dataGridViewFiles);
            Controls.Add(btnCancel);
            Controls.Add(btnSaveChanges);
            Name = "FilesEditorForm";
            Text = "editor";
            ((System.ComponentModel.ISupportInitialize)dataGridViewFiles).EndInit();
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSaveChanges;
        private Button btnCancel;
        private DataGridView dataGridViewFiles;
        private TextBox countBox;
        private TextBox txtSearch;
        private Label lblSearchResults;
        private Button btnPrev;
        private Panel searchPanel;
        private Button btnNext;
    }
}