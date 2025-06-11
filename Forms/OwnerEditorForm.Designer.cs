namespace pyjump.Forms
{
    partial class OwnerEditorForm
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
            searchPanel = new Panel();
            btnNext = new Button();
            lblSearchResults = new Label();
            btnPrev = new Button();
            txtSearch = new TextBox();
            countBox = new TextBox();
            dataGridViewEntries = new DataGridView();
            btnCancel = new Button();
            btnSaveChanges = new Button();
            searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewEntries).BeginInit();
            SuspendLayout();
            // 
            // searchPanel
            // 
            searchPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            searchPanel.Controls.Add(btnNext);
            searchPanel.Controls.Add(lblSearchResults);
            searchPanel.Controls.Add(btnPrev);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Location = new Point(575, 362);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(182, 78);
            searchPanel.TabIndex = 13;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(144, 34);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(26, 24);
            btnNext.TabIndex = 7;
            btnNext.Text = "↓";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // lblSearchResults
            // 
            lblSearchResults.AutoSize = true;
            lblSearchResults.Location = new Point(71, 43);
            lblSearchResults.Name = "lblSearchResults";
            lblSearchResults.Size = new Size(30, 15);
            lblSearchResults.TabIndex = 5;
            lblSearchResults.Text = "0 / 0";
            // 
            // btnPrev
            // 
            btnPrev.Location = new Point(144, 7);
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new Size(26, 21);
            btnPrev.TabIndex = 6;
            btnPrev.Text = "↑";
            btnPrev.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(38, 17);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(100, 23);
            txtSearch.TabIndex = 4;
            // 
            // countBox
            // 
            countBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            countBox.Location = new Point(22, 365);
            countBox.Name = "countBox";
            countBox.ReadOnly = true;
            countBox.Size = new Size(100, 23);
            countBox.TabIndex = 12;
            // 
            // dataGridViewEntries
            // 
            dataGridViewEntries.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewEntries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewEntries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewEntries.Location = new Point(22, 53);
            dataGridViewEntries.Name = "dataGridViewEntries";
            dataGridViewEntries.Size = new Size(735, 303);
            dataGridViewEntries.TabIndex = 11;
            dataGridViewEntries.CellContentClick += dataGridViewEntries_CellContentClick;
            dataGridViewEntries.CellValueChanged += dataGridViewEntries_CellContentEdit;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(679, 11);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(99, 36);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnSaveChanges.Location = new Point(303, 362);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(185, 74);
            btnSaveChanges.TabIndex = 9;
            btnSaveChanges.Text = "Save changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // OwnerEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(searchPanel);
            Controls.Add(countBox);
            Controls.Add(dataGridViewEntries);
            Controls.Add(btnCancel);
            Controls.Add(btnSaveChanges);
            Name = "OwnerEditorForm";
            Text = "OwnerEditorForm";
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewEntries).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel searchPanel;
        private Button btnNext;
        private Label lblSearchResults;
        private Button btnPrev;
        private TextBox txtSearch;
        private TextBox countBox;
        private DataGridView dataGridViewEntries;
        private Button btnCancel;
        private Button btnSaveChanges;
    }
}