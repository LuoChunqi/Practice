using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginDemo
{
    public partial class RemainingTimeForm : Form
    {
        public RemainingTimeForm(string message)
        {
            InitializeComponent(message);
        }

        private void InitializeComponent(string message)
        {
            // 窗体设置
            this.Text = "授权剩余时间提示";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 提示信息标签
            var lblMessage = new Label
            {
                Text = message,
                Font = new Font("微软雅黑", 12),
                ForeColor = Color.Green,
                AutoSize = false,
                Size = new Size(350, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(25, 40)
            };

            // 确认按钮
            var btnOK = new Button
            {
                Text = "确定",
                Size = new Size(100, 30),
                Location = new Point(150, 120)
            };
            btnOK.Click += (s, e) => this.Close();

            // 添加控件
            this.Controls.Add(lblMessage);
            this.Controls.Add(btnOK);
        }
    }
}
