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

        public string AppPath { get; set; }

        public Settings()
        {
            // 初始化数据
            this.username = "User";
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
                        this.Username = (string)jObject["Username"] ?? string.Empty;
                        Console.WriteLine(logTag + "设置已读取");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }

        }
    }
}
