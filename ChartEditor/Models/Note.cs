using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace ChartEditor.Models
{
    /// <summary>
    /// 音符
    /// </summary>
    public class Note
    {
        private int id;
        public int Id { get { return id; } }

        /// <summary>
        /// 音符的时间
        /// </summary>
        private BeatTime startBeatTime;
        public BeatTime StartBeatTime { get { return startBeatTime; } set { startBeatTime = value; } }

        private int startTime = 0;
        public int StartTime { get { return startTime; } set { startTime = value; } }

        /// <summary>
        /// 音符种类
        /// </summary>
        private NoteType type;
        public NoteType Type { get { return type; } set { type = value; } }

        /// <summary>
        /// 所属轨道
        /// </summary>
        public Track Track { get; set; }

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

        public Note() { }

        public Note(BeatTime time, NoteType type, Track track, int id, int startTime = 0)
        {
            this.id = id;
            this.startBeatTime = time;
            this.type = type;
            this.Track = track;
            this.startTime = startTime;
        }

        /// <summary>
        /// 获取音符的矩形
        /// </summary>
        public Rect GetRect()
        {
            if (this.rectangle == null) return Rect.Empty;
            return new Rect(Canvas.GetLeft(this.rectangle), Canvas.GetBottom(this.rectangle), this.rectangle.Width, this.rectangle.Height);
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
        /// 比较两个Note的时间先后
        /// </summary>
        public int CompareTo(Note y)
        {
            if (this.StartBeatTime.IsEarlierThan(y.StartBeatTime)) return -1;
            else if (this.StartBeatTime.IsEqualTo(y.StartBeatTime)) return 0;
            else return 1;
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual void UpdateTime(BpmList bpmList)
        {
            this.startTime = bpmList.GetBeatTimeMs(this.StartBeatTime);
        }

        /// <summary>
        /// 转化为JObject
        /// </summary>
        public virtual JObject ToJson()
        {
            return new JObject
            {
                ["Id"] = this.id,
                ["Type"] = (int)this.type,
                ["StartBeatTime"] = this.startBeatTime.ToBeatString(),
                ["StartTime"] = this.startTime
            };
        }

        /// <summary>
        /// 从Json转化
        /// </summary>
        public static Note FromJson(JObject jObject)
        {
            if (jObject == null) return null;
            var id = jObject.Value<int?>("Id");
            if (id == null) return null;
            var typeIndex = jObject.Value<int?>("Type");
            if (typeIndex == null || !Enum.IsDefined(typeof(NoteType), typeIndex.Value))
            {
                return null;
            }
            var time = BeatTime.FromBeatString(jObject.Value<string>("Time"));
            if (time == null) return null;
            return new Note
            {
                id = id.Value,
                type = (NoteType)typeIndex,
                startBeatTime = time
            };
        }
    }

    /// <summary>
    /// 点击音符
    /// </summary>
    public class TapNote : Note
    {
        public TapNote(BeatTime startBeatTime, Track track, int id, int startTime = 0)
            : base(startBeatTime, NoteType.Tap, track, id, startTime)
        {

        }
    }

    /// <summary>
    /// 接住音符
    /// </summary>
    public class CatchNote : Note
    {
        public CatchNote(BeatTime startBeatTime, Track track, int id, int startTime = 0)
            : base(startBeatTime, NoteType.Catch, track, id, startTime)
        {

        }
    }

    public class FlickNote : Note
    {
        public FlickNote(BeatTime startBeatTime, Track track, int id, int startTime = 0)
            : base(startBeatTime, NoteType.Flick, track, id, startTime)
        {
            
        }
    }

    public class HoldNote : Note
    {
        /// <summary>
        /// 结束时间
        /// </summary>
        private BeatTime endBeatTime;
        public BeatTime EndBeatTime { get { return endBeatTime; } set { endBeatTime = value; } }

        private int endTime = 0;
        public int EndTime { get { return endTime; } set { endTime = value; } }

        public HoldNote(BeatTime startBeatTime, BeatTime endBeatTime, Track track, int id, int startTime = 0, int endTime = 0)
            : base(startBeatTime, NoteType.Hold, track, id, startTime)
        {
            this.endBeatTime = endBeatTime;
            this.endTime = endTime;
        }

        /// <summary>
        /// 获取鼠标位置的拉伸状态。0-不能拉伸；1-可以拉伸起始点；2-可以拉伸结束点
        /// </summary>
        public int GetStretchState(Point? point, Canvas trackCanvas)
        {
            if (!point.HasValue || trackCanvas == null || this.Rectangle == null) return 0;
            double pointY = trackCanvas.Height - point.Value.Y;
            double pointX = point.Value.X;
            if (pointX < Canvas.GetLeft(this.Rectangle) || pointX > Canvas.GetLeft(this.Rectangle) + this.Rectangle.Width)
            {
                return 0;
            }
            double startY = Canvas.GetBottom(this.Rectangle);
            double endY = Canvas.GetBottom(this.Rectangle) + this.Rectangle.Height;
            double endDelta = pointY - endY;
            double startDelta = startY - pointY;
            double testY = 10.0;
            if (endDelta <= testY && endDelta > 0) return 2;
            if (startDelta <= testY && startDelta > 0) return 1;
            return 0;
        }

        /// <summary>
        /// 转化为JObject
        /// </summary>
        public override JObject ToJson()
        {
            var json = base.ToJson();
            json.Add("EndBeatTime", this.endBeatTime.ToBeatString());
            json.Add("EndTime", this.endTime);
            return json;
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        public override void UpdateTime(BpmList bpmList)
        {
            base.UpdateTime(bpmList);
            this.endTime = bpmList.GetBeatTimeMs(this.endBeatTime);
        }
    }

    public enum NoteType
    {
        Tap,
        Flick,
        Hold,
        Catch
    }
}
