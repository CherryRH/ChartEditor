using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChartEditor.Models
{
    /// <summary>
    /// 谱面曲目
    /// </summary>
    public class ChartMusic
    {
        private static string logTag = "[ChartMusic]";

        /// <summary>
        /// 曲名
        /// </summary>
        private string title;
        public string Title { get { return title; } set { title = value; } }

        /// <summary>
        /// 作曲家
        /// </summary>
        private string artist;
        public string Artist { get { return artist; } set { artist = value; } }

        /// <summary>
        /// BPM每分钟节拍数
        /// </summary>
        private double bpm;
        public double Bpm { get { return bpm; } set { bpm = value; } }

        /// <summary>
        /// 曲目时长
        /// </summary>
        private double duration;
        public double Duration { get { return duration; } set { duration = value; } }

        /// <summary>
        /// 曲目文件夹路径
        /// </summary>
        private string folderPath;
        public string FolderPath { get { return folderPath; } set { folderPath = value; } }

        /// <summary>
        /// 创建时间
        /// </summary>
        private DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } set { createdAt = value; } }

        /// <summary>
        /// 上一次更新时间
        /// </summary>
        private DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } set { updatedAt = value; } }

        /// <summary>
        /// 封面图片路径
        /// </summary>
        private string coverPath;
        public string CoverPath { get { return coverPath; } set { coverPath = value; } }

        public ChartMusic()
        {

        }

        public ChartMusic(string title, string composer, double bpm, double duration, string folderPath, DateTime? createdAt)
        {
            this.title = title;
            this.artist = composer;
            this.bpm = bpm;
            this.duration = duration;
            this.createdAt = createdAt ?? DateTime.Now;
            this.updatedAt = DateTime.Now;
            this.folderPath = folderPath;
            this.coverPath = this.GetCoverPath();
        }

        /// <summary>
        /// 转化为Json字符串
        /// </summary>
        public string toJsonString()
        {
            JObject jObject = new JObject
            {
                ["Title"] = this.title ?? string.Empty,
                ["Artist"] = this.artist ?? string.Empty,
                ["Bpm"] = this.bpm,
                ["Duration"] = this.duration,
                ["CreatedAt"] = this.createdAt,
                ["UpdatedAt"] = this.updatedAt
            };
            return jObject.ToString(Formatting.Indented);
        }

        /// <summary>
        /// 从目录路径和Json字符串构造
        /// </summary>
        public ChartMusic(string folder, string jsonString)
        {
            try
            {
                this.folderPath = folder;
                this.coverPath = this.GetCoverPath();
                JObject jObject = JsonConvert.DeserializeObject<JObject>(jsonString);
                // 解析并赋值属性
                this.title = (string)jObject["Title"] ?? string.Empty;
                this.artist = (string)jObject["Artist"] ?? string.Empty;
                this.bpm = (double)jObject["Bpm"];
                this.duration = (double)jObject["Duration"];
                this.createdAt = (DateTime)jObject["CreatedAt"];
                this.updatedAt = (DateTime)jObject["UpdatedAt"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + $"从 Json 构造 ChartMusic 对象时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取封面图片路径
        /// </summary>
        private string GetCoverPath()
        {
            return Path.Combine(this.folderPath, "cover.png");
        }

        /// <summary>
        /// 获取音频路径
        /// </summary>
        public string GetMusicPath()
        {
            return Path.Combine(this.folderPath, "music.ogg");
        }

        /// <summary>
        /// 获取时长的分秒格式字符串
        /// </summary>
        /// <returns></returns>
        public string GetDurationFormatString()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(this.duration);
            return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public void UpdateAtNow()
        {
            this.updatedAt = DateTime.Now;
        }
    }
}
