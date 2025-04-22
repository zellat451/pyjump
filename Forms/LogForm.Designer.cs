namespace pyjump.Forms
{
    partial class LogForm
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
            richTextBoxLog = new RichTextBox();
            SuspendLayout();
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(0, 0);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBoxLog.Size = new Size(800, 450);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // LogForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(richTextBoxLog);
            Name = "LogForm";
            Text = "LogForm";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBoxLog;
    }
}