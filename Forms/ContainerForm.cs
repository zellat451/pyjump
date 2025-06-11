using pyjump.Services;

namespace pyjump.Forms
{
    public partial class ContainerForm : Form
    {
        public ContainerForm()
        {
            InitializeComponent();
            var mainForm = new PyJumpForm();
            var loadForm = new LoadingForm();
            var logForm = new LogForm();
            SingletonServices.RegisterForm(mainForm);
            SingletonServices.RegisterForm(loadForm);
            SingletonServices.RegisterForm(logForm);
            LoadChildForm(mainForm);
            LoadChildForm(loadForm, isLoadForm: true);
            LoadChildForm(logForm, isLogForm: true);
        }

        public void LoadChildForm(Form childForm, bool isLogForm = false, bool isLoadForm = false)
        {
            if (isLoadForm && isLogForm)
            {
                throw new ArgumentException("Cannot load both LoadForm and LogForm at the same time.");
            }

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            if (isLoadForm)
            {
                panelLoading.Controls.Clear();
                panelLoading.Controls.Add(childForm);
            }
            else if (isLogForm)
            {
                panelLogs.Controls.Clear();
                panelLogs.Controls.Add(childForm);
            }
            else
            {
                panelForm.Controls.Clear();
                panelForm.Controls.Add(childForm);
            }
            childForm.Show();
        }

    }
}
