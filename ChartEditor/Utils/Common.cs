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
        /// 游戏屏幕坐标系范围
        /// </summary>
        public static double XMax = 800;
        public static double YMax = 600;
        public static double XMin = -800;
        public static double YMin = -600;

        /// <summary>
        /// 轨道编辑面板默认列数
        /// </summary>
        public static int ColumnNum = 4;

        /// <summary>
        /// 每一列的宽度
        /// </summary>
        public static double ColumnWidth = 60;

        /// <summary>
        /// 列之间间隔的宽度占比
        /// </summary>
        public static double ColumnGap = 1.0 / 12.0;

        /// <summary>
        /// 每一行的宽度（一拍为一行）
        /// </summary>
        public static double RowWidth = 160;

        /// <summary>
        /// 判定线所处位置比例
        /// </summary>
        public static double JudgeLineRate = 0.2;
        public static double JudgeLineRateOp = 0.8;

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
        /// 获取曲目和谱面所在文件夹路径
        /// </summary>
        public static string GetChartMusicFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "charts");
        }

        /// <summary>
        /// 获取资源所在文件夹路径
        /// </summary>
        public static string GetResourcesFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "Resources");
        }

        /// <summary>
        /// 获取谱面导出所在文件夹路径
        /// </summary>
        public static string GetExportFolderPath()
        {
            return Path.Combine(GetDataFolderPath(), "exports");
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
            if (!Directory.Exists(GetExportFolderPath()))
            {
                Directory.CreateDirectory(GetExportFolderPath());
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

    public enum MessageType
    {
        Notice,
        Warn,
        Error
    }
}
