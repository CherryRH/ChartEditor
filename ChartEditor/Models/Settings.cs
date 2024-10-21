using ChartEditor.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Models
{
    /// <summary>
    /// 制谱器设置
    /// </summary>
    public class Settings
    {
        private static string logTag = "[Settings]";

        /// <summary>
        /// 版本号
        /// </summary>
        private string appVersion;
        public string AppVersion { get { return appVersion; } }

        /// <summary>
        /// 谱师名
        /// </summary>
        private string username;
        public string Username { get { return username; } set { username = value; } }

        private AutoSaveType autoSaveType;
        public AutoSaveType AutoSaveType { get { return autoSaveType; } set { autoSaveType = value; } }

        public string AppPath { get; set; }

        public Settings()
        {
            // 初始化数据
            this.username = "User";
            this.autoSaveType = AutoSaveType.OneMinute;
            // 获取版本号
            this.appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            // 读取本地设置
            this.ReadSettings();
            this.AppPath = Common.GetAppFolderPath();
        }

        ~Settings()
        {
            this.SaveSettings();
        }

        public string ToJsonString()
        {
            JObject jObject = new JObject
            {
                ["AppVersion"] = this.appVersion,
                ["Username"] = this.username,
                ["AutoSaveType"] = this.autoSaveType.ToString()
            };

            return jObject.ToString(Formatting.Indented);
        }

        /// <summary>
        /// 保存设置到本地
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                string settingsFilePath = Path.Combine(Common.GetConfigFolderPath(), "settings.json");

                File.WriteAllText(settingsFilePath, this.ToJsonString());
            
                Console.WriteLine(logTag + "设置已保存");
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }

        }

        /// <summary>
        /// 读取本地设置
        /// </summary>
        public void ReadSettings()
        {
            try
            {
                string settingsFilePath = Path.Combine(Common.GetConfigFolderPath(), "settings.json");

                if (File.Exists(settingsFilePath))
                {
                    JObject jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(settingsFilePath));
                    if (jObject != null)
                    {
                        // 用户名
                        this.username = jObject.Value<string>("Username") ?? string.Empty;
                        // 自动保存类型
                        string autoSaveTypeString = jObject.Value<string>("AutoSaveType") ?? string.Empty;
                        if (Enum.TryParse(autoSaveTypeString, out AutoSaveType parsedAutoSaveType))
                        {
                            this.autoSaveType = parsedAutoSaveType;
                        }
                        else
                        {
                            this.autoSaveType = AutoSaveType.OneMinute;
                        }

                        Console.WriteLine(logTag + "设置已读取");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }

        }

        /// <summary>
        /// 获取自动保存时间间隔（秒）
        /// </summary>
        public double GetAutoSaveInterval()
        {
            switch (this.autoSaveType)
            {
                case AutoSaveType.Never: return double.MaxValue;
                case AutoSaveType.OneMinute: return 60.0;
                case AutoSaveType.FiveMinutes: return 300.0;
                case AutoSaveType.TenMinutes: return 600.0;
            }
            return double.MaxValue;
        }
    }

    /// <summary>
    /// 自动保存类型
    /// </summary>
    public enum AutoSaveType
    {
        Never,
        OneMinute,
        FiveMinutes,
        TenMinutes
    }
}
