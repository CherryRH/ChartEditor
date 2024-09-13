using ChartEditor.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace ChartEditor.Utils.Drawers
{
    /// <summary>
    /// 绘制轨道网格
    /// </summary>
    public class TrackGridDrawer : FrameworkElement
    {
        // 缓存已经绘制过的网格，提升速度
        private Dictionary<string, DrawingVisual> gridCache = new Dictionary<string, DrawingVisual>();
        // 最不常使用LFU
        private Dictionary<string, int> frequencyMap = new Dictionary<string, int>();
        private static int MaxCacheNum = 200;

        // 当前网格
        private DrawingVisual currentGrid = null;

        public TrackGridDrawer()
        {
            
        }

        private void AddCache(string key, DrawingVisual drawingVisual)
        {
            if (this.gridCache.Count == MaxCacheNum)
            {
                // 取出最少使用的释放内存
                KeyValuePair<string, int> lfuPair = this.frequencyMap.OrderBy(kv => kv.Value).First();
                this.gridCache[lfuPair.Key] = null;
                this.gridCache.Remove(lfuPair.Key);
                this.frequencyMap.Remove(lfuPair.Key);
            }
            this.gridCache.Add(key, drawingVisual);
            this.frequencyMap.Add(key, 1);
        }

        private DrawingVisual GetCache(string key)
        {
            // 检查缓存中是否已经存在
            if (this.gridCache.TryGetValue(key, out DrawingVisual cachedVisual))
            {
                if (this.frequencyMap.ContainsKey(key))
                {
                    this.frequencyMap[key]++;
                }
                else
                {
                    this.frequencyMap.Add(key, 1);
                }

                return cachedVisual;
            }
            return null;
        }

        public void DrawTrackGrid(int beatNum, double totalWidth, double totalHeight, double canvasViewerHeight, double columnWidth, double rowWidth, int columnNum, int divide)
        {
            this.RemoveVisualChild(this.currentGrid);

            string key = $"{beatNum}_{totalWidth}_{totalHeight}_{canvasViewerHeight}_{columnWidth}_{rowWidth}_{columnNum}_{divide}";
            DrawingVisual cacheDrawingVisual = GetCache(key);
            if (cacheDrawingVisual != null)
            {
                this.currentGrid = cacheDrawingVisual;
                this.AddVisualChild(cacheDrawingVisual);
                return;
            }

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                // 绘制列
                Pen columnBorderPen1 = new Pen(Brushes.Black, 2);
                Pen columnBorderPen2 = new Pen(Brushes.Black, 1);
                for (int i = 0; i <= columnNum; i++)
                {
                    if (i == 0 || i == columnNum)
                    {
                        dc.DrawLine(columnBorderPen1, new Point(i * columnWidth + Common.BeatBarWidth, canvasViewerHeight * Common.JudgeLineRateOp), new Point(i * columnWidth + Common.BeatBarWidth, totalHeight + canvasViewerHeight * Common.JudgeLineRateOp));
                    }
                    else
                    {
                        dc.DrawLine(columnBorderPen2, new Point(i * columnWidth + Common.BeatBarWidth, canvasViewerHeight * Common.JudgeLineRateOp), new Point(i * columnWidth + Common.BeatBarWidth, totalHeight + canvasViewerHeight * Common.JudgeLineRateOp));
                    }
                }
                // 绘制行
                Pen beatPen = new Pen(Brushes.Purple, 1);
                Pen dividePen1 = new Pen(Brushes.DarkGreen, 0.8);
                Pen dividePen2 = new Pen(Brushes.Cyan, 0.8);
                Pen dividePen3 = new Pen(Brushes.Gray, 0.5);

                for (int i = 0; i <= beatNum; i++)
                {
                    double lineY = canvasViewerHeight * Common.JudgeLineRateOp + i * rowWidth;
                    // 绘制拍线
                    dc.DrawLine(beatPen, new Point(Common.BeatBarWidth, lineY), new Point(Common.BeatBarWidth + totalWidth, lineY));

                    if (i < beatNum)
                    {
                        // 绘制拍号
                        FormattedText formattedText = new FormattedText(
                            (beatNum - i).ToString(),
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Consolas"),
                            16,
                            Brushes.Gray,
                            VisualTreeHelper.GetDpi(drawingVisual).PixelsPerDip
                        );
                        dc.DrawText(formattedText, new Point(0, canvasViewerHeight * Common.JudgeLineRateOp + (i + 0.5) * rowWidth - formattedText.Height / 2));
                        // 绘制分割线
                        switch (divide)
                        {
                            case 2:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    break;
                                }
                            case 4:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 4));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 4));
                                    break;
                                }
                            case 8:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 4));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 8));
                                    break;
                                }
                            case 16:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 4));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 9 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 9 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 11 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 11 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 13 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 13 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 15 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 15 / 16));
                                    break;
                                }
                            case 32:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 4));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 9 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 9 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 11 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 11 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 13 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 13 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 15 / 16), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 15 / 16));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 9 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 9 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 11 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 11 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 13 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 13 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 15 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 15 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 17 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 17 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 19 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 19 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 21 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 21 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 23 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 23 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 25 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 25 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 27 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 27 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 29 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 29 / 32));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 31 / 32), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 31 / 32));
                                    break;
                                }
                            case 3:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 3));
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 2 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 2 / 3));
                                    break;
                                }
                            case 6:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 3));
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 2 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 2 / 3));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 6), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 6));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 6), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 6));
                                    break;
                                }
                            case 12:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 3));
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 2 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 2 / 3));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 6), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 6));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 6), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 6));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 11 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 11 / 12));
                                    break;
                                }
                            case 24:
                                {
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 3));
                                    dc.DrawLine(dividePen1, new Point(Common.BeatBarWidth, lineY + rowWidth * 2 / 3), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 2 / 3));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 6), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 6));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 2), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 2));
                                    dc.DrawLine(dividePen2, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 6), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 6));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 4), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 4));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 11 / 12), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 11 / 12));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 1 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 1 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 3 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 3 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 11 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 11 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 13 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 13 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 5 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 5 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 17 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 17 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 19 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 19 / 24));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 7 / 8), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 7 / 8));
                                    dc.DrawLine(dividePen3, new Point(Common.BeatBarWidth, lineY + rowWidth * 23 / 24), new Point(Common.BeatBarWidth + totalWidth, lineY + rowWidth * 23 / 24));
                                    break;
                                }
                            default: { break; }
                        }
                    }
                }
            }

            this.AddCache(key, drawingVisual);
            this.currentGrid = drawingVisual;
            this.AddVisualChild(drawingVisual);
        }

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException();
            return this.currentGrid;
        }
    }
}
