using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pyjump.Forms
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            labelProgress.Text = LabelText;
        }

        private string LabelText = "Loading... 0%";

        public void SetMax(int max)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetMax(max));
            }
            else
            {
                progressBar.Maximum = max;
            }
        }

        public void PrepareLoadingBar(string labelText, int max)
        {
            if (InvokeRequired)
            {
                Invoke(() => PrepareLoadingBar(labelText, max));
            }
            else
            {
                LabelText = labelText;
                progressBar.Maximum = max;
                progressBar.Value = 0;
                labelProgress.Text = $"{LabelText}... 0%";
            }
        }

        public void SetLabel(string text)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetLabel(text));
            }
            else
            {
                LabelText = text;
                int percent = (int)((progressBar.Value / (double)progressBar.Maximum) * 100);
                labelProgress.Text = $"{LabelText}... {percent}%";
            }
        }

        public void IncrementProgress(int value = 1)
        {
            if (InvokeRequired)
            {
                Invoke(() => IncrementProgress(value));
            }
            else
            {
                progressBar.Value = Math.Min(progressBar.Maximum, Math.Max(progressBar.Minimum, progressBar.Value + value));
                int percent = (int)((progressBar.Value / (double)progressBar.Maximum) * 100);
                labelProgress.Text = $"{LabelText}... {percent}%";
            }
        }

        public int GetProgress()
        {
            if (InvokeRequired)
            {
                return (int)Invoke(() => GetProgress());
            }
            else
            {
                return progressBar.Value;
            }
        }

        public void SetProgress(int value)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetProgress(value));
            }
            else
            {
                progressBar.Value = Math.Min(progressBar.Maximum, Math.Max(progressBar.Minimum, value));
                int percent = (int)((progressBar.Value / (double)progressBar.Maximum) * 100);
                labelProgress.Text = $"{LabelText}... {percent}%";
            }
        }
    }
}
