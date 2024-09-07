using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 通用项
    /// </summary>
    public class Common
    {
        private static string logTag = "[Common]";

        /// <summary>
        /// 关键文件名
        /// </summary>
        public static string ChartMusicConfigFileName = "music_config.json";
        public static string ChartFileName = "chart.json";

        /// <summary>
        /// 获取应用所在文件夹路径
        /// </summary>
        public static string GetAppFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取数据所在文件夹路径
        /// </summary>
        public static string GetDataFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取设置所在文件夹路径
        /// </summary>
        public static string GetConfigFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "config");
        }

        /// <summary>
        /// 获取歌曲和谱面所在文件夹路径
        /// </summary>
        public static string GetChartMusicFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "chart");
        }

        /// <summary>
        /// 获取资源所在文件夹路径
        /// </summary>
        public static string GetResourcesFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "Resources");
        }

        /// <summary>
        /// 创建应用所需的基础文件夹
        /// </summary>
        public static void SetBasicFolders()
        {
            if (!Directory.Exists(GetConfigFolderPath()))
            {
                Directory.CreateDirectory(GetConfigFolderPath());
            }
            if (!Directory.Exists(GetChartMusicFolderPath()))
            {
                Directory.CreateDirectory(GetChartMusicFolderPath());
            }
            Console.WriteLine(logTag + "基础文件夹已设置");
        }

        /// <summary>
        /// 获取ffmpeg.exe路径
        /// </summary>
        public static string GetFFmpegPath()
        {
            return Path.Combine(GetResourcesFolderPath(), "ffmpeg/bin/ffmpeg.exe");
        }
    }
}
