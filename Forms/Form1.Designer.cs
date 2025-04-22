namespace pyjump
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnFullScan = new Button();
            btnFullScanComplete = new Button();
            btnRemoveBrokenJumps = new Button();
            btnRemoveBrokenWhitelist = new Button();
            btnEditWhitelist = new Button();
            btnLoadWhitelist = new Button();
            SuspendLayout();
            // 
            // btnFullScan
            // 
            btnFullScan.Location = new Point(46, 59);
            btnFullScan.Name = "btnFullScan";
            btnFullScan.Size = new Size(233, 91);
            btnFullScan.TabIndex = 0;
            btnFullScan.Text = "fullScan";
            btnFullScan.UseVisualStyleBackColor = true;
            btnFullScan.Click += btnFullScan_Click;
            // 
            // btnFullScanComplete
            // 
            btnFullScanComplete.Location = new Point(449, 96);
            btnFullScanComplete.Name = "btnFullScanComplete";
            btnFullScanComplete.Size = new Size(284, 129);
            btnFullScanComplete.TabIndex = 1;
            btnFullScanComplete.Text = "fullScan complete";
            btnFullScanComplete.UseVisualStyleBackColor = true;
            btnFullScanComplete.Click += btnFullScanComplete_Click;
            // 
            // btnRemoveBrokenJumps
            // 
            btnRemoveBrokenJumps.Location = new Point(46, 274);
            btnRemoveBrokenJumps.Name = "btnRemoveBrokenJumps";
            btnRemoveBrokenJumps.Size = new Size(284, 129);
            btnRemoveBrokenJumps.TabIndex = 2;
            btnRemoveBrokenJumps.Text = "remove Broken Jumps";
            btnRemoveBrokenJumps.UseVisualStyleBackColor = true;
            btnRemoveBrokenJumps.Click += btnRemoveBrokenJumps_Click;
            // 
            // btnRemoveBrokenWhitelist
            // 
            btnRemoveBrokenWhitelist.Location = new Point(449, 274);
            btnRemoveBrokenWhitelist.Name = "btnRemoveBrokenWhitelist";
            btnRemoveBrokenWhitelist.Size = new Size(284, 129);
            btnRemoveBrokenWhitelist.TabIndex = 3;
            btnRemoveBrokenWhitelist.Text = "remove Broken Whitelists";
            btnRemoveBrokenWhitelist.UseVisualStyleBackColor = true;
            btnRemoveBrokenWhitelist.Click += btnRemoveBrokenWhitelist_Click;
            // 
            // btnEditWhitelist
            // 
            btnEditWhitelist.Location = new Point(333, 12);
            btnEditWhitelist.Name = "btnEditWhitelist";
            btnEditWhitelist.Size = new Size(107, 45);
            btnEditWhitelist.TabIndex = 4;
            btnEditWhitelist.Text = "Edit Whitelist";
            btnEditWhitelist.UseVisualStyleBackColor = true;
            btnEditWhitelist.Click += btnEditWhitelist_Click;
            // 
            // btnLoadWhitelist
            // 
            btnLoadWhitelist.Location = new Point(46, 165);
            btnLoadWhitelist.Name = "btnLoadWhitelist";
            btnLoadWhitelist.Size = new Size(233, 87);
            btnLoadWhitelist.TabIndex = 5;
            btnLoadWhitelist.Text = "Load Whitelist";
            btnLoadWhitelist.UseVisualStyleBackColor = true;
            btnLoadWhitelist.Click += btnLoadWhitelist_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnLoadWhitelist);
            Controls.Add(btnEditWhitelist);
            Controls.Add(btnRemoveBrokenWhitelist);
            Controls.Add(btnRemoveBrokenJumps);
            Controls.Add(btnFullScanComplete);
            Controls.Add(btnFullScan);
            Name = "Form1";
            Text = "pyjump";
            ResumeLayout(false);
        }

        #endregion

        private Button btnFullScan;
        private Button btnFullScanComplete;
        private Button btnRemoveBrokenJumps;
        private Button btnRemoveBrokenWhitelist;
        private Button btnEditWhitelist;
        private Button btnLoadWhitelist;
    }
}
