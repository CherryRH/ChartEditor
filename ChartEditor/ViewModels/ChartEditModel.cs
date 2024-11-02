using ChartEditor.Models;
using ChartEditor.Utils;
using ChartEditor.Utils.Cache;
using ChartEditor.Windows;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChartEditor.ViewModels
{
    public class ChartEditModel : INotifyPropertyChanged
    {
        private static string logTag = "[ChartEditModel]";

        /// <summary>
        /// 谱面编辑窗口实例
        /// </summary>
        public ChartWindow ChartWindow { get; private set; }

        /// <summary>
        /// 谱面信息
        /// </summary>
        public ChartInfo ChartInfo { get; }

        public BitmapImage Cover
        {
            get
            {
                return CoverImageCache.Instance.GetImage(this.ChartInfo.ChartMusic.CoverPath);
            }
        }

        /// <summary>
        /// 轨道，包含所有音符
        /// </summary>
        private List<Track> tracks = new List<Track>();
        public List<Track> Tracks { get { return tracks; } }

        // 选中的Track（只允许选择一个）
        private Track pickedTrack = null;
        public Track PickedTrack { get { return pickedTrack; } set { pickedTrack = value; } }
        // 选中的Note（允许选择多个）
        private List<Note> pickedNotes = new List<Note>();
        public List<Note> PickedNotes
        {
            get { return pickedNotes; }
            set
            {
                pickedNotes = value;
                // 通知属性面板更新

            }
        }

        public int ColumnNum { get { return this.ChartInfo.ColumnNum; } }
        /// <summary>
        /// 对元素进行实时统计
        /// </summary>
        private int trackNum = 0;
        public int TrackNum { get { return trackNum; } set { trackNum = value; OnPropertyChanged("TrackNum"); } }

        private int tapNoteNum = 0;
        public int TapNoteNum { get { return tapNoteNum; } set { tapNoteNum = value; OnPropertyChanged("TapNoteNum"); OnPropertyChanged("NoteNum"); } }

        private int flickNoteNum = 0;
        public int FlickNoteNum { get { return flickNoteNum; } set { flickNoteNum = value; OnPropertyChanged("FlickNoteNum"); OnPropertyChanged("NoteNum"); } }

        private int holdNoteNum = 0;
        public int HoldNoteNum { get { return holdNoteNum; } set { holdNoteNum = value; OnPropertyChanged("HoldNoteNum"); OnPropertyChanged("NoteNum"); } }

        private int catchNoteNum = 0;
        public int CatchNoteNum { get { return catchNoteNum; } set { catchNoteNum = value; OnPropertyChanged("CatchNoteNum"); OnPropertyChanged("NoteNum"); } }
        public int NoteNum
        {
            get
            {
                return TapNoteNum + FlickNoteNum + HoldNoteNum + CatchNoteNum;
            }
        }

        // 当前轨道id
        private int currentTrackId = 0;
        public int CurrentTrackId { get { return currentTrackId; } }
        public int GetNextTrackId() => currentTrackId++;
        // 当前音符id
        private int currentNoteId = 0;
        public int CurrentNoteId { get { return currentNoteId; } }
        public int GetNextNoteId() => currentNoteId++;

        /// <summary>
        /// 每一列的宽度
        /// </summary>
        private double columnWidth = Common.ColumnWidth;
        public double ColumnWidth { get { return columnWidth; } set { columnWidth = value; OnPropertyChanged(nameof(ColumnWidth)); } }

        /// <summary>
        /// 每一行的宽度（一拍为一行）
        /// </summary>
        private double rowWidth = Common.RowWidth;
        public double RowWidth { get { return rowWidth; } set { rowWidth = value; OnPropertyChanged(nameof(RowWidth)); } }
        public double DivideWidth { get { return rowWidth / divide; } }

        /// <summary>
        /// 谱面实际Bpm（后续改为Bpm表）
        /// </summary>
        private double bpm;
        public double Bpm { get { return bpm; } set { bpm = value; } }

        /// <summary>
        /// 总拍数
        /// </summary>
        public int BeatNum
        {
            get { return (int)Math.Ceiling(this.ChartInfo.ChartMusic.Duration / 60 * this.Bpm) + 1; }
        }

        /// <summary>
        /// 一拍的时长（四分音符）
        /// </summary>
        public double BeatTime { get { return 60 / (this.Bpm * this.speed); } }

        /// <summary>
        /// 轨道所有列的总尺寸
        /// </summary>
        public double TotalHeight { get { return BeatNum * this.RowWidth; } }
        public double TotalWidth { get { return this.ColumnNum * this.ColumnWidth; } }

        /// <summary>
        /// 曲目速度/谱面速度
        /// </summary>
        private double speed = 1.0;
        public double Speed { get { return speed; } set { speed = value; OnPropertyChanged(nameof(Speed)); } }

        /// <summary>
        /// 滚动条每秒移动的距离
        /// </summary>
        public double ScrollSpeed { get { return RowWidth / this.BeatTime; } }

        /// <summary>
        /// 曲目实际结束的位置
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
        /// 曲目当前播放时间
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
        /// 曲目当前节拍
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
            this.currentTime = this.ChartInfo.Delay;
        }

        /// <summary>
        /// 曲目总时长
        /// </summary>
        public string MusicTimeString
        {
            get
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(this.ChartInfo.ChartMusic.Duration);
                return string.Format("{0:D2}:{1:D2}.{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
            }
        }

        /// <summary>
        /// 音符选中的序号
        /// </summary>
        private int noteSelectedIndex = -1;
        public int NoteSelectedIndex { get { return noteSelectedIndex; } set { noteSelectedIndex = value; } }

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

        public ChartEditModel(ChartInfo chartInfo, ChartWindow chartWindow)
        {
            this.ChartWindow = chartWindow;
            this.ChartInfo = chartInfo;
            this.currentTime = this.ChartInfo.Delay;
            this.bpm = this.ChartInfo.ChartMusic.Bpm;
        }

        /// <summary>
        /// 判断指定起始位置能否加入Track
        /// </summary>
        public bool AddTrackHeader(BeatTime start, int columnIndex)
        {
            foreach (Track track in tracks)
            {
                if (columnIndex != track.ColumnIndex) continue;
                if (!start.IsEarlierThan(track.StartTime) && !start.IsLaterThan(track.EndTime))
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
            foreach (Track item in tracks)
            {
                if (endColumnIndex != item.ColumnIndex) continue;
                if (!end.IsEarlierThan(item.StartTime) && start.IsEarlierThan(item.StartTime))
                {
                    return null;
                }
            }
            // 可以添加轨道
            Track track = new Track(start, end, endColumnIndex, this.GetNextTrackId());
            this.tracks.Add(track);
            this.TrackNum++;
            return track;
        }

        /// <summary>
        /// 添加TapNote/FlickNote/CatchNote
        /// </summary>
        public Note AddNote(BeatTime beatTime, int columnIndex, NoteType noteType)
        {
            // Note必须要添加到一个Track中
            foreach (Track track in tracks)
            {
                if (track.IsInTrack(beatTime, columnIndex))
                {
                    Note newNote = track.AddNote(beatTime, noteType, this.GetNextNoteId());
                    if (newNote != null)
                    {
                        // 添加成功
                        switch (noteType)
                        {
                            case NoteType.Tap: this.TapNoteNum++; break;
                            case NoteType.Flick: this.FlickNoteNum++; break;
                            case NoteType.Catch: this.CatchNoteNum++; break;
                        }
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
                    HoldNote newHoldNote = item.AddHoldNoteFooter(startTime, endTime, this.GetNextNoteId());
                    if (newHoldNote != null)
                    {
                        // 添加成功
                        this.HoldNoteNum++;
                        return newHoldNote;
                    }
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// 删除Note
        /// </summary>
        public void DeleteNote(Note note)
        {
            if (note == null) return;
            // 更新统计数据
            switch (note.Type)
            {
                case NoteType.Tap: this.TapNoteNum--; break;
                case NoteType.Hold: this.HoldNoteNum--; break;
                case NoteType.Flick: this.FlickNoteNum--; break;
                case NoteType.Catch: this.CatchNoteNum--; break;
            }
            note.Track.DeleteNote(note);
        }

        /// <summary>
        /// 尝试改变Track的起始时间
        /// </summary>
        public bool TryChangeTrackStartTime(Track track, BeatTime beatTime)
        {
            if (beatTime.IsLaterThan(track.EndTime)) return false;
            foreach (Note note in track.Notes)
            {
                if (beatTime.IsLaterThan(note.Time)) return false;
            }
            foreach (Track item in this.tracks)
            {
                if (track.ColumnIndex != item.ColumnIndex || item == track) continue;
                if (item.EndTime.IsEarlierThan(track.StartTime) && !beatTime.IsLaterThan(item.EndTime)) return false;
            }
            track.StartTime = beatTime;
            return true;
        }

        /// <summary>
        /// 尝试改变Track的结束时间
        /// </summary>
        public bool TryChangeTrackEndTime(Track track, BeatTime beatTime)
        {
            if (beatTime.IsEarlierThan(track.StartTime)) return false;
            foreach (Note note in track.Notes)
            {
                if (note is HoldNote holdNote)
                {
                    if (beatTime.IsEarlierThan(holdNote.EndTime)) return false;
                }
                else
                {
                    if (beatTime.IsEarlierThan(note.Time)) return false;
                }
            }
            foreach (Track item in this.tracks)
            {
                if (track.ColumnIndex != item.ColumnIndex || item == track) continue;
                if (item.StartTime.IsLaterThan(track.EndTime) && !beatTime.IsEarlierThan(item.StartTime)) return false;
            }
            track.EndTime = beatTime;
            return true;
        }

        /// <summary>
        /// 尝试改变HoldNote的起始时间
        /// </summary>
        public bool TryChangeHoldNoteStartTime(HoldNote holdNote, BeatTime beatTime)
        {
            if (!beatTime.IsEarlierThan(holdNote.EndTime) || beatTime.IsEarlierThan(holdNote.Track.StartTime)) return false;
            foreach (Note note in holdNote.Track.Notes)
            {
                if (holdNote == note) continue;
                if (note is HoldNote item)
                {
                    if (beatTime.IsEarlierThan(item.EndTime) && !holdNote.Time.IsEarlierThan(item.EndTime)) return false;
                }
            }
            holdNote.Time = beatTime;
            return true;
        }

        /// <summary>
        /// 尝试改变HoldNote的结束时间
        /// </summary>
        public bool TryChangeHoldNoteEndTime(HoldNote holdNote, BeatTime beatTime)
        {
            if (!beatTime.IsLaterThan(holdNote.Time) || beatTime.IsLaterThan(holdNote.Track.EndTime)) return false;
            foreach (Note note in holdNote.Track.Notes)
            {
                if (holdNote == note) continue;
                if (note is HoldNote item)
                {
                    if (beatTime.IsLaterThan(item.Time) && !holdNote.EndTime.IsLaterThan(item.Time)) return false;
                }
            }
            holdNote.EndTime = beatTime;
            return true;
        }

        /// <summary>
        /// 尝试移动选中的Note
        /// </summary>
        public bool TryMovePickedNote(BeatTime startBeatTime, BeatTime beatTime)
        {
            List<BeatTime> movedTimes = new List<BeatTime>();
            // 拍数差值
            BeatTime diffBeatTime = beatTime.Difference(startBeatTime);
            foreach (Note note in this.pickedNotes)
            {
                // 计算新拍数
                BeatTime newTime = note.Time.Sum(diffBeatTime);
                movedTimes.Add(newTime);
                // 超出Track范围
                if (!note.Track.ContainsBeatTime(newTime)) return false;
                // 与其他note冲突
                if (note is HoldNote holdNote)
                {
                    BeatTime newEndTime = holdNote.EndTime.Sum(diffBeatTime);
                    // HoldNote末端超出Track范围
                    if (!note.Track.ContainsBeatTime(newEndTime)) return false;
                    foreach (Note trackNote in note.Track.Notes)
                    {
                        if (this.pickedNotes.Contains(trackNote)) continue;
                        if (trackNote is HoldNote trackHoldNote)
                        {
                            if (newTime.IsEarlierThan(trackHoldNote.EndTime) && !newTime.IsEarlierThan(trackHoldNote.Time)) return false;
                            if (newEndTime.IsLaterThan(trackHoldNote.Time) && !newEndTime.IsLaterThan(trackHoldNote.EndTime)) return false;
                            if (newTime.IsEarlierThan(trackHoldNote.Time) && newEndTime.IsLaterThan(trackHoldNote.EndTime)) return false;
                        }
                    }
                }
                else
                {
                    foreach (Note trackNote in note.Track.Notes)
                    {
                        if (trackNote.Type == NoteType.Hold || this.pickedNotes.Contains(trackNote)) continue;
                        if (trackNote.Time.IsEqualTo(newTime)) return false;
                    }
                }
            }
            // 更新时间
            for (int i = 0; i < this.pickedNotes.Count; i++)
            {
                Note tmpNote = this.pickedNotes[i];
                BeatTime tmpBeatTime = movedTimes[i];
                if (tmpNote is HoldNote tmpHoldNote)
                {
                    tmpHoldNote.MoveHoldNoteToBeatTime(tmpBeatTime);
                }
                else tmpNote.MoveNoteToBeatTime(tmpBeatTime);
            }
            return true;
        }

        /// <summary>
        /// 尝试移动选中的Track
        /// </summary>
        public bool TryMovePickedTrack (BeatTime beatTime, int columnIndex)
        {
            // 拍数差值
            BeatTime diffBeatTime = beatTime.Difference(this.pickedTrack.StartTime);
            // 不能超出网格范围
            if (beatTime.Beat < 0) return false;
            BeatTime movedEndTime = this.pickedTrack.EndTime.Sum(diffBeatTime);
            if (movedEndTime.Beat >= this.BeatNum) return false;
            // 不能与其他轨道冲突
            foreach (Track track in this.tracks)
            {
                if (track == this.pickedTrack || track.ColumnIndex != columnIndex) continue;
                if (!beatTime.IsLaterThan(track.EndTime) && !beatTime.IsEarlierThan(track.StartTime)) return false;
                if (!movedEndTime.IsLaterThan(track.EndTime) && !movedEndTime.IsEarlierThan(track.StartTime)) return false;
                if (beatTime.IsEarlierThan(track.StartTime) && movedEndTime.IsLaterThan(track.EndTime)) return false;
            }
            // 更新时间和列号
            this.pickedTrack.MoveTrackToBeatTime(beatTime);
            this.pickedTrack.ColumnIndex = columnIndex;
            return true;
        }

        /// <summary>
        /// 计算Canvas坐标的列序号
        /// </summary>
        public int GetColumnIndexFromPoint(Point? point)
        {
            return (int)((point.Value.X) / this.columnWidth);
        }

        /// <summary>
        /// 计算Canvas坐标的节拍
        /// </summary>
        public BeatTime GetBeatTimeFromPoint(Point? point, double canvasHeight)
        {
            BeatTime newBeatTime = new BeatTime(this.divide);
            double judgeLineOffset = canvasHeight - point.Value.Y;
            if (judgeLineOffset < 0) judgeLineOffset = 0;
            if (judgeLineOffset >= canvasHeight) judgeLineOffset = canvasHeight - 0.1;
            newBeatTime.UpdateFromJudgeLineOffset(judgeLineOffset, this.rowWidth);
            return newBeatTime;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
