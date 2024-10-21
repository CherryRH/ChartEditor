﻿using ChartEditor.Models;
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

namespace ChartEditor.UserControls.Boards
{
    /// <summary>
    /// TrackEditBoard.xaml 的交互逻辑
    /// </summary>
    public partial class TrackEditBoard : UserControl
    {
        private static string logTag = "[TrackEditBoard]";

        public MainWindowModel MainWindowModel { get; set; }

        // 是否处于初始化中
        private bool isInitializing = true;

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
                this.isInitializing = false;
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

        private void TrackCanvasViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point? canvasPoint = e.GetPosition(TrackCanvas);
            if (this.IsPointInTrackGrid(canvasPoint) && !this.TrackEditBoardController.IsCtrlDown)
            {
                this.TrackEditBoardController.OnMouseMoveInTrackCanvas(canvasPoint);
            }
            else
            {
                Point? point = e.GetPosition(TrackCanvasViewer);
                this.TrackEditBoardController.OnMouseMoveInTrackCanvasViewer(point);
            }
        }

        private void TrackCanvasViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point? canvasPoint = e.GetPosition(TrackCanvas);
            if (this.IsPointInTrackGrid(canvasPoint) && !this.TrackEditBoardController.IsCtrlDown)
            {
                this.TrackEditBoardController.OnMouseDownInTrackCanvas(canvasPoint, e.ButtonState);
            }
            else
            {
                Point? point = e.GetPosition(TrackCanvasViewer);
                this.TrackEditBoardController.OnMouseDownInTrackCanvasViewer(point, e.ButtonState);
            }
        }

        private void TrackCanvasViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point? point = e.GetPosition(TrackCanvasViewer);
            this.TrackEditBoardController.OnMouseUpInTrackCanvasViewer(point);
        }

        private void PickerButton_Checked(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.SetIsPicking(true);
            Mouse.OverrideCursor = Cursors.Hand;
            PickerButton.Background = Brushes.LightGray;
            TrackCanvasViewer.Focus();
        }

        private void PickerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TrackEditBoardController.SetIsPicking(false);
            Mouse.OverrideCursor = null;
            PickerButton.Background = Brushes.White;
            TrackCanvasViewer.Focus();
        }

        /// <summary>
        /// 实现TrackCanvasViewer中按键相关逻辑
        /// </summary>
        private void TrackCanvasViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                this.TrackEditBoardController.SetShift(true);
            }
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                this.TrackEditBoardController.SetCtrl(true);
            }
        }

        private void TrackCanvasViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                this.TrackEditBoardController.SetShift(false);
            }
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
                this.Model.ChartInfo.UpdateAtNow();
                this.UpdateCurrentBeatTime();
                if (!ChartUtilV1.SaveChartInfo(this.Model.ChartInfo))
                {
                    this.SetMessage("谱面信息保存失败", 2, MessageType.Error);
                    return;
                }
                this.SetMessage("谱面信息保存成功", 2, MessageType.Notice);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

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
            // 同步滚动条
            if (TrackCanvasViewer.VerticalOffset != verticalOffset) TrackCanvasViewer.ScrollToVerticalOffset(verticalOffset);
            if (TimeLineCanvasViewer.VerticalOffset != verticalOffset) TimeLineCanvasViewer.ScrollToVerticalOffset(verticalOffset);
            // 更新当前时间信息
            if (!this.isInitializing)
            {
                this.Model.UpdateCurrentBeatTime(TrackCanvasViewer.VerticalOffset, TrackCanvasViewer.ExtentHeight, TrackCanvasViewer.ActualHeight);
            }
        }

        private void TrackCanvasViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (this.TrackEditBoardController.IsPlaying)
                {
                    PlayButton.IsChecked = false;
                }
                else
                {
                    PlayButton.IsChecked = true;
                }
            }
            
        }

        private void TrackCanvasViewer_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void NoteSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TrackEditBoardController.OnNoteSelectionChanged();
        }

        private void TrackCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void TrackCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
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
            this.TrackEditBoardController.OnMouseDownInTimeLineCanvasViewer(point, e.ButtonState);
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
    }
}
