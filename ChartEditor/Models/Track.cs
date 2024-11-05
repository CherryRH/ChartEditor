using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using ChartEditor.Utils;

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
        private BeatTime startTime;
        public BeatTime StartTime { get { return startTime; } set { startTime = value; } }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        private BeatTime endTime;
        public BeatTime EndTime { get { return endTime; } set { endTime = value; } }

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

        public Track(BeatTime startTime, BeatTime endTime, int columnIndex, int id)
        {
            this.id = id;
            this.startTime = startTime;
            this.endTime = endTime;
            this.columnIndex = columnIndex;
            // 初始化关键点序列，并添加起始点（起始点必须存在，不允许删除）
            this.positionKeyPoints = new List<PositionKeyPoint>();
            this.positionKeyPoints.Add(new PositionKeyPoint(0, 0, this.startTime));
            this.angleKeyPoints = new List<AngleKeyPoint>();
            this.angleKeyPoints.Add(new AngleKeyPoint(0, this.startTime));
            this.opacityKeyPoints = new List<OpacityKeyPoint>();
            this.opacityKeyPoints.Add(new OpacityKeyPoint(1, this.endTime));
        }

        /// <summary>
        /// BeatTime是否在轨道内
        /// </summary>
        public bool ContainsBeatTime(BeatTime beatTime)
        {
            if (beatTime == null) return false;
            if (beatTime.IsEarlierThan(this.startTime) || beatTime.IsLaterThan(this.endTime)) return false;
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
            if (startTime.IsEqualTo(this.endTime)) return false;
            // 不能与其他HoldNote重叠
            SkipListNode<BeatTime, HoldNote> preNode = this.holdNoteSkipList.GetPreNode(startTime);
            if (preNode != this.holdNoteSkipList.Head && startTime.IsEarlierThan(preNode.Value.EndTime)) return false;
            SkipListNode<BeatTime, HoldNote> nextNode = preNode.Next[0];
            if (nextNode != null && !startTime.IsEarlierThan(nextNode.Value.Time)) return false;
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
            if (nextNode != null && endTime.IsLaterThan(nextNode.Value.Time)) return null;
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
            if (note is HoldNote) this.HoldNoteSkipList.Delete(note.Time);
            else this.noteSkipList.Delete(note.Time);
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
            return this.startTime.IsEqualTo(this.endTime);
        }

        /// <summary>
        /// 将轨道移动到某拍数（音符也跟随移动）
        /// </summary>
        public void MoveTrackToBeatTime(BeatTime beatTime)
        {
            // 拍数差值
            BeatTime diffBeatTime = beatTime.Difference(this.StartTime);
            this.startTime = beatTime;
            this.endTime = this.endTime.Sum(diffBeatTime);
            SkipListNode<BeatTime, Note> current1 = this.noteSkipList.FirstNode;
            while (current1 != null)
            {
                Note note = current1.Value;
                note.Time.Add(diffBeatTime);
                current1.Key = note.Time;
                current1 = current1.Next[0];
            }
            SkipListNode<BeatTime, HoldNote> current2 = this.holdNoteSkipList.FirstNode;
            while (current2 != null)
            {
                HoldNote holdNote = current2.Value;
                holdNote.Time.Add(diffBeatTime);
                holdNote.EndTime.Add(diffBeatTime);
                current2.Key = holdNote.Time;
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
    }
}
