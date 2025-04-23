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
            ((System.ComponentModel.ISupportInitialize)dataGridViewFiles).BeginInit();
            SuspendLayout();
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Location = new Point(312, 364);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(186, 74);
            btnSaveChanges.TabIndex = 0;
            btnSaveChanges.Text = "Save changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(689, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(99, 36);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Click += btnCancel_Click;
            // 
            // dataGridViewWhitelist
            // 
            dataGridViewFiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewFiles.Location = new Point(31, 54);
            dataGridViewFiles.Name = "dataGridViewWhitelist";
            dataGridViewFiles.Size = new Size(736, 304);
            dataGridViewFiles.TabIndex = 2;
            dataGridViewFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewFiles.CellContentClick += dataGridViewFiles_CellContentClick;
            // 
            // WhitelistEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.Sizable;
            AutoSize = false;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(800, 450);
            Controls.Add(dataGridViewFiles);
            Controls.Add(btnCancel);
            Controls.Add(btnSaveChanges);
            Name = "FilesEditorForm";
            Text = "FilesEditorForm";
            ((System.ComponentModel.ISupportInitialize)dataGridViewFiles).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnSaveChanges;
        private Button btnCancel;
        private DataGridView dataGridViewFiles;
    }
}