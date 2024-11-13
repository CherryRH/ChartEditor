using ChartEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartEditor.Utils
{
    /// <summary>
    /// 谱面计时器
    /// </summary>
    public class Timer
    {
        private double startTime = 0;
        public double StartTime { get { return startTime; } }

        private ChartEditModel ChartEditModel;

        private Stopwatch stopwatch = new Stopwatch();

        private double Delay { get { return ChartEditModel.ChartInfo.Delay; } }

        public Timer(ChartEditModel chartEditModel)
        {
            this.ChartEditModel = chartEditModel;
        }

        /// <summary>
        /// 在指定时间启动计时器
        /// </summary>
        public void StartAt(double startTime)
        {
            this.startTime = startTime - this.Delay;
            stopwatch.Restart();
        }

        /// <summary>
        /// 重新启动计时器
        /// </summary>
        public void ReStart()
        {
            this.startTime = 0;
            stopwatch.Restart();
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        public void Stop()
        {
            this.stopwatch.Reset();
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        public double GetCurrentTime()
        {
            return this.startTime + stopwatch.Elapsed.TotalSeconds;
        }
    }
}
