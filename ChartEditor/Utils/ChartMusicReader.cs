using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    public class ChartMusicReader
    {
        private static string logTag = "[ChartMusicReader]";

        /// <summary>
        /// 从chart文件夹内读取歌曲列表
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
                // 获取所有歌曲文件夹路径
                string[] folders = Directory.GetDirectories(Common.GetChartMusicFolderPath());

                foreach (var folder in folders)
                {
                    string configFilePath = Path.Combine(folder, Common.ChartMusicConfigFileName);

                    if (File.Exists(configFilePath))
                    {
                        chartMusicList.Add(new ChartMusic(folder, File.ReadAllText(configFilePath)));
                    }
                }

                Console.WriteLine(logTag + "歌曲列表已读取");

                return chartMusicList;
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return chartMusicList;
            }
        }

        public ChartMusicReader() { }
    }
}
