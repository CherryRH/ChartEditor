using ChartEditor.Models;
using ChartEditor.Utils;
using ChartEditor.Utils.Cache;
using ChartEditor.Utils.ChartUtils;
using ChartEditor.Windows;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
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
        public ChartWindow ChartWindow { get; set; }

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
        /// 每一列的轨道
        /// </summary>
        private List<SkipList<BeatTime, Track>> trackSkipLists = new List<SkipList<BeatTime, Track>>();
        public List<SkipList<BeatTime, Track>> TrackSkipLists { get { return trackSkipLists; } }

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
                this.ChartInfo.Volume = TapNoteNum + FlickNoteNum + HoldNoteNum + CatchNoteNum;
                return this.ChartInfo.Volume;
            }
        }

        // 当前轨道id
        public int CurrentTrackId { get { return this.ChartInfo.TrackMaxId; } }
        public int GetNextTrackId() => this.ChartInfo.TrackMaxId++;
        // 当前音符id
        public int CurrentNoteId { get { return this.ChartInfo.NoteMaxId; } }
        public int GetNextNoteId() => this.ChartInfo.NoteMaxId++;

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
        /// Bpm列表，实现变速
        /// </summary>
        private BpmList bpmList;
        public BpmList BpmList { get { return bpmList; } }

        /// <summary>
        /// 谱面实际Bpm（后续改为Bpm表）
        /// </summary>
        public double Bpm { get { return bpmList.Bpm; } }

        /// <summary>
        /// 总拍数
        /// </summary>
        public int BeatNum { get { return bpmList.GetAllBeatNum(this.ChartInfo.ChartMusic.Duration) + 1; } }

        /// <summary>
        /// 一拍的时长（四分音符）
        /// </summary>
        public double BeatTime { get { return bpmList.GetBeatDuration(); } }

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
        /// 总音量
        /// </summary>
        private float globalVolume = 100;
        public float GlobalVolume { get { return globalVolume; } set { globalVolume = value; OnPropertyChanged(nameof(GlobalVolume)); } }

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
            this.ChartInfo = chartInfo;
            this.currentTime = this.ChartInfo.Delay;
            this.bpmList = new BpmList(this.ChartInfo.ChartMusic.Bpm);
            for (int i = 0; i < this.ColumnNum; i++)
            {
                this.trackSkipLists.Add(new SkipList<BeatTime, Track>(16, (BeatTime x, BeatTime y) => x.CompareTo(y)));
            }
        }

        /// <summary>
        /// 判断指定起始位置能否加入Track
        /// </summary>
        public bool AddTrackHeader(BeatTime start, int columnIndex)
        {
            SkipList<BeatTime, Track> trackList = this.trackSkipLists[columnIndex];
            SkipListNode<BeatTime, Track> preNode = trackList.GetPreNode(start);
            if (preNode != trackList.Head && !start.IsLaterThan(preNode.Value.EndBeatTime)) return false;
            SkipListNode<BeatTime, Track> nextNode = preNode.Next[0];
            if (nextNode != null && !start.IsEarlierThan(nextNode.Value.StartBeatTime)) return false;
            return true;
        }

        /// <summary>
        /// 判断指定终点位置能否加入Track
        /// </summary>
        public Track AddTrackFooter(BeatTime start, int startColumnIndex, BeatTime end, int endColumnIndex)
        {
            if (startColumnIndex != endColumnIndex || end.IsEarlierThan(start)) return null;
            SkipList<BeatTime, Track> trackList = this.trackSkipLists[startColumnIndex];
            SkipListNode<BeatTime, Track> preNode = trackList.GetPreNode(start);
            SkipListNode<BeatTime, Track> nextNode = preNode.Next[0];
            if (nextNode != null && !end.IsEarlierThan(nextNode.Value.StartBeatTime)) return null;
            // 可以添加轨道
            Track track = new Track(start, end, endColumnIndex, this.GetNextTrackId());
            // 更新时间
            track.UpdateTime(this.bpmList);
            trackList.Insert(start, track);
            this.TrackNum++;
            return track;
        }

        /// <summary>
        /// 添加TapNote/FlickNote/CatchNote
        /// </summary>
        public Note AddNote(BeatTime beatTime, int columnIndex, NoteType noteType)
        {
            // Note必须要添加到一个Track中
            SkipList<BeatTime, Track> trackList = this.trackSkipLists[columnIndex];
            // 找到时间所处的track
            Track targetTrack = null;
            SkipListNode<BeatTime, Track> preNode = trackList.GetPreNode(beatTime);
            if (preNode != trackList.Head && preNode.Value.ContainsBeatTime(beatTime)) targetTrack = preNode.Value;
            else
            {
                SkipListNode<BeatTime, Track> nextNode = preNode.Next[0];
                if (nextNode != null && nextNode.Value.ContainsBeatTime(beatTime)) targetTrack = nextNode.Value;
            }
            if (targetTrack == null) return null;

            Note newNote = targetTrack.AddNote(beatTime, noteType, this.GetNextNoteId());
            if (newNote != null)
            {
                // 添加成功
                switch (noteType)
                {
                    case NoteType.Tap: this.TapNoteNum++; break;
                    case NoteType.Flick: this.FlickNoteNum++; break;
                    case NoteType.Catch: this.CatchNoteNum++; break;
                }
                // 更新时间
                newNote.UpdateTime(this.bpmList);
                return newNote;
            }
            return null;
        }

        /// <summary>
        /// 判断指定起始位置能否加入HoldNote
        /// </summary>
        public bool AddHoldNoteHeader(BeatTime start, int columnIndex)
        {
            // Note必须要添加到一个Track中
            SkipList<BeatTime, Track> trackList = this.trackSkipLists[columnIndex];
            // 找到时间所处的track
            Track targetTrack = null;
            SkipListNode<BeatTime, Track> preNode = trackList.GetPreNode(start);
            if (preNode != trackList.Head && preNode.Value.ContainsBeatTime(start)) targetTrack = preNode.Value;
            else
            {
                SkipListNode<BeatTime, Track> nextNode = preNode.Next[0];
                if (nextNode != null && nextNode.Value.ContainsBeatTime(start)) targetTrack = nextNode.Value;
            }
            if (targetTrack == null) return false;
            if (targetTrack.EndBeatTime.IsEqualTo(start)) return false;

            return targetTrack.AddHoldNoteHeader(start);
        }

        /// <summary>
        /// 添加HoldNoteFooter
        /// </summary>
        public HoldNote AddHoldNoteFooter(BeatTime startTime, int startColumnIndex, BeatTime endTime, int endColumnIndex)
        {
            if (startColumnIndex != endColumnIndex || !startTime.IsEarlierThan(endTime)) return null;
            SkipList<BeatTime, Track> trackList = this.trackSkipLists[startColumnIndex];
            // 找到时间所处的track
            Track targetTrack = null;
            SkipListNode<BeatTime, Track> preNode = trackList.GetPreNode(startTime);
            if (preNode != trackList.Head && preNode.Value.ContainsBeatTime(startTime)) targetTrack = preNode.Value;
            else
            {
                SkipListNode<BeatTime, Track> nextNode = preNode.Next[0];
                if (nextNode != null && nextNode.Value.ContainsBeatTime(startTime)) targetTrack = nextNode.Value;
            }
            if (targetTrack == null) return null;

            HoldNote newHoldNote = targetTrack.AddHoldNoteFooter(startTime, endTime, this.GetNextNoteId());
            if (newHoldNote != null)
            {
                // 添加成功
                this.HoldNoteNum++;
                // 更新时间
                newHoldNote.UpdateTime(this.bpmList);
                return newHoldNote;
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
        /// 删除Track
        /// </summary>
        public void DeleteTrack(Track track)
        {
            if (track == null) return;
            
            if (this.trackSkipLists[track.ColumnIndex].Delete(track.StartBeatTime))
            {
                // 更新统计数据
                this.TrackNum--;
                var currentNoteNode = track.NoteSkipList.FirstNode;
                while (currentNoteNode != null)
                {
                    switch (currentNoteNode.Value.Type)
                    {
                        case NoteType.Tap: this.TapNoteNum--; break;
                        case NoteType.Flick: this.FlickNoteNum--; break;
                        case NoteType.Catch: this.CatchNoteNum--; break;
                    }
                    currentNoteNode = currentNoteNode.Next[0];
                }
                this.HoldNoteNum -= track.HoldNoteSkipList.Count;
            }
        }

        /// <summary>
        /// 开始改变选中的Track的时间时
        /// </summary>
        public void ChangePickedTrackTimeBegin()
        {
            this.trackSkipLists[this.pickedTrack.ColumnIndex].Delete(this.pickedTrack.StartBeatTime);
        }

        /// <summary>
        /// 改变选中的Track的时间结束时
        /// </summary>
        public void ChangePickedTrackTimeOver()
        {
            this.trackSkipLists[this.pickedTrack.ColumnIndex].Insert(this.pickedTrack.StartBeatTime, this.pickedTrack);
            // 更新时间
            this.pickedTrack.UpdateTime(this.bpmList);
        }

        /// <summary>
        /// 尝试改变Track的起始时间
        /// </summary>
        public bool TryChangeTrackStartTime(Track track, BeatTime beatTime)
        {
            if (beatTime.IsLaterThan(track.EndBeatTime) || beatTime.Beat < 0) return false;
            if (track.NoteSkipList.FirstNode != null && track.NoteSkipList.FirstNode.Key.IsEarlierThan(beatTime)) return false;
            if (track.HoldNoteSkipList.FirstNode != null && track.HoldNoteSkipList.FirstNode.Key.IsEarlierThan(beatTime)) return false;
            // 不能超过前一个Track的末端
            SkipList<BeatTime, Track> skipList = this.trackSkipLists[track.ColumnIndex];
            SkipListNode<BeatTime, Track> preNode = skipList.GetPreNode(track.StartBeatTime);
            if (preNode != skipList.Head && !beatTime.IsLaterThan(preNode.Value.EndBeatTime)) return false;
            SkipListNode<BeatTime, Track> thisNode = preNode.Next[0];
            if (thisNode.Value != track) return false;
            track.StartBeatTime = beatTime;
            // 更新时间
            track.StartTime = this.bpmList.GetBeatTimeMs(track.StartBeatTime);

            thisNode.Key = beatTime;
            return true;
        }

        /// <summary>
        /// 尝试改变Track的结束时间
        /// </summary>
        public bool TryChangeTrackEndTime(Track track, BeatTime beatTime)
        {
            if (beatTime.IsEarlierThan(track.StartBeatTime) || beatTime.Beat >= this.BeatNum) return false;
            if (track.NoteSkipList.LastNode != track.NoteSkipList.Head && track.NoteSkipList.LastNode.Key.IsLaterThan(beatTime)) return false;
            if (track.HoldNoteSkipList.LastNode != track.HoldNoteSkipList.Head && track.HoldNoteSkipList.LastNode.Value.EndBeatTime.IsLaterThan(beatTime)) return false;
            // 不能超过下一个Track
            if (this.TrackSkipLists[track.ColumnIndex].TryGetNode(track.StartBeatTime, out SkipListNode<BeatTime, Track> node) && node.Value == track)
            {
                // 下一个节点
                SkipListNode<BeatTime, Track> nextNode = node.Next[0];
                if (nextNode != null && !beatTime.IsEarlierThan(nextNode.Key)) return false;
            }
            else return false;
            track.EndBeatTime = beatTime;
            // 更新时间
            track.EndTime = this.bpmList.GetBeatTimeMs(track.EndBeatTime);
            return true;
        }

        /// <summary>
        /// 开始改变选中的Note的时间时
        /// </summary>
        public void ChangePickedNoteTimeBegin()
        {
            foreach (Note note in this.pickedNotes)
            {
                if (note is HoldNote holdNote) holdNote.Track.HoldNoteSkipList.Delete(holdNote.StartBeatTime);
                else note.Track.NoteSkipList.Delete(note.StartBeatTime);
            }
        }

        /// <summary>
        /// 改变选中的Note的时间结束时
        /// </summary>
        public void ChangePickedNoteTimeOver()
        {
            foreach (Note note in this.pickedNotes)
            {
                if (note is HoldNote holdNote)
                {
                    holdNote.Track.HoldNoteSkipList.Insert(holdNote.StartBeatTime, holdNote);
                    holdNote.UpdateTime(this.bpmList);
                }
                else
                {
                    note.Track.NoteSkipList.Insert(note.StartBeatTime, note);
                    note.UpdateTime(this.bpmList);
                }
            }
        }

        /// <summary>
        /// 尝试改变HoldNote的起始时间
        /// </summary>
        public bool TryChangeHoldNoteStartTime(HoldNote holdNote, BeatTime beatTime)
        {
            if (!beatTime.IsEarlierThan(holdNote.EndBeatTime) || beatTime.IsEarlierThan(holdNote.Track.StartBeatTime)) return false;
            // 不能超过前一个HoldNote的末端
            SkipListNode<BeatTime, HoldNote> preNode = holdNote.Track.HoldNoteSkipList.GetPreNode(holdNote.StartBeatTime);
            if (preNode != holdNote.Track.HoldNoteSkipList.Head && beatTime.IsEarlierThan(preNode.Value.EndBeatTime)) return false;
            SkipListNode<BeatTime, HoldNote> thisNode = preNode.Next[0];
            if (thisNode.Value != holdNote) return false;
            holdNote.StartBeatTime = beatTime;
            // 更新时间
            holdNote.StartTime = this.bpmList.GetBeatTimeMs(holdNote.StartBeatTime);

            thisNode.Key = beatTime;
            return true;
        }

        /// <summary>
        /// 尝试改变HoldNote的结束时间
        /// </summary>
        public bool TryChangeHoldNoteEndTime(HoldNote holdNote, BeatTime beatTime)
        {
            if (!beatTime.IsLaterThan(holdNote.StartBeatTime) || beatTime.IsLaterThan(holdNote.Track.EndBeatTime)) return false;
            // 不能超过下一个HoldNote
            if (holdNote.Track.HoldNoteSkipList.TryGetNode(holdNote.StartBeatTime, out SkipListNode<BeatTime, HoldNote> node) && node.Value == holdNote)
            {
                // 下一个节点
                SkipListNode<BeatTime, HoldNote> nextNode = node.Next[0];
                if (nextNode != null && beatTime.IsLaterThan(nextNode.Key)) return false;
            }
            else return false;
            holdNote.EndBeatTime = beatTime;
            // 更新时间
            holdNote.EndTime = this.bpmList.GetBeatTimeMs(holdNote.EndBeatTime);
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
                BeatTime newTime = note.StartBeatTime.Sum(diffBeatTime);
                movedTimes.Add(newTime);
                // 超出Track范围
                if (!note.Track.ContainsBeatTime(newTime)) return false;
                // 与其他note冲突
                if (note is HoldNote holdNote)
                {
                    BeatTime newEndTime = holdNote.EndBeatTime.Sum(diffBeatTime);
                    // HoldNote末端超出Track范围
                    if (!holdNote.Track.ContainsBeatTime(newEndTime)) return false;
                    // 是否在新位置产生冲突
                    SkipListNode<BeatTime, HoldNote> preNode = holdNote.Track.HoldNoteSkipList.GetPreNode(newTime);
                    if (preNode != holdNote.Track.HoldNoteSkipList.Head && preNode.Value.EndBeatTime.IsLaterThan(newTime)) return false;
                    SkipListNode<BeatTime, HoldNote> nextNode = preNode.Next[0];
                    if (nextNode != null && nextNode.Value.StartBeatTime.IsEarlierThan(newEndTime)) return false;
                }
                else
                {
                    // 新位置是否冲突
                    if (note.Track.NoteSkipList.TryGetValue(newTime, out Note value)) return false;
                }
            }
            // 更新时间
            for (int i = 0; i < this.pickedNotes.Count; i++)
            {
                Note tmpNote = this.pickedNotes[i];
                BeatTime tmpBeatTime = movedTimes[i];
                tmpNote.StartBeatTime = tmpBeatTime;
                if (tmpNote is HoldNote tmpHoldNote)
                {
                    tmpHoldNote.EndBeatTime.Add(diffBeatTime);
                }
            }
            return true;
        }

        /// <summary>
        /// 尝试移动选中的Track
        /// </summary>
        public bool TryMovePickedTrack (BeatTime beatTime, int columnIndex)
        {
            // 拍数差值
            BeatTime diffBeatTime = beatTime.Difference(this.pickedTrack.StartBeatTime);
            // 不能超出网格范围
            if (beatTime.Beat < 0) return false;
            BeatTime movedEndTime = this.pickedTrack.EndBeatTime.Sum(diffBeatTime);
            if (movedEndTime.Beat >= this.BeatNum) return false;
            // 不能与其他轨道冲突
            SkipList<BeatTime, Track> skipList = this.trackSkipLists[columnIndex];
            SkipListNode<BeatTime, Track> preNode = skipList.GetPreNode(beatTime);
            if (preNode != skipList.Head && !preNode.Value.EndBeatTime.IsEarlierThan(beatTime)) return false;
            SkipListNode<BeatTime, Track> nextNode = preNode.Next[0];
            if (nextNode != null && !nextNode.Key.IsLaterThan(movedEndTime)) return false;
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

        /// <summary>
        /// 将Tracks转化为Json
        /// </summary>
        public JArray GetTracksJson()
        {
            JArray jArray = new JArray();
            for (int i = 0; i < this.trackSkipLists.Count; i++)
            {
                JObject tracksJObject = new JObject();
                JArray tracksJArray = new JArray();
                SkipListNode<BeatTime, Track> current = this.trackSkipLists[i].FirstNode;
                while (current != null)
                {
                    tracksJArray.Add(current.Value.ToJson());
                    current = current.Next[0];
                }
                tracksJObject.Add("Tracks", tracksJArray);
                jArray.Add(tracksJObject);
            }
            return jArray;
        }

        /// <summary>
        /// 将工作区参数转化为Json
        /// </summary>
        public JObject GetWorkspaceJson()
        {
            return new JObject
            {
                ["ColumnWidth"] = this.columnWidth,
                ["RowWidth"] = this.rowWidth,
                ["Divide"] = this.divide,
                ["Speed"] = this.speed,
                ["CurrentBeat"] = this.currentBeat.ToBeatString(),
                ["GlobalVolume"] = this.globalVolume,
                ["MusicVolume"] = this.musicVolume,
                ["NoteVolume"] = this.noteVolume
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
