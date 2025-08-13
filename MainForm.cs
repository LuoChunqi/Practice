using System.Windows.Forms;

namespace LoginSystem
{
    public partial class MainForm : Form
    {
        // 状态显示元素
        private Label lblStatus;
        private System.Timers.Timer _statusTimer;

        // 状态跟踪

        private int remainingDays;

        public MainForm()
        {
        }

        // 修正后的构造函数
        public MainForm(int remainingDays)
           
        {
            this.remainingDays = remainingDays; // 统一字段命名
            LicenceManager.LicenseExpired += OnLicenseExpired;
        }
        private void OnLicenseExpired()
        {
            // 确保在UI线程执行
            this.Invoke((MethodInvoker)delegate
            {
                MessageBox.Show("授权已到期，将返回登录页面", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // 关闭当前窗体，返回登录页
                this.Close();
            });
        }
    }
}