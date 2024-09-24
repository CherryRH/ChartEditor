using ChartEditor.Models;
using ChartEditor.UserControls.Dialogs;
using ChartEditor.Utils;
using ChartEditor.Utils.Drawers;
using ChartEditor.Utils.MusicUtils;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NAudio.Vorbis;
using NAudio.Wave;
using NVorbis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChartEditor.UserControls.Boards
{
    /// <summary>
    /// TrackEditBoard.xaml 的交互逻辑
    /// </summary>
    public partial class TrackEditBoard : UserControl
    {
        private static string logTag = "[TrackEditBoard]";

        // 是否处于初始化或加载中
        private bool isInitializing = true;

        private NoteDrawer noteDrawer;

        private TrackGridDrawer trackGridDrawer;

        private TimeLineDrawer timeLineDrawer;

        public MainWindowModel MainWindowModel { get; set; }

        private MusicPlayer musicPlayer;

        private DateTime musicStartDateTime;

        private double musicStartTime;

        private Point? lastMousePosition = null;

        private bool isScrollViewerDragging = false;

        // TrackHeader的位置
        private BeatTime trackHeaderPutBeatTime = null;
        private int trackHeaderPutColumnIndex = 0;

        // HoldNoteHeader的位置
        private BeatTime holdNoteHeaderPutBeatTime = null;
        private int holdNoteHeaderPutColumnIndex = 0;

        // Track正在放置中
        private bool isTrackPutting = false;

        // HoldNote正在放置中
        private bool isHoldNotePutting = false;

        // 播放状态
        private bool isPlaying = false;

        // 选择状态
        private bool isPicking = false;

        public ChartEditModel Model
        {
            get { return (ChartEditModel)this.DataContext; }
        }

        public TrackEditBoard()
        {
            InitializeComponent();
            this.Loaded += TrackEditBoard_Loaded;
            this.trackGridDrawer = new TrackGridDrawer();
            TrackCanvas.Children.Add(this.trackGridDrawer);
            this.timeLineDrawer = new TimeLineDrawer();
            TimeLineCanvas.Children.Add(this.timeLineDrawer);
            // 设置UI颜色
            this.SetUIColor();
        }

        ~TrackEditBoard()
        {
            
        }

        /// <summary>
        /// 释放占用的文件
        /// </summary>
        public void Dispose()
        {
            this.musicPlayer.Dispose();
            GC.Collect();
            Console.WriteLine(logTag + "占用文件已释放");
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Model.MusicVolume))
            {
                this.musicPlayer.SetVolume(this.Model.MusicVolume / 100);
            }
            else if (e.PropertyName == nameof(this.Model.ColumnWidth))
            {
                this.DrawTrackGrid();
                this.noteDrawer.RedrawWhenColumnWidthChanged();
            }
            else if (e.PropertyName == nameof(this.Model.RowWidth))
            {
                this.DrawTrackGrid();
                this.DrawTimeLine();
                this.noteDrawer.RedrawWhenRowWidthChanged();
            }
            else if (e.PropertyName == nameof(this.Model.Divide))
            {
                this.DrawTrackGrid();
                this.Model.UpdateCurrentBeatTime(TrackCanvasViewer.VerticalOffset, TrackCanvasViewer.ExtentHeight, TrackCanvasViewer.ActualHeight);
            }
        }

        private void TrackEditBoard_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Model is ChartEditModel)
                {
                    this.Model.PropertyChanged += Model_PropertyChanged;
                    // 初始化音乐播放器
                    this.musicPlayer = new MusicPlayer(this.Model.ChartInfo, this.MusicPlayer_PlaybackStopped);
                    
                    // 添加UI同步绘制
                    CompositionTarget.Rendering += CompositionTarget_Rendering;

                    // 绘制网格
                    this.DrawTrackGrid();

                    // 绘制时间轴
                    this.DrawTimeLine();

                    // 启动时回到当前节拍
                    this.ScrollToCurrentBeat();
                    this.isInitializing = false;
                    Console.WriteLine(logTag + "加载完成");

                    // 初始化Note绘制器
                    this.noteDrawer = new NoteDrawer(TrackCanvas, TrackCanvasViewer, this.Model);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                double currentHeight = ((DateTime.Now - this.musicStartDateTime).TotalSeconds + this.musicStartTime) * this.Model.ScrollSpeed;
                TrackCanvasViewer.ScrollToVerticalOffset(this.Model.TotalHeight - currentHeight);
            }

            if (this.Model.NoteSelectedIndex != -1 && this.canvasMousePosition.HasValue && this.IsPointInTrackGrid(canvasMousePosition) && !isPlaying && !isPicking)
            {
                this.noteDrawer.ShowPreviewerAt(this.canvasMousePosition, this.Model.Divide);
            }
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawTrackGrid()
        {
            this.trackGridDrawer.DrawTrackGrid(this.Model.BeatNum, this.Model.TotalWidth, this.Model.TotalHeight
                , this.Model.ColumnWidth, this.Model.RowWidth, this.Model.ColumnNum, this.Model.Divide);
            // 计算Canvas尺寸
            TrackCanvas.Width = this.Model.TotalWidth;
            TrackCanvas.Height = this.Model.TotalHeight;
            TrackCanvasFloor.Width = this.Model.TotalWidth + 50;
            TrackCanvasFloor.Height = this.Model.TotalHeight + TrackCanvasViewer.ActualHeight;
            Canvas.SetTop(TrackCanvas, TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
        }

        /// <summary>
        /// 绘制时间轴
        /// </summary>
        private void DrawTimeLine()
        {
            this.timeLineDrawer.DrawTimeLine(this.Model.BeatNum, this.Model.RowWidth);
            // 计算Canvas尺寸
            TimeLineCanvas.Height = this.Model.TotalHeight;
            TimeLineCanvasFloor.Height = this.Model.TotalHeight + TimeLineCanvasViewer.ActualHeight;
            Canvas.SetTop(TimeLineCanvas, TimeLineCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
        }

        /// <summary>
        /// 当CanvasViewer尺寸变化时重绘
        /// </summary>
        private void TrackCanvasViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TrackCanvasFloor.Height = this.Model.TotalHeight + TrackCanvasViewer.ActualHeight;
            Canvas.SetTop(TrackCanvas, TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
            this.DrawJudgementLine();
        }

        /// <summary>
        /// 绘制判定线
        /// </summary>
        private void DrawJudgementLine()
        {
            JudgementLine.X1 = 0;
            JudgementLine.Y1 = TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp;
            JudgementLine.X2 = TrackCanvasViewer.ActualWidth;
            JudgementLine.Y2 = TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp;
        }

        /// <summary>
        /// 跳转到当前Beat的位置
        /// </summary>
        private void ScrollToCurrentBeat(BeatTime beat = null)
        {
            BeatTime currentBeat = null;
            if (beat != null)
            {
                currentBeat = beat;
            }
            else
            {
                currentBeat = this.Model.CurrentBeat;
            }
            TrackCanvasViewer.ScrollToVerticalOffset(TrackCanvasViewer.ExtentHeight - TrackCanvasViewer.ActualHeight - currentBeat.GetJudgeLineOffset(this.Model.RowWidth));
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        private void SetMessage(string message, double time, MessageType messageType = MessageType.Notice)
        {
            MessageBoxText.Text = message;
            switch (messageType)
            {
                case MessageType.Notice:
                    {
                        MessageBox.BorderBrush = System.Windows.Media.Brushes.Black;
                        MessageBoxText.Foreground = System.Windows.Media.Brushes.Black;
                        break;
                    }
                case MessageType.Warn:
                    {
                        MessageBox.BorderBrush = System.Windows.Media.Brushes.Orange;
                        MessageBoxText.Foreground = System.Windows.Media.Brushes.Orange;
                        break;
                    }
                case MessageType.Error:
                    {
                        MessageBox.BorderBrush = System.Windows.Media.Brushes.Red;
                        MessageBoxText.Foreground = System.Windows.Media.Brushes.Red;
                        break;
                    }
            }
            
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                BeginTime = TimeSpan.FromSeconds(time)
            };
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Children.Add(fadeOutAnimation);
            Storyboard.SetTarget(fadeInAnimation, MessageBox);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(fadeOutAnimation, MessageBox);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Begin();
        }

        public enum MessageType
        {
            Notice,
            Warn,
            Error
        }

        /// <summary>
        /// 判断一个点是否在网格范围内
        /// </summary>
        private bool IsPointInTrackGrid(Point? point)
        {
            if (point.Value.X <= 0 || point.Value.X >= TrackCanvas.ActualWidth) return false;
            if (point.Value.Y <= 0 || point.Value.Y >= TrackCanvas.ActualHeight) return false;
            return true;
        }

        /// <summary>
        /// 实现Ctrl+鼠标左键拖动Canvas的效果
        /// 实现Note的预览与放置
        /// </summary>
        private void TrackCanvasViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    lastMousePosition = e.GetPosition(TrackCanvasViewer);
                    this.isScrollViewerDragging = true;
                    TrackCanvasViewer.CaptureMouse();
                }
            }
            else
            {
                // 放置Note事件
                Point? mousePosition = e.GetPosition(TrackCanvas);
                if (mousePosition.HasValue && this.IsPointInTrackGrid(mousePosition))
                {
                    BeatTime beatTime = new BeatTime(this.Model.Divide);
                    beatTime.UpdateFromJudgeLineOffset(TrackCanvas.Height - mousePosition.Value.Y, this.Model.RowWidth);
                    int columnIndex = (int)(mousePosition.Value.X / this.Model.ColumnWidth);
                    this.TryPutNote(beatTime, columnIndex);
                }
            }
        }

        private void TrackCanvasViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.isScrollViewerDragging && lastMousePosition.HasValue)
            {
                Point currentMousePosition = e.GetPosition(TrackCanvasViewer);
                double deltaX = currentMousePosition.X - lastMousePosition.Value.X;
                double deltaY = currentMousePosition.Y - lastMousePosition.Value.Y;
                TrackCanvasViewer.ScrollToHorizontalOffset(TrackCanvasViewer.HorizontalOffset - deltaX);
                TrackCanvasViewer.ScrollToVerticalOffset(TrackCanvasViewer.VerticalOffset - deltaY);
                lastMousePosition = currentMousePosition;
            }
            this.canvasMousePosition = e.GetPosition(TrackCanvas);
        }

        private void TrackCanvasViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isScrollViewerDragging)
            {
                this.isScrollViewerDragging = false;
                lastMousePosition = null;
                TrackCanvasViewer.ReleaseMouseCapture();
            }
        }

        private void PickerButton_Checked(object sender, RoutedEventArgs e)
        {
            this.isPicking = true;
            Mouse.OverrideCursor = Cursors.Hand;
            PickerButton.Background = Brushes.LightGray;
            TrackCanvasViewer.Focus();
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

        private void PickerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isPicking = false;
            Mouse.OverrideCursor = null;
            PickerButton.Background = Brushes.White;
            TrackCanvasViewer.Focus();
            this.noteDrawer.ShowPreviewer();
        }

        /// <summary>
        /// 实现TrackCanvasViewer中按键相关逻辑
        /// </summary>
        private void TrackCanvasViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                PickerButton.IsChecked = true;
                Mouse.OverrideCursor = Cursors.Hand;
            }
        }

        private void TrackCanvasViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                PickerButton.IsChecked = false;
                Mouse.OverrideCursor = null;
            }
        }

        private async void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new VolumeDialog(this.Model), "TrackEditDialog");
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReplayChart();
        }

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            this.PlayChart();
        }

        private void MusicPlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlayButton.IsChecked = false;
        }

        /// <summary>
        /// 重播谱面
        /// </summary>
        private void ReplayChart()
        {
            try
            {
                if (!this.musicPlayer.ReplayMusic())
                {
                    this.SetMessage("音乐播放异常，再试一次吧", 3, MessageType.Error);
                    return;
                }
                // 临时取消事件订阅，防止触发 PlayButton_Checked
                PlayButton.Checked -= PlayButton_Checked;

                PlayButton.IsChecked = true;
                PlayIcon.Kind = PackIconKind.Pause;
                this.Model.ResetCurrentBeatTime();
                this.ScrollToCurrentBeat();
                this.musicStartDateTime = DateTime.Now;
                this.musicStartTime = this.Model.CurrentTime;
                isPlaying = true;
                this.noteDrawer.HidePreviewer();
                // 恢复事件订阅
                PlayButton.Checked += PlayButton_Checked;
            }
            catch (Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
                
            }
        }

        /// <summary>
        /// 播放谱面
        /// </summary>
        private void PlayChart()
        {
            try
            {
                if (this.musicPlayer.IsMusicOver(this.Model.CurrentTime))
                {
                    this.ReplayChart();
                    return;
                }
                if (!this.musicPlayer.PlayMusic(this.Model.CurrentTime))
                {
                    this.SetMessage("音乐播放异常，再试一次吧", 3, MessageType.Error);
                    return;
                }

                PlayIcon.Kind = PackIconKind.Pause;
                this.musicStartDateTime = DateTime.Now;
                this.musicStartTime = this.Model.CurrentTime;
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
        private void PauseChart()
        {
            this.musicPlayer.PauseMusic();

            PlayIcon.Kind = PackIconKind.Play;
            this.isPlaying = false;
            this.noteDrawer.ShowPreviewer();
        }

        private void PlayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.PauseChart();
        }

        private async void PlaySpeedButton_Click(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new PlaySpeedDialog(this.Model), "TrackEditDialog");
        }

        private async void SizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.isInitializing = true;
            // 隐藏Note预置图形
            this.noteDrawer.HidePreviewer();
            await DialogHost.Show(new TrackGridSizeDialog(this.Model), "TrackEditDialog");
            if (!this.isPlaying)
            {
                BeatTime lastBeat = new BeatTime(this.Model.CurrentBeat);
                this.ScrollToCurrentBeat(lastBeat);
            }
            this.isInitializing = false;
            
            // 显示预置图形
            this.noteDrawer.ShowPreviewer();
        }

        private async void DivideButton_Click(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new DivideDialog(this.Model), "TrackEditDialog");
        }

        private void BpmManagerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InfomationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TrackCanvasViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 同步时间轴滚动条
            TimeLineCanvasViewer.ScrollToVerticalOffset(TrackCanvasViewer.VerticalOffset);
            if (!this.isInitializing)
            {
                this.Model.UpdateCurrentBeatTime(TrackCanvasViewer.VerticalOffset, TrackCanvasViewer.ExtentHeight, TrackCanvasViewer.ActualHeight);
            }
        }

        private void TrackCanvasViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (this.isPlaying)
                {
                    PlayButton.IsChecked = false;
                    this.PauseChart();
                }
                else
                {
                    PlayButton.IsChecked = true;
                    this.PlayChart();
                }
            }
            
        }

        private void TrackCanvasViewer_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void NoteSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            this.noteDrawer.SwitchPreviewer(this.Model.NoteSelectedIndex);
        }

        private Point? canvasMousePosition = null;
        private void TrackCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isInitializing || this.isScrollViewerDragging || this.isPlaying) return;
            
        }

        private void TrackCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        /// <summary>
        /// 处理尝试添加Note事件
        /// </summary>
        private void TryPutNote(BeatTime beatTime, int columnIndex)
        {
            if (this.isPicking || this.isPlaying) return;
            switch (this.Model.NoteSelectedIndex)
            {
                case 0:
                    {
                        if (this.isTrackPutting)
                        {
                            Track newTrack = this.Model.AddTrackFooter(this.trackHeaderPutBeatTime, this.trackHeaderPutColumnIndex, beatTime, columnIndex);
                            if (newTrack != null)
                            {
                                this.noteDrawer.HideTrackHeader();
                                // 显示轨道
                                this.noteDrawer.CreateTrackItem(newTrack);
                                this.isTrackPutting = false;
                            }
                            else
                            {
                                this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        else
                        {
                            bool tryResult = this.Model.AddTrackHeader(beatTime, columnIndex);
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
                                this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        break;
                    }
                case 1:
                    {
                        TapNote newTapNote = (TapNote)this.Model.AddNote(beatTime, columnIndex, NoteType.Tap);
                        if (newTapNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNoteItem(newTapNote);
                        }
                        else
                        {
                            this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                        }
                        break;
                    }
                case 2:
                    {
                        FlickNote newFlickNote = (FlickNote)this.Model.AddNote(beatTime, columnIndex, NoteType.Flick);
                        if (newFlickNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNoteItem(newFlickNote);
                        }
                        else
                        {
                            this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                        }
                        break;
                    }
                case 3:
                    {
                        if (this.isHoldNotePutting)
                        {
                            HoldNote newHoldNote = this.Model.AddHoldNoteFooter(this.holdNoteHeaderPutBeatTime, this.holdNoteHeaderPutColumnIndex, beatTime, columnIndex);
                            if (newHoldNote != null)
                            {
                                this.noteDrawer.HideHoldNoteHeader();
                                this.noteDrawer.CreateNoteItem(newHoldNote);
                                this.isHoldNotePutting = false;
                            }
                            else
                            {
                                this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        else
                        {
                            bool tryResult = this.Model.AddHoldNoteHeader(beatTime, columnIndex);
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
                                this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                            }
                        }
                        break;
                    }
                case 4:
                    {
                        CatchNote newCatchNote = (CatchNote)this.Model.AddNote(beatTime, columnIndex, NoteType.Catch);
                        if (newCatchNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNoteItem(newCatchNote);
                        }
                        else
                        {
                            this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// 设置UI的颜色
        /// </summary>
        private void SetUIColor()
        {
            TrackSelectBox.Foreground = ColorProvider.TrackBorderBrush;
            TapNoteSelectBox.Foreground = ColorProvider.TapNoteBorderBrush;
            FlickNoteSelectBox.Foreground = ColorProvider.FlickNoteBorderBrush;
            HoldNoteSelectBox.Foreground = ColorProvider.HoldNoteBorderBrush;
            CatchNoteSelectBox.Foreground = ColorProvider.CatchNoteBorderBrush;
        }
    }
}
