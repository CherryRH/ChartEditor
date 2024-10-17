using ChartEditor.Models;
using ChartEditor.UserControls.Boards;
using ChartEditor.Utils.Drawers;
using ChartEditor.Utils.MusicUtils;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static ChartEditor.UserControls.Boards.TrackEditBoard;

namespace ChartEditor.Utils.Controllers
{
    /// <summary>
    /// 管理TrackEditBoard的功能，解耦UI交互逻辑
    /// </summary>
    public class TrackEditBoardController
    {
        private static string logTag = "[TrackEditBoardController]";

        // TrackEditBoard及常用子组件
        private TrackEditBoard TrackEditBoard;
        private Canvas TrackCanvas { get { return TrackEditBoard.TrackCanvas; } }
        private ChartEditModel ChartEditModel { get { return TrackEditBoard.Model; } }

        /// <summary>
        /// 绘图器
        /// </summary>
        private NoteDrawer noteDrawer;

        private TrackGridDrawer trackGridDrawer;

        private TimeLineDrawer timeLineDrawer;

        private bool isSizeChanging = false;

        /// <summary>
        /// 音乐播放
        /// </summary>
        private MusicPlayer musicPlayer;

        private DateTime musicStartDateTime;
        public DateTime MusicStartDateTime { get { return musicStartDateTime; } }

        private double musicStartTime;
        public double MusicStartTime { get { return musicStartTime; } }
        // 播放状态
        private bool isPlaying = false;
        public bool IsPlaying { get { return isPlaying; } }

        /// <summary>
        /// 鼠标控制
        /// </summary>
        public Point? DragMousePosition { get; private set; }
        public Point? CanvasMousePosition { get; private set; }

        private bool isScrollViewerDragging = false;
        public bool IsScrollViewerDragging { get { return isScrollViewerDragging; } }
        // 选择状态
        private bool isPicking = false;
        public bool IsPicking { get { return isPicking; } }

        // 键盘状态
        public bool IsShiftDown { get; private set; }
        public bool IsCtrlDown { get; private set; }
        public bool IsAltDown { get; private set; }

        /// <summary>
        /// 放置状态
        /// </summary>
        // TrackHeader的位置
        private BeatTime trackHeaderPutBeatTime = null;
        private int trackHeaderPutColumnIndex = 0;

        // HoldNoteHeader的位置
        private BeatTime holdNoteHeaderPutBeatTime = null;
        private int holdNoteHeaderPutColumnIndex = 0;

        // Track正在放置中
        private bool isTrackPutting = false;
        public bool IsTrackPutting { get { return isTrackPutting; } }

        // HoldNote正在放置中
        private bool isHoldNotePutting = false;
        public bool IsHoldNotePutting { get { return isHoldNotePutting; } }

        public TrackEditBoardController(TrackEditBoard trackEditBoard)
        {
            this.TrackEditBoard = trackEditBoard;
            this.trackGridDrawer = new TrackGridDrawer();
            this.TrackCanvas.Children.Add(this.trackGridDrawer);
            this.timeLineDrawer = new TimeLineDrawer();
            this.TrackEditBoard.TimeLineCanvas.Children.Add(this.timeLineDrawer);
        }

        /// <summary>
        /// 加载
        /// </summary>
        public bool Load()
        {
            try
            {
                // 初始化音乐播放器
                this.musicPlayer = new MusicPlayer(this.TrackEditBoard.Model.ChartInfo, this.MusicPlayer_PlaybackStopped);
                // 绘制网格
                this.DrawTrackGrid();
                // 绘制时间轴
                this.DrawTimeLine();
                // 初始化Note绘制器
                this.noteDrawer = new NoteDrawer(this.TrackEditBoard);
                Console.WriteLine(logTag + "加载完成");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + "加载失败：" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 组件实时渲染
        /// </summary>
        public void Rendering()
        {
            if (this.isPlaying)
            {
                double currentHeight = ((DateTime.Now - this.musicStartDateTime).TotalSeconds + this.musicStartTime) * this.ChartEditModel.ScrollSpeed;
                this.TrackEditBoard.TrackCanvasViewer.ScrollToVerticalOffset(this.ChartEditModel.TotalHeight - currentHeight);
            }

            if (this.ChartEditModel.NoteSelectedIndex != -1 && this.CanvasMousePosition.HasValue && this.TrackEditBoard.IsPointInTrackGrid(this.CanvasMousePosition) && !this.isPlaying && !this.isPicking)
            {
                this.noteDrawer.ShowPreviewerAt(this.CanvasMousePosition);
            }
        }

        private void MusicPlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            this.TrackEditBoard.PlayButton.IsChecked = false;
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer中移动
        /// </summary>
        public void OnMouseMoveInTrackCanvasViewer(Point? mousePosition)
        {
            if (this.isPlaying) return;

            if (this.isScrollViewerDragging && this.DragMousePosition.HasValue)
            {
                double deltaX = mousePosition.Value.X - this.DragMousePosition.Value.X;
                double deltaY = mousePosition.Value.Y - this.DragMousePosition.Value.Y;
                this.TrackEditBoard.TrackCanvasViewer.ScrollToHorizontalOffset(this.TrackEditBoard.TrackCanvasViewer.HorizontalOffset - deltaX);
                this.TrackEditBoard.TrackCanvasViewer.ScrollToVerticalOffset(this.TrackEditBoard.TrackCanvasViewer.VerticalOffset - deltaY);
                this.DragMousePosition = mousePosition;
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer中点击
        /// </summary>
        public void OnMouseDownInTrackCanvasViewer(Point? mousePosition, MouseButtonState mouseButtonState)
        {
            if (this.isPlaying)
            {
                // 暂停播放
                this.TrackEditBoard.PlayButton.IsChecked = false;
            }
            else
            {
                if (mouseButtonState == MouseButtonState.Pressed && !this.TrackEditBoard.IsPointOverScrollBar(mousePosition))
                {
                    this.DragMousePosition = mousePosition;
                    this.isScrollViewerDragging = true;
                    this.TrackEditBoard.TrackCanvasViewer.CaptureMouse();
                }
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer中释放
        /// </summary>
        public void OnMouseUpInTrackCanvasViewer(Point? mousePosition)
        {
            if (this.isPlaying) return;
            if (this.isScrollViewerDragging)
            {
                this.isScrollViewerDragging = false;
                this.DragMousePosition = null;
                TrackEditBoard.TrackCanvasViewer.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvas中移动
        /// </summary>
        public void OnMouseMoveInTrackCanvas(Point? mousePosition)
        {
            if (this.isPlaying) return;
            this.CanvasMousePosition = mousePosition;
        }

        /// <summary>
        /// 当鼠标在TrackCanvas中点击
        /// </summary>
        public void OnMouseDownInTrackCanvas(Point? mousePosition, MouseButtonState mouseButtonState)
        {
            if (this.isPlaying)
            {
                // 暂停播放
                this.TrackEditBoard.PlayButton.IsChecked = false;
            }
            else
            {
                BeatTime beatTime = this.ChartEditModel.GetBeatTimeFromPoint(mousePosition, this.TrackEditBoard.TrackCanvas.ActualHeight);
                int columnIndex = this.ChartEditModel.GetColumnIndexFromPoint(mousePosition);
                if (this.isPicking)
                {
                    // 选择事件

                }
                else
                {
                    // 放置事件
                    this.TryPutNote(beatTime, columnIndex);
                }
            }
        }

        /// <summary>
        /// 当Note选择改变时
        /// </summary>
        public void OnNoteSelectionChanged()
        {
            // 退出picking状态
            if (this.isPicking) this.TrackEditBoard.PickerButton.IsChecked = false;
            // 清除当前对应放置状态
            if (this.noteDrawer.PreviewerIndex == 0)
            {
                this.isTrackPutting = false;
                this.noteDrawer.HideTrackHeader();
            }
            else if (this.noteDrawer.PreviewerIndex == 3)
            {
                this.isHoldNotePutting = false;
                this.noteDrawer.HideHoldNoteHeader();
            }
            // 切换预览图形
            this.noteDrawer.SwitchPreviewer(this.ChartEditModel.NoteSelectedIndex);
        }

        /// <summary>
        /// 设置isPicking
        /// </summary>
        public void SetIsPicking(bool isPicking)
        {
            if (isPicking == this.isPicking) return;
            this.isPicking = isPicking;
            if (this.isPicking)
            {
                this.noteDrawer.HidePreviewer();
                // 清除所有放置状态
                if (this.isTrackPutting)
                {
                    this.isTrackPutting = false;
                    this.noteDrawer.HideTrackHeader();
                }
                else if (this.isHoldNotePutting)
                {
                    this.isHoldNotePutting = false;
                    this.noteDrawer.HideHoldNoteHeader();
                }
            }
            else
            {
                this.noteDrawer.ShowPreviewer();
            }
        }

        /// <summary>
        /// 设置TrackCanvas的Size变化状态
        /// </summary>
        public void SetIsSizeChanging(bool isSizeChanging)
        {
            if (isSizeChanging == this.isSizeChanging) return;
            this.isSizeChanging = isSizeChanging;
            if (this.isSizeChanging)
            {

            }
            else
            {

            }
        }

        /// <summary>
        /// 设置Shift状态，Shift用于切换放置模式和选择模式
        /// </summary>
        public void SetShift(bool isDown)
        {
            if (isDown == this.IsShiftDown) return;
            this.IsShiftDown = isDown;
            if (this.IsShiftDown)
            {
                this.TrackEditBoard.PickerButton.IsChecked = true;
                Mouse.OverrideCursor = Cursors.Hand;
            }
            else
            {
                this.TrackEditBoard.PickerButton.IsChecked = false;
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// 设置Ctrl状态，Ctrl用于切换至视图操作模式（停止网格操作）
        /// </summary>
        public void SetCtrl(bool isDown)
        {
            if (isDown == this.IsCtrlDown) return;
            this.IsCtrlDown = isDown;
        }

        /// <summary>
        /// 重播谱面
        /// </summary>
        public void ReplayChart()
        {
            try
            {
                if (!this.musicPlayer.ReplayMusic())
                {
                    this.TrackEditBoard.SetMessage("音乐播放异常，再试一次吧", 3, MessageType.Error);
                    return;
                }
                this.TrackEditBoard.PlayButton.IsChecked = true;
                this.TrackEditBoard.PlayIcon.Kind = PackIconKind.Pause;
                this.ChartEditModel.ResetCurrentBeatTime();
                this.TrackEditBoard.ScrollToCurrentBeat();
                this.musicStartDateTime = DateTime.Now;
                this.musicStartTime = this.ChartEditModel.CurrentTime;
                isPlaying = true;
                this.noteDrawer.HidePreviewer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());

            }
        }

        /// <summary>
        /// 播放谱面
        /// </summary>
        public void PlayChart()
        {
            try
            {
                if (this.musicPlayer.IsMusicOver(this.ChartEditModel.CurrentTime))
                {
                    this.ReplayChart();
                    return;
                }
                if (!this.musicPlayer.PlayMusic(this.ChartEditModel.CurrentTime))
                {
                    this.TrackEditBoard.SetMessage("音乐播放异常，再试一次吧", 3, MessageType.Error);
                    return;
                }

                this.TrackEditBoard.PlayIcon.Kind = PackIconKind.Pause;
                this.musicStartDateTime = DateTime.Now;
                this.musicStartTime = this.ChartEditModel.CurrentTime;
                this.isPlaying = true;
                this.noteDrawer.HidePreviewer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());

            }
        }

        /// <summary>
        /// 暂停谱面
        /// </summary>
        public void PauseChart()
        {
            this.musicPlayer.PauseMusic();

            this.TrackEditBoard.PlayIcon.Kind = PackIconKind.Play;
            this.isPlaying = false;
            this.noteDrawer.ShowPreviewer();
        }

        /// <summary>
        /// 处理尝试添加Note事件
        /// </summary>
        private void TryPutNote(BeatTime beatTime, int columnIndex)
        {
            if (this.isPicking || this.isPlaying) return;
            switch (this.TrackEditBoard.Model.NoteSelectedIndex)
            {
                case 0:
                    {
                        if (this.isTrackPutting)
                        {
                            Track newTrack = this.ChartEditModel.AddTrackFooter(this.trackHeaderPutBeatTime, this.trackHeaderPutColumnIndex, beatTime, columnIndex);
                            if (newTrack != null)
                            {
                                this.noteDrawer.HideTrackHeader();
                                // 显示轨道
                                this.noteDrawer.CreateTrackItem(newTrack);
                                this.isTrackPutting = false;
                            }
                            else
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        else
                        {
                            bool tryResult = this.ChartEditModel.AddTrackHeader(beatTime, columnIndex);
                            if (tryResult)
                            {
                                // 轨道头部放置成功，记录位置
                                this.isTrackPutting = true;
                                this.trackHeaderPutBeatTime = beatTime;
                                this.trackHeaderPutColumnIndex = columnIndex;
                                this.noteDrawer.ShowTrackHeaderAt(beatTime, columnIndex);
                            }
                            else
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        TapNote newTapNote = (TapNote)this.ChartEditModel.AddNote(beatTime, columnIndex, NoteType.Tap);
                        if (newTapNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNoteItem(newTapNote);
                        }
                        else
                        {
                            this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                        }
                        break;
                    }
                case 2:
                    {
                        FlickNote newFlickNote = (FlickNote)this.ChartEditModel.AddNote(beatTime, columnIndex, NoteType.Flick);
                        if (newFlickNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNoteItem(newFlickNote);
                        }
                        else
                        {
                            this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                        }
                        break;
                    }
                case 3:
                    {
                        if (this.isHoldNotePutting)
                        {
                            HoldNote newHoldNote = this.ChartEditModel.AddHoldNoteFooter(this.holdNoteHeaderPutBeatTime, this.holdNoteHeaderPutColumnIndex, beatTime, columnIndex);
                            if (newHoldNote != null)
                            {
                                this.noteDrawer.HideHoldNoteHeader();
                                this.noteDrawer.CreateNoteItem(newHoldNote);
                                this.isHoldNotePutting = false;
                            }
                            else
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        else
                        {
                            bool tryResult = this.ChartEditModel.AddHoldNoteHeader(beatTime, columnIndex);
                            if (tryResult)
                            {
                                // HoldNote头部放置成功，记录位置
                                this.isHoldNotePutting = true;
                                this.holdNoteHeaderPutBeatTime = beatTime;
                                this.holdNoteHeaderPutColumnIndex = columnIndex;
                                this.noteDrawer.ShowHoldNoteHeaderAt(beatTime, columnIndex);
                            }
                            else
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        break;
                    }
                case 4:
                    {
                        CatchNote newCatchNote = (CatchNote)this.ChartEditModel.AddNote(beatTime, columnIndex, NoteType.Catch);
                        if (newCatchNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNoteItem(newCatchNote);
                        }
                        else
                        {
                            this.TrackEditBoard.SetMessage("此处不允许放置", 3, MessageType.Warn);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 更改音量，0-1之间
        /// </summary>
        public void OnMusicVolumeChanged(float musicVolume)
        {
            this.musicPlayer.SetVolume(musicVolume);
        }

        /// <summary>
        /// 当列宽变化时
        /// </summary>
        public void OnColumnWidthChanged()
        {
            this.DrawTrackGrid();
            this.noteDrawer.RedrawWhenColumnWidthChanged();
        }

        /// <summary>
        /// 当行宽变化时
        /// </summary>
        public void OnRowWidthChanged()
        {
            this.DrawTrackGrid();
            this.DrawTimeLine();
            this.noteDrawer.RedrawWhenRowWidthChanged();
        }

        /// <summary>
        /// 当分割变化时
        /// </summary>
        public void OnDivideChanged()
        {
            this.DrawTrackGrid();
            this.TrackEditBoard.UpdateCurrentBeatTime();
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawTrackGrid()
        {
            this.trackGridDrawer.DrawTrackGrid(this.ChartEditModel.BeatNum, this.ChartEditModel.TotalWidth, this.ChartEditModel.TotalHeight
                , this.ChartEditModel.ColumnWidth, this.ChartEditModel.RowWidth, this.ChartEditModel.ColumnNum, this.ChartEditModel.Divide);
            // 计算Canvas尺寸
            this.TrackCanvas.Width = this.ChartEditModel.TotalWidth;
            this.TrackCanvas.Height = this.ChartEditModel.TotalHeight;
            this.TrackEditBoard.TrackCanvasFloor.Width = this.ChartEditModel.TotalWidth + 50;
            this.TrackEditBoard.TrackCanvasFloor.Height = this.ChartEditModel.TotalHeight + this.TrackEditBoard.TrackCanvasViewer.ActualHeight;
            Canvas.SetTop(this.TrackCanvas, this.TrackEditBoard.TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
        }

        /// <summary>
        /// 绘制时间轴
        /// </summary>
        private void DrawTimeLine()
        {
            this.timeLineDrawer.DrawTimeLine(this.ChartEditModel.BeatNum, this.ChartEditModel.RowWidth);
            // 计算Canvas尺寸
            this.TrackEditBoard.TimeLineCanvas.Height = this.ChartEditModel.TotalHeight;
            this.TrackEditBoard.TimeLineCanvasFloor.Height = this.ChartEditModel.TotalHeight + this.TrackEditBoard.TimeLineCanvasViewer.ActualHeight;
            Canvas.SetTop(this.TrackEditBoard.TimeLineCanvas, this.TrackEditBoard.TimeLineCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.musicPlayer.Dispose();
            GC.Collect();
            Console.WriteLine(logTag + "占用文件已释放");
        }
    }
}
