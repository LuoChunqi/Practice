using System;
using System.Windows.Forms;
using LoginDemo;

namespace LoginSystem
{
    public partial class ResetLicenseForm : Form
    {
        // 存储生成的密钥
        public string GeneratedKey { get; private set; }

        public ResetLicenseForm()
        {
            InitializeComponent();
        }

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtDays.Text, out int days) && days > 0)
            {
                try
                {
                    // 生成随机盐值
                    string salt = Guid.NewGuid().ToString("N").Substring(0, 6);

                    // 生成明文（格式：SIASUN天数 + 随机盐值）
                    string plainText = $"{days}SIASUN{salt}";

                    // 加密生成密钥
                    GeneratedKey = DesHelper.Encrypt(plainText);

                    // 在UI上显示密钥（可选，根据需求）
                    lblGeneratedKey.Text = GeneratedKey;

                    // 在后台显示生成的密钥（例如，控制台或日志）
                    Console.WriteLine($"生成的激活码: {GeneratedKey}");
                    // 如果需要记录到日志文件，可以在这里添加日志记录代码

                    // 显示成功消息
                    MessageBox.Show("密钥已生成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 设置 DialogResult 为 OK，以便调用者知道密钥已生成
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"生成密钥失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("请输入有效的天数！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}