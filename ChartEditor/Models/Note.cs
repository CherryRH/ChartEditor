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
        private int id;
        public int Id { get { return id; } }

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

        public Note(BeatTime time, NoteType type, Track track, int id)
        {
            this.id = id;
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
    }

    /// <summary>
    /// 点击音符
    /// </summary>
    public class TapNote : Note
    {
        public TapNote(BeatTime time, Track track, int id)
            : base(time, NoteType.Tap, track, id)
        {

        }
    }

    /// <summary>
    /// 接住音符
    /// </summary>
    public class CatchNote : Note
    {
        public CatchNote(BeatTime time, Track track, int id)
            : base(time, NoteType.Catch, track, id)
        {

        }
    }

    public class FlickNote : Note
    {
        public FlickNote(BeatTime time, Track track, int id)
            : base(time, NoteType.Flick, track, id)
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

        public HoldNote(BeatTime time, BeatTime endTime, Track track, int id)
            : base(time, NoteType.Hold, track, id)
        {
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
    }

    public enum NoteType
    {
        Tap,
        Flick,
        Hold,
        Catch
    }
}
