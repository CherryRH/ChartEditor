using ChartEditor.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Png;

namespace ChartEditor.Models
{
    /// <summary>
    /// 谱面基础信息
    /// </summary>
    public class ChartInfo
    {
        private static string logTag = "[ChartInfo]";

        /// <summary>
        /// 谱面歌曲信息
        /// </summary>
        private ChartMusic chartMusic;
        public ChartMusic ChartMusic { get { return chartMusic; } }

        /// <summary>
        /// 谱面名称
        /// </summary>
        private string name;
        public string Name { get { return name; } set { name = value; } }

        /// <summary>
        /// 难度
        /// </summary>
        private int difficulty;
        public int Difficulty { get { return difficulty; } set { difficulty = value; } }

        /// <summary>
        /// 谱面作者
        /// </summary>
        private string author;
        public string Author { get { return author; } set { author = value; } }

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
        /// 物量
        /// </summary>
        private int volume;
        public int Volume { get { return volume; } set { volume = value; } }

        /// <summary>
        /// 歌曲速度/谱面速度
        /// </summary>
        private double speed;
        public double Speed { get { return speed; } set { speed = value; } }

        /// <summary>
        /// 谱面时长
        /// </summary>
        private double duration;
        public double Duration { get { return duration; } set { duration = value; } }

        /// <summary>
        /// 谱面延迟
        /// </summary>
        private double delay;
        public double Delay { get { return delay; } set { delay = value; } }

        /// <summary>
        /// 预览时间
        /// </summary>
        private double preview;
        public double Preview { get { return preview; } set { preview = value; } }

        /// <summary>
        /// 是否镜像
        /// </summary>
        private bool isMirrored;
        public bool IsMirrored { get { return isMirrored; } set { isMirrored = value; } }

        /// <summary>
        /// 谱面文件夹路径
        /// </summary>
        private string folderPath;
        public string FolderPath { get { return folderPath; } set { folderPath = value; } }

        public ChartInfo()
        {

        }

        public ChartInfo(ChartMusic chartMusic, string name, string author) 
        {
            this.chartMusic = chartMusic;
            this.name = name;
            this.author = author;
            this.folderPath = Path.Combine(this.chartMusic.FolderPath, name);
            this.createdAt = DateTime.Now;
            this.updatedAt = DateTime.Now;
            this.difficulty = 0;
            this.volume = 0;
            this.speed = 1.0;
            this.duration = 0;
            this.delay = 0;
            this.preview = 0;
            this.isMirrored = false;
        }

        /// <summary>
        /// 转化为JObject
        /// </summary>
        public JObject ToJson()
        {
            return new JObject
            {
                ["Name"] = this.name,
                ["Difficulty"] = this.difficulty,
                ["Author"] = this.author,
                ["CreatedAt"] = this.createdAt,
                ["UpdatedAt"] = this.updatedAt,
                ["Volume"] = this.volume,
                ["Speed"] = this.speed,
                ["Duration"] = this.duration,
                ["Delay"] = this.delay,
                ["Preview"] = this.preview,
                ["IsMirrored"] = this.isMirrored
            };
        }

        /// <summary>
        /// 从目录路径和JObject构造
        /// </summary>
        public ChartInfo(string folder, JObject jObject, ChartMusic chartMusic)
        {
            try
            {
                this.chartMusic = chartMusic;
                this.folderPath = folder;
                if (jObject == null) { return; }
                // 解析并赋值属性
                this.name = (string)jObject["Name"] ?? string.Empty;
                this.difficulty = (int)jObject["Difficulty"];
                this.author = (string)jObject["Author"] ?? string.Empty;
                this.createdAt = (DateTime)jObject["CreatedAt"];
                this.updatedAt = (DateTime)jObject["UpdatedAt"];
                this.volume = (int)jObject["Volume"];
                this.speed = (double)jObject["Speed"];
                this.duration = (double)jObject["Duration"];
                this.delay = (double)jObject["Delay"];
                this.preview = (double)jObject["Preview"];
                this.isMirrored = (bool)jObject["IsMirrored"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + $"从 Json 构造 ChartInfo 对象时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取谱面文件路径
        /// </summary>
        public string GetChartFilePath()
        {
            return Path.Combine(this.folderPath, Common.ChartFileName);
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
