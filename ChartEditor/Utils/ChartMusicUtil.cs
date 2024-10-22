using ChartEditor.Models;
using ChartEditor.Utils.Cache;
using ChartEditor.ViewModels;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    public class ChartMusicUtil
    {
        private static string logTag = "[ChartMusicUtil]";

        /// <summary>
        /// 从chart文件夹内读取曲目列表
        /// </summary>
        public static List<ChartMusic> ReadChartMusics()
        {
            List<ChartMusic> chartMusicList = new List<ChartMusic>();
            try
            {
                if (!Directory.Exists(Common.GetChartMusicFolderPath()))
                {
                    return chartMusicList;
                }
                // 获取所有曲目文件夹路径
                string[] folders = Directory.GetDirectories(Common.GetChartMusicFolderPath());

                foreach (var folder in folders)
                {
                    string configFilePath = Path.Combine(folder, Common.ChartMusicConfigFileName);

                    if (CheckChartMusicFolder(folder, out string coverFilePath))
                    {
                        chartMusicList.Add(new ChartMusic(folder, coverFilePath, File.ReadAllText(configFilePath)));
                    }
                }

                Console.WriteLine(logTag + "曲目列表已读取");

                return chartMusicList;
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return chartMusicList;
            }
        }

        public ChartMusicUtil() { }

        /// <summary>
        /// 检测曲目文件是否完整
        /// </summary>
        public static bool CheckChartMusicFolder(string folderPath, out string coverFilePath)
        {
            try
            {
                if (!Directory.Exists(folderPath)) {
                    coverFilePath = "";
                    return false;
                }
                if (!File.Exists(Path.Combine(folderPath, Common.ChartMusicConfigFileName))) {
                    coverFilePath = "";
                    return false;
                }
                if (!File.Exists(Path.Combine(folderPath, Common.ChartMusicFileName))) {
                    coverFilePath = "";
                    return false;
                }
                var coverFiles = Directory.EnumerateFiles(folderPath, "cover_*.png", System.IO.SearchOption.TopDirectoryOnly);
                if (coverFiles.Count() == 0)
                {
                    coverFilePath = "";
                    return false;
                }
                coverFilePath = coverFiles.FirstOrDefault();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                coverFilePath = "";
                return false;
            }
        }

        /// <summary>
        /// 删除曲目及其所有谱面和文件（回收站）
        /// </summary>
        public static bool DeleteChartMusic(ChartMusic chartMusic)
        {
            try
            {
                if (Directory.Exists(chartMusic.FolderPath))
                {
                    FileSystem.DeleteDirectory(chartMusic.FolderPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    return true;
                }
                else
                {
                    throw new Exception("曲目文件夹不存在");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 更新曲目文件
        /// </summary>
        public static bool SaveChartMusic(ChartMusic chartMusic)
        {
            try
            {
                if (Directory.Exists(chartMusic.FolderPath))
                {
                    File.WriteAllText(Path.Combine(chartMusic.FolderPath, Common.ChartMusicConfigFileName), chartMusic.toJsonString());
                    return true;
                }
                else
                {
                    throw new Exception("曲目文件夹不存在");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 生成曲目唯一文件夹名称
        /// </summary>
        public static string GenerateNewChartMusicFolderPath(DateTime createdAt)
        {
            string timeStamp = createdAt.ToString("yyyyMMdd_HHmmss");
            string randomSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            string folderName = $"music_{timeStamp}_{randomSuffix}";
            return Path.Combine(Common.GetChartMusicFolderPath(), folderName);
        }
    }
}
