using ChartEditor.Models;
using ChartEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ChartEditor.ViewModels
{
    public class ChartEditModel : INotifyPropertyChanged
    {
        private static string logTag = "[ChartEditModel]";
        /// <summary>
        /// 谱面信息
        /// </summary>
        private ChartInfo chartInfo;
        public ChartInfo ChartInfo { get { return chartInfo; } }

        /// <summary>
        /// 轨道，包含所有音符
        /// </summary>
        private List<Track> tracks;
        public List<Track> Tracks { get { return tracks; } }

        private int columnNum = Common.ColumnNum;
        public int ColumnNum { get { return columnNum; } }

        /// <summary>
        /// 每一列的宽度
        /// </summary>
        private double columnWidth = Common.ColumnWidth;
        public double ColumnWidth { get { return columnWidth; } set { columnWidth = value; OnPropertyChanged(nameof(ColumnWidth)); } }

        /// <summary>
        /// 列间隔
        /// </summary>
        private double columnGap = Common.ColumnGap;
        public double ColumnGap { get { return columnGap; } set { columnGap = value; } }

        /// <summary>
        /// 每一行的宽度（一拍为一行）
        /// </summary>
        private double rowWidth = Common.RowWidth;
        public double RowWidth { get { return rowWidth; } set { rowWidth = value; OnPropertyChanged(nameof(RowWidth)); } }

        /// <summary>
        /// 总拍数
        /// </summary>
        public int BeatNum
        {
            get { return (int)Math.Ceiling(this.ChartInfo.ChartMusic.Duration / 60 * this.ChartInfo.ChartMusic.Bpm) + 1; }
        }

        /// <summary>
        /// 一拍的时长（四分音符）
        /// </summary>
        public double BeatTime { get { return 60 / (this.ChartInfo.ChartMusic.Bpm * this.speed); } }

        /// <summary>
        /// 轨道所有列的总尺寸
        /// </summary>
        public double TotalHeight { get { return BeatNum * this.RowWidth; } }
        public double TotalWidth { get { return this.ColumnNum * this.ColumnWidth; } }

        /// <summary>
        /// 歌曲速度/谱面速度
        /// </summary>
        private double speed = 1.0;
        public double Speed { get { return speed; } set { speed = value; OnPropertyChanged(nameof(Speed)); } }

        /// <summary>
        /// 滚动条每秒移动的距离
        /// </summary>
        public double ScrollSpeed { get { return RowWidth / this.BeatTime; } }

        /// <summary>
        /// 歌曲实际结束的位置
        /// </summary>
        public double ActualHeight { get { return ScrollSpeed * this.ChartInfo.ChartMusic.Duration; } }

        /// <summary>
        /// 每拍分割数
        /// </summary>
        private int divide = 4;
        public int Divide { get { return divide; } set { 
                divide = value;
                OnPropertyChanged(nameof(Divide));
                this.currentBeat.Divide = divide;
                OnPropertyChanged(nameof(CurrentBeatStr));
            } }

        /// <summary>
        /// 歌曲当前播放时间
        /// </summary>
        private double currentTime = 0;
        public double CurrentTime { get { return currentTime; } set { currentTime = value; } }
        public string CurrentTimeStr
        {
            get
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
                return string.Format("{0:D2}:{1:D2}.{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            }
        }

        /// <summary>
        /// 歌曲当前节拍
        /// </summary>
        private BeatTime currentBeat = new BeatTime();
        public BeatTime CurrentBeat { get { return currentBeat; } set { currentBeat = value; } }
        public string CurrentBeatStr { get { return currentBeat.ToBeatString(); } }

        /// <summary>
        /// 更新当前节拍和时间
        /// </summary>
        public void UpdateCurrentBeatTime(double canvasOffset, double canvasExtentHeight, double canvasHeight)
        {
            // 判定线到0拍线的距离
            double judgeLineOffset = canvasExtentHeight - canvasOffset - canvasHeight;
            // 拍数
            this.currentBeat.UpdateFromJudgeLineOffset(judgeLineOffset, this.RowWidth);
            // 时间
            this.currentTime = judgeLineOffset / this.RowWidth * this.BeatTime + this.ChartInfo.Delay;
            this.OnPropertyChanged(nameof(CurrentBeatStr));
            this.OnPropertyChanged(nameof(CurrentTimeStr));
        }

        /// <summary>
        /// 重置当前节拍和时间
        /// </summary>
        public void ResetCurrentBeatTime()
        {
            this.currentBeat.Reset();
            this.currentTime = this.chartInfo.Delay;
        }

        /// <summary>
        /// 歌曲总时长
        /// </summary>
        public string MusicTimeString
        {
            get
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(this.chartInfo.ChartMusic.Duration);
                return string.Format("{0:D2}:{1:D2}.{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            }
        }

        /// <summary>
        /// 音符选中的序号
        /// </summary>
        private int noteSelectedIndex = -1;
        public int NoteSelectedIndex { get { return noteSelectedIndex; } set { noteSelectedIndex = value; } }

        /// <summary>
        /// 鼠标选中的元素
        /// </summary>
        public List<Rectangle> SelectedElements {  get; set; }

        /// <summary>
        /// 音乐音量
        /// </summary>
        private float musicVolume = 50;
        public float MusicVolume { get { return musicVolume; } set { musicVolume = value; OnPropertyChanged(nameof(MusicVolume)); } }

        /// <summary>
        /// 音符音量
        /// </summary>
        private float noteVolume = 50;
        public float NoteVolume { get { return noteVolume; } set { noteVolume = value; OnPropertyChanged(nameof(NoteVolume)); } }

        public ChartEditModel(ChartInfo chartInfo)
        {
            this.chartInfo = chartInfo;
            this.tracks = new List<Track>();
            this.currentTime = this.chartInfo.Delay;
        }

        /// <summary>
        /// 判断指定起始位置能否加入Track
        /// </summary>
        public bool AddTrackHeader(BeatTime start, int columnIndex)
        {
            foreach (var item in tracks)
            {
                if (columnIndex == item.ColumnIndex && (start.IsEqualTo(item.StartTime) || start.IsLaterThan(item.StartTime)) && start.IsEarlierThan(item.EndTime))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断指定终点位置能否加入Track
        /// </summary>
        public Track AddTrackFooter(BeatTime start, int startColumnIndex, BeatTime end, int endColumnIndex)
        {
            if (startColumnIndex != endColumnIndex || end.IsEarlierThan(start)) return null;
            foreach (var item in tracks)
            {
                if (endColumnIndex == item.ColumnIndex && end.IsLaterThan(item.StartTime) && start.IsEarlierThan(item.StartTime))
                {
                    return null;
                }
            }
            // 可以添加轨道
            Track track = new Track(start, end, endColumnIndex);
            this.tracks.Add(track);
            Console.WriteLine(logTag + "成功放置一个轨道");
            return track;
        }

        /// <summary>
        /// 添加TapNote/FLickNote/CatchNote
        /// </summary>
        public Note AddNote(BeatTime beatTime, int columnIndex, NoteType noteType)
        {
            // Note必须要添加到一个Track中
            foreach (var item in tracks)
            {
                if (item.IsInTrack(beatTime, columnIndex))
                {
                    Note newNote = item.AddNote(beatTime, noteType);
                    if (newNote != null)
                    {
                        return newNote;
                    }
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断指定起始位置能否加入HoldNote
        /// </summary>
        public bool AddHoldNoteHeader(BeatTime start, int columnIndex)
        {
            foreach (var item in tracks)
            {
                if (item.IsInTrack(start, columnIndex))
                {
                    if (item.AddHoldNoteHeader(start))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 添加HoldNoteFooter
        /// </summary>
        public HoldNote AddHoldNoteFooter(BeatTime startTime, int startColumnIndex, BeatTime endTime, int endColumnIndex)
        {
            if (startColumnIndex != endColumnIndex || !startTime.IsEarlierThan(endTime)) return null;
            // Note必须要添加到一个Track中
            foreach (var item in tracks)
            {
                if (item.IsInTrack(endTime, endColumnIndex))
                {
                    HoldNote newHoldNote = item.AddHoldNoteFooter(startTime, endTime);
                    if (newHoldNote != null)
                    {
                        return newHoldNote;
                    }
                    break;
                }
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
