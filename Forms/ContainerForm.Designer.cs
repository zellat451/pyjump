namespace pyjump.Forms
{
    partial class ContainerForm
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
            panelLogs = new Panel();
            panelLoading = new Panel();
            panelForm = new Panel();
            SuspendLayout();
            // 
            // panelLogs
            // 
            panelLogs.Dock = DockStyle.Left;
            panelLogs.Location = new Point(0, 0);
            panelLogs.Name = "panelLogs";
            panelLogs.Size = new Size(377, 576);
            panelLogs.TabIndex = 0;
            // 
            // panelLoading
            // 
            panelLoading.Dock = DockStyle.Bottom;
            panelLoading.Location = new Point(377, 458);
            panelLoading.Name = "panelLoading";
            panelLoading.Size = new Size(789, 118);
            panelLoading.TabIndex = 1;
            // 
            // panelForm
            // 
            panelForm.Dock = DockStyle.Fill;
            panelForm.Location = new Point(377, 0);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(789, 458);
            panelForm.TabIndex = 2;
            // 
            // ContainerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1166, 576);
            Controls.Add(panelForm);
            Controls.Add(panelLoading);
            Controls.Add(panelLogs);
            Name = "ContainerForm";
            Text = "ContainerForm";
            ResumeLayout(false);
        }

        #endregion

        private Panel panelLogs;
        private Panel panelLoading;
        private Panel panelForm;
    }
}