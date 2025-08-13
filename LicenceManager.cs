using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LoginDemo;

namespace LoginSystem
{
    public static class LicenceManager
    {
        public static readonly string LicenceFilePath = Path.Combine(Application.StartupPath, "licence.txt");
        private static System.Timers.Timer _timer;
        private static double _remainingSeconds = 1; // 敢改licence.txt文件就变1秒使用时长
        private static bool _isExpired;
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, DateTime> _usedKeys = new Dictionary<string, DateTime>();
        private static DateTime _lastUsedTime = DateTime.Now;

        public static bool IsExpired { get; internal set; }

        public static event Action LicenseExpired; // 新增事件
        // 获取已使用密钥列表
        public static Dictionary<string, DateTime> GetUsedKeys()
        {
            lock (_lock)
            {
                return new Dictionary<string, DateTime>(_usedKeys);
            }
        }

        public static bool IsKeyUsed(string key)
        {
            lock (_lock)
            {
                return _usedKeys.ContainsKey(key);
            }
        }

        public static DateTime GetKeyUsageTime(string key)
        {
            lock (_lock)
            {
                return _usedKeys.TryGetValue(key, out DateTime time) ? time : DateTime.MinValue;
            }
        }

        /// <summary>
        /// 添加已使用的密钥
        /// </summary>
        /// <param name="key">激活码</param>
        public static void AddUsedKey(string key)
        {
            lock (_lock)
            {
                if (!_usedKeys.ContainsKey(key))
                {
                    _usedKeys[key] = DateTime.Now;
                    SaveLicenseData(); // 确保调用此方法以保存更改
                }
            }
        }

public static void DebugDecryptLicenseFile()
{
    try
    {
        if (!File.Exists(LicenceFilePath))
        {
            MessageBox.Show("授权文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 读取加密文件内容
        string encryptedContent = File.ReadAllText(LicenceFilePath, Encoding.UTF8);
        
        // 解密内容
        string decryptedContent = DesHelper.Decrypt(encryptedContent);
        
        // 显示解密结果
        MessageBox.Show($"解密结果:\n\n{decryptedContent}", "授权文件内容", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"解密失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        public static void ResetLicenseWithKey(string encryptedKey)
        {
            lock (_lock)
            {
                try
                {
                    // 解密密钥
                    string decryptedKey = DesHelper.Decrypt(encryptedKey);
                    if (decryptedKey == null || !decryptedKey.Contains("SIASUN"))
                    {
                        throw new ArgumentException("无效的密钥格式");
                    }

                    // 提取盐值和天数
                    int siAsunIndex = decryptedKey.IndexOf("SIASUN");
                    string salt = decryptedKey.Substring(0, siAsunIndex);
                    string daysStr = decryptedKey.Substring(siAsunIndex + 6); // "SIASUN".Length = 6

                    if (!int.TryParse(daysStr, out int resetDays))
                    {
                        throw new ArgumentException("密钥中的天数无效");
                    }

                    // 检查密钥是否已使用
                    if (IsKeyUsed(encryptedKey))
                    {
                        DateTime usedTime = GetKeyUsageTime(encryptedKey);
                        throw new InvalidOperationException($"该密钥已于 {usedTime:yyyy-MM-dd HH:mm:ss} 使用过");
                    }

                    // 验证密钥格式（确保天数合理）
                    if (resetDays <= 0 || resetDays > 3650) // 假设最大3650天
                    {
                        throw new ArgumentException("密钥中的天数不合法");
                    }

                    // 更新授权时间
                    _remainingSeconds = resetDays * 86400;
                    _usedKeys[encryptedKey] = DateTime.Now;

                    // 保存到licence.txt（加密存储）
                    SaveLicenseData();

                    MessageBox.Show("授权重置成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"重置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 重置授权，基于提供的激活码
        /// </summary>
        /// <param name="activationCode">激活码</param>
        public static void ResetLicense(string activationCode)
        {
            try
            {
                // 验证激活码
                if (!DesHelper.IsValidActivationCode(activationCode))
                {
                    throw new ArgumentException("无效的激活码");
                }

                // 提取重置天数
                int resetDays = DesHelper.GetResetDaysFromActivationCode(activationCode);
                if (resetDays == -1)
                {
                    throw new ArgumentException("激活码格式不正确");
                }

                // 检查是否已使用过
                if (IsKeyUsed(activationCode))
                {
                    DateTime usedTime = GetKeyUsageTime(activationCode);
                    throw new InvalidOperationException($"该激活码已于 {usedTime:yyyy-MM-dd HH:mm:ss} 使用过");
                }

                // 重置授权
                _remainingSeconds = resetDays * 86400; // 转换为秒
                _usedKeys[activationCode] = DateTime.Now;

                // 保存到 licence.txt（加密存储）
                SaveLicenseData();

                // 验证同步
                string fileContent = File.ReadAllText(LicenceFilePath);
                string decrypted = DesHelper.Decrypt(fileContent);
                if (!decrypted.Contains($"TIMEING={_remainingSeconds}"))
                {
                    throw new Exception("文件与内存不同步！");
                }

                // 如果不在调试模式，记录激活码的使用
                if (!LoginForm.DebugMode)
                {
                    AddUsedKey(activationCode);
                }

                Console.WriteLine($"授权已重置为 {resetDays} 天");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Initialize()
        {
            string directory = Path.GetDirectoryName(LicenceFilePath);

            if (!File.Exists(LicenceFilePath))
            {
                // 如果授权文件不存在，弹出缺少必要的文件，点击确定后强制退出程序
                MessageBox.Show(
                    "缺少必要的授权文件，程序无法运行。",  // Message
                    "错误",                              // Title
                    MessageBoxButtons.OK,                // Buttons
                    MessageBoxIcon.Error                 // Icon
                );

                // Exit the application with an error code
                Environment.Exit(1);  // 1 typically indicates an error exit code
            }

            try
            {
                LoadLicenseData();

                if (_remainingSeconds <= 0)
                {
                    MessageBox.Show("授权已过期，请续费", "授权到期", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //Environment.Exit(1); // 非正常退出
                }

                _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += (s, e) => UpdateRemainingTime();
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"授权文件损坏: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // 非正常退出
            }
        }

        // 加载授权数据
        private static void LoadLicenseData()
        {
            try
            {
                if (!File.Exists(LicenceFilePath))
                {
                   
                    return;
                }

                string fileContent = File.ReadAllText(LicenceFilePath, Encoding.UTF8);
                string decryptedContent = DesHelper.Decrypt(fileContent);

                // 验证解密内容是否有效
                if (string.IsNullOrEmpty(decryptedContent) || !decryptedContent.Contains("TIMEING="))
                {
                    
                    return;
                }

                var match = Regex.Match(decryptedContent,
                    @"TIMEING=(\d+)\|\|KEYS:(.*?)\|\|使用时间：(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})");

                if (match.Success)
                {
                    _remainingSeconds = double.Parse(match.Groups[1].Value);

                    // 清空旧密钥
                    _usedKeys.Clear();

                    // 解析密钥
                    string keysData = match.Groups[2].Value;
                    foreach (string keyEntry in keysData.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = keyEntry.Split('@');
                        if (parts.Length == 2)
                        {
                            try
                            {
                                _usedKeys[parts[0]] = ParseDateTime(parts[1]);
                            }
                            catch
                            {
                                Debug.WriteLine($"时间格式解析失败: {parts[1]}");
                            }
                        }
                    }

                    // 解析使用时间
                    try
                    {
                        _lastUsedTime = ParseDateTime(match.Groups[3].Value);
                    }
                    catch
                    {
                        _lastUsedTime = DateTime.Now;
                    }
                }
                else
                {
                
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载授权文件错误: {ex.Message}");
             
            }
        }

        private static void UpdateRemainingTime()
        {
            lock (_lock)
            {
                _remainingSeconds--;
                SaveLicenseData();

                if (_remainingSeconds <= 0 && !_isExpired)
                {
                    _isExpired = true;
                    _timer.Stop();

                    // 触发事件（通知订阅者）
                    LicenseExpired?.Invoke();
                }
            }
        }

        // 保存授权数据
        public static void SaveLicenseData()
        {
            // 确保_remainingSeconds是有效值
            if (_remainingSeconds < 0)
            {
                _remainingSeconds = 0;
            }

            try
            {
                // 构建授权信息字符串
                string keysRecord = string.Join("|",
                    _usedKeys.Select(kvp => $"{kvp.Key}@{FormatDateTime(kvp.Value)}"));

                string content = $"TIMEING={_remainingSeconds}||KEYS:{keysRecord}||使用时间：{FormatDateTime(DateTime.Now)}";
                string prefixedContent = "SIASUN_LICENSE:" + content;

                // 验证加密前内容
                if (string.IsNullOrEmpty(content))
                {
                    throw new Exception("授权内容为空");
                }

                // 加密内容
                string encryptedContent = DesHelper.Encrypt(prefixedContent);
                if (string.IsNullOrEmpty(encryptedContent))
                {
                    throw new Exception("加密失败");
                }

                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(LicenceFilePath));

                // 写入临时文件
                string tempFile = LicenceFilePath + ".tmp";
                File.WriteAllText(tempFile, encryptedContent, Encoding.UTF8);

                // 替换原文件（原子操作）
                File.Replace(tempFile, LicenceFilePath, null);

                //Console.WriteLine($"[{DateTime.Now}] 授权文件保存成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] 保存授权文件失败: {ex.Message}");
                throw; // 抛出异常让调用者处理
            }
        }

        public static int GetUsageStatus()
        {
            return (int)_remainingSeconds;
        }

        public static void RecordLogin()
        {
            lock (_lock)
            {
                if (_remainingSeconds <= 0)
                {
                    MessageBox.Show("授权已过期，请续费", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Environment.Exit(1); // 非正常退出
                }
                _lastUsedTime = DateTime.Now;
                SaveLicenseData();
            }
        }

        // DateTime 转 Unix 时间戳
        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static DateTime ParseDateTime(string dateTimeString)
        {
            return DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null);
        }
    }
}