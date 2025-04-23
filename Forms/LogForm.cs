namespace pyjump.Forms
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();
        }

        public void Log(string message)
        {
            if (!IsHandleCreated)
                return; // Prevent errors if form hasn't fully loaded

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Log(message)));
                return;
            }

            richTextBoxLog.AppendText(message + Environment.NewLine);
            richTextBoxLog.ScrollToCaret();
        }
    }
}
