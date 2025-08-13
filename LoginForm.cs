using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using LoginDemo;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LoginSystem
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button reSet;
        private Label lblMessage;
        private System.Windows.Forms.Timer authorizationTimer;
        private ComboBox cmbDuration;
        public Button btnGenerateKey;

        
        private static readonly Regex KeyRegex;

        // 转换规则
        private static readonly Dictionary<char, char> ConversionRules = new Dictionary<char, char>()
        {
           
        };
        public static bool DebugMode { get; set; } = false;
        public LoginForm()
        {

            InitializeControls();
            // 初始化时检查授权状态
            var remainingDays= LicenceManager.GetUsageStatus();
            //if (remainingDays == 0)
            //{
            //    MessageBox.Show("授权已失效，请重置授权", "授权失效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    Application.Exit();
            //}
            if (remainingDays == 100)
            {
                MessageBox.Show("系统使用时长所剩不多，请及时重置", "授权失效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               
            }
            UpdateResetButtonVisibility(); // 新增初始化时按钮状态检查
            InitializeAuthorizationTimer();
        }

        private void UpdateResetButtonVisibility()
        {
            var remainingDays = LicenceManager.GetUsageStatus();
            reSet.Visible = remainingDays <= 2100000000; // 重置按钮剩余天数≤多少时显示
        }

        /// <summary>
        /// 将密文转换为明文
        /// </summary>
        private string ConvertCipherToPlaintext(string cipherText)
        {
            var plaintext = new StringBuilder();
            foreach (char c in cipherText)
            {
                if (ConversionRules.ContainsKey(c))
                {
                    plaintext.Append(ConversionRules[c]);
                }
                else
                {
                    plaintext.Append(c); // 如果字符不在规则中，直接保留
                }
            }
            return plaintext.ToString();
        }

        /// <summary>
        /// 验证明文是否符合规则
        /// </summary>
        private bool IsPlaintextValid(string plaintext)
        {
            return KeyRegex.IsMatch(plaintext);
        }

        /// <summary>
        /// 初始化授权状态检查定时器（每秒检查一次）
        /// </summary>
        private void InitializeAuthorizationTimer()
        {
            authorizationTimer = new System.Windows.Forms.Timer
            {
                Interval = 120000 // 120秒
            };
            authorizationTimer.Tick += OnAuthorizationCheck;
            authorizationTimer.Start();
        }

        /// <summary>
        /// 定时器Tick事件，每秒检查授权状态
        /// </summary>
        private void OnAuthorizationCheck(object sender, EventArgs e)
        {
            var remainingDays= LicenceManager.GetUsageStatus();
            UpdateResetButtonVisibility(); // 每次检查更新按钮状态
        }

        /// <summary>
        /// 初始化窗体控件
        /// </summary>
        private void InitializeControls()
        {
            this.Text = "新松自动化立体仓库登录系统";
            this.ClientSize = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 用户名输入
            var lblUsername = new Label { Text = "用户名：", Location = new Point(30, 30), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(120, 30), Size = new Size(200, 25) };
            this.Controls.AddRange(new Control[] { lblUsername, txtUsername });

            // 密码输入
            var lblPassword = new Label { Text = "密码：", Location = new Point(30, 80), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(120, 80), Size = new Size(200, 25), PasswordChar = '*' };
            this.Controls.AddRange(new Control[] { lblPassword, txtPassword });

            // 登录按钮
            btnLogin = new Button { Text = "登录", Location = new Point(200, 150), Size = new Size(100, 30) };
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // 重置按钮
            reSet = new Button { Text = "重置", Location = new Point(80, 150), Size = new Size(100, 30) };
            reSet.Click += BtnReset_Click;
            this.Controls.Add(reSet);

            // 消息标签
            lblMessage = new Label { Location = new Point(30, 120), AutoSize = true };
            this.Controls.Add(lblMessage);






            //调试解密按钮，一键解密licence.txt文件内容
/*            var btnDebug = new Button { Text = "调试解密", Location = new Point(80, 180), Size = new Size(100, 30) };
            btnDebug.Click += (s, e) => LicenceManager.DebugDecryptLicenseFile();
            this.Controls.Add(btnDebug);*/


        }

        /// <summary>
        /// 登录按钮点击事件处理
        /// </summary>
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowErrorMessage("用户名和密码不能为空！");
                    return;
                }

                // 验证登录凭据
                if (!ValidateCredentials(username, password))
                {
                    ShowErrorMessage("用户名或密码错误！");
                    txtUsername.Text = "";
                    txtPassword.Text = "";
                    return;
                }

                // 处理成功登录
                HandleSuccessfulLogin();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"登录过程中发生错误: {ex.Message}");
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            string connectionString = "Server=localhost;Database=loginform;Uid=root;Pwd=root;SslMode=None";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void HandleSuccessfulLogin()
        {
            // 记录登录
            LicenceManager.RecordLogin();

            // 检查授权状态
            int remainingSeconds = LicenceManager.GetUsageStatus();

            if (remainingSeconds == 0)
            {
                MessageBox.Show("授权已失效，请续费以恢复使用权限",
                              "授权失效",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                Application.Exit();
                return;
            }

            // 显示剩余时间
            ShowRemainingTime(remainingSeconds);

            // 进入主界面
            this.Hide();
            new MainForm(remainingSeconds).Show();
        }

        private void ShowRemainingTime(int totalSeconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
            string message = $"授权剩余时间:\n" +
                            $"{timeSpan.Days}天 {timeSpan.Hours}小时 " +
                            $"{timeSpan.Minutes}分钟 {timeSpan.Seconds}秒";
            new RemainingTimeForm(message).ShowDialog();
        }

        private void ShowErrorMessage(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = Color.Red;
        }

        /// <summary>
        /// 重置按钮点击事件处理
        /// </summary>
        // LoginForm.cs
        public void BtnReset_Click(object sender, EventArgs e)
        {

/*            // 第一步：显示 ResetLicenseForm 让用户生成密钥
            using (ResetLicenseForm resetLicenseForm = new ResetLicenseForm())
            {
                //// 设置 GenerateKey 按钮的 DialogResult 为 None，防止自动关闭
                //resetLicenseForm.btnGenerateKey.DialogResult = DialogResult.None;

                if (resetLicenseForm.ShowDialog() == DialogResult.OK)
                {
                    // 用户已生成密钥，现在显示激活码重置页面
                    ShowResetKeyForm(resetLicenseForm.GeneratedKey);
                }
                else
                {
                    // 用户取消了 ResetLicenseForm
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Text = "授权重置已取消";
                }
            }*/


            string generatedKey = ""; // 如果需要动态生成密钥，可以在这里生成
            ShowResetKeyForm(generatedKey);





        }

        /// <summary>
        /// 显示激活码重置窗体并处理重置逻辑
        /// </summary>
        private void ShowResetKeyForm(string generatedKey)
        {
            using (var resetForm = new Form())
            {
                resetForm.Text = "授权重置";
                resetForm.Size = new Size(350, 250); // 调整大小以容纳更多控件
                resetForm.StartPosition = FormStartPosition.CenterParent;

                // 控件初始化
                var lblInstructions = new Label { Text = "请输入生成的激活码以重置授权：", Top = 20, Left = 20, Width = 300 };
                var lblKey = new Label { Text = "激活码：", Top = 60, Left = 20 };
                var txtKey = new TextBox { Top = 85, Left = 20, Width = 300 };

                // 如果有生成的密钥，可以预填充（可选）
                // txtKey.Text = generatedKey;

                var lblGeneratedKey = new Label { Text = $"生成的密钥（可选查看）：{generatedKey}", Top = 120, Left = 20, Width = 300, Visible = false }; 
                // 根据需要显示或隐藏
                 // 如果需要显示生成的密钥给用户，可以取消注释以下一行
                 // lblGeneratedKey.Visible = true;

                var btnSubmit = new Button { Text = "提交", Top = 160, Left = 60 };
                var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Top = 160, Left = 160 };

                resetForm.Controls.AddRange(new Control[] { lblInstructions, lblKey, txtKey, lblGeneratedKey, btnSubmit, btnCancel });

                // 提交按钮点击事件
                btnSubmit.Click += (s, args) =>
                {
                    if (string.IsNullOrWhiteSpace(txtKey.Text))
                    {
                        MessageBox.Show("请输入激活码", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    try
                    {
                        // 验证激活码
                        if (!DesHelper.IsValidActivationCode(txtKey.Text.Trim()))
                        {
                            MessageBox.Show("激活码无效", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        int resetDays = DesHelper.GetResetDaysFromActivationCode(txtKey.Text.Trim());
                        if (resetDays == -1)
                        {
                            MessageBox.Show("激活码格式不正确", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // 检查是否已使用过
                        if (LicenceManager.IsKeyUsed(txtKey.Text.Trim()))
                        {
                            DateTime usedTime = LicenceManager.GetKeyUsageTime(txtKey.Text.Trim());
                            MessageBox.Show($"该激活码已于 {usedTime:yyyy-MM-dd HH:mm:ss} 使用过",
                                          "警告",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Warning);
                            return;
                        }

                        // 重置授权
                        LicenceManager.ResetLicense(txtKey.Text.Trim());

                        // 如果不在调试模式，记录激活码的使用
                        if (!LoginForm.DebugMode)
                        {
                            LicenceManager.AddUsedKey(txtKey.Text.Trim());
                        }

                        MessageBox.Show("授权重置成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetForm.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"重置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                // 处理窗体关闭事件
                if (resetForm.ShowDialog() == DialogResult.OK)
                {
                    lblMessage.ForeColor = Color.Green;
                    lblMessage.Text = "授权重置成功";
                    UpdateResetButtonVisibility();
                }
                else
                {
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Text = "授权重置已取消";
                }
            }
        }

        private void UpdateLicenceFile(int selectedDays)
        {
            try
            {
                // 计算剩余秒数
                int remainingSeconds = selectedDays * 86400;

                // 获取已使用的密钥
                var usedKeys = LicenceManager.GetUsedKeys();
                var records = new List<string>();

                foreach (var keyPair in usedKeys)
                {
                    records.Add($"TIMEING={remainingSeconds}||KEYS:{keyPair.Key}||使用时间：{keyPair.Value:yyyy-MM-dd HH:mm:ss}");
                }

                File.WriteAllLines(LicenceManager.LicenceFilePath, records);

                MessageBox.Show("授权文件已更新", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新授权文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
     
        static void Main()
        {
            // 设置调试模式（仅用于开发和测试）
            // 在实际发布时，应注释掉或通过配置文件控制
            LoginForm.DebugMode = false; // 设置为 false 以启用正式模式
            // 初始化授权管理器
            LicenceManager.Initialize();




            // 调试模式下自动解密
            if (LoginForm.DebugMode)
            {
                LicenceManager.DebugDecryptLicenseFile();
            }




            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}