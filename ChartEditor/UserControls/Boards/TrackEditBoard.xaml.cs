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

        public MainWindowModel MainWindowModel { get; set; }

        private TrackGridDrawer trackGridDrawer;

        private MusicPlayer musicPlayer;

        private DateTime musicStartDateTime;

        private double musicStartTime;

        private Point? lastMousePosition = null;

        private bool isScrollViewerDragging = false;

        private BeatTime notePutBeatTime = null;

        private bool isTrackPutting = false;

        private bool isHoldNotePutting = false;

        // 是否处于滚动状态
        private bool isScrolling = false;

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
            }
            else if (e.PropertyName == nameof(this.Model.RowWidth))
            {
                this.DrawTrackGrid();
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
                    
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                    // 启动时回到当前节拍
                    this.ScrollToCurrentBeat();
                    this.isInitializing = false;
                    Console.WriteLine(logTag + "加载完成");

                    // 初始化Note绘制器
                    this.noteDrawer = new NoteDrawer(TrackCanvas);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(logTag + ex.ToString());
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isScrolling)
            {
                double currentHeight = ((DateTime.Now - this.musicStartDateTime).TotalSeconds + this.musicStartTime) * this.Model.ScrollSpeed;
                TrackCanvasViewer.ScrollToVerticalOffset(this.Model.TotalHeight - currentHeight);
            }

            if (this.Model.NoteSelectedIndex != -1 && this.canvasMousePosition.HasValue && this.IsPointInTrackGrid(canvasMousePosition) && !isScrolling)
            {
                this.noteDrawer.ShowPreviewNote(this.Model.NoteSelectedIndex, this.canvasMousePosition, this.Model.ColumnWidth, this.Model.RowWidth, 
                    TrackCanvas.ActualHeight, TrackCanvasViewer.ActualHeight, this.Model.Divide);
            }
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawTrackGrid()
        {
            // 计算Canvas尺寸
            this.trackGridDrawer.DrawTrackGrid(this.Model.BeatNum, this.Model.TotalWidth, this.Model.TotalHeight, TrackCanvasViewer.ActualHeight
                , this.Model.ColumnWidth, this.Model.RowWidth, this.Model.ColumnNum, this.Model.Divide);
            TrackCanvas.Width = this.Model.TotalWidth + Common.BeatBarWidth * 2;
            TrackCanvas.Height = this.Model.TotalHeight + TrackCanvasViewer.ActualHeight;
        }

        /// <summary>
        /// 当Canvas尺寸变化时重绘
        /// </summary>
        private void TrackCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.DrawTrackGrid();
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
            if (point.Value.X <= Common.BeatBarWidth || point.Value.X >= TrackCanvas.ActualWidth - Common.BeatBarWidth) return false;
            double canvasBottomBlank = Common.JudgeLineRate * TrackCanvasViewer.ActualHeight;
            if (point.Value.Y <= TrackCanvasViewer.ActualHeight - canvasBottomBlank || point.Value.Y >= TrackCanvas.ActualHeight - canvasBottomBlank) return false;
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
                    beatTime.UpdateFromJudgeLineOffset(TrackCanvas.ActualHeight - mousePosition.Value.Y - Common.JudgeLineRate * TrackCanvasViewer.ActualHeight, this.Model.RowWidth);
                    int columnIndex = (int)((mousePosition.Value.X - Common.BeatBarWidth) / this.Model.ColumnWidth);
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
            Mouse.OverrideCursor = Cursors.Hand;
            TrackCanvasViewer.Focus();
        }

        private void PickerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
            TrackCanvasViewer.Focus();
        }

        /// <summary>
        /// 实现TrackCanvasViewer中按键相关逻辑
        /// </summary>
        private void TrackCanvasViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                this.Model.IsCatching = true;
                Mouse.OverrideCursor = Cursors.Hand;
            }
        }

        private void TrackCanvasViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                this.Model.IsCatching = true;
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
            this.PauseChart();
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

                PlayButton.IsChecked = true;
                this.Model.ResetCurrentBeatTime();
                this.ScrollToCurrentBeat();
                PlayIcon.Kind = PackIconKind.Pause;
                this.musicStartDateTime = DateTime.Now;
                this.musicStartTime = this.Model.CurrentTime;
                isScrolling = true;
            }
            catch(Exception ex)
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
                isScrolling = true;
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
            PlayButton.IsChecked = false;
            isScrolling = false;
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
            this.noteDrawer.HidePreviewNote(this.Model.NoteSelectedIndex);
            BeatTime lastBeat = new BeatTime(this.Model.CurrentBeat);
            await DialogHost.Show(new TrackGridSizeDialog(this.Model), "TrackEditDialog");
            this.isInitializing = false;
            this.ScrollToCurrentBeat(lastBeat);
            // 显示预置图形
            this.noteDrawer.ShowPreviewNote(this.Model.NoteSelectedIndex);
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
            if (isInitializing) return;
            this.Model.UpdateCurrentBeatTime(TrackCanvasViewer.VerticalOffset, TrackCanvasViewer.ExtentHeight, TrackCanvasViewer.ActualHeight);
        }

        private void TrackCanvasViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (this.isScrolling)
                {
                    this.PauseChart();
                }
                else
                {
                    this.PlayChart();
                }
            }
            
        }

        private void TrackCanvasViewer_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void NoteSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.noteDrawer.ShowPreviewNote(this.Model.NoteSelectedIndex);
        }

        private Point? canvasMousePosition = null;
        private void TrackCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isInitializing || this.isScrollViewerDragging || this.isScrolling) return;
            
        }

        private void TrackCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        /// <summary>
        /// 处理尝试添加Note事件
        /// </summary>
        private void TryPutNote(BeatTime beatTime, int columnIndex)
        {
            if (this.isTrackPutting)
            {

            }
            else if (this.isHoldNotePutting)
            {

            }
            else
            {
                if (this.Model.NoteSelectedIndex == 0)
                {
                    bool tryResult = this.Model.AddTrackHead(beatTime, columnIndex);
                    if (tryResult)
                    {
                        // 轨道头部放置成功
                        this.isTrackPutting = true;

                    }
                    else
                    {
                        this.SetMessage("此处不允许放置", 3, MessageType.Warn);
                    }
                }
            }
        }
    }
}
