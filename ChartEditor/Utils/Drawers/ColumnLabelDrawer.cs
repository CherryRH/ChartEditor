using ChartEditor.UserControls.Boards;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace ChartEditor.Utils.Drawers
{
    /// <summary>
    /// 列标签绘制
    /// </summary>
    public class ColumnLabelDrawer
    {
        private TrackEditBoard TrackEditBoard;

        private Canvas TrackCanvasFloor;

        private ScrollViewer TrackCanvasViewer { get { return TrackEditBoard.TrackCanvasViewer; } }

        private int ColumnNum { get { return TrackEditBoard.Model.ColumnNum; } }

        private double ColumnWidth { get { return TrackEditBoard.Model.ColumnWidth; } }

        private List<TextBlock> columnLabels = new List<TextBlock>();

        public ColumnLabelDrawer(TrackEditBoard trackEditBoard)
        {
            this.TrackEditBoard = trackEditBoard;
            this.TrackCanvasFloor = trackEditBoard.TrackCanvasFloor;
            for (int i = 0; i < this.ColumnNum; i++)
            {
                // 构造列标签
                TextBlock textBlock = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    Width = this.ColumnWidth,
                    Background = ColorProvider.ColumnLabelBrush,
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 16,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                this.columnLabels.Add(textBlock);
                this.TrackCanvasFloor.Children.Add(textBlock);
                Canvas.SetBottom(textBlock, 0);
                Canvas.SetLeft(textBlock, i * this.ColumnWidth);
            }
        }

        /// <summary>
        /// 更新列标签的位置
        /// </summary>
        public void UpdateOffset()
        {
            foreach (TextBlock textBlock in this.columnLabels)
            {
                Canvas.SetTop(textBlock, this.TrackCanvasViewer.VerticalOffset + this.TrackCanvasViewer.ActualHeight - textBlock.ActualHeight);
            }
        }

        /// <summary>
        /// 当列宽变化时重绘
        /// </summary>
        public void RedrawWhenColumnWidthChanged()
        {
            for (int i = 0; i < this.ColumnNum; i++)
            {
                TextBlock textBlock = this.columnLabels[i];
                textBlock.Width = this.ColumnWidth;
                Canvas.SetLeft(textBlock, i * this.ColumnWidth);
            }
        }
    }
}
