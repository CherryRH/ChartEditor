using ChartEditor.Models;
using ChartEditor.ViewModels;
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
        /// 读取指定曲目的谱面列表
        /// </summary>
        public static List<ChartInfo> ReadChartList(ChartMusic chartMusic)
        {
            List<ChartInfo> chartInfos = new List<ChartInfo>();
            try
            {
                if (chartMusic == null) throw new Exception("ChartMusic对象为空");
                if (!Directory.Exists(chartMusic.FolderPath))
                {
                    return chartInfos;
                }
                // 获取所有谱面文件夹路径
                string[] folders = Directory.GetDirectories(chartMusic.FolderPath);

                foreach (var folder in folders)
                {
                    string chartFilePath = Path.Combine(folder, Common.ChartFileName);

                    if (CheckChartFolder(folder))
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
                Console.WriteLine(logTag + ex.Message);
                return chartInfos;
            }
        }

        /// <summary>
        /// 检测谱面文件是否完整
        /// </summary>
        public static bool CheckChartFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return false;
                }
                if (!File.Exists(Path.Combine(folderPath, Common.ChartFileName)))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 保存更新后的谱面信息
        /// </summary>
        public static bool SaveChartInfo(ChartInfo chartInfo)
        {
            try
            {
                if (chartInfo == null) throw new Exception("ChartInfo对象为空");
                string chartFilePath = Path.Combine(chartInfo.FolderPath, Common.ChartFileName);
                // 写入文件
                string json = File.ReadAllText(chartFilePath);
                var jObject = JObject.Parse(json);
                if (jObject["ChartInfo"] != null)
                {
                    jObject["ChartInfo"] = chartInfo.ToJson();
                    File.WriteAllText(chartFilePath, jObject.ToString(Formatting.Indented));
                }
                else
                {
                    throw new Exception("ChartInfo不存在");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 保存谱面
        /// </summary>
        public static bool SaveChart(ChartEditModel chartEditModel)
        {
            try
            {

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 保存工作区
        /// </summary>
        public static bool SaveWorkPlace(ChartEditModel chartEditModel)
        {
            try
            {

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 生成谱面唯一文件夹名称
        /// </summary>
        public static string GenerateNewChartFolderPath(string musicFolderPath, DateTime createdAt)
        {
            string timeStamp = createdAt.ToString("yyyyMMdd_HHmmss");
            string randomSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            string folderName = $"chart_{timeStamp}_{randomSuffix}";
            return Path.Combine(musicFolderPath, folderName);
        }
    }
}
