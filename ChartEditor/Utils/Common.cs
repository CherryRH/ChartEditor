using ChartEditor.Models;
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

        public static string DefaultMusicCover = "pack://application:,,,/Resources/Textures/音乐.png";

        /// <summary>
        /// 轨道编辑面板默认列数
        /// </summary>
        public static int ColumnNum = 4;

        /// <summary>
        /// 每一列的宽度
        /// </summary>
        public static double ColumnWidth = 80;
        public static double ColumnWidthTick = 10;
        public static double ColumnWidthMin = 20;
        public static double ColumnWidthMax = 150;

        /// <summary>
        /// 轨道的边缘占比
        /// </summary>
        public static double TrackPadding = 0.1;

        /// <summary>
        /// 音符的边缘占比
        /// </summary>
        public static double NotePadding = 0.2;

        /// <summary>
        /// 每一行的宽度（一拍为一行）
        /// </summary>
        public static double RowWidth = 200;
        public static double RowWidthTick = 20;
        public static double RowWidthMin = 20;
        public static double RowWidthMax = 400;

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
        public static string ChartMusicFileName = "music.ogg";
        public static string WorkspaceName = "workspace.json";

        public static Dictionary<int, string> HitSoundPaths = new Dictionary<int, string>
        {
            { 0, Path.Combine(GetResourcesFolderPath(), "Audios/HitSong0.wav") },
            { 1, Path.Combine(GetResourcesFolderPath(), "Audios/HitSong1.wav") },
            { 2, Path.Combine(GetResourcesFolderPath(), "Audios/HitSong2.wav") }
        };

        /// <summary>
        /// 获取应用所在文件夹路径
        /// </summary>
        public static string GetAppFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 获取设置所在文件夹路径
        /// </summary>
        public static string GetConfigFolderPath()
        {
            return Path.Combine(GetAppFolderPath(), "config");
        }

        /// <summary>
        /// 获取曲目和谱面所在文件夹路径
        /// </summary>
        public static string GetChartMusicFolderPath()
        {
            return Path.Combine(GetAppFolderPath(), "charts");
        }

        /// <summary>
        /// 获取资源所在文件夹路径
        /// </summary>
        public static string GetResourcesFolderPath()
        {
            return Path.Combine(GetAppFolderPath(), "Resources");
        }

        /// <summary>
        /// 获取谱面导出所在文件夹路径
        /// </summary>
        public static string GetExportFolderPath()
        {
            return Path.Combine(GetAppFolderPath(), "exports");
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

        /// <summary>
        /// 生成谱面编辑窗口标题
        /// </summary>
        public static string GenerateChartWindowTitle(ChartInfo chartInfo)
        {
            if (chartInfo == null) return "谱面好像丢了呢~";
            else return chartInfo.Name + " - " + chartInfo.ChartMusic.Title;
        }
    }

    public enum MessageType
    {
        Notice,
        Warn,
        Error
    }
}
