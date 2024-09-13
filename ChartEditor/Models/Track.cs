using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Models
{
    /// <summary>
    /// 轨道
    /// </summary>
    public class Track
    {
        public int Id { get; set; }

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
        private List<Note> notes;
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

        public Track() { }

        public Track(BeatTime startTime, BeatTime endTime)
        {
            this.columnIndex = 0;
            this.startTime = startTime;
            this.endTime = endTime;
            this.notes = new List<Note>();
            // 初始化关键点序列，并添加起始点（起始点必须存在，不允许删除）
            this.positionKeyPoints = new List<PositionKeyPoint>();
            this.positionKeyPoints.Add(new PositionKeyPoint(0, 0, this.startTime));
            this.angleKeyPoints = new List<AngleKeyPoint>();
            this.angleKeyPoints.Add(new AngleKeyPoint(0, this.startTime));
            this.opacityKeyPoints = new List<OpacityKeyPoint>();
            this.opacityKeyPoints.Add(new OpacityKeyPoint(1, this.endTime));
        }
    }
}
