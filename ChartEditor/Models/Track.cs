using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;

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
        /// 包含的音符
        /// </summary>
        private List<Note> notes = new List<Note>();
        public List<Note> Notes { get { return notes; } set { notes = value; } }

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
        /// 是否在轨道范围内
        /// </summary>
        public bool IsInTrack(BeatTime beatTime, int columnIndex)
        {
            if (beatTime == null || columnIndex != this.columnIndex) return false;
            if (beatTime.IsEarlierThan(this.startTime) || beatTime.IsLaterThan(this.endTime)) return false;
            return true;
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
            foreach(Note note in this.notes)
            {
                if (note.Type == NoteType.Hold) continue;
                if (note.Time.IsEqualTo(beatTime)) return null;
            }
            switch (noteType)
            {
                case NoteType.Tap:
                    {
                        TapNote tapNote = new TapNote(beatTime, this, id);
                        this.Notes.Add(tapNote);
                        return tapNote;
                    }
                case NoteType.Flick:
                    {
                        FlickNote flickNote = new FlickNote(beatTime, this, id);
                        this.Notes.Add(flickNote);
                        return flickNote;
                    }
                case NoteType.Catch:
                    {
                        CatchNote catchNote = new CatchNote(beatTime, this, id);
                        this.Notes.Add(catchNote);
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
            foreach(var note in this.Notes)
            {
                if (note.Type == NoteType.Hold && !startTime.IsEarlierThan(note.Time) && startTime.IsEarlierThan(((HoldNote)note).EndTime))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 尝试添加HoldNote
        /// </summary>
        public HoldNote AddHoldNoteFooter(BeatTime startTime , BeatTime endTime, int id)
        {
            // 不能与其他HoldNote重叠
            foreach (var note in this.Notes)
            {
                if (note.Type == NoteType.Hold && startTime.IsEarlierThan(note.Time) && endTime.IsLaterThan(note.Time))
                {
                    return null;
                }
            }
            // 可以添加HoldNote
            HoldNote holdNote = new HoldNote(startTime, endTime, this, id);
            this.Notes.Add(holdNote);
            return holdNote;
        }

        /// <summary>
        /// 删除一个Note
        /// </summary>
        public void DeleteNote(Note note)
        {
            this.notes.Remove(note);
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
            foreach (Note note in this.Notes)
            {
                BeatTime tmpBeatTime = note.Time.Sum(diffBeatTime);
                if (note is HoldNote holdNote)
                {
                    holdNote.MoveHoldNoteToBeatTime(tmpBeatTime);
                }
                else note.MoveNoteToBeatTime(tmpBeatTime);
            }
        }
    }
}
