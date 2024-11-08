using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using ChartEditor.Utils;
using Newtonsoft.Json.Linq;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace ChartEditor.Models
{
    /// <summary>
    /// 轨道
    /// </summary>
    public class Track
    {
        private int id;
        public int Id { get { return id; } }

        /// <summary>
        /// 开始时间
        /// </summary>
        private BeatTime startBeatTime;
        public BeatTime StartBeatTime { get { return startBeatTime; } set { startBeatTime = value; } }

        private int startTime = 0;
        public int StartTime { get { return startTime; } set { startTime = value; } }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        private BeatTime endBeatTime;
        public BeatTime EndBeatTime { get { return endBeatTime; } set { endBeatTime = value; } }

        private int endTime = 0;
        public int EndTime { get { return endTime; } set { endTime = value; } }

        /// <summary>
        /// 非HoldNote列表
        /// </summary>
        private SkipList<BeatTime, Note> noteSkipList = new SkipList<BeatTime, Note>(16, (BeatTime x, BeatTime y) => x.CompareTo(y));
        public SkipList<BeatTime, Note> NoteSkipList { get { return noteSkipList; } }

        /// <summary>
        /// HoldNote列表
        /// </summary>
        private SkipList<BeatTime, HoldNote> holdNoteSkipList = new SkipList<BeatTime, HoldNote>(16, (BeatTime x, BeatTime y) => x.CompareTo(y));
        public SkipList<BeatTime, HoldNote> HoldNoteSkipList { get { return holdNoteSkipList; } }

        /// <summary>
        /// 列序号
        /// </summary>
        private int columnIndex;
        public int ColumnIndex { get { return columnIndex; } set { columnIndex = value; } }

        /// <summary>
        /// 位置关键点序列
        /// </summary>
        private List<PositionKeyPoint> positionKeyPoints;
        public List<PositionKeyPoint> PositionKeyPoints { get { return positionKeyPoints; } set { positionKeyPoints = value; } }

        /// <summary>
        /// 角度关键点序列
        /// </summary>
        private List<AngleKeyPoint> angleKeyPoints;
        public List<AngleKeyPoint > AngleKeyPoints { get {return angleKeyPoints; } set { angleKeyPoints = value; } }

        /// <summary>
        /// 不透明度关键点序列
        /// </summary>
        private List<OpacityKeyPoint> opacityKeyPoints;
        public List<OpacityKeyPoint> OpacityKeyPoints { get { return opacityKeyPoints; } set { opacityKeyPoints = value; } }

        /// <summary>
        /// 对应的图形
        /// </summary>
        private Rectangle rectangle = null;
        public Rectangle Rectangle { get { return rectangle; } set { rectangle = value; } }

        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool isPicked = false;
        public bool IsPicked { get { return isPicked; } set { isPicked = value; } }

        public Track() { }

        public Track(BeatTime startBeatTime, BeatTime endBeatTime, int columnIndex, int id, int startTime = 0, int endTime = 0)
        {
            this.id = id;
            this.startBeatTime = startBeatTime;
            this.endBeatTime = endBeatTime;
            this.columnIndex = columnIndex;
            this.startTime = startTime;
            this.endTime = endTime;
            // 初始化关键点序列，并添加起始点（起始点必须存在，不允许删除）
            this.positionKeyPoints = new List<PositionKeyPoint>();
            this.positionKeyPoints.Add(new PositionKeyPoint(0, 0, this.startBeatTime));
            this.angleKeyPoints = new List<AngleKeyPoint>();
            this.angleKeyPoints.Add(new AngleKeyPoint(0, this.startBeatTime));
            this.opacityKeyPoints = new List<OpacityKeyPoint>();
            this.opacityKeyPoints.Add(new OpacityKeyPoint(1, this.endBeatTime));
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public void UpdateTime(BpmList bpmList, bool ifUpdateNote = true)
        {
            this.startTime = bpmList.GetBeatTimeMs(this.startBeatTime);
            this.endTime = bpmList.GetBeatTimeMs(this.endBeatTime);
            if (!ifUpdateNote) return;

            SkipListNode<BeatTime, Note> current1 = this.noteSkipList.FirstNode;
            while (current1 != null)
            {
                Note note = current1.Value;
                note.UpdateTime(bpmList);
                current1 = current1.Next[0];
            }
            SkipListNode<BeatTime, HoldNote> current2 = this.holdNoteSkipList.FirstNode;
            while (current2 != null)
            {
                HoldNote holdNote = current2.Value;
                holdNote.UpdateTime(bpmList);
                current2 = current2.Next[0];
            }
        }

        /// <summary>
        /// BeatTime是否在轨道内
        /// </summary>
        public bool ContainsBeatTime(BeatTime beatTime)
        {
            if (beatTime == null) return false;
            if (beatTime.IsEarlierThan(this.startBeatTime) || beatTime.IsLaterThan(this.endBeatTime)) return false;
            return true;
        }

        /// <summary>
        /// 获取鼠标位置的拉伸状态。0-不能拉伸；1-可以拉伸起始点；2-可以拉伸结束点
        /// </summary>
        public int GetStretchState(Point? point, Canvas trackCanvas)
        {
            if (!point.HasValue || trackCanvas == null || this.rectangle == null) return 0;
            double pointY = trackCanvas.Height - point.Value.Y;
            double pointX = point.Value.X;
            if (pointX < Canvas.GetLeft(this.rectangle) || pointX > Canvas.GetLeft(this.rectangle) + this.rectangle.Width)
            {
                return 0;
            }
            double startY = Canvas.GetBottom(this.rectangle);
            double endY = Canvas.GetBottom(this.rectangle) + this.rectangle.Height;
            double endDelta = pointY - endY;
            double startDelta = startY - pointY;
            double testY = 10.0;
            if (endDelta <= testY && endDelta > 0) return 2;
            if (startDelta <= testY && startDelta > 0) return 1;
            return 0;
        }

        /// <summary>
        /// 一个坐标是否在矩形范围内
        /// </summary>
        public bool ContainsPoint(Point? point, Canvas trackCanvas)
        {
            if (!point.HasValue || trackCanvas == null || this.Rectangle == null) return false;
            double pointY = trackCanvas.Height - point.Value.Y;
            double pointX = point.Value.X;
            if (pointX >= Canvas.GetLeft(this.Rectangle) && pointX <= Canvas.GetLeft(this.Rectangle) + this.Rectangle.Width
                && pointY >= Canvas.GetBottom(this.Rectangle) && pointY <= Canvas.GetBottom(this.Rectangle) + this.Rectangle.Height)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试添加非Hold的Note
        /// </summary>
        public Note AddNote(BeatTime beatTime, NoteType noteType, int id)
        {
            // 不能与其他非Hold的Note重叠
            if (this.noteSkipList.TryGetValue(beatTime, out Note value)) return null;
            switch (noteType)
            {
                case NoteType.Tap:
                    {
                        TapNote tapNote = new TapNote(beatTime, this, id);
                        this.noteSkipList.Insert(beatTime, tapNote);
                        return tapNote;
                    }
                case NoteType.Flick:
                    {
                        FlickNote flickNote = new FlickNote(beatTime, this, id);
                        this.noteSkipList.Insert(beatTime, flickNote);
                        return flickNote;
                    }
                case NoteType.Catch:
                    {
                        CatchNote catchNote = new CatchNote(beatTime, this, id);
                        this.noteSkipList.Insert(beatTime, catchNote);
                        return catchNote;
                    }
            }
            return null;
        }

        /// <summary>
        /// 尝试添加HoldNoteHeader
        /// </summary>
        public bool AddHoldNoteHeader(BeatTime startTime)
        {
            // 不能放在轨道结尾处
            if (startTime.IsEqualTo(this.endBeatTime)) return false;
            // 不能与其他HoldNote重叠
            SkipListNode<BeatTime, HoldNote> preNode = this.holdNoteSkipList.GetPreNode(startTime);
            if (preNode != this.holdNoteSkipList.Head && startTime.IsEarlierThan(preNode.Value.EndBeatTime)) return false;
            SkipListNode<BeatTime, HoldNote> nextNode = preNode.Next[0];
            if (nextNode != null && !startTime.IsEarlierThan(nextNode.Value.StartBeatTime)) return false;
            return true;
        }

        /// <summary>
        /// 尝试添加HoldNote
        /// </summary>
        public HoldNote AddHoldNoteFooter(BeatTime startTime, BeatTime endTime, int id)
        {
            // 不能与其他HoldNote重叠
            SkipListNode<BeatTime, HoldNote> preNode = this.holdNoteSkipList.GetPreNode(startTime);
            SkipListNode<BeatTime, HoldNote> nextNode = preNode.Next[0];
            if (nextNode != null && endTime.IsLaterThan(nextNode.Value.StartBeatTime)) return null;
            // 可以添加HoldNote
            HoldNote holdNote = new HoldNote(startTime, endTime, this, id);
            this.holdNoteSkipList.Insert(startTime, holdNote);
            return holdNote;
        }

        /// <summary>
        /// 删除一个Note
        /// </summary>
        public void DeleteNote(Note note)
        {
            if (note is HoldNote) this.HoldNoteSkipList.Delete(note.StartBeatTime);
            else this.noteSkipList.Delete(note.StartBeatTime);
        }

        /// <summary>
        /// 获取轨道的矩形
        /// </summary>
        public Rect GetRect()
        {
            if (this.rectangle == null) return Rect.Empty;
            return new Rect(Canvas.GetLeft(this.rectangle), Canvas.GetBottom(this.rectangle), this.rectangle.Width, this.rectangle.Height);
        }

        /// <summary>
        /// 是否为首尾相等的轨道
        /// </summary>
        public bool IsMiniTrack()
        {
            return this.startBeatTime.IsEqualTo(this.endBeatTime);
        }

        /// <summary>
        /// 将轨道移动到某拍数（音符也跟随移动）
        /// </summary>
        public void MoveTrackToBeatTime(BeatTime beatTime)
        {
            // 拍数差值
            BeatTime diffBeatTime = beatTime.Difference(this.StartBeatTime);
            this.startBeatTime = beatTime;
            this.endBeatTime = this.endBeatTime.Sum(diffBeatTime);
            SkipListNode<BeatTime, Note> current1 = this.noteSkipList.FirstNode;
            while (current1 != null)
            {
                Note note = current1.Value;
                note.StartBeatTime.Add(diffBeatTime);
                current1.Key = note.StartBeatTime;
                current1 = current1.Next[0];
            }
            SkipListNode<BeatTime, HoldNote> current2 = this.holdNoteSkipList.FirstNode;
            while (current2 != null)
            {
                HoldNote holdNote = current2.Value;
                holdNote.StartBeatTime.Add(diffBeatTime);
                holdNote.EndBeatTime.Add(diffBeatTime);
                current2.Key = holdNote.StartBeatTime;
                current2 = current2.Next[0];
            }
        }

        /// <summary>
        /// 打印所有Note列表
        /// </summary>
        public void PrintNoteSkipList()
        {
            Console.WriteLine("Tap, Flick, Catch:");
            SkipListNode<BeatTime, Note> current1 = this.noteSkipList.FirstNode;
            while (current1 != null)
            {
                Console.WriteLine(current1.Value.Track.ColumnIndex + " " + current1.Key.ToBeatString() + " " + current1.Value.Type);
                current1 = current1.Next[0];
            }
            Console.WriteLine("Hold:");
            SkipListNode<BeatTime, HoldNote> current2 = this.holdNoteSkipList.FirstNode;
            while (current2 != null)
            {
                Console.WriteLine(current2.Value.Track.ColumnIndex + " " + current2.Key.ToBeatString() + " " + current2.Value.Type);
                current2 = current2.Next[0];
            }
        }

        /// <summary>
        /// 转化为Json
        /// </summary>
        public JObject ToJson()
        {
            JObject jObject = new JObject
            {
                ["Id"] = this.id,
                ["StartBeatTime"] = this.startBeatTime.ToBeatString(),
                ["StartTime"] = this.startTime,
                ["EndBeatTime"] = this.endBeatTime.ToBeatString(),
                ["EndTime"] = this.endTime,
            };
            // Note列表
            JArray noteJArray = new JArray();
            SkipListNode<BeatTime, Note> currentNoteNode = this.noteSkipList.FirstNode;
            while (currentNoteNode != null)
            {
                noteJArray.Add(currentNoteNode.Value.ToJson());
                currentNoteNode = currentNoteNode.Next[0];
            }
            jObject.Add("Notes", noteJArray);
            // HoldNote列表
            JArray holdNoteJArray = new JArray();
            SkipListNode<BeatTime, HoldNote> currentHoldNoteNode = this.holdNoteSkipList.FirstNode;
            while (currentHoldNoteNode != null)
            {
                holdNoteJArray.Add(currentHoldNoteNode.Value.ToJson());
                currentHoldNoteNode = currentHoldNoteNode.Next[0];
            }
            jObject.Add("HoldNotes", holdNoteJArray);
            return jObject;
        }
    }
}
