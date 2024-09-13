using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Models
{
    /// <summary>
    /// 音符
    /// </summary>
    public class Note
    {
        public int Id { get; set; }

        /// <summary>
        /// 音符的时间
        /// </summary>
        private BeatTime time;
        public BeatTime Time { get { return time; } set { time = value; } }

        /// <summary>
        /// 音符种类
        /// </summary>
        private NoteType type;
        public NoteType Type { get { return type; } set { type = value; } }

        /// <summary>
        /// 所属轨道
        /// </summary>
        public Track Track { get; set; }

        public Note() { }

        public Note(int id, BeatTime time, NoteType type, Track track)
        {
            this.Id = id;
            this.time = time;
            this.type = type;
            this.Track = track;
        }
    }

    /// <summary>
    /// 点击音符
    /// </summary>
    public class TapNote : Note
    {
        public TapNote(int id, BeatTime time, Track track)
            : base(id, time, NoteType.Tap, track)
        {

        }
    }

    /// <summary>
    /// 接住音符
    /// </summary>
    public class CatchNote : Note
    {
        public CatchNote(int id, BeatTime time, Track track)
            : base(id, time, NoteType.Tap, track)
        {

        }
    }

    public class FlickNote : Note
    {
        /// <summary>
        /// 是否是向上划
        /// </summary>
        private bool isUp;
        public bool IsUp { get { return isUp; } set { isUp = value; } }

        public FlickNote(int id, BeatTime time, Track track, bool isUp = true)
            : base(id, time, NoteType.Flick, track)
        {
            this.isUp = isUp;
        }
    }

    public class HoldNote : Note
    {
        /// <summary>
        /// 结束时间
        /// </summary>
        private BeatTime endTime;
        public BeatTime EndTime { get { return endTime; } set { endTime = value; } }

        public HoldNote(int id, BeatTime time, BeatTime endTime, Track track)
            : base(id, time, NoteType.Hold, track)
        {
            this.endTime = endTime;
        }
    }

    public enum NoteType
    {
        Tap,
        Flick,
        Hold
    }
}
