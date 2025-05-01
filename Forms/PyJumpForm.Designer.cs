using pyjump.Services;

namespace pyjump
{
    partial class PyJumpForm
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
            btnClearAll = new Button();
            btnDeleteBroken = new Button();
            btnLogging = new Button();
            btnThreading = new Button();
            panelThreading = new Panel();
            btnThreadCount = new Button();
            labelThreadCount = new Label();
            textBoxThreadCountLoad = new TextBox();
            btnDataImport = new Button();
            panelData = new Panel();
            btnDataExport = new Button();
            panelThreading.SuspendLayout();
            panelData.SuspendLayout();
            SuspendLayout();
            // 
            // btnScanFile
            // 
            btnScanFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnScanFile.Location = new Point(271, 83);
            btnScanFile.Name = "btnScanFile";
            btnScanFile.Size = new Size(233, 87);
            btnScanFile.TabIndex = 0;
            btnScanFile.Text = "Scan Files";
            btnScanFile.UseVisualStyleBackColor = true;
            btnScanFile.Click += btnScanFiles_Click;
            // 
            // btnEditWhitelist
            // 
            btnEditWhitelist.Location = new Point(271, 12);
            btnEditWhitelist.Name = "btnEditWhitelist";
            btnEditWhitelist.Size = new Size(107, 45);
            btnEditWhitelist.TabIndex = 4;
            btnEditWhitelist.Text = "Edit Whitelist";
            btnEditWhitelist.UseVisualStyleBackColor = true;
            btnEditWhitelist.Click += btnEditWhitelist_Click;
            // 
            // btnScanWhitelist
            // 
            btnScanWhitelist.Location = new Point(12, 83);
            btnScanWhitelist.Name = "btnScanWhitelist";
            btnScanWhitelist.Size = new Size(233, 87);
            btnScanWhitelist.TabIndex = 5;
            btnScanWhitelist.Text = "Scan Whitelist";
            btnScanWhitelist.UseVisualStyleBackColor = true;
            btnScanWhitelist.Click += btnScanWhitelist_Click;
            // 
            // btnEditFiles
            // 
            btnEditFiles.Location = new Point(397, 12);
            btnEditFiles.Name = "btnEditFiles";
            btnEditFiles.Size = new Size(107, 45);
            btnEditFiles.TabIndex = 6;
            btnEditFiles.Text = "Edit Files";
            btnEditFiles.UseVisualStyleBackColor = true;
            btnEditFiles.Click += btnEditFiles_Click;
            // 
            // btnResetWhitelistTimes
            // 
            btnResetWhitelistTimes.Location = new Point(61, 176);
            btnResetWhitelistTimes.Name = "btnResetWhitelistTimes";
            btnResetWhitelistTimes.Size = new Size(139, 30);
            btnResetWhitelistTimes.TabIndex = 7;
            btnResetWhitelistTimes.Text = "Reset Whitelist Times";
            btnResetWhitelistTimes.UseVisualStyleBackColor = true;
            btnResetWhitelistTimes.Click += btnResetWhitelistTimes_Click;
            // 
            // btnBuildSheets
            // 
            btnBuildSheets.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBuildSheets.Location = new Point(542, 83);
            btnBuildSheets.Name = "btnBuildSheets";
            btnBuildSheets.Size = new Size(222, 87);
            btnBuildSheets.TabIndex = 8;
            btnBuildSheets.Text = "Build sheets";
            btnBuildSheets.UseVisualStyleBackColor = true;
            btnBuildSheets.Click += btnBuildSheets_Click;
            // 
            // btnGoToSheet
            // 
            btnGoToSheet.Location = new Point(12, 12);
            btnGoToSheet.Name = "btnGoToSheet";
            btnGoToSheet.Size = new Size(96, 34);
            btnGoToSheet.TabIndex = 9;
            btnGoToSheet.Text = "Go to sheet";
            btnGoToSheet.UseVisualStyleBackColor = true;
            btnGoToSheet.Click += btnGoToSheet_Click;
            // 
            // btnFren
            // 
            btnFren.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFren.Location = new Point(456, 236);
            btnFren.Name = "btnFren";
            btnFren.Size = new Size(308, 182);
            btnFren.TabIndex = 10;
            btnFren.Text = ":)";
            btnFren.UseVisualStyleBackColor = true;
            btnFren.Click += btnFren_Click;
            // 
            // btnForceMatch
            // 
            btnForceMatch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnForceMatch.Location = new Point(330, 176);
            btnForceMatch.Name = "btnForceMatch";
            btnForceMatch.Size = new Size(122, 30);
            btnForceMatch.TabIndex = 11;
            btnForceMatch.Text = "Force match Type";
            btnForceMatch.UseVisualStyleBackColor = true;
            btnForceMatch.Click += btnForceMatch_Click;
            // 
            // btnClearAll
            // 
            btnClearAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClearAll.Location = new Point(26, 385);
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Size = new Size(106, 33);
            btnClearAll.TabIndex = 12;
            btnClearAll.Text = "Clear all data";
            btnClearAll.UseVisualStyleBackColor = true;
            btnClearAll.Click += btnClearAll_Click;
            // 
            // btnDeleteBroken
            // 
            btnDeleteBroken.Location = new Point(180, 224);
            btnDeleteBroken.Name = "btnDeleteBroken";
            btnDeleteBroken.Size = new Size(154, 86);
            btnDeleteBroken.TabIndex = 13;
            btnDeleteBroken.Text = "Delete broken entries";
            btnDeleteBroken.UseVisualStyleBackColor = true;
            btnDeleteBroken.Click += btnDeleteBroken_Click;
            // 
            // btnLogging
            // 
            btnLogging.Anchor = AnchorStyles.Bottom;
            btnLogging.Location = new Point(200, 384);
            btnLogging.Name = "btnLogging";
            btnLogging.Size = new Size(86, 54);
            btnLogging.TabIndex = 14;
            btnLogging.UseVisualStyleBackColor = true;
            btnLogging.Click += btnLogging_Click;
            // 
            // btnThreading
            // 
            btnThreading.Anchor = AnchorStyles.Bottom;
            btnThreading.Location = new Point(24, 65);
            btnThreading.Name = "btnThreading";
            btnThreading.Size = new Size(86, 54);
            btnThreading.TabIndex = 15;
            btnThreading.UseVisualStyleBackColor = true;
            btnThreading.Click += btnThreading_Click;
            // 
            // panelThreading
            // 
            panelThreading.Anchor = AnchorStyles.Bottom;
            panelThreading.Controls.Add(btnThreadCount);
            panelThreading.Controls.Add(labelThreadCount);
            panelThreading.Controls.Add(textBoxThreadCountLoad);
            panelThreading.Controls.Add(btnThreading);
            panelThreading.Location = new Point(292, 316);
            panelThreading.Name = "panelThreading";
            panelThreading.Size = new Size(131, 122);
            panelThreading.TabIndex = 16;
            // 
            // btnThreadCount
            // 
            btnThreadCount.Location = new Point(92, 10);
            btnThreadCount.Name = "btnThreadCount";
            btnThreadCount.Size = new Size(33, 25);
            btnThreadCount.TabIndex = 18;
            btnThreadCount.Text = "✔";
            btnThreadCount.UseVisualStyleBackColor = true;
            btnThreadCount.Click += btnThreadCount_Click;
            // 
            // labelThreadCount
            // 
            labelThreadCount.AutoSize = true;
            labelThreadCount.Location = new Point(10, 38);
            labelThreadCount.Name = "labelThreadCount";
            labelThreadCount.Size = new Size(72, 15);
            labelThreadCount.TabIndex = 17;
            labelThreadCount.Text = "Max threads";
            // 
            // textBoxThreadCountLoad
            // 
            textBoxThreadCountLoad.Location = new Point(10, 12);
            textBoxThreadCountLoad.Name = "textBoxThreadCountLoad";
            textBoxThreadCountLoad.Size = new Size(76, 23);
            textBoxThreadCountLoad.TabIndex = 16;
            // 
            // btnDataImport
            // 
            btnDataImport.Location = new Point(21, 25);
            btnDataImport.Name = "btnDataImport";
            btnDataImport.Size = new Size(85, 34);
            btnDataImport.TabIndex = 17;
            btnDataImport.Text = "Data import";
            btnDataImport.UseVisualStyleBackColor = true;
            btnDataImport.Click += btnDataImport_Click;
            // 
            // panelData
            // 
            panelData.Anchor = AnchorStyles.Left;
            panelData.Controls.Add(btnDataExport);
            panelData.Controls.Add(btnDataImport);
            panelData.Location = new Point(12, 224);
            panelData.Name = "panelData";
            panelData.Size = new Size(126, 127);
            panelData.TabIndex = 18;
            // 
            // btnDataExport
            // 
            btnDataExport.Location = new Point(21, 63);
            btnDataExport.Name = "btnDataExport";
            btnDataExport.Size = new Size(85, 33);
            btnDataExport.TabIndex = 18;
            btnDataExport.Text = "Data export";
            btnDataExport.UseVisualStyleBackColor = true;
            btnDataExport.Click += btnDataExport_Click;
            // 
            // PyJumpForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panelData);
            Controls.Add(panelThreading);
            Controls.Add(btnLogging);
            Controls.Add(btnDeleteBroken);
            Controls.Add(btnClearAll);
            Controls.Add(btnForceMatch);
            Controls.Add(btnFren);
            Controls.Add(btnGoToSheet);
            Controls.Add(btnBuildSheets);
            Controls.Add(btnResetWhitelistTimes);
            Controls.Add(btnEditFiles);
            Controls.Add(btnScanWhitelist);
            Controls.Add(btnEditWhitelist);
            Controls.Add(btnScanFile);
            Name = "PyJumpForm";
            Text = "pyjump";
            panelThreading.ResumeLayout(false);
            panelThreading.PerformLayout();
            panelData.ResumeLayout(false);
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
        private Button btnClearAll;
        private Button btnDeleteBroken;
        private Button btnLogging;
        private Button btnThreading;
        private Panel panelThreading;
        private Label labelThreadCount;
        private TextBox textBoxThreadCountLoad;
        private Button btnThreadCount;
        private Button btnDataImport;
        private Panel panelData;
        private Button btnDataExport;
    }
}
