﻿using System;
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
    /// 音符
    /// </summary>
    public class Note
    {
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

        public Note(BeatTime time, NoteType type, Track track)
        {
            this.time = time;
            this.type = type;
            this.Track = track;
        }

        /// <summary>
        /// 获取音符的矩形
        /// </summary>
        public Rect GetRect()
        {
            if (this.rectangle == null) return Rect.Empty;
            return new Rect(Canvas.GetLeft(this.rectangle), Canvas.GetBottom(this.rectangle), this.rectangle.Width, this.rectangle.Height);
        }
    }

    /// <summary>
    /// 点击音符
    /// </summary>
    public class TapNote : Note
    {
        public TapNote(BeatTime time, Track track)
            : base(time, NoteType.Tap, track)
        {

        }
    }

    /// <summary>
    /// 接住音符
    /// </summary>
    public class CatchNote : Note
    {
        public CatchNote(BeatTime time, Track track)
            : base(time, NoteType.Catch, track)
        {

        }
    }

    public class FlickNote : Note
    {
        public FlickNote(BeatTime time, Track track)
            : base(time, NoteType.Flick, track)
        {
            
        }
    }

    public class HoldNote : Note
    {
        /// <summary>
        /// 结束时间
        /// </summary>
        private BeatTime endTime;
        public BeatTime EndTime { get { return endTime; } set { endTime = value; } }

        public HoldNote(BeatTime time, BeatTime endTime, Track track)
            : base(time, NoteType.Hold, track)
        {
            this.endTime = endTime;
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
