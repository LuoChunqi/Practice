using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LoginDemo
{
    public static class DesHelper
    {
        // 8字节密钥
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("SIASUN12"); // 8字节密钥，请确保与 LicenceManager 中的一致
        // 8字节初始化向量
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("12345678"); // 8字节IV，请确保与 LicenceManager 中的一致

        /// <summary>
        /// 加密明文（必须以 'SIASUN' 开头）
        /// </summary>
        /// <param name="plainText">明文（必须以 'SIASUN' 开头，并包含天数）</param>
        /// <returns>Base64 编码的密文</returns>
        // 修改前（仅支持 int resetDays）
        // public static string Encrypt(int resetDays)

        // 修改后（支持 string plainText）
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText) || !plainText.Contains("SIASUN"))
            {
                throw new ArgumentException("格式错误");
            }

            try
            {
                using (var des = DES.Create())
                {
                    des.Key = Key;
                    des.IV = IV;

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (var sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                            }
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("加密失败", ex);
            }
        }


        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(cipherText);

                using (var des = DES.Create())
                {
                    des.Key = Key;
                    des.IV = IV;

                    using (var ms = new MemoryStream(encryptedBytes))
                    {
                        using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                string decrypted = sr.ReadToEnd();
                                // 确保解密后的字符串符合 "天数SIASUN盐" 格式
                                if (decrypted.Contains("SIASUN"))
                                {
                                    return decrypted; // 返回完整解密后的字符串
                                }
                                return null;
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 验证激活码是否有效
        /// </summary>
        /// <param name="activationCode">激活码</param>
        /// <returns>如果有效返回 true，否则返回 false</returns>
        public static bool IsValidActivationCode(string activationCode)
        {
            try
            {
                string decrypted = Decrypt(activationCode);
                return decrypted != null && decrypted.Contains("SIASUN");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从激活码中提取重置天数
        /// </summary>
        /// <param name="activationCode">激活码</param>
        /// <returns>重置天数，如果无效返回 -1</returns>
        public static int GetResetDaysFromActivationCode(string activationCode)
        {
            try
            {
                string decrypted = Decrypt(activationCode);
                if (decrypted != null && decrypted.Contains("SIASUN"))
                {
                    // 提取 "天数SIASUN" 前面的部分（即天数）
                    int siAsunIndex = decrypted.IndexOf("SIASUN");
                    if (siAsunIndex > 0) // 确保 SIASUN 不在开头
                    {
                        string daysStr = decrypted.Substring(0, siAsunIndex); // ✅ 正确提取天数
                        string salt = decrypted.Substring(siAsunIndex + 6); // ✅ 正确提取盐值

                        if (int.TryParse(daysStr, out int days) && days > 0)
                        {
                            return days; // 返回有效的天数
                        }
                    }
                }
            }
            catch
            {
                // 解密失败或格式不正确
            }
            return -1; // 无效的激活码
        }

        /// <summary>
        /// 在调试模式下解密并显示 licence.txt 文件的内容
        /// </summary>
        public static void DebugDecryptLicenceFile()
        {
            string licenceFilePath = Path.Combine(Application.StartupPath, "licence.txt");

            if (!File.Exists(licenceFilePath))
            {
                Console.WriteLine("授权文件缺失，请检查");
                return;
            }

            try
            {
                string encryptedData = File.ReadAllText(licenceFilePath);
                string decryptedData = Decrypt(encryptedData);
                if (decryptedData != null)
                {
                    Console.WriteLine($"授权文件内容（解密后）:\n{decryptedData}");
                }
                else
                {
                    Console.WriteLine("解密授权文件失败，文件可能已损坏或格式不正确");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解密授权文件失败: {ex.Message}");
            }
        }
    }
}