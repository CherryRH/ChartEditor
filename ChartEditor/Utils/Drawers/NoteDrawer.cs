using ChartEditor.Models;
using ChartEditor.UserControls.Boards;
using ChartEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        // 是否正在显示预览图形
        private bool isPreviewerShowing = false;
        // 是否正在显示trackHeader
        private bool isTrackHeaderShowing = false;
        // 是否正在显示holdNoteHeader
        private bool isHoldNoteHeaderShowing = false;

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
        private static double MinHeight = 12;
        private static int StrokeThickness = 1;
        private static int HighLightStrokeThickness = 3;
        private static int PickedStrokeThickness = 3;
        private static int Radius = 5;
        private static double PreviewerOpacity = 0.4;
        private static double HeaderOpacity = 0.6;
        private static double TrackOpacity = 0.8;

        // 图形ZIndex
        private static int TrackZIndex = 0;
        private static int MiniTrackZIndex = 10;
        private static int HoldNoteZIndex = 20;
        private static int NoteZIndex = 30;
        private static int HeaderZIndex = 50;
        private static int PreviewerZIndex = 100;

        // 动画
        private static DoubleAnimation RectangleStrokeAnimation = AnimationProvider.GetRepeatDoubleAnimation(12, 0, 1.5);

        public NoteDrawer(TrackEditBoard trackEditBoard)
        {
            this.trackCanvas = trackEditBoard.TrackCanvas;
            this.scrollViewer = trackEditBoard.TrackCanvasViewer;
            this.ChartEditModel = trackEditBoard.Model;
            this.previewers = new List<Rectangle>();

            this.InitPreviewNotes();
        }

        /// <summary>
        /// 在列宽变化时重绘
        /// </summary>
        public void RedrawWhenColumnWidthChanged()
        {
            double trackPadding = Common.TrackPadding * this.ChartEditModel.ColumnWidth;
            double notePadding = Common.NotePadding * this.ChartEditModel.ColumnWidth;
            // 重绘Header
            if (this.isTrackHeaderShowing)
            {
                this.trackHeader.Width = this.ChartEditModel.ColumnWidth - 2 * trackPadding;
                Canvas.SetLeft(this.trackHeader, this.lastTrackHeaderColumnIndex * this.ChartEditModel.ColumnWidth + trackPadding);
            }
            if (this.isHoldNoteHeaderShowing)
            {
                this.holdNoteHeader.Width = this.ChartEditModel.ColumnWidth - 2 * notePadding;
                Canvas.SetLeft(this.holdNoteHeader, this.lastHoldNoteHeaderColumnIndex * this.ChartEditModel.ColumnWidth + notePadding);
            }
            // 重绘所有Track和Note
            foreach (Track track in this.ChartEditModel.Tracks)
            {
                track.Rectangle.Width = this.ChartEditModel.ColumnWidth - 2 * trackPadding;
                Canvas.SetLeft(track.Rectangle, track.ColumnIndex * this.ChartEditModel.ColumnWidth + trackPadding);
                foreach(Note note in track.Notes)
                {
                    note.Rectangle.Width = this.ChartEditModel.ColumnWidth - 2 * notePadding;
                    Canvas.SetLeft(note.Rectangle, note.Track.ColumnIndex * this.ChartEditModel.ColumnWidth + notePadding);
                }
            }
        }

        /// <summary>
        /// 在行宽变化时重绘
        /// </summary>
        public void RedrawWhenRowWidthChanged()
        {
            // 重绘Header
            if (this.isTrackHeaderShowing)
            {
                Canvas.SetBottom(this.trackHeader, this.lastTrackHeaderBeatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            }
            if (this.isHoldNoteHeaderShowing)
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
                Tag = "Track",
                Width = this.ChartEditModel.ColumnWidth - 2 * Common.TrackPadding * this.ChartEditModel.ColumnWidth,
                Height = MinHeight + (track.EndTime.GetEquivalentBeat() - track.StartTime.GetEquivalentBeat()) * this.ChartEditModel.RowWidth,
                Stroke = ColorProvider.TrackBorderBrush,
                StrokeThickness = StrokeThickness,
                Fill = ColorProvider.TrackGradientBrush,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = TrackOpacity
            };
            // 将矩形的上下文绑定在track上
            newTrack.DataContext = track;

            track.Rectangle = newTrack;
            this.trackCanvas.Children.Add(newTrack);
            Canvas.SetLeft(newTrack, track.ColumnIndex * this.ChartEditModel.ColumnWidth + Common.TrackPadding * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(newTrack, track.StartTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 保持在最底层
            if (track.IsMiniTrack()) Canvas.SetZIndex(newTrack, MiniTrackZIndex);
            else Canvas.SetZIndex(newTrack, TrackZIndex);
        }

        /// <summary>
        /// 当时间改变时重绘Track
        /// </summary>
        public void RedrawTrackWhenTimeChanged(Track track)
        {
            track.Rectangle.Height = MinHeight + (track.EndTime.GetEquivalentBeat() - track.StartTime.GetEquivalentBeat()) * this.ChartEditModel.RowWidth;
            track.Rectangle.Width = this.ChartEditModel.ColumnWidth - 2 * Common.TrackPadding * this.ChartEditModel.ColumnWidth;
            Canvas.SetLeft(track.Rectangle, track.ColumnIndex * this.ChartEditModel.ColumnWidth + Common.TrackPadding * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(track.Rectangle, track.StartTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
        }

        /// <summary>
        /// 当时间改变时重绘HoldNote
        /// </summary>
        public void RedrawHoldNoteWhenTimeChanged(HoldNote holdNote)
        {
            holdNote.Rectangle.Height = MinHeight + (holdNote.EndTime.GetEquivalentBeat() - holdNote.Time.GetEquivalentBeat()) * this.ChartEditModel.RowWidth;
            holdNote.Rectangle.Width = this.ChartEditModel.ColumnWidth - 2 * Common.NotePadding * this.ChartEditModel.ColumnWidth;
            Canvas.SetLeft(holdNote.Rectangle, holdNote.Track.ColumnIndex * this.ChartEditModel.ColumnWidth + Common.NotePadding * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(holdNote.Rectangle, holdNote.Time.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
        }

        /// <summary>
        /// 新建一个Note
        /// </summary>
        public void CreateNoteItem(Note note)
        {
            Rectangle newNote = new Rectangle
            {
                Width = this.ChartEditModel.ColumnWidth - 2 * Common.NotePadding * this.ChartEditModel.ColumnWidth,
                StrokeThickness = StrokeThickness,
                RadiusX = Radius,
                RadiusY = Radius
            };
            // 将矩形的上下文绑定在note上
            newNote.DataContext = note;
            switch (note.Type)
            {
                case NoteType.Tap:
                    {
                        newNote.Tag = "Tap";
                        newNote.Height = MinHeight;
                        newNote.Stroke = ColorProvider.TapNoteBorderBrush;
                        newNote.Fill = ColorProvider.TapNoteBrush;
                        break;
                    }
                case NoteType.Catch:
                    {
                        newNote.Tag = "Catch";
                        newNote.Height = MinHeight;
                        newNote.Stroke = ColorProvider.CatchNoteBorderBrush;
                        newNote.Fill = ColorProvider.CatchNoteBrush;
                        break;
                    }
                case NoteType.Hold:
                    {
                        newNote.Tag = "Hold";
                        newNote.Height = MinHeight + (((HoldNote)note).EndTime.GetEquivalentBeat() - note.Time.GetEquivalentBeat()) * this.ChartEditModel.RowWidth;
                        newNote.Stroke = ColorProvider.HoldNoteBorderBrush;
                        newNote.Fill = ColorProvider.HoldNoteGradientBrush;
                        break;
                    }
                case NoteType.Flick:
                    {
                        newNote.Tag = "Flick";
                        newNote.Height = MinHeight;
                        newNote.Stroke = ColorProvider.FlickNoteBorderBrush;
                        newNote.Fill = ColorProvider.FlickNoteGradientBrush;
                        break;
                    }
            }
            note.Rectangle = newNote;
            this.trackCanvas.Children.Add(newNote);
            Canvas.SetLeft(newNote, note.Track.ColumnIndex * this.ChartEditModel.ColumnWidth + Common.NotePadding * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(newNote, note.Time.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // HoldNote在其他Note下层
            if (note.Type == NoteType.Hold)
            {
                Canvas.SetZIndex(newNote, HoldNoteZIndex);
            }
            else
            {
                Canvas.SetZIndex(newNote, NoteZIndex);
            }
        }

        /// <summary>
        /// 移除一个音符图形
        /// </summary>
        public void RemoveNote(Note note)
        {
            if (note == null) return;
            this.trackCanvas.Children.Remove(note.Rectangle);
        }

        /// <summary>
        /// 在指定位置显示预览图形
        /// </summary>
        public void ShowPreviewerAt(Point? point)
        {
            if (!point.HasValue || !this.ifShowPreviewer) return;
            // 判断位置是否相同
            BeatTime newBeatTime = this.ChartEditModel.GetBeatTimeFromPoint(point, this.trackCanvas.ActualHeight);
            int columnIndex = this.ChartEditModel.GetColumnIndexFromPoint(point);
            if (this.lastPreviewBeatTime.IsEqualTo(newBeatTime) && this.lastPreviewColumnIndex == columnIndex && this.isPreviewerShowing)
            {
                return;
            }
            this.lastPreviewBeatTime = newBeatTime;
            this.lastPreviewColumnIndex = columnIndex;
            if (!this.trackCanvas.Children.Contains(this.previewers[this.previewerIndex])) this.trackCanvas.Children.Add(this.previewers[this.previewerIndex]);
            // 重置宽度和位置
            this.previewers[this.previewerIndex].Width = this.ChartEditModel.ColumnWidth - (this.previewerIndex == 0 ? Common.TrackPadding : Common.NotePadding) * 2 * this.ChartEditModel.ColumnWidth;
            Canvas.SetLeft(this.previewers[this.previewerIndex], columnIndex * this.ChartEditModel.ColumnWidth + (this.previewerIndex == 0 ? Common.TrackPadding : Common.NotePadding) * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.previewers[this.previewerIndex], newBeatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 确保在最顶层
            Canvas.SetZIndex(this.previewers[this.previewerIndex], PreviewerZIndex);
            this.isPreviewerShowing = true;
        }

        public void ShowPreviewer()
        {
            this.ifShowPreviewer = true;
        }

        public void HidePreviewer()
        {
            this.ifShowPreviewer = false;
            if (this.previewerIndex != -1) this.trackCanvas.Children.Remove(this.previewers[this.previewerIndex]);
            this.isPreviewerShowing = false;
        }

        /// <summary>
        /// 在指定位置显示TrackHeader
        /// </summary>
        public void ShowTrackHeaderAt(BeatTime beatTime, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.ChartEditModel.ColumnNum) return;
            this.isTrackHeaderShowing = true;
            // 重置宽度和位置
            this.lastTrackHeaderBeatTime = beatTime;
            this.lastTrackHeaderColumnIndex = columnIndex;
            this.trackHeader.Width = this.ChartEditModel.ColumnWidth - 2 * Common.TrackPadding * this.ChartEditModel.ColumnWidth;
            this.trackCanvas.Children.Add(this.trackHeader);
            Canvas.SetLeft(this.trackHeader, columnIndex * this.ChartEditModel.ColumnWidth + Common.TrackPadding * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.trackHeader, beatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 在次顶层
            Canvas.SetZIndex(this.trackHeader, HeaderZIndex);
        }

        /// <summary>
        /// 隐藏TrackHeader
        /// </summary>
        public void HideTrackHeader()
        {
            this.isTrackHeaderShowing = false;
            this.trackCanvas.Children.Remove(this.trackHeader);
        }

        /// <summary>
        /// 在指定位置显示HoldNoteHeader
        /// </summary>
        public void ShowHoldNoteHeaderAt(BeatTime beatTime, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.ChartEditModel.ColumnNum) return;
            this.isHoldNoteHeaderShowing = true;
            // 重置宽度和位置
            this.lastHoldNoteHeaderBeatTime = beatTime;
            this.lastHoldNoteHeaderColumnIndex = columnIndex;
            this.holdNoteHeader.Width = this.ChartEditModel.ColumnWidth - 2 * Common.NotePadding * this.ChartEditModel.ColumnWidth;
            this.trackCanvas.Children.Add(this.holdNoteHeader);
            Canvas.SetLeft(this.holdNoteHeader, columnIndex * this.ChartEditModel.ColumnWidth + Common.NotePadding * this.ChartEditModel.ColumnWidth);
            Canvas.SetBottom(this.holdNoteHeader, beatTime.GetJudgeLineOffset(this.ChartEditModel.RowWidth));
            // 在次顶层
            Canvas.SetZIndex(this.trackHeader, HeaderZIndex);
        }

        /// <summary>
        /// 隐藏HoldNoteHeader
        /// </summary>
        public void HideHoldNoteHeader()
        {
            this.isHoldNoteHeaderShowing = false;
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
            this.isPreviewerShowing = false;
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
        /// 让一个矩形高光
        /// </summary>
        public void RectHighLight(Rectangle rectangle)
        {
            if (rectangle == null) return;
            rectangle.StrokeThickness = HighLightStrokeThickness;
            // 设置虚线
            rectangle.StrokeDashArray = new DoubleCollection() { 2, 2, 6, 2 };
            // 启动虚线旋转动画
            rectangle.BeginAnimation(Shape.StrokeDashOffsetProperty, RectangleStrokeAnimation);
            // 略微突出ZIndex
            if (rectangle.DataContext is Track track)
            {
                if (track.IsMiniTrack()) Canvas.SetZIndex(rectangle, MiniTrackZIndex + 1);
                else Canvas.SetZIndex(rectangle, TrackZIndex + 1);
            }
            else if (rectangle.DataContext is Note note)
            {
                Canvas.SetZIndex(rectangle, (note.Type == NoteType.Hold ? HoldNoteZIndex : NoteZIndex) + 1);
            }
        }

        /// <summary>
        /// 让一个矩形被选中
        /// </summary>
        public void RectPicked(Rectangle rectangle)
        {
            if (rectangle == null) return;
            rectangle.StrokeDashArray = null;
            rectangle.StrokeThickness = PickedStrokeThickness;
            // 略微突出ZIndex
            if (rectangle.DataContext is Track track)
            {
                if (track.IsMiniTrack()) Canvas.SetZIndex(rectangle, MiniTrackZIndex + 2);
                else Canvas.SetZIndex(rectangle, TrackZIndex + 2);
            }
            else if (rectangle.DataContext is Note note)
            {
                Canvas.SetZIndex(rectangle, (note.Type == NoteType.Hold ? HoldNoteZIndex : NoteZIndex) + 2);
            }
        }

        /// <summary>
        /// 让一个矩形恢复普通状态
        /// </summary>
        public void ClearRectState(Rectangle rectangle)
        {
            if (rectangle == null) return;
            rectangle.StrokeThickness = StrokeThickness;
            rectangle.StrokeDashArray = null;
            // 停止虚线动画
            rectangle.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
            // 恢复ZIndex
            if (rectangle.DataContext is Track track)
            {
                if (track.IsMiniTrack()) Canvas.SetZIndex(rectangle, MiniTrackZIndex);
                else Canvas.SetZIndex(rectangle, TrackZIndex);
            }
            else if (rectangle.DataContext is Note note)
            {
                Canvas.SetZIndex(rectangle, note.Type == NoteType.Hold ? HoldNoteZIndex : NoteZIndex);
            }
        }

        /// <summary>
        /// 初始化预览图形
        /// </summary>
        private void InitPreviewNotes()
        {
            double noteWidth = (1.0 - 2 * Common.NotePadding) * Common.ColumnWidth;
            double trackWidth = (1.0 - 2 * Common.TrackPadding) * Common.ColumnWidth;
            // 5种预览图形
            this.previewers.Add(new Rectangle
            {
                Name = "TrackPreviewer",
                Tag = "Previewer",
                Width = trackWidth,
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
                Tag = "Previewer",
                Width = noteWidth,
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
                Tag = "Previewer",
                Width = noteWidth,
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
                Tag = "Previewer",
                Width = noteWidth,
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
                Tag = "Previewer",
                Width = noteWidth,
                Height = MinHeight,
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
                Tag = "Header",
                Width = trackWidth,
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
                Tag = "Header",
                Width = noteWidth,
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
