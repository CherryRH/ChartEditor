using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Models
{
    /// <summary>
    /// Bpm列表
    /// </summary>
    public class BpmList
    {
        private double bpm;
        public double Bpm { get { return bpm; } set { bpm = value; } }

        public BpmList(double bpm)
        {
            this.bpm = bpm;
        }

        /// <summary>
        /// 获取BeatTime的谱面毫秒数
        /// </summary>
        public int GetBeatTimeMs(BeatTime beatTime)
        {
            double beatSecond = beatTime.GetEquivalentBeat() * this.GetBeatDuration();
            return (int)(beatSecond * 1000);
        }

        /// <summary>
        /// 获取对应BeatTime的一拍的时间
        /// </summary>
        public double GetBeatDuration(BeatTime beatTime = null)
        {
            return 60 / this.bpm;
        }

        public int GetAllBeatNum(double musicDuration)
        {
            return (int)Math.Ceiling(musicDuration / 60 * this.bpm);
        }
    }
}
