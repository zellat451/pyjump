using System.Collections.Concurrent;
using pyjump.Services;

namespace pyjump.Forms
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
            _logTimer = new()
            {
                Interval = 1000 // update once a second
            };
            _logTimer.Tick += (s, e) => FlushLogQueue();
            _logTimer.Start();
        }

        private readonly ConcurrentQueue<string> _logQueue = new();
        private readonly System.Windows.Forms.Timer _logTimer;
        public void Log(string message)
        {
            _logQueue.Enqueue($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
        }

        private void FlushLogQueue()
        {
            if (richTextBoxLog.IsDisposed || richTextBoxLog.Disposing) return;

            if (richTextBoxLog.Lines.Length > 1000)
            {
                richTextBoxLog.Clear();
            }

            var nbMsg = _logQueue.Count;
            while (_logQueue.TryDequeue(out var msg))
            {
                // safety check
                if (richTextBoxLog.IsDisposed || richTextBoxLog.Disposing) return;

                // Append the message to the RichTextBox
                richTextBoxLog.AppendText(msg + Environment.NewLine);

                // allow to copy the logs in a file
                if (SingletonServices.AllowLogFile)
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    File.AppendAllText(Path.Combine(path, $"logform-{DateTime.UtcNow:yyyy-MM-dd}.log"), msg + Environment.NewLine);
                }
            }

            if (nbMsg > 0)
            {
                richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
                richTextBoxLog.ScrollToCaret();
            }
        }

        private void btnCancelTask_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to cancel the current task?", "Cancel Task", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;
            ScopedServices.CancellationTokenSource.Cancel();
            Log("User requested cancellation.");
        }
    }
}
