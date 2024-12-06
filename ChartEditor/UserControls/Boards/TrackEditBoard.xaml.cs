using ChartEditor.Models;
using ChartEditor.UserControls.Dialogs;
using ChartEditor.Utils;
using ChartEditor.Utils.ChartUtils;
using ChartEditor.Utils.Controllers;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChartEditor.UserControls.Boards
{
    /// <summary>
    /// TrackEditBoard.xaml 的交互逻辑
    /// </summary>
    public partial class TrackEditBoard : UserControl
    {
        private static string logTag = "[TrackEditBoard]";

        public MainWindowModel MainWindowModel { get; set; }

        public Settings Settings { get { return MainWindowModel.Settings; } }

        private TrackEditBoardController TrackEditBoardController;

        /// <summary>
        /// 消息框队列
        /// </summary>
        public SnackbarMessageQueue MessageQueue { get; } = new SnackbarMessageQueue();

        public ChartEditModel Model
        {
            get { return (ChartEditModel)this.DataContext; }
        }

        public TrackEditBoard()
        {
            InitializeComponent();
            
            // 设置UI颜色
            this.SetUIColor();

            TrackEditBoardSnackbar.MessageQueue = this.MessageQueue;

            this.Loaded += TrackEditBoard_Loaded;
        }

        /// <summary>
        /// 释放占用的文件
        /// </summary>
        public void Dispose()
        {
            this.TrackEditBoardController.Dispose();
        }

        /// <summary>
        /// 属性变化回调
        /// </summary>
        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Model.MusicVolume))
            {
                this.TrackEditBoardController.OnMusicVolumeChanged(this.Model.MusicVolume / 100);
            }
            else if (e.PropertyName == nameof(this.Model.NoteVolume))
            {
                this.TrackEditBoardController.OnNoteVolumeChanged(this.Model.NoteVolume / 100);
            }
            else if (e.PropertyName == nameof(this.Model.GlobalVolume))
            {
                this.TrackEditBoardController.OnGlobalVolumeChanged(this.Model.GlobalVolume / 100);
            }
            else if (e.PropertyName == nameof(this.Model.ColumnWidth))
            {
                this.TrackEditBoardController.OnColumnWidthChanged();
            }
            else if (e.PropertyName == nameof(this.Model.RowWidth))
            {
                this.TrackEditBoardController.OnRowWidthChanged();
            }
            else if (e.PropertyName == nameof(this.Model.Divide))
            {
                this.TrackEditBoardController.OnDivideChanged();
            }
        }

        private void TrackEditBoard_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Model is ChartEditModel)
            {
                // 创建控制器
                this.TrackEditBoardController = new TrackEditBoardController(this);
                // 加载控制器
                if (!this.TrackEditBoardController.Load())
                {
                    // 加载失败
                    return;
                }
                
                // 启动时回到当前节拍
                this.ScrollToCurrentBeat();

                this.Model.PropertyChanged += Model_PropertyChanged;
                // 添加UI同步绘制
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            this.TrackEditBoardController.Rendering();
        }

        /// <summary>
        /// 当CanvasViewer尺寸变化时重绘
        /// </summary>
        private void TrackCanvasViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TrackCanvasFloor.Height = this.Model.TotalHeight + TrackCanvasViewer.ActualHeight;
            Canvas.SetTop(TrackCanvas, TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
            TimeLineCanvasFloor.Height = this.Model.TotalHeight + TimeLineCanvasViewer.ActualHeight;
            Canvas.SetTop(TimeLineCanvas, TimeLineCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
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
        public void ScrollToCurrentBeat(BeatTime beat = null)
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
        /// 判断一个点是否在网格范围内
        /// </summary>
        public bool IsPointInTrackGrid(Point? point)
        {
            if (point.Value.X <= 0 || point.Value.X >= TrackCanvas.ActualWidth) return false;
            if (point.Value.Y <= 0 || point.Value.Y >= TrackCanvas.ActualHeight) return false;
            return true;
        }

        /// <summary>
        /// 判断一个点是否在滚动条上
        /// </summary>
        public bool IsPointOverScrollBar(Point? point)
        {
            var hitResult = VisualTreeHelper.HitTest(TrackCanvasViewer, point.Value)?.VisualHit;
            // 检查是否点击在滚动条上（通常是 ScrollBar 或 Track）
            while (hitResult != null)
            {
                if (hitResult is ScrollBar) return true;
                hitResult = VisualTreeHelper.GetParent(hitResult);
            }
            return false;
        }

        /// <summary>
        /// 更新当前节拍和时间
        /// </summary>
        public void UpdateCurrentBeatTime()
        {
            this.Model.UpdateCurrentBeatTime(TrackCanvasViewer.VerticalOffset, TrackCanvasViewer.ExtentHeight, TrackCanvasViewer.ActualHeight);
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        public void SetMessage(string message, double time, MessageType messageType = MessageType.Notice)
        {
            switch (messageType)
            {
                case MessageType.Notice:
                    {
                        TrackEditBoardSnackbar.Foreground = ColorProvider.NoticeMessageBrush;
                        this.MessageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(time));
                        break;
                    }
                case MessageType.Warn:
                    {
                        TrackEditBoardSnackbar.Foreground = ColorProvider.WarnMessageBrush;
                        this.MessageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(time));
                        break;
                    }
                case MessageType.Error:
                    {
                        TrackEditBoardSnackbar.Foreground = ColorProvider.ErrorMessageBrush;
                        this.MessageQueue.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(time));
                        break;
                    }
            }
        }

        private void TrackCanvasViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point? canvasPoint = e.GetPosition(TrackCanvas);
            Point? viewerPoint = e.GetPosition(TrackCanvasViewer);
            bool isOverScrollBar = this.IsPointOverScrollBar(viewerPoint);
            if (!isOverScrollBar)
            {
                this.TrackEditBoardController.OnMouseDownOverTrackCanvasFloor(canvasPoint, e.LeftButton);
            }
            if (this.IsPointInTrackGrid(canvasPoint) && !this.TrackEditBoardController.IsCtrlDown)
            {
                this.TrackEditBoardController.OnMouseDownInTrackCanvas(canvasPoint, e.LeftButton);
            }
            else if (!isOverScrollBar)
            {
                this.TrackEditBoardController.OnMouseDownInTrackCanvasViewer(viewerPoint, e.LeftButton);
            }
        }

        private void TrackCanvasViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point? canvasPoint = e.GetPosition(TrackCanvas);
            Point? viewerPoint = e.GetPosition(TrackCanvasViewer);
            this.TrackEditBoardController.OnMouseMoveOverTrackCanvasViewer(viewerPoint);
            this.TrackEditBoardController.OnMouseMoveOverTrackCanvasFloor(canvasPoint);
            if (this.IsPointInTrackGrid(canvasPoint) && !this.TrackEditBoardController.IsCtrlDown)
            {
                this.TrackEditBoardController.OnMouseMoveInTrackCanvas(canvasPoint);
            }
            else
            {
                this.TrackEditBoardController.OnMouseMoveInTrackCanvasViewer(viewerPoint);
            }
        }

        private void TrackCanvasViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point? canvasPoint = e.GetPosition(TrackCanvas);
            Point? viewerPoint = e.GetPosition(TrackCanvasViewer);
            this.TrackEditBoardController.OnMouseUpInTrackCanvasViewer(viewerPoint);
            this.TrackEditBoardController.OnMouseUpOverTrackCanvasFloor(canvasPoint);
        }

        private void PickerButton_Checked(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.SetIsPicking(true);
            Mouse.OverrideCursor = Cursors.Hand;
            PickerButton.Background = Brushes.MediumPurple;
        }

        private void PickerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.SetIsPicking(false);
            Mouse.OverrideCursor = null;
            PickerButton.Background = Brushes.White;
        }

        private void TrackEditCard_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                PickerButton.IsChecked = !PickerButton.IsChecked;
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                this.TrackEditBoardController.SetCtrl(true);
            }
        }

        private void TrackEditCard_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                this.TrackEditBoardController.SetCtrl(false);
            }
        }

        private async void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new VolumeDialog(this.Model), "TrackEditDialog");
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            // 临时取消事件订阅，防止触发 PlayButton_Checked
            PlayButton.Checked -= PlayButton_Checked;
            this.TrackEditBoardController.ReplayChart();
            // 恢复事件订阅
            PlayButton.Checked += PlayButton_Checked;
        }

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.PlayChart();
        }

        private void PlayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.PauseChart();
        }

        private async void PlaySpeedButton_Click(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new PlaySpeedDialog(this.Model), "TrackEditDialog");
        }

        private async void SizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.SetIsSizeChanging(true);
            await DialogHost.Show(new TrackGridSizeDialog(this.Model), "TrackEditDialog");
            this.TrackEditBoardController.SetIsSizeChanging(false);
        }

        private async void DivideButton_Click(object sender, RoutedEventArgs e)
        {
            await DialogHost.Show(new DivideDialog(this.Model), "TrackEditDialog");
        }

        private void BpmManagerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void InfomationButton_Click(object sender, RoutedEventArgs e)
        {
            Object result = await DialogHost.Show(new ChartInfoDialog(this.Model), "TrackEditDialog");
            if (result as bool? == true)
            {
                // 更新并保存文件
                this.UpdateCurrentBeatTime();
                if (!ChartUtilV1.SaveChartInfo(this.Model.ChartInfo))
                {
                    this.SetMessage("谱面信息保存失败", 2, MessageType.Error);
                    return;
                }
                // 更新主页曲目
                this.MainWindowModel.UpdateChartMusic(this.Model.ChartInfo.ChartMusic);
                // 更新谱面编辑窗口名
                this.Model.ChartWindow.Title = Common.GenerateChartWindowTitle(this.Model.ChartInfo);
                this.SetMessage("谱面信息保存成功", 2, MessageType.Notice);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 更新并保存文件
            if (!await ChartUtilV1.SaveChart(this.Model))
            {
                this.SetMessage("谱面保存失败", 2, MessageType.Error);
                return;
            }
            // 更新主页曲目
            this.MainWindowModel.UpdateChartMusic(this.Model.ChartInfo.ChartMusic);
            this.SetMessage("谱面保存成功", 2, MessageType.Notice);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TrackCanvasViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.SetScrollViewerVerticalOffset(e.VerticalOffset);
        }

        /// <summary>
        /// 设置纵向滚动条偏移位置
        /// </summary>
        public void SetScrollViewerVerticalOffset(double verticalOffset)
        {
            if (this.IsLoaded)
            {
                // 更新当前时间信息
                this.Model.UpdateCurrentBeatTime(TrackCanvasViewer.VerticalOffset, TrackCanvasViewer.ExtentHeight, TrackCanvasViewer.ActualHeight);
                this.TrackEditBoardController.OnScrollChanged();
            }
            // 同步滚动条
            if (TrackCanvasViewer.VerticalOffset != verticalOffset) TrackCanvasViewer.ScrollToVerticalOffset(verticalOffset);
            if (TimeLineCanvasViewer.VerticalOffset != verticalOffset) TimeLineCanvasViewer.ScrollToVerticalOffset(verticalOffset);
        }

        private void TrackEditCard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                PlayButton.IsChecked = !PlayButton.IsChecked;
            }
            else if (e.Key == Key.D)
            {
                if (this.TrackEditBoardController.IsCtrlDown)
                {
                    DeleteNoteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            else if (e.Key == Key.Delete)
            {
                DeleteNoteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else if (e.Key == Key.S)
            {
                if (this.TrackEditBoardController.IsCtrlDown)
                {
                    SaveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
            else if (e.Key == Key.D1)
            {
                NoteSelectBox.SelectedIndex = 0;
            }
            else if (e.Key == Key.D2)
            {
                NoteSelectBox.SelectedIndex = 1;
            }
            else if (e.Key == Key.D3)
            {
                NoteSelectBox.SelectedIndex = 2;
            }
            else if (e.Key == Key.D4)
            {
                NoteSelectBox.SelectedIndex = 3;
            }
            else if (e.Key == Key.D5)
            {
                NoteSelectBox.SelectedIndex = 4;
            }
            else if (e.Key == Key.D0)
            {
                // 测试按键
                this.TrackEditBoardController.TestKeyDown();
            }
        }

        private void TrackEditCard_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void NoteSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TrackEditBoardController.OnNoteSelectionChanged();
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

        private void TimeLineCanvasViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point? point = e.GetPosition(TimeLineCanvasViewer);
            this.TrackEditBoardController.OnMouseDownInTimeLineCanvasViewer(point, e.LeftButton);
        }

        private void TimeLineCanvasViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point? point = e.GetPosition(TimeLineCanvasViewer);
            this.TrackEditBoardController.OnMouseMoveInTimeLineCanvasViewer(point);
        }

        private void TimeLineCanvasViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point? point = e.GetPosition(TimeLineCanvasViewer);
            this.TrackEditBoardController.OnMouseUpInTimeLineCanvasViewer(point);
        }

        private void TimeLineCanvasViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.SetScrollViewerVerticalOffset(e.VerticalOffset);
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.DeletePickedNotes();
        }

        private void TrackEditCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!TrackCanvasViewer.IsFocused)
            {
                TrackCanvasViewer.Focus();
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 检测鼠标是否在某个轨道或音符内
        /// </summary>
        public bool IsMouseInTrackOrNote(Point? point)
        {
            IInputElement hitElement = TrackCanvas.InputHitTest(point.Value);
            if (hitElement is Rectangle rectangle)
            {
                if (rectangle.DataContext is Models.Track || rectangle.DataContext is Note)
                {
                    return true;
                }
            }
            return false;
        }

        private void DeleteTrackButton_Click(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.DeletePickedTrack();
        }
    }
}
