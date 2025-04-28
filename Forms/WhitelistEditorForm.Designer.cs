namespace pyjump.Forms
{
    partial class WhitelistEditorForm
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
            dataGridViewWhitelist = new DataGridView();
            countBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridViewWhitelist).BeginInit();
            SuspendLayout();
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnSaveChanges.Location = new Point(312, 364);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(186, 74);
            btnSaveChanges.TabIndex = 0;
            btnSaveChanges.Text = "Save changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(689, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(99, 36);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // dataGridViewWhitelist
            // 
            dataGridViewWhitelist.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewWhitelist.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewWhitelist.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewWhitelist.Location = new Point(31, 54);
            dataGridViewWhitelist.Name = "dataGridViewWhitelist";
            dataGridViewWhitelist.Size = new Size(736, 304);
            dataGridViewWhitelist.TabIndex = 2;
            dataGridViewWhitelist.CellContentClick += dataGridViewWhitelist_CellContentClick;
            // 
            // textBox1
            // 
            countBox.Location = new Point(31, 12);
            countBox.Name = "countBox";
            countBox.ReadOnly = true;
            countBox.Size = new Size(100, 23);
            countBox.TabIndex = 3;
            // 
            // WhitelistEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(800, 450);
            Controls.Add(countBox);
            Controls.Add(dataGridViewWhitelist);
            Controls.Add(btnCancel);
            Controls.Add(btnSaveChanges);
            Name = "WhitelistEditorForm";
            Text = "editor";
            ((System.ComponentModel.ISupportInitialize)dataGridViewWhitelist).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSaveChanges;
        private Button btnCancel;
        private DataGridView dataGridViewWhitelist;
        private TextBox countBox;
    }
}