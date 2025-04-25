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
            btnScanFile = new Button();
            btnEditWhitelist = new Button();
            btnScanWhitelist = new Button();
            btnEditFiles = new Button();
            btnResetWhitelistTimes = new Button();
            btnBuildSheets = new Button();
            btnGoToSheet = new Button();
            btnFren = new Button();
            btnForceMatch = new Button();
            SuspendLayout();
            // 
            // btnScanFile
            // 
            btnScanFile.Location = new Point(30, 251);
            btnScanFile.Name = "btnScanFile";
            btnScanFile.Size = new Size(233, 91);
            btnScanFile.TabIndex = 0;
            btnScanFile.Text = "Scan Files";
            btnScanFile.UseVisualStyleBackColor = true;
            btnScanFile.Click += btnScanFiles_Click;
            // 
            // btnEditWhitelist
            // 
            btnEditWhitelist.Location = new Point(348, 12);
            btnEditWhitelist.Name = "btnEditWhitelist";
            btnEditWhitelist.Size = new Size(107, 45);
            btnEditWhitelist.TabIndex = 4;
            btnEditWhitelist.Text = "Edit Whitelist";
            btnEditWhitelist.UseVisualStyleBackColor = true;
            btnEditWhitelist.Click += btnEditWhitelist_Click;
            // 
            // btnScanWhitelist
            // 
            btnScanWhitelist.Location = new Point(30, 92);
            btnScanWhitelist.Name = "btnScanWhitelist";
            btnScanWhitelist.Size = new Size(233, 87);
            btnScanWhitelist.TabIndex = 5;
            btnScanWhitelist.Text = "Scan Whitelist";
            btnScanWhitelist.UseVisualStyleBackColor = true;
            btnScanWhitelist.Click += btnScanWhitelist_Click;
            // 
            // btnEditFiles
            // 
            btnEditFiles.Location = new Point(518, 12);
            btnEditFiles.Name = "btnEditFiles";
            btnEditFiles.Size = new Size(107, 45);
            btnEditFiles.TabIndex = 6;
            btnEditFiles.Text = "Edit Files";
            btnEditFiles.UseVisualStyleBackColor = true;
            btnEditFiles.Click += btnEditFiles_Click;
            // 
            // btnResetWhitelistTimes
            // 
            btnResetWhitelistTimes.Location = new Point(71, 185);
            btnResetWhitelistTimes.Name = "btnResetWhitelistTimes";
            btnResetWhitelistTimes.Size = new Size(139, 30);
            btnResetWhitelistTimes.TabIndex = 7;
            btnResetWhitelistTimes.Text = "Reset Whitelist Times";
            btnResetWhitelistTimes.UseVisualStyleBackColor = true;
            btnResetWhitelistTimes.Click += btnResetWhitelistTimes_Click;
            // 
            // btnBuildSheets
            // 
            btnBuildSheets.Location = new Point(461, 92);
            btnBuildSheets.Name = "btnBuildSheets";
            btnBuildSheets.Size = new Size(222, 87);
            btnBuildSheets.TabIndex = 8;
            btnBuildSheets.Text = "Build sheets";
            btnBuildSheets.UseVisualStyleBackColor = true;
            btnBuildSheets.Click += btnBuildSheets_Click;
            // 
            // btnGoToSheet
            // 
            btnGoToSheet.Location = new Point(94, 17);
            btnGoToSheet.Name = "btnGoToSheet";
            btnGoToSheet.Size = new Size(96, 34);
            btnGoToSheet.TabIndex = 9;
            btnGoToSheet.Text = "Go to sheet";
            btnGoToSheet.UseVisualStyleBackColor = true;
            btnGoToSheet.Click += btnGoToSheet_Click;
            // 
            // btnFren
            // 
            btnFren.Location = new Point(419, 205);
            btnFren.Name = "btnFren";
            btnFren.Size = new Size(308, 182);
            btnFren.TabIndex = 10;
            btnFren.Text = ":)";
            btnFren.UseVisualStyleBackColor = true;
            btnFren.Click += btnFren_Click;
            // 
            // btnForceMatch
            // 
            btnForceMatch.Location = new Point(674, 17);
            btnForceMatch.Name = "btnForceMatch";
            btnForceMatch.Size = new Size(89, 38);
            btnForceMatch.TabIndex = 11;
            btnForceMatch.Text = "Force match Type";
            btnForceMatch.UseVisualStyleBackColor = true;
            btnForceMatch.Click += btnForceMatch_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnForceMatch);
            Controls.Add(btnFren);
            Controls.Add(btnGoToSheet);
            Controls.Add(btnBuildSheets);
            Controls.Add(btnResetWhitelistTimes);
            Controls.Add(btnEditFiles);
            Controls.Add(btnScanWhitelist);
            Controls.Add(btnEditWhitelist);
            Controls.Add(btnScanFile);
            Name = "Form1";
            Text = "pyjump";
            ResumeLayout(false);
        }

        #endregion

        private Button btnScanFile;
        private Button btnEditWhitelist;
        private Button btnScanWhitelist;
        private Button btnEditFiles;
        private Button btnResetWhitelistTimes;
        private Button btnBuildSheets;
        private Button btnGoToSheet;
        private Button btnFren;
        private Button btnForceMatch;
    }
}
