using ChartEditor.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils.ChartUtils
{
    /// <summary>
    /// 谱面工具类，实现谱面文件读写相关操作
    /// </summary>
    public class ChartUtilV1
    {
        public static string logTag = "[ChartUtilV1]";

        /// <summary>
        /// 谱面格式版本号
        /// </summary>
        public static int ChartVersion = 1;

        /// <summary>
        /// 根据谱面基础信息创建谱面文件
        /// </summary>
        public static void CreateBasicChartFile(ChartInfo chartInfo)
        {
            if (chartInfo == null) { return; }

            try
            {
                if (!Directory.Exists(chartInfo.FolderPath))
                {
                    Directory.CreateDirectory(chartInfo.FolderPath);
                }
                string chartFilePath = chartInfo.GetChartFilePath();

                JObject chartJObject = new JObject();
                chartJObject.Add("ChartVersion", ChartVersion);
                chartJObject.Add("ChartInfo", chartInfo.ToJson());

                File.WriteAllText(chartFilePath, chartJObject.ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }
        }

        /// <summary>
        /// 读取指定歌曲的谱面列表
        /// </summary>
        public static List<ChartInfo> ReadChartList(ChartMusic chartMusic)
        {
            List<ChartInfo> chartInfos = new List<ChartInfo>();
            try
            {
                if (!Directory.Exists(chartMusic.FolderPath))
                {
                    return chartInfos;
                }
                // 获取所有谱面文件夹路径
                string[] folders = Directory.GetDirectories(chartMusic.FolderPath);

                foreach (var folder in folders)
                {
                    string chartFilePath = Path.Combine(folder, Common.ChartFileName);

                    if (File.Exists(chartFilePath))
                    {
                        JObject jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(chartFilePath));
                        chartInfos.Add(new ChartInfo(folder, (JObject)jObject["ChartInfo"], chartMusic));
                    }
                }

                Console.WriteLine(logTag + "谱面列表已读取");
                return chartInfos;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + $"{ex.Message}");
                return chartInfos;
            }
        }
    }
}
