using ChartEditor.Models;
using ChartEditor.Utils.Drawers;
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
                chartJObject.Add("Data", new JArray());
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
                        string json = File.ReadAllText(chartFilePath);
                        var jObject = JObject.Parse(json);
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
                chartInfo.UpdateAtNow();
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
                    throw new Exception("Json中ChartInfo不存在");
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
        public async static Task<bool> SaveChart(ChartEditModel chartEditModel)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (chartEditModel == null) throw new Exception("ChartEditModel对象为空");
                    chartEditModel.ChartInfo.UpdateAtNow();
                    string chartFilePath = Path.Combine(chartEditModel.ChartInfo.FolderPath, Common.ChartFileName);
                    // 写入文件
                    string json = File.ReadAllText(chartFilePath);
                    var jObject = JObject.Parse(json);
                    if (jObject["ChartInfo"] != null)
                    {
                        jObject["ChartInfo"] = chartEditModel.ChartInfo.ToJson();
                    }
                    else
                    {
                        throw new Exception("Json中ChartInfo不存在");
                    }
                    if (jObject["Data"] != null)
                    {
                        jObject["Data"] = chartEditModel.GetTracksJson();
                    }
                    else
                    {
                        throw new Exception("Json中Data不存在");
                    }
                    File.WriteAllText(chartFilePath, jObject.ToString(Formatting.Indented));
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取谱面数据对象
        /// </summary>
        public static JArray GetChartData(ChartEditModel chartEditModel)
        {
            try
            {
                if (chartEditModel == null) throw new Exception("ChartEditModel对象为空");
                string chartFilePath = Path.Combine(chartEditModel.ChartInfo.FolderPath, Common.ChartFileName);
                string json = File.ReadAllText(chartFilePath);
                var jObject = JObject.Parse(json);
                if (jObject["Data"] != null)
                {
                    return (JArray)jObject["Data"];
                }
                else
                {
                    throw new Exception("Json中Data不存在");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 加载谱面数据
        /// </summary>
        public async static Task<bool> LoadChartData(ChartEditModel chartEditModel)
        {
            try
            {
                await Task.Run(() =>
                {
                    JArray chartData = GetChartData(chartEditModel) ?? throw new Exception("谱面数据读取失败");
                    int index = 0;
                    foreach (var columnJToken in chartData)
                    {
                        JArray tracks = (JArray)columnJToken["Tracks"];
                        if (tracks == null)
                        {
                            index++;
                            continue;
                        }
                        SkipList<BeatTime, Track> trackList = chartEditModel.TrackSkipLists[index];
                        // 遍历Track数组
                        foreach (var trackJToken in tracks)
                        {
                            int trackId = trackJToken.Value<int?>("Id") ?? throw new Exception("Track的Id解析失败");
                            BeatTime trackStartTime = BeatTime.FromBeatString(trackJToken.Value<string>("StartTime")) ?? throw new Exception("Track的StartTime解析失败");
                            BeatTime trackEndTime = BeatTime.FromBeatString(trackJToken.Value<string>("EndTime")) ?? throw new Exception("Track的EndTime解析失败");
                            Track track = new Track(trackStartTime, trackEndTime, index, trackId);
                            // 插入列表
                            trackList.Insert(trackStartTime, track);
                            // 遍历Note数组
                            JArray notes = (JArray)trackJToken["Notes"];
                            if (notes != null)
                            {
                                foreach (var noteJToken in notes)
                                {
                                    int noteId = noteJToken.Value<int?>("Id") ?? throw new Exception("Note的Id解析失败");
                                    BeatTime noteTime = BeatTime.FromBeatString(noteJToken.Value<string>("Time")) ?? throw new Exception("Note的Time解析失败");
                                    var typeIndex = noteJToken.Value<int?>("Type");
                                    if (typeIndex == null || !Enum.IsDefined(typeof(NoteType), typeIndex.Value)) throw new Exception("Note的Type解析失败");
                                    Note note = null;
                                    switch ((NoteType)typeIndex)
                                    {
                                        case NoteType.Tap: note = new TapNote(noteTime, track, noteId); break;
                                        case NoteType.Flick: note = new FlickNote(noteTime, track, noteId); break;
                                        case NoteType.Catch: note = new CatchNote(noteTime, track, noteId); break;
                                    }
                                    // 插入列表
                                    track.NoteSkipList.Insert(noteTime, note);
                                }
                            }
                            
                            // 遍历HoldNote数组
                            JArray holdNotes = (JArray)trackJToken["HoldNotes"];
                            if (holdNotes != null)
                            {
                                foreach (var holdNoteJToken in holdNotes)
                                {
                                    int holdNoteId = holdNoteJToken.Value<int?>("Id") ?? throw new Exception("HoldNote的Id解析失败");
                                    BeatTime holdNoteTime = BeatTime.FromBeatString(holdNoteJToken.Value<string>("Time")) ?? throw new Exception("HoldNote的Time解析失败");
                                    BeatTime holdNoteEndTime = BeatTime.FromBeatString(holdNoteJToken.Value<string>("EndTime")) ?? throw new Exception("HoldNote的EndTime解析失败");
                                    HoldNote holdNote = new HoldNote(holdNoteTime, holdNoteEndTime, track, holdNoteId);
                                    // 插入列表
                                    track.HoldNoteSkipList.Insert(holdNoteTime, holdNote);
                                }
                            }
                        }
                        // 增加列序号
                        index++;
                    }
                });
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(logTag + e.Message);
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
                if (chartEditModel == null) throw new Exception("ChartEditModel对象为空");
                string workspaceFilePath = Path.Combine(chartEditModel.ChartInfo.FolderPath, Common.WorkspaceName);
                File.WriteAllText(workspaceFilePath, chartEditModel.GetWorkspaceJson().ToString(Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 加载工作区文件
        /// </summary>
        public static bool LoadWorkPlaceData(ChartEditModel chartEditModel)
        {
            try
            {
                if (chartEditModel == null) throw new Exception("ChartEditModel对象为空");
                string workspaceFilePath = Path.Combine(chartEditModel.ChartInfo.FolderPath, Common.WorkspaceName);
                if (File.Exists(workspaceFilePath))
                {
                    string json = File.ReadAllText(workspaceFilePath);
                    var jObject = JObject.Parse(json);
                    chartEditModel.ColumnWidth = jObject.Value<double?>("ColumnWidth") ?? Common.ColumnWidth;
                    chartEditModel.RowWidth = jObject.Value<double?>("RowWidth") ?? Common.RowWidth;
                    chartEditModel.Divide = jObject.Value<int?>("Divide") ?? 4;
                    chartEditModel.Speed = jObject.Value<double?>("Speed") ?? 1.0;
                    chartEditModel.CurrentBeat = BeatTime.FromBeatString(jObject.Value<string>("CurrentBeat")) ?? new BeatTime();
                    chartEditModel.GlobalVolume = jObject.Value<float?>("GlobalVolume") ?? 100;
                    chartEditModel.MusicVolume = jObject.Value<float?>("MusicVolume") ?? 50;
                    chartEditModel.NoteVolume = jObject.Value<float?>("NoteVolume") ?? 50;
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
