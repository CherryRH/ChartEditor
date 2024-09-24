using ChartEditor.Models;
using ChartEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ChartEditor.Utils.Drawers
{
    /// <summary>
    /// 实现Canvas中Note的绘制
    /// Id: 0 - Track, 1 - TapNote, 2 - FlickNote, 3 - HoldNote, 4 - CatchNote
    /// </summary>
    public class NoteDrawer
    {
        /// <summary>
        /// TrackCanvas对象
        /// </summary>
        private Canvas trackCanvas;

        /// <summary>
        /// TrackScrollViewer对象
        /// </summary>
        private ScrollViewer scrollViewer;

        /// <summary>
        /// ChartEditModel对象
        /// </summary>
        private ChartEditModel ChartEditModel;

        /// <summary>
        /// 预览图形Track，Tap，Flick，Hold，Catch
        /// </summary>
        private List<Rectangle> previewers;
        // 当前显示的预览图形编号
        private int previewerIndex = -1;
        public int PreviewerIndex { get { return previewerIndex; } }
        // 是否显示预览图形
        private bool ifShowPreviewer = true;

        /// <summary>
        /// 头部图形TrackHead，HoldHead
        /// </summary>
        private Rectangle trackHeader;
        private Rectangle holdNoteHeader;

        /// <summary>
        /// 当前预览图形的拍数和列数
        /// </summary>
        private BeatTime lastPreviewBeatTime = new BeatTime();
        private int lastPreviewColumnIndex = 0;
        private BeatTime lastTrackHeaderBeatTime = new BeatTime();
        private int lastTrackHeaderColumnIndex = 0;
        private BeatTime lastHoldNoteHeaderBeatTime = new BeatTime();
        private int lastHoldNoteHeaderColumnIndex = 0;

        /// <summary>
        /// 矩形参数
        /// </summary>
        public static double MinHeight = 10;
        public static int StrokeThickness = 2;
        public static int Radius = 5;
        public static double PreviewerOpacity = 0.2;
        public static double HeaderOpacity = 0.3;

        public NoteDrawer(Canvas canvas, ScrollViewer scrollViewer, ChartEditModel chartEditModel)
        {
            this.trackCanvas = canvas;
            this.scrollViewer = scrollViewer;
            this.ChartEditModel = chartEditModel;
            this.previewers = new List<Rectangle>();

            this.InitPreviewNotes();
        }

        /// <summary>
        /// 在列宽变化时重绘
        /// </summary>
        public void RedrawWhenColumnWidthChanged()
        {
            // 重绘Header
            if (this.trackCanvas.Children.Contains(this.trackHeader))
            {
                this.trackHeader.Width = this.ChartEditModel.ColumnWidth;
                Canvas.SetLeft(this.trackHeader, this.lastTrackHeaderColumnIndex * this.ChartEditModel.ColumnWidth);
            }
            if (this.trackCanvas.Children.Contains(this.holdNoteHeader))
            {
                this.holdNoteHeader.Width = this.ChartEditModel.ColumnWidth - 2 * Common.ColumnGap * this.ChartEditModel.ColumnWidth;
                Canvas.SetLeft(this.holdNoteHeader, this.lastHoldNoteHeaderColumnIndex * this.ChartEditModel.ColumnWidth + Common.ColumnGap * this.ChartEditModel.ColumnWidth);
            }
            // 重绘所有Track和Note
            foreach (var item in this.ChartEditModel.Tracks)
            {
                item.Rectangle.Width = this.ChartEditModel.ColumnWidth;
                Canvas.SetLeft(item.Rectangle, item.ColumnIndex * this.ChartEditModel.ColumnWidth);
                foreach(var note in item.Notes)
                {
                    note.Rectangle.Width = this.ChartEditModel.ColumnWidth - 2 * Common.ColumnGap * this.ChartEditModel.ColumnWidth;
                    Canvas.SetLeft(note.Rectangle, note.Track.ColumnIndex * this.ChartEditModel.ColumnWidth + Common.ColumnGap * this.ChartEditModel.ColumnWidth);
                }
            }
        }

        /// <summary>
        /// 在行宽变化时重绘
        /// </summary>
        public void RedrawWhenRowWidthChanged()
        {
            // 重绘Header
            if (this.trackCanvas.Children.Contains(this.trackHeader))
            {
                Canvas.SetBottom(this.trackHeader, this.lastTrackHeaderBeatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            }
            if (this.trackCanvas.Children.Contains(this.holdNoteHeader))
            {
                Canvas.SetBottom(this.holdNoteHeader, this.lastHoldNoteHeaderBeatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            }
            // 重绘所有Track和Note
            foreach(var item in this.ChartEditModel.Tracks)
            {
                item.Rectangle.Height = MinHeight + (item.EndTime.GetEquivalentBeat() - item.StartTime.GetEquivalentBeat()) * this.ChartEditModel.RowWidth;
                Canvas.SetBottom(item.Rectangle, item.StartTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
                foreach (var note in item.Notes)
                {
                    if (note.Type == NoteType.Hold)
                    {
                        note.Rectangle.Height = MinHeight + (((HoldNote)note).EndTime.GetEquivalentBeat() - note.Time.GetEquivalentBeat()) * this.ChartEditModel.RowWidth;
                    }
                    Canvas.SetBottom(note.Rectangle, note.Time.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
                }
            }
        }

        /// <summary>
        /// 新建一个Track
        /// </summary>
        public void CreateTrackItem(Track track)
        {
            Rectangle newTrack = new Rectangle
            {
                Width = this.ChartEditModel.ColumnWidth,
                Height = MinHeight + (track.EndTime.GetEquivalentBeat() - track.StartTime.GetEquivalentBeat()) * this.ChartEditModel.RowWidth,
                Stroke = ColorProvider.TrackBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.TrackGradientBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = HeaderOpacity
            };
            track.Rectangle = newTrack;
            this.trackCanvas.Children.Add(newTrack);
            Canvas.SetLeft(newTrack, track.ColumnIndex * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(newTrack, track.StartTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 保持在最底层
            Canvas.SetZIndex(newTrack, 0);
        }

        /// <summary>
        /// 新建一个Note
        /// </summary>
        public void CreateNoteItem(Note note)
        {
            Rectangle newNote = new Rectangle
            {
                Width = this.ChartEditModel.ColumnWidth - 2 * Common.ColumnGap * this.ChartEditModel.ColumnWidth,
                StrokeThickness = StrokeThickness,
                RadiusX = Radius,
                RadiusY = Radius
            };
            switch (note.Type)
            {
                case NoteType.Tap:
                    {
                        newNote.Height = MinHeight;
                        newNote.Stroke = ColorProvider.TapNoteBorderBrush;
                        newNote.Fill = ColorProvider.TapNoteBrush;
                        break;
                    }
                case NoteType.Catch:
                    {
                        newNote.Height = MinHeight * 2 / 3;
                        newNote.Stroke = ColorProvider.CatchNoteBorderBrush;
                        newNote.Fill = ColorProvider.CatchNoteBrush;
                        break;
                    }
                case NoteType.Hold:
                    {
                        newNote.Height = MinHeight + (((HoldNote)note).EndTime.GetEquivalentBeat() - note.Time.GetEquivalentBeat()) * this.ChartEditModel.RowWidth;
                        newNote.Stroke = ColorProvider.HoldNoteBorderBrush;
                        newNote.Fill = ColorProvider.HoldNoteGradientBrush;
                        break;
                    }
                case NoteType.Flick:
                    {
                        newNote.Height = MinHeight;
                        newNote.Stroke = ColorProvider.FlickNoteBorderBrush;
                        newNote.Fill = ColorProvider.FlickNoteGradientBrush;
                        break;
                    }
            }
            note.Rectangle = newNote;
            this.trackCanvas.Children.Add(newNote);
            Canvas.SetLeft(newNote, note.Track.ColumnIndex * this.ChartEditModel.ColumnWidth + Common.ColumnGap * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(newNote, note.Time.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // HoldNote在其他Note下层
            if (note.Type == NoteType.Hold)
            {
                Canvas.SetZIndex(newNote, 1);
            }
            else
            {
                Canvas.SetZIndex(newNote, 2);
            }
        }

        /// <summary>
        /// 在指定位置显示预览图形
        /// </summary>
        public void ShowPreviewerAt(Point? point, int divide)
        {
            if (!point.HasValue || !this.ifShowPreviewer) return;
            // 判断位置是否相同
            BeatTime newBeatTime = new BeatTime(divide);
            newBeatTime.UpdateFromJudgeLineOffset(this.trackCanvas.ActualHeight - point.Value.Y, this.ChartEditModel.RowWidth);
            int columnIndex = (int)((point.Value.X) / this.ChartEditModel.ColumnWidth);
            if (this.lastPreviewBeatTime.IsEqualTo(newBeatTime) && this.lastPreviewColumnIndex == columnIndex)
            {
                return;
            }
            this.lastPreviewBeatTime = newBeatTime;
            this.lastPreviewColumnIndex = columnIndex;
            if (!this.trackCanvas.Children.Contains(this.previewers[this.previewerIndex])) this.trackCanvas.Children.Add(this.previewers[this.previewerIndex]);
            // 重置宽度和位置
            this.previewers[this.previewerIndex].Width = this.ChartEditModel.ColumnWidth - (this.previewerIndex == 0 ? 0 : 2 * Common.ColumnGap * this.ChartEditModel.ColumnWidth);
            Canvas.SetLeft(this.previewers[this.previewerIndex], columnIndex * this.ChartEditModel.ColumnWidth + (this.previewerIndex == 0 ? 0 : Common.ColumnGap * this.ChartEditModel.ColumnWidth));
            Canvas.SetBottom(this.previewers[this.previewerIndex], newBeatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 确保在最顶层
            Canvas.SetZIndex(this.previewers[this.previewerIndex], 100);
        }

        public void ShowPreviewer()
        {
            this.ifShowPreviewer = true;
        }

        public void HidePreviewer()
        {
            this.ifShowPreviewer = false;
            if (this.previewerIndex != -1) this.trackCanvas.Children.Remove(this.previewers[this.previewerIndex]); ;
        }

        /// <summary>
        /// 在指定位置显示TrackHeader
        /// </summary>
        public void ShowTrackHeaderAt(BeatTime beatTime, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.ChartEditModel.ColumnNum) return;
            // 重置宽度和位置
            this.lastTrackHeaderBeatTime = beatTime;
            this.lastTrackHeaderColumnIndex = columnIndex;
            this.trackHeader.Width = this.ChartEditModel.ColumnWidth;
            this.trackCanvas.Children.Add(this.trackHeader);
            Canvas.SetLeft(this.trackHeader, columnIndex * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.trackHeader, beatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 在次顶层
            Canvas.SetZIndex(this.trackHeader, 99);
        }

        /// <summary>
        /// 隐藏TrackHeader
        /// </summary>
        public void HideTrackHeader()
        {
            this.trackCanvas.Children.Remove(this.trackHeader);
        }

        /// <summary>
        /// 在指定位置显示HoldNoteHeader
        /// </summary>
        public void ShowHoldNoteHeaderAt(BeatTime beatTime, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.ChartEditModel.ColumnNum) return;
            // 重置宽度和位置
            this.lastHoldNoteHeaderBeatTime = beatTime;
            this.lastHoldNoteHeaderColumnIndex = columnIndex;
            this.holdNoteHeader.Width = this.ChartEditModel.ColumnWidth - 2 * Common.ColumnGap * this.ChartEditModel.ColumnWidth;
            this.trackCanvas.Children.Add(this.holdNoteHeader);
            Canvas.SetLeft(this.holdNoteHeader, columnIndex * this.ChartEditModel.ColumnWidth + Common.ColumnGap * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.holdNoteHeader, beatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 在次顶层
            Canvas.SetZIndex(this.trackHeader, 99);
        }

        /// <summary>
        /// 隐藏HoldNoteHeader
        /// </summary>
        public void HideHoldNoteHeader()
        {
            this.trackCanvas.Children.Remove(this.holdNoteHeader);
        }

        /// <summary>
        /// 切换当前显示的预览图形
        /// </summary>
        public void SwitchPreviewer(int id)
        {
            if (id == -1)
            {
                this.HideAllPreviewers();
                return;
            }
            if (id < 0 || id >= this.previewers.Count || this.previewerIndex == id) return;
            if (this.previewerIndex != -1) this.trackCanvas.Children.Remove(this.previewers[this.previewerIndex]);
            this.previewerIndex = id;
        }

        /// <summary>
        /// 隐藏所有预置图形
        /// </summary>
        public void HideAllPreviewers()
        {
            foreach (var item in previewers)
            {
                this.trackCanvas.Children.Remove(item);
            }
        }

        /// <summary>
        /// 初始化预览图形
        /// </summary>
        private void InitPreviewNotes()
        {
            double width = (1.0 - 2 * Common.ColumnGap) * Common.ColumnWidth;
            // 5种预览图形
            this.previewers.Add(new Rectangle
            {
                Name = "TrackPreviewer",
                Width = Common.ColumnWidth,
                Height = MinHeight,
                Stroke = ColorProvider.TrackBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.TrackGradientBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = PreviewerOpacity
            });
            this.previewers.Add(new Rectangle
            {
                Name = "TapNotePreviewer",
                Width = width,
                Height = MinHeight,
                Stroke = ColorProvider.TapNoteBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.TapNoteBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = PreviewerOpacity
            });
            this.previewers.Add(new Rectangle
            {
                Name = "FlickNotePreviewer",
                Width = width,
                Height = MinHeight,
                Stroke = ColorProvider.FlickNoteBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.FlickNoteGradientBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = PreviewerOpacity
            });
            this.previewers.Add(new Rectangle
            {
                Name = "HoldNotePreviewer",
                Width = width,
                Height = MinHeight,
                Stroke = ColorProvider.HoldNoteBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.HoldNoteBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = PreviewerOpacity
            });
            this.previewers.Add(new Rectangle
            {
                Name = "CatchNotePreviewer",
                Width = width,
                Height = MinHeight * 2 / 3,
                Stroke = ColorProvider.CatchNoteBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.CatchNoteBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = PreviewerOpacity
            });
            // 2种开头图形
            this.trackHeader = new Rectangle
            {
                Name = "TrackHeader",
                Width = Common.ColumnWidth,
                Height = MinHeight,
                Stroke = ColorProvider.TrackBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.TrackGradientBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = HeaderOpacity
            };
            this.holdNoteHeader = new Rectangle
            {
                Name = "HoldNoteHeader",
                Width = width,
                Height = MinHeight,
                Stroke = ColorProvider.HoldNoteBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.HoldNoteBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = HeaderOpacity
            };
        }
    }
}
