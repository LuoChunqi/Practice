// ResetLicenseForm.Designer.cs
namespace LoginSystem
{
    partial class ResetLicenseForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtDays = new System.Windows.Forms.TextBox();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            this.lblGeneratedKey = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtDays
            // 
            this.txtDays.Location = new System.Drawing.Point(120, 50);
            this.txtDays.Name = "txtDays";
            this.txtDays.Size = new System.Drawing.Size(100, 20);
            this.txtDays.TabIndex = 0;
            // 
            // btnGenerateKey
            // 
            this.btnGenerateKey.Location = new System.Drawing.Point(120, 90);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(100, 30);
            this.btnGenerateKey.TabIndex = 1;
            this.btnGenerateKey.Text = "生成密钥";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            // 
            // lblGeneratedKey
            // 
            this.lblGeneratedKey.AutoSize = true;
            this.lblGeneratedKey.Location = new System.Drawing.Point(120, 140);
            this.lblGeneratedKey.Name = "lblGeneratedKey";
            this.lblGeneratedKey.Size = new System.Drawing.Size(0, 13);
            this.lblGeneratedKey.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "重置天数：";
            // 
            // ResetLicenseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Controls.Add(this.label1);
            //this.Controls.Add(this.lblGeneratedKey);
            this.Controls.Add(this.btnGenerateKey);
            this.Controls.Add(this.txtDays);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResetLicenseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "生成重置密钥";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDays;
        private System.Windows.Forms.Button btnGenerateKey;
        private System.Windows.Forms.Label lblGeneratedKey;
        private System.Windows.Forms.Label label1;
    }
}