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
            panelImpExp = new Panel();
            btnDataExport = new Button();
            button1 = new Button();
            panelW = new Panel();
            panelF = new Panel();
            panelEdit = new Panel();
            panelClear = new Panel();
            cbClearF = new CheckBox();
            cbClearW = new CheckBox();
            panel1 = new Panel();
            cbDelF = new CheckBox();
            cbDelW = new CheckBox();
            cbFrenDelLinksW = new CheckBox();
            cbFrenScanF = new CheckBox();
            cbFrenScanW = new CheckBox();
            cbFrenBuildSheets = new CheckBox();
            cbFrenDelLinksF = new CheckBox();
            panelFren = new Panel();
            cbFrenOpenSheet = new CheckBox();
            panelThreading.SuspendLayout();
            panelImpExp.SuspendLayout();
            panelW.SuspendLayout();
            panelF.SuspendLayout();
            panelEdit.SuspendLayout();
            panelClear.SuspendLayout();
            panel1.SuspendLayout();
            panelFren.SuspendLayout();
            SuspendLayout();
            // 
            // btnScanFile
            // 
            btnScanFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnScanFile.Location = new Point(7, 3);
            btnScanFile.Name = "btnScanFile";
            btnScanFile.Size = new Size(233, 87);
            btnScanFile.TabIndex = 0;
            btnScanFile.Text = "Scan Files";
            btnScanFile.UseVisualStyleBackColor = true;
            btnScanFile.Click += btnScanFiles_Click;
            // 
            // btnEditWhitelist
            // 
            btnEditWhitelist.Location = new Point(3, 3);
            btnEditWhitelist.Name = "btnEditWhitelist";
            btnEditWhitelist.Size = new Size(107, 45);
            btnEditWhitelist.TabIndex = 4;
            btnEditWhitelist.Text = "Edit Whitelist";
            btnEditWhitelist.UseVisualStyleBackColor = true;
            btnEditWhitelist.Click += btnEditWhitelist_Click;
            // 
            // btnScanWhitelist
            // 
            btnScanWhitelist.Location = new Point(3, 3);
            btnScanWhitelist.Name = "btnScanWhitelist";
            btnScanWhitelist.Size = new Size(233, 87);
            btnScanWhitelist.TabIndex = 5;
            btnScanWhitelist.Text = "Scan Whitelist";
            btnScanWhitelist.UseVisualStyleBackColor = true;
            btnScanWhitelist.Click += btnScanWhitelist_Click;
            // 
            // btnEditFiles
            // 
            btnEditFiles.Location = new Point(112, 3);
            btnEditFiles.Name = "btnEditFiles";
            btnEditFiles.Size = new Size(107, 45);
            btnEditFiles.TabIndex = 6;
            btnEditFiles.Text = "Edit Files";
            btnEditFiles.UseVisualStyleBackColor = true;
            btnEditFiles.Click += btnEditFiles_Click;
            // 
            // btnResetWhitelistTimes
            // 
            btnResetWhitelistTimes.Location = new Point(3, 96);
            btnResetWhitelistTimes.Name = "btnResetWhitelistTimes";
            btnResetWhitelistTimes.Size = new Size(106, 39);
            btnResetWhitelistTimes.TabIndex = 7;
            btnResetWhitelistTimes.Text = "Reset Whitelist Times";
            btnResetWhitelistTimes.UseVisualStyleBackColor = true;
            btnResetWhitelistTimes.Click += btnResetWhitelistTimes_Click;
            // 
            // btnBuildSheets
            // 
            btnBuildSheets.Location = new Point(510, 86);
            btnBuildSheets.Name = "btnBuildSheets";
            btnBuildSheets.Size = new Size(210, 87);
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
            btnFren.Location = new Point(3, 21);
            btnFren.Name = "btnFren";
            btnFren.Size = new Size(221, 142);
            btnFren.TabIndex = 10;
            btnFren.Text = ":)";
            btnFren.UseVisualStyleBackColor = true;
            btnFren.Click += btnFren_Click;
            // 
            // btnForceMatch
            // 
            btnForceMatch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnForceMatch.Location = new Point(59, 94);
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
            btnClearAll.Location = new Point(3, 12);
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Size = new Size(81, 76);
            btnClearAll.TabIndex = 12;
            btnClearAll.Text = "Clear all data";
            btnClearAll.UseVisualStyleBackColor = true;
            btnClearAll.Click += btnClearAll_Click;
            // 
            // btnDeleteBroken
            // 
            btnDeleteBroken.Location = new Point(3, 14);
            btnDeleteBroken.Name = "btnDeleteBroken";
            btnDeleteBroken.Size = new Size(78, 73);
            btnDeleteBroken.TabIndex = 13;
            btnDeleteBroken.Text = "Delete broken entries";
            btnDeleteBroken.UseVisualStyleBackColor = true;
            btnDeleteBroken.Click += btnDeleteBroken_Click;
            // 
            // btnLogging
            // 
            btnLogging.Anchor = AnchorStyles.Bottom;
            btnLogging.Location = new Point(210, 384);
            btnLogging.Name = "btnLogging";
            btnLogging.Size = new Size(86, 54);
            btnLogging.TabIndex = 14;
            btnLogging.Text = "logging";
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
            btnThreading.Text = "threading";
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
            panelThreading.Location = new Point(311, 316);
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
            btnDataImport.Location = new Point(3, 42);
            btnDataImport.Name = "btnDataImport";
            btnDataImport.Size = new Size(85, 34);
            btnDataImport.TabIndex = 17;
            btnDataImport.Text = "Data import";
            btnDataImport.UseVisualStyleBackColor = true;
            btnDataImport.Click += btnDataImport_Click;
            // 
            // panelImpExp
            // 
            panelImpExp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panelImpExp.Controls.Add(btnDataExport);
            panelImpExp.Controls.Add(btnDataImport);
            panelImpExp.Location = new Point(203, 246);
            panelImpExp.Name = "panelImpExp";
            panelImpExp.Size = new Size(102, 84);
            panelImpExp.TabIndex = 18;
            // 
            // btnDataExport
            // 
            btnDataExport.Location = new Point(3, 3);
            btnDataExport.Name = "btnDataExport";
            btnDataExport.Size = new Size(85, 33);
            btnDataExport.TabIndex = 18;
            btnDataExport.Text = "Data export";
            btnDataExport.UseVisualStyleBackColor = true;
            btnDataExport.Click += btnDataExport_Click;
            // 
            // button1
            // 
            button1.Location = new Point(129, 96);
            button1.Name = "button1";
            button1.Size = new Size(107, 39);
            button1.TabIndex = 19;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // panelW
            // 
            panelW.Controls.Add(btnScanWhitelist);
            panelW.Controls.Add(btnResetWhitelistTimes);
            panelW.Controls.Add(button1);
            panelW.Location = new Point(12, 83);
            panelW.Name = "panelW";
            panelW.Size = new Size(243, 141);
            panelW.TabIndex = 20;
            // 
            // panelF
            // 
            panelF.Controls.Add(btnScanFile);
            panelF.Controls.Add(btnForceMatch);
            panelF.Location = new Point(261, 83);
            panelF.Name = "panelF";
            panelF.Size = new Size(243, 141);
            panelF.TabIndex = 21;
            // 
            // panelEdit
            // 
            panelEdit.Controls.Add(btnEditWhitelist);
            panelEdit.Controls.Add(btnEditFiles);
            panelEdit.Location = new Point(268, 9);
            panelEdit.Name = "panelEdit";
            panelEdit.Size = new Size(222, 68);
            panelEdit.TabIndex = 22;
            // 
            // panelClear
            // 
            panelClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panelClear.Controls.Add(cbClearF);
            panelClear.Controls.Add(cbClearW);
            panelClear.Controls.Add(btnClearAll);
            panelClear.Location = new Point(12, 338);
            panelClear.Name = "panelClear";
            panelClear.Size = new Size(179, 100);
            panelClear.TabIndex = 25;
            // 
            // cbClearF
            // 
            cbClearF.AutoSize = true;
            cbClearF.Location = new Point(87, 51);
            cbClearF.Name = "cbClearF";
            cbClearF.Size = new Size(49, 19);
            cbClearF.TabIndex = 14;
            cbClearF.Text = "Files";
            cbClearF.UseVisualStyleBackColor = true;
            // 
            // cbClearW
            // 
            cbClearW.AutoSize = true;
            cbClearW.Location = new Point(87, 26);
            cbClearW.Name = "cbClearW";
            cbClearW.Size = new Size(72, 19);
            cbClearW.TabIndex = 13;
            cbClearW.Text = "Whitelist";
            cbClearW.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            panel1.Controls.Add(cbDelF);
            panel1.Controls.Add(cbDelW);
            panel1.Controls.Add(btnDeleteBroken);
            panel1.Location = new Point(12, 232);
            panel1.Name = "panel1";
            panel1.Size = new Size(179, 100);
            panel1.TabIndex = 26;
            // 
            // cbDelF
            // 
            cbDelF.AutoSize = true;
            cbDelF.Location = new Point(87, 56);
            cbDelF.Name = "cbDelF";
            cbDelF.Size = new Size(49, 19);
            cbDelF.TabIndex = 15;
            cbDelF.Text = "Files";
            cbDelF.UseVisualStyleBackColor = true;
            // 
            // cbDelW
            // 
            cbDelW.AutoSize = true;
            cbDelW.Location = new Point(87, 31);
            cbDelW.Name = "cbDelW";
            cbDelW.Size = new Size(72, 19);
            cbDelW.TabIndex = 14;
            cbDelW.Text = "Whitelist";
            cbDelW.UseVisualStyleBackColor = true;
            // 
            // cbFrenDelLinksW
            // 
            cbFrenDelLinksW.AutoSize = true;
            cbFrenDelLinksW.Location = new Point(230, 73);
            cbFrenDelLinksW.Name = "cbFrenDelLinksW";
            cbFrenDelLinksW.Size = new Size(112, 19);
            cbFrenDelLinksW.TabIndex = 27;
            cbFrenDelLinksW.Text = "Broken Whitelist";
            cbFrenDelLinksW.UseVisualStyleBackColor = true;
            // 
            // cbFrenScanF
            // 
            cbFrenScanF.AutoSize = true;
            cbFrenScanF.Checked = true;
            cbFrenScanF.CheckState = CheckState.Checked;
            cbFrenScanF.Location = new Point(230, 48);
            cbFrenScanF.Name = "cbFrenScanF";
            cbFrenScanF.Size = new Size(77, 19);
            cbFrenScanF.TabIndex = 28;
            cbFrenScanF.Text = "Scan Files";
            cbFrenScanF.UseVisualStyleBackColor = true;
            // 
            // cbFrenScanW
            // 
            cbFrenScanW.AutoSize = true;
            cbFrenScanW.Checked = true;
            cbFrenScanW.CheckState = CheckState.Checked;
            cbFrenScanW.Location = new Point(230, 25);
            cbFrenScanW.Name = "cbFrenScanW";
            cbFrenScanW.Size = new Size(100, 19);
            cbFrenScanW.TabIndex = 29;
            cbFrenScanW.Text = "Scan Whitelist";
            cbFrenScanW.UseVisualStyleBackColor = true;
            // 
            // cbFrenBuildSheets
            // 
            cbFrenBuildSheets.AutoSize = true;
            cbFrenBuildSheets.Checked = true;
            cbFrenBuildSheets.CheckState = CheckState.Checked;
            cbFrenBuildSheets.Location = new Point(230, 121);
            cbFrenBuildSheets.Name = "cbFrenBuildSheets";
            cbFrenBuildSheets.Size = new Size(89, 19);
            cbFrenBuildSheets.TabIndex = 30;
            cbFrenBuildSheets.Text = "Build sheets";
            cbFrenBuildSheets.UseVisualStyleBackColor = true;
            // 
            // cbFrenDelLinksF
            // 
            cbFrenDelLinksF.AutoSize = true;
            cbFrenDelLinksF.Location = new Point(230, 98);
            cbFrenDelLinksF.Name = "cbFrenDelLinksF";
            cbFrenDelLinksF.Size = new Size(89, 19);
            cbFrenDelLinksF.TabIndex = 31;
            cbFrenDelLinksF.Text = "Broken Files";
            cbFrenDelLinksF.UseVisualStyleBackColor = true;
            // 
            // panelFren
            // 
            panelFren.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelFren.Controls.Add(cbFrenOpenSheet);
            panelFren.Controls.Add(btnFren);
            panelFren.Controls.Add(cbFrenBuildSheets);
            panelFren.Controls.Add(cbFrenDelLinksF);
            panelFren.Controls.Add(cbFrenScanW);
            panelFren.Controls.Add(cbFrenScanF);
            panelFren.Controls.Add(cbFrenDelLinksW);
            panelFren.Location = new Point(452, 263);
            panelFren.Name = "panelFren";
            panelFren.Size = new Size(344, 175);
            panelFren.TabIndex = 32;
            // 
            // cbFrenOpenSheet
            // 
            cbFrenOpenSheet.AutoSize = true;
            cbFrenOpenSheet.Checked = true;
            cbFrenOpenSheet.CheckState = CheckState.Checked;
            cbFrenOpenSheet.Location = new Point(230, 144);
            cbFrenOpenSheet.Name = "cbFrenOpenSheet";
            cbFrenOpenSheet.Size = new Size(86, 19);
            cbFrenOpenSheet.TabIndex = 32;
            cbFrenOpenSheet.Text = "Open sheet";
            cbFrenOpenSheet.UseVisualStyleBackColor = true;
            // 
            // PyJumpForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panelFren);
            Controls.Add(panel1);
            Controls.Add(panelClear);
            Controls.Add(panelEdit);
            Controls.Add(panelF);
            Controls.Add(panelW);
            Controls.Add(panelImpExp);
            Controls.Add(panelThreading);
            Controls.Add(btnLogging);
            Controls.Add(btnGoToSheet);
            Controls.Add(btnBuildSheets);
            Name = "PyJumpForm";
            Text = "pyjump";
            panelThreading.ResumeLayout(false);
            panelThreading.PerformLayout();
            panelImpExp.ResumeLayout(false);
            panelW.ResumeLayout(false);
            panelF.ResumeLayout(false);
            panelEdit.ResumeLayout(false);
            panelClear.ResumeLayout(false);
            panelClear.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelFren.ResumeLayout(false);
            panelFren.PerformLayout();
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
        private Panel panelImpExp;
        private Button btnDataExport;
        private Button button1;
        private Panel panelW;
        private Panel panelF;
        private Panel panelEdit;
        private Panel panelClear;
        private Panel panel1;
        private CheckBox cbClearF;
        private CheckBox cbClearW;
        private CheckBox cbDelF;
        private CheckBox cbDelW;
        private CheckBox cbFrenDelLinksW;
        private CheckBox cbFrenScanF;
        private CheckBox cbFrenScanW;
        private CheckBox cbFrenBuildSheets;
        private CheckBox cbFrenDelLinksF;
        private Panel panelFren;
        private CheckBox cbFrenOpenSheet;
    }
}
