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
            btnCancelTask = new Button();
            SuspendLayout();
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxLog.Location = new Point(0, 1);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBoxLog.Size = new Size(800, 413);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // btnCancelTask
            // 
            btnCancelTask.Anchor = AnchorStyles.Bottom;
            btnCancelTask.Location = new Point(351, 420);
            btnCancelTask.Name = "btnCancelTask";
            btnCancelTask.Size = new Size(107, 27);
            btnCancelTask.TabIndex = 1;
            btnCancelTask.Text = "Cancel";
            btnCancelTask.UseVisualStyleBackColor = true;
            btnCancelTask.Click += btnCancelTask_Click;
            // 
            // LogForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancelTask);
            Controls.Add(richTextBoxLog);
            Name = "LogForm";
            Text = "logs";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox richTextBoxLog;
        private Button btnCancelTask;
    }
}