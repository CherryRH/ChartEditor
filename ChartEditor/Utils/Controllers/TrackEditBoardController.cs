using ChartEditor.Models;
using ChartEditor.UserControls.Boards;
using ChartEditor.Utils.AudioUtils;
using ChartEditor.Utils.Drawers;
using ChartEditor.Utils.MusicUtils;
using ChartEditor.ViewModels;
using MaterialDesignThemes.Wpf;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
        private ScrollViewer TrackCanvasViewer { get { return TrackEditBoard.TrackCanvasViewer; } }
        private ScrollViewer TimeLineCanvasViewer { get { return TrackEditBoard.TimeLineCanvasViewer; } }
        private ChartEditModel ChartEditModel { get { return TrackEditBoard.Model; } }
        private ChartInfo ChartInfo { get { return ChartEditModel.ChartInfo; } }

        /// <summary>
        /// 绘图器
        /// </summary>
        private NoteDrawer noteDrawer;

        private TrackGridDrawer trackGridDrawer;

        private TimeLineDrawer timeLineDrawer;

        private ColumnLabelDrawer columnLabelDrawer;

        private SelectBoxDrawer selectBoxDrawer;

        private bool isSizeChanging = false;
        // 当前高光的Track
        private HashSet<Track> highLightTracks = new HashSet<Track>();
        // 当前高光的Note
        private HashSet<Note> highLightNotes = new HashSet<Note>();
        // 多选框选中的Note
        private HashSet<Note> selectBoxPickedNotes = new HashSet<Note>();
        // 拖动起始位置
        private Point? stretchingStartPoint = null;
        private BeatTime stretchingStartBeatTime = null;
        // 正在拉伸的Track
        private Track stretchingTrack = null;
        // 正在拉伸的HoldNote
        private HoldNote stretchingHoldNote = null;
        // 是否在拉伸结束点
        private bool isStretchingEndPoint = true;
        // 是否在移动选中的轨道
        private bool isMovingTrack = false;
        // 是否在移动选中的音符
        private bool isMovingNote = false;
        // 是否已经进入移动状态
        private bool hasMoved = false;
        // 移动起始位置
        private Point? movingStartPoint = null;
        private BeatTime movingStartBeatTime = null;
        private BeatTime movingCurrentBeatTime = null;
        private int movingCurrentColumnIndex = 0;

        /// <summary>
        /// 谱面计时器
        /// </summary>
        private Timer timer;

        /// <summary>
        /// 混音器
        /// </summary>
        private AudioMixer audioMixer;

        /// <summary>
        /// 音乐播放器
        /// </summary>
        private MusicPlayer musicPlayer;

        /// <summary>
        /// 打击音播放器
        /// </summary>
        private HitSoundPlayer hitSoundPlayer;

        // 播放状态
        private bool isPlaying = false;
        public bool IsPlaying { get { return isPlaying; } }

        /// <summary>
        /// 鼠标控制
        /// </summary>
        public Point? DragMousePosition { get; private set; } = new Point?();
        public Point? CanvasMousePosition { get; private set; } = new Point?();

        private bool isScrollViewerDragging = false;

        private bool isSelectBoxDragging = false;

        // 选择状态
        private bool isPicking = false;
        // 删除状态
        private bool isDeleting = false;

        // 键盘状态
        public bool IsShiftDown { get; private set; } = false;
        public bool IsCtrlDown { get; private set; } = false;
        public bool IsAltDown { get; private set; } = false;

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

        // 当前拍数锚点
        private BeatTime beatTimeAnchor = new BeatTime();

        public TrackEditBoardController(TrackEditBoard trackEditBoard)
        {
            this.TrackEditBoard = trackEditBoard;
            // 初始化网格绘制器
            this.trackGridDrawer = new TrackGridDrawer();
            this.TrackCanvas.Children.Add(this.trackGridDrawer);
            // 初始化时间条绘制器
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
                // 初始化计时器
                this.timer = new Timer(this.ChartEditModel);
                // 初始化混音器
                this.audioMixer = new AudioMixer();
                // 初始化音乐播放器
                this.musicPlayer = new MusicPlayer(this.ChartEditModel, this.timer);
                // 初始化打击音播放器
                this.hitSoundPlayer = new HitSoundPlayer(this.ChartEditModel, this.timer);
                // 绘制网格
                this.DrawTrackGrid();
                // 绘制时间轴
                this.DrawTimeLine();
                // 初始化Note绘制器
                this.noteDrawer = new NoteDrawer(this.TrackEditBoard);
                // 绘制谱面（轨道和音符）
                this.LoadChart();
                // 初始化列标签绘制器
                this.columnLabelDrawer = new ColumnLabelDrawer(this.TrackEditBoard);
                // 初始化多选框绘制器
                this.selectBoxDrawer = new SelectBoxDrawer(this.TrackEditBoard);

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
                if (this.TrackCanvasViewer.VerticalOffset <= 0)
                {
                    this.TrackEditBoard.PlayButton.IsChecked = false;
                }
                else
                {
                    // 当前谱面高度
                    double currentHeight = this.timer.GetCurrentTime() * this.ChartEditModel.ScrollSpeed;
                    this.TrackCanvasViewer.ScrollToVerticalOffset(this.ChartEditModel.TotalHeight - currentHeight);
                }
            }
            else
            {
                if (this.isSizeChanging)
                {
                    this.TrackEditBoard.ScrollToCurrentBeat(this.beatTimeAnchor);
                }
            }

            if (this.ChartEditModel.NoteSelectedIndex != -1 && this.CanvasMousePosition.HasValue 
                && this.TrackEditBoard.IsPointInTrackGrid(this.CanvasMousePosition) 
                && !this.isPlaying && !this.isPicking)
            {
                this.noteDrawer.ShowPreviewerAt(this.CanvasMousePosition);
            }
        }

        /// <summary>
        /// 加载谱面（轨道和音符）
        /// </summary>
        private void LoadChart()
        {
            // 统计个数
            int trackNum = 0;
            int tapNoteNum = 0;
            int flickNoteNum = 0;
            int holdNoteNum = 0;
            int catchNoteNum = 0;
            foreach (SkipList<BeatTime, Track> trackList in this.ChartEditModel.TrackSkipLists)
            {
                var currentTrackNode = trackList.FirstNode;
                while (currentTrackNode != null)
                {
                    Track track = currentTrackNode.Value;
                    this.noteDrawer.CreateTrack(track);
                    track.Rectangle.MouseEnter += TrackRectangle_MouseEnter;
                    track.Rectangle.MouseLeave += TrackRectangle_MouseLeave;
                    track.Rectangle.MouseDown += TrackRectangle_MouseDown;
                    trackNum++;
                    var currentNoteNode = track.NoteSkipList.FirstNode;
                    while (currentNoteNode != null)
                    {
                        Note note = currentNoteNode.Value;
                        this.noteDrawer.CreateNote(note);
                        note.Rectangle.MouseEnter += NoteRectangle_MouseEnter;
                        note.Rectangle.MouseLeave += NoteRectangle_MouseLeave;
                        note.Rectangle.MouseDown += NoteRectangle_MouseDown;
                        switch (note.Type)
                        {
                            case NoteType.Tap: tapNoteNum++; break;
                            case NoteType.Flick: flickNoteNum++; break;
                            case NoteType.Catch: catchNoteNum++; break;
                        }
                        currentNoteNode = currentNoteNode.Next[0];
                    }
                    var currentHoldNoteNode = track.HoldNoteSkipList.FirstNode;
                    while (currentHoldNoteNode != null)
                    {
                        HoldNote holdNote = currentHoldNoteNode.Value;
                        this.noteDrawer.CreateNote(holdNote);
                        holdNote.Rectangle.MouseEnter += NoteRectangle_MouseEnter;
                        holdNote.Rectangle.MouseLeave += NoteRectangle_MouseLeave;
                        holdNote.Rectangle.MouseDown += NoteRectangle_MouseDown;
                        holdNoteNum++;
                        currentHoldNoteNode = currentHoldNoteNode.Next[0];
                    }
                    currentTrackNode = currentTrackNode.Next[0];
                }
            }
            // 更新统计个数
            this.ChartEditModel.TrackNum = trackNum;
            this.ChartEditModel.TapNoteNum = tapNoteNum;
            this.ChartEditModel.FlickNoteNum = flickNoteNum;
            this.ChartEditModel.HoldNoteNum = holdNoteNum;
            this.ChartEditModel.CatchNoteNum = catchNoteNum;
        }

        /// <summary>
        /// 测试按键按下时
        /// </summary>
        public void TestKeyDown()
        {
            this.hitSoundPlayer.PlayHitSound(0);
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer中移动
        /// </summary>
        public void OnMouseMoveInTrackCanvasViewer(Point? mousePosition)
        {
            if (this.isPlaying) return;
            if (!this.isPicking)
            {
                if (this.isScrollViewerDragging && this.DragMousePosition.HasValue)
                {
                    double deltaX = mousePosition.Value.X - this.DragMousePosition.Value.X;
                    double deltaY = mousePosition.Value.Y - this.DragMousePosition.Value.Y;
                    this.TrackCanvasViewer.ScrollToHorizontalOffset(this.TrackCanvasViewer.HorizontalOffset - deltaX);
                    this.TrackCanvasViewer.ScrollToVerticalOffset(this.TrackCanvasViewer.VerticalOffset - deltaY);
                    this.DragMousePosition = mousePosition;
                }
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer中点击
        /// </summary>
        public void OnMouseDownInTrackCanvasViewer(Point? mousePosition, MouseButtonState mouseButtonState)
        {
            if (mouseButtonState != MouseButtonState.Pressed) return;
            if (this.isPlaying)
            {
                // 暂停播放
                this.TrackEditBoard.PlayButton.IsChecked = false;
            }
            else
            {
                if (this.IsCtrlDown)
                {

                }
                else
                {
                    // 清除选中的音符
                    if (this.stretchingHoldNote == null) this.ClearAllPickedNotes();
                }
                if (!this.isPicking)
                {
                    // 选择状态时禁用拖动效果
                    this.DragMousePosition = mousePosition;
                    this.TrackCanvasViewer.CaptureMouse();
                    this.isScrollViewerDragging = true;
                }
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer中释放
        /// </summary>
        public void OnMouseUpInTrackCanvasViewer(Point? mousePosition)
        {
            if (this.isPlaying) return;
            if (!this.isPicking)
            {
                if (this.isScrollViewerDragging)
                {
                    this.isScrollViewerDragging = false;
                    this.DragMousePosition = null;
                    this.TrackCanvasViewer.ReleaseMouseCapture();
                }
            }
            else
            {
                
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvasViewer上方移动时
        /// </summary>
        public void OnMouseMoveOverTrackCanvasViewer(Point? mousePoint)
        {
            if (this.isPlaying) return;
            if (this.isPicking)
            {
                if (this.isSelectBoxDragging || this.stretchingStartBeatTime != null || this.movingStartBeatTime != null)
                {
                    double testWidth = this.TrackCanvasViewer.ActualWidth / 20;
                    double testHeight = this.TrackCanvasViewer.ActualHeight / 20;
                    double maxSpeedY = 6;
                    double maxSpeedX = 3;
                    double pointY = mousePoint.Value.Y;
                    double pointX = mousePoint.Value.X;
                    // 当鼠标靠近边界时，向对应方向滚动
                    if (pointY <= testHeight)
                    {
                        double speed = Math.Min(maxSpeedY, (testHeight - pointY) * maxSpeedY / testHeight);
                        this.TrackCanvasViewer.ScrollToVerticalOffset(Math.Max(0, this.TrackCanvasViewer.VerticalOffset - speed));
                    }
                    else if (pointY >= this.TrackCanvasViewer.ActualHeight - testHeight)
                    {
                        double speed = Math.Min(maxSpeedY, (pointY - this.TrackCanvasViewer.ActualHeight + testHeight) * maxSpeedY / testHeight);
                        this.TrackCanvasViewer.ScrollToVerticalOffset(Math.Min(this.TrackCanvasViewer.ScrollableHeight, this.TrackCanvasViewer.VerticalOffset + speed));
                    }
                    if (pointX <= testWidth)
                    {
                        double speed = Math.Min(maxSpeedX, (testWidth - pointX) * maxSpeedX / testWidth);
                        this.TrackCanvasViewer.ScrollToHorizontalOffset(Math.Max(0, this.TrackCanvasViewer.HorizontalOffset - speed));
                    }
                    else if (pointX >= this.TrackCanvasViewer.ActualWidth - testWidth)
                    {
                        double speed = Math.Min(maxSpeedX, (pointX - this.TrackCanvasViewer.ActualWidth + testWidth) * maxSpeedX / testWidth);
                        this.TrackCanvasViewer.ScrollToHorizontalOffset(Math.Min(this.TrackCanvasViewer.ScrollableWidth, this.TrackCanvasViewer.HorizontalOffset + speed));
                    }
                }
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvas中点击
        /// </summary>
        public void OnMouseDownInTrackCanvas(Point? mousePosition, MouseButtonState mouseButtonState)
        {
            if (mouseButtonState != MouseButtonState.Pressed) return;
            if (this.isPlaying)
            {
                // 暂停播放
                this.TrackEditBoard.PlayButton.IsChecked = false;
            }
            else
            {
                if (this.isPicking)
                {
                    // 判断是否开始移动
                    if (!this.isSelectBoxDragging && this.stretchingStartBeatTime == null && this.movingStartBeatTime == null)
                    {
                        // 是否在某个被选中的音符中
                        foreach (Note note in this.ChartEditModel.PickedNotes)
                        {
                            if (note.ContainsPoint(mousePosition, this.TrackCanvas))
                            {
                                this.movingStartPoint = mousePosition;
                                this.movingCurrentBeatTime = note.StartBeatTime;
                                this.movingStartBeatTime = note.StartBeatTime;
                                this.movingCurrentColumnIndex = this.ChartEditModel.GetColumnIndexFromPoint(mousePosition);
                                this.isMovingNote = true;
                                break;
                            }
                        }
                        if (!this.isMovingNote && this.ChartEditModel.PickedTrack != null)
                        {
                            if (this.ChartEditModel.PickedTrack.ContainsPoint(mousePosition, this.TrackCanvas))
                            {
                                this.movingStartPoint = mousePosition;
                                this.movingStartBeatTime = this.ChartEditModel.PickedTrack.StartBeatTime;
                                this.movingCurrentBeatTime = this.ChartEditModel.PickedTrack.StartBeatTime;
                                this.movingCurrentColumnIndex = this.ChartEditModel.GetColumnIndexFromPoint(mousePosition);
                                this.isMovingTrack = true;
                            }
                        }
                    }
                }
                else
                {
                    BeatTime beatTime = this.ChartEditModel.GetBeatTimeFromPoint(mousePosition, this.TrackEditBoard.TrackCanvas.ActualHeight);
                    int columnIndex = this.ChartEditModel.GetColumnIndexFromPoint(mousePosition);
                    // 放置事件
                    this.TryPutTrackOrNote(beatTime, columnIndex);
                }
                if (this.IsCtrlDown)
                {

                }
                else
                {
                    // 清除选中的音符
                    if (this.stretchingHoldNote == null && !this.isMovingNote) this.ClearAllPickedNotes();
                }
            }
        }

        /// <summary>
        /// 当鼠标在TrackCanvas中移动
        /// </summary>
        public void OnMouseMoveInTrackCanvas(Point? mousePosition)
        {
            if (this.isPlaying) return;
            this.CanvasMousePosition = mousePosition;
            if (this.isPicking)
            {
                if (!this.isSelectBoxDragging && this.stretchingStartBeatTime == null && this.movingStartBeatTime != null)
                {
                    double pointY = mousePosition.Value.Y;
                    double startPointY = this.movingStartPoint.Value.Y;
                    BeatTime mouseBeatTime = this.movingStartBeatTime.CreateByOffsetY(this.ChartEditModel.Divide, this.ChartEditModel.RowWidth, pointY - startPointY);
                    int columnIndex = this.ChartEditModel.GetColumnIndexFromPoint(mousePosition);
                    if (this.isMovingNote)
                    {
                        if (!this.movingCurrentBeatTime.IsEqualTo(mouseBeatTime) || this.movingCurrentColumnIndex != columnIndex)
                        {
                            if (!this.hasMoved)
                            {
                                this.ChartEditModel.ChangePickedNoteTimeBegin();
                            }
                            this.hasMoved = true;
                            Mouse.OverrideCursor = Cursors.SizeAll;
                            this.TrackCanvas.CaptureMouse();
                            bool result = this.ChartEditModel.TryMovePickedNote(this.movingCurrentBeatTime, mouseBeatTime, this.movingCurrentColumnIndex, columnIndex);
                            if (result)
                            {
                                this.movingCurrentColumnIndex = columnIndex;
                                this.movingCurrentBeatTime = mouseBeatTime;
                                // 重绘Note位置
                                foreach (Note note in this.ChartEditModel.PickedNotes)
                                {
                                    this.noteDrawer.RedrawNoteWhenPosChanged(note);
                                }
                            }
                        }
                    }
                    else if (this.isMovingTrack)
                    {
                        if (!this.movingCurrentBeatTime.IsEqualTo(mouseBeatTime) || this.movingCurrentColumnIndex != columnIndex)
                        {
                            if (!this.hasMoved)
                            {
                                this.ChartEditModel.ChangePickedTrackTimeBegin();
                            }
                            this.hasMoved = true;
                            Mouse.OverrideCursor = Cursors.SizeAll;
                            this.TrackCanvas.CaptureMouse();
                            bool result = this.ChartEditModel.TryMovePickedTrack(mouseBeatTime, columnIndex);
                            if (result)
                            {
                                this.movingCurrentColumnIndex = columnIndex;
                                this.movingCurrentBeatTime = mouseBeatTime;
                                // 移动Track位置
                                this.noteDrawer.RedrawTrackWhenPosChanged(this.ChartEditModel.PickedTrack);
                            }
                        }
                    }
                }
            }
        }

        public void OnMouseDownOverTrackCanvasFloor(Point? mousePosition, MouseButtonState mouseButtonState)
        {
            if (mouseButtonState != MouseButtonState.Pressed) return;
            if (this.isPlaying) return;
            if (this.isPicking)
            {
                if (this.stretchingStartBeatTime == null && this.movingStartBeatTime == null && !this.isSelectBoxDragging)
                {
                    if (this.ChartEditModel.PickedNotes.Count == 1 && this.ChartEditModel.PickedNotes[0] is HoldNote holdNote)
                    {
                        switch (holdNote.GetStretchState(mousePosition, this.TrackCanvas))
                        {
                            case 0: break;
                            case 1:
                                {
                                    this.stretchingStartPoint = mousePosition;
                                    this.stretchingStartBeatTime = holdNote.StartBeatTime;
                                    this.isStretchingEndPoint = false;
                                    this.TrackCanvas.CaptureMouse();
                                    this.stretchingHoldNote = holdNote;
                                    break;
                                }
                            case 2:
                                {
                                    this.stretchingStartPoint = mousePosition;
                                    this.stretchingStartBeatTime = holdNote.EndBeatTime;
                                    this.isStretchingEndPoint = true;
                                    this.TrackCanvas.CaptureMouse();
                                    this.stretchingHoldNote = holdNote;
                                    break;
                                }
                        }
                    }
                    if (this.stretchingStartBeatTime == null && this.ChartEditModel.PickedTrack != null)
                    {
                        switch (this.ChartEditModel.PickedTrack.GetStretchState(mousePosition, this.TrackCanvas))
                        {
                            case 0: break;
                            case 1:
                                {
                                    this.stretchingStartPoint = mousePosition;
                                    this.stretchingStartBeatTime = this.ChartEditModel.PickedTrack.StartBeatTime;
                                    this.isStretchingEndPoint = false;
                                    this.TrackCanvas.CaptureMouse();
                                    this.stretchingTrack = this.ChartEditModel.PickedTrack;
                                    break;
                                }
                            case 2:
                                {
                                    this.stretchingStartPoint = mousePosition;
                                    this.stretchingStartBeatTime = this.ChartEditModel.PickedTrack.EndBeatTime;
                                    this.isStretchingEndPoint = true;
                                    this.TrackCanvas.CaptureMouse();
                                    this.stretchingTrack = this.ChartEditModel.PickedTrack;
                                    break;
                                }
                        }
                    }
                }
                // 尝试进入复选框状态
                if (!this.isSelectBoxDragging && this.stretchingStartBeatTime == null && this.movingStartBeatTime == null)
                {
                    this.isSelectBoxDragging = this.selectBoxDrawer.SetSelectedBoxAt(mousePosition);
                }
            }
        }

        public void OnMouseMoveOverTrackCanvasFloor(Point? mousePosition)
        {
            if (this.isPlaying) return;
            if (this.isPicking)
            {
                if (this.isSelectBoxDragging)
                {
                    this.selectBoxDrawer.DragSelectBoxTo(mousePosition);
                    // 查找并更新所有在多选框中的音符
                    this.SelectBoxNotes();
                }
                else
                {
                    if (this.stretchingStartBeatTime == null && this.movingStartBeatTime == null)
                    {
                        bool ifAtEndOfTrack = false;
                        bool ifAtEndOfHoldNote = false;
                        // 检测鼠标是否在被选中轨道的两端
                        if (this.ChartEditModel.PickedTrack != null)
                        {
                            if (this.ChartEditModel.PickedTrack.GetStretchState(mousePosition, this.TrackCanvas) != 0) ifAtEndOfTrack = true;
                        }
                        // 检测鼠标是否在唯一被选中HoldNote的两端
                        if (this.ChartEditModel.PickedNotes.Count == 1 && this.ChartEditModel.PickedNotes[0] is HoldNote holdNote)
                        {
                            if (holdNote.GetStretchState(mousePosition, this.TrackCanvas) != 0) ifAtEndOfHoldNote = true;
                        }
                        if (ifAtEndOfTrack || ifAtEndOfHoldNote) Mouse.OverrideCursor = Cursors.SizeNS;
                        else if (Mouse.OverrideCursor != Cursors.Hand) Mouse.OverrideCursor = Cursors.Hand;
                    }
                    else if (this.stretchingTrack != null)
                    {
                        double pointY = mousePosition.Value.Y;
                        double startPointY = this.stretchingStartPoint.Value.Y;
                        BeatTime beatTime = this.stretchingStartBeatTime.CreateByOffsetY(this.ChartEditModel.Divide, this.ChartEditModel.RowWidth, pointY - startPointY);
                        if (this.isStretchingEndPoint && !beatTime.IsEqualTo(this.stretchingTrack.EndBeatTime))
                        {
                            bool result = this.ChartEditModel.TryChangeTrackEndTime(this.stretchingTrack, beatTime);
                            if (result) this.noteDrawer.RedrawTrackWhenTimeChanged(this.stretchingTrack);
                        }
                        else if (!this.isStretchingEndPoint && !beatTime.IsEqualTo(this.stretchingTrack.StartBeatTime))
                        {
                            bool result = this.ChartEditModel.TryChangeTrackStartTime(this.stretchingTrack, beatTime);
                            if (result) this.noteDrawer.RedrawTrackWhenTimeChanged(this.stretchingTrack);
                        }
                    }
                    else if (this.stretchingHoldNote != null)
                    {
                        double pointY = mousePosition.Value.Y;
                        double startPointY = this.stretchingStartPoint.Value.Y;
                        BeatTime beatTime = this.stretchingStartBeatTime.CreateByOffsetY(this.ChartEditModel.Divide, this.ChartEditModel.RowWidth, pointY - startPointY);
                        if (this.isStretchingEndPoint && !beatTime.IsEqualTo(this.stretchingHoldNote.EndBeatTime))
                        {
                            bool result = this.ChartEditModel.TryChangeHoldNoteEndTime(this.stretchingHoldNote, beatTime);
                            if (result) this.noteDrawer.RedrawHoldNoteWhenTimeChanged(this.stretchingHoldNote);
                        }
                        else if (!this.isStretchingEndPoint && !beatTime.IsEqualTo(this.stretchingHoldNote.StartBeatTime))
                        {
                            bool result = this.ChartEditModel.TryChangeHoldNoteStartTime(this.stretchingHoldNote, beatTime);
                            if (result)
                            {
                                this.noteDrawer.RedrawHoldNoteWhenTimeChanged(this.stretchingHoldNote);
                            }
                        }
                    }
                }
            }
        }

        public void OnMouseUpOverTrackCanvasFloor(Point? mousePosition)
        {
            if (this.isPlaying) return;
            if (this.isPicking)
            {
                if (this.isSelectBoxDragging)
                {
                    foreach (Note note in this.selectBoxPickedNotes)
                    {
                        this.ChartEditModel.PickedNotes.Add(note);
                    }
                    this.selectBoxPickedNotes.Clear();
                    this.selectBoxDrawer.HideSelectBox();
                    this.isSelectBoxDragging = false;
                }
                if (this.stretchingTrack != null)
                {
                    this.stretchingTrack = null;
                    this.stretchingStartBeatTime = null;
                    Mouse.OverrideCursor = Cursors.Hand;
                    this.TrackCanvas.ReleaseMouseCapture();
                }
                if (this.stretchingHoldNote != null)
                {
                    this.stretchingHoldNote = null;
                    this.stretchingStartBeatTime = null;
                    Mouse.OverrideCursor = Cursors.Hand;
                    this.TrackCanvas.ReleaseMouseCapture();
                }
                if (this.isMovingNote)
                {
                    this.isMovingNote = false;
                    Mouse.OverrideCursor = Cursors.Hand;
                    this.movingStartBeatTime = null;
                    this.TrackCanvas.ReleaseMouseCapture();
                    this.hasMoved = false;
                    this.ChartEditModel.ChangePickedNoteTimeOver();
                }
                if (this.isMovingTrack)
                {
                    this.isMovingTrack = false;
                    Mouse.OverrideCursor = Cursors.Hand;
                    this.movingStartBeatTime = null;
                    this.TrackCanvas.ReleaseMouseCapture();
                    this.hasMoved = false;
                    this.ChartEditModel.ChangePickedTrackTimeOver();
                }
            }
        }

        /// <summary>
        /// 当鼠标在TimeLineCanvasViewer中移动
        /// </summary>
        public void OnMouseMoveInTimeLineCanvasViewer(Point? mousePosition)
        {
            if (this.isPlaying) return;

            if (this.isScrollViewerDragging && this.DragMousePosition.HasValue)
            {
                double deltaX = mousePosition.Value.X - this.DragMousePosition.Value.X;
                double deltaY = mousePosition.Value.Y - this.DragMousePosition.Value.Y;
                this.TimeLineCanvasViewer.ScrollToHorizontalOffset(this.TimeLineCanvasViewer.HorizontalOffset - deltaX);
                this.TimeLineCanvasViewer.ScrollToVerticalOffset(this.TimeLineCanvasViewer.VerticalOffset - deltaY);
                this.DragMousePosition = mousePosition;
            }
        }

        /// <summary>
        /// 当鼠标在TimeLineCanvasViewer中点击
        /// </summary>
        public void OnMouseDownInTimeLineCanvasViewer(Point? mousePosition, MouseButtonState mouseButtonState)
        {
            if (mouseButtonState != MouseButtonState.Pressed) return;
            if (this.isPlaying)
            {
                // 暂停播放
                this.TrackEditBoard.PlayButton.IsChecked = false;
            }
            else
            {
                this.DragMousePosition = mousePosition;
                this.isScrollViewerDragging = true;
                this.TimeLineCanvasViewer.CaptureMouse();
            }
        }

        /// <summary>
        /// 当鼠标在TimeLineCanvasViewer中释放
        /// </summary>
        public void OnMouseUpInTimeLineCanvasViewer(Point? mousePosition)
        {
            if (this.isPlaying) return;
            if (this.isScrollViewerDragging)
            {
                this.isScrollViewerDragging = false;
                this.DragMousePosition = null;
                this.TimeLineCanvasViewer.ReleaseMouseCapture();
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

                this.ForceTriggerTrackOrNoteMouseEnter();
            }
            else
            {
                this.noteDrawer.ShowPreviewer();
                // 清除所有高光
                this.ClearAllRectHighLight();
                // 隐藏多选框
                this.selectBoxDrawer.HideSelectBox();
                this.isSelectBoxDragging = false;
                // 清除拉伸状态
                if (this.stretchingTrack != null) this.stretchingTrack = null;
                if (this.stretchingHoldNote != null)
                {
                    this.stretchingHoldNote = null;
                    this.ClearAllPickedNotes();
                }
                this.TrackCanvas.ReleaseMouseCapture();
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
                this.noteDrawer.HidePreviewer();
                // 保存拍数锚点
                this.beatTimeAnchor = new BeatTime(this.ChartEditModel.CurrentBeat);
            }
            else
            {
                this.noteDrawer.ShowPreviewer();
            }
        }

        /// <summary>
        /// 设置Ctrl状态，Ctrl用于切换至视图操作模式（停止网格操作）
        /// </summary>
        public void SetCtrl(bool isDown)
        {
            if (isDown == this.IsCtrlDown) return;
            this.IsCtrlDown = isDown;
            if (isDown)
            {
                // 隐藏预览图形
                this.noteDrawer.HidePreviewer();
            }
            else
            {
                this.noteDrawer.ShowPreviewer();
            }
        }

        /// <summary>
        /// 重播谱面
        /// </summary>
        public void ReplayChart()
        {
            try
            {
                if (!this.musicPlayer.SetMusic(Math.Max(this.ChartInfo.Delay, double.Epsilon)))
                {
                    this.TrackEditBoard.SetMessage("音乐播放异常，再试一次吧", 2, MessageType.Error);
                    return;
                }
                // 重置打击音播放节点
                this.hitSoundPlayer.ResetPlayNodes();

                this.timer.ReStart();
                if (!this.musicPlayer.PlayMusic())
                {
                    this.TrackEditBoard.SetMessage("音乐播放异常，再试一次吧", 2, MessageType.Error);
                    return;
                }
                this.hitSoundPlayer.ResumePlayLoop();

                this.TrackEditBoard.PlayButton.IsChecked = true;
                this.TrackEditBoard.PlayIcon.Kind = PackIconKind.Pause;
                this.ChartEditModel.ResetCurrentBeatTime();
                this.TrackEditBoard.ScrollToCurrentBeat();
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
                if (this.musicPlayer.IsMusicAboutOver(this.ChartEditModel.CurrentTime))
                {
                    this.ReplayChart();
                    return;
                }
                if (!this.musicPlayer.SetMusic(this.ChartEditModel.CurrentTime))
                {
                    this.TrackEditBoard.SetMessage("音乐播放异常，再试一次吧", 2, MessageType.Error);
                    return;
                }
                // 搜索打击音播放节点
                this.hitSoundPlayer.SetPlayNodes();

                this.timer.StartAt(this.ChartEditModel.CurrentTime);
                if (!this.musicPlayer.PlayMusic())
                {
                    this.TrackEditBoard.SetMessage("音乐播放异常，再试一次吧", 2, MessageType.Error);
                    return;
                }
                this.hitSoundPlayer.ResumePlayLoop();

                this.TrackEditBoard.PlayIcon.Kind = PackIconKind.Pause;
                this.isPlaying = true;
                // 清除状态
                if (this.isPicking)
                {
                    this.isSelectBoxDragging = false;
                    this.selectBoxDrawer.HideSelectBox();
                    this.stretchingTrack = null;
                    this.stretchingHoldNote = null;
                    Mouse.OverrideCursor = Cursors.Hand;
                }
                else
                {
                    this.noteDrawer.HidePreviewer();
                }
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
            this.timer.Stop();
            this.musicPlayer.PauseMusic();
            this.hitSoundPlayer.PausePlayLoop();

            this.TrackEditBoard.PlayIcon.Kind = PackIconKind.Play;
            this.isPlaying = false;

            this.noteDrawer.ShowPreviewer();
        }

        /// <summary>
        /// 处理尝试添加Track或Note事件
        /// </summary>
        private void TryPutTrackOrNote(BeatTime beatTime, int columnIndex)
        {
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
                                this.noteDrawer.CreateTrack(newTrack);
                                this.isTrackPutting = false;
                                this.SetTrackEvent(newTrack);
                            }
                            else if (this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置Track", 1, MessageType.Warn);
                            }
                        }
                        else
                        {
                            // 尝试放置或插入
                            int tryResult = this.ChartEditModel.AddTrackHeader(beatTime, columnIndex, out Track insertedTrack);
                            if (tryResult == 1)
                            {
                                // 轨道头部放置成功，记录位置
                                this.isTrackPutting = true;
                                this.trackHeaderPutBeatTime = beatTime;
                                this.trackHeaderPutColumnIndex = columnIndex;
                                this.noteDrawer.ShowTrackHeaderAt(beatTime, columnIndex);
                            }
                            else if (tryResult == 2)
                            {
                                // 插入轨道（从指定位置切断）
                                Track newTrack = this.ChartEditModel.InsertTrack(insertedTrack, beatTime);
                                this.noteDrawer.CreateTrack(newTrack);
                                this.SetTrackEvent(newTrack);
                            }
                            else if (tryResult == 0 && this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置或插入Track", 1, MessageType.Warn);
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
                            this.noteDrawer.CreateNote(newTapNote);
                            this.SetNoteEvent(newTapNote);
                        }
                        else if (this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                        {
                            this.TrackEditBoard.SetMessage("此处不允许放置TapNote", 1, MessageType.Warn);
                        }
                        break;
                    }
                case 2:
                    {
                        FlickNote newFlickNote = (FlickNote)this.ChartEditModel.AddNote(beatTime, columnIndex, NoteType.Flick);
                        if (newFlickNote != null)
                        {
                            // 显示Note
                            this.noteDrawer.CreateNote(newFlickNote);
                            this.SetNoteEvent(newFlickNote);
                        }
                        else if (this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                        {
                            this.TrackEditBoard.SetMessage("此处不允许放置FlickNote", 1, MessageType.Warn);
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
                                this.noteDrawer.CreateNote(newHoldNote);
                                this.isHoldNotePutting = false;
                                this.SetNoteEvent(newHoldNote);
                            }
                            else if (this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置HoldNote", 1, MessageType.Warn);
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
                            else if (this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                            {
                                this.TrackEditBoard.SetMessage("此处不允许放置HoldNote", 1, MessageType.Warn);
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
                            this.noteDrawer.CreateNote(newCatchNote);
                            this.SetNoteEvent(newCatchNote);
                        }
                        else if (this.TrackEditBoard.Settings.TrackOrNotePutWarnEnabled)
                        {
                            this.TrackEditBoard.SetMessage("此处不允许放置CatchNote", 1, MessageType.Warn);
                        }
                        break;
                    }
            }
        }

        private void SetTrackEvent(Track track)
        {
            if (track == null) return;
            // 设置移入移出事件
            track.Rectangle.MouseEnter += TrackRectangle_MouseEnter;
            track.Rectangle.MouseLeave += TrackRectangle_MouseLeave;
            // 设置点击事件
            track.Rectangle.MouseDown += TrackRectangle_MouseDown;
        }

        private void SetNoteEvent(Note note)
        {
            if (note == null) return;
            note.Rectangle.MouseEnter += NoteRectangle_MouseEnter;
            note.Rectangle.MouseLeave += NoteRectangle_MouseLeave;
            note.Rectangle.MouseDown += NoteRectangle_MouseDown;
        }

        private void NoteRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 点击选中未选中的Note
            if (this.isPicking && !this.isPlaying)
            {
                if (sender is Rectangle rectangle)
                {
                    if (rectangle.DataContext is Note note)
                    {
                        if (!note.IsPicked)
                        {
                            if (!this.IsCtrlDown)
                            {
                                this.ClearAllPickedNotes();
                            }
                            // 添加此音符
                            note.IsPicked = true;
                            this.ChartEditModel.PickedNotes.Add(note);
                            this.noteDrawer.RectPicked(rectangle);
                            // 选择后要从高光中去除
                            this.highLightNotes.Remove(note);
                            // 尝试移动
                            this.movingStartPoint = e.GetPosition(this.TrackCanvas);
                            this.movingCurrentBeatTime = note.StartBeatTime;
                            this.movingStartBeatTime = note.StartBeatTime;
                            this.isMovingNote = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有选中的音符
        /// </summary>
        public void ClearAllPickedNotes()
        {
            foreach (Note it in this.ChartEditModel.PickedNotes)
            {
                it.IsPicked = false;
                this.noteDrawer.ClearRectState(it.Rectangle);
            }
            this.ChartEditModel.PickedNotes.Clear();
        }

        private void TrackRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 点击选中未选中的Track
            if (this.isPicking && !this.isPlaying)
            {
                if (sender is Rectangle rectangle)
                {
                    if (rectangle.DataContext is Track track)
                    {
                        if (!track.IsPicked)
                        {
                            // 取消之前已选择的Track
                            if (this.ChartEditModel.PickedTrack != null)
                            {
                                this.ChartEditModel.PickedTrack.IsPicked = false;
                                this.noteDrawer.ClearRectState(this.ChartEditModel.PickedTrack.Rectangle);
                            }
                            track.IsPicked = true;
                            this.noteDrawer.RectPicked(rectangle);
                            this.ChartEditModel.PickedTrack = track;
                            // 选择后从高光中去除
                            this.highLightTracks.Remove(track);
                            // 尝试移动
                            this.movingStartPoint = e.GetPosition(this.TrackCanvas);
                            this.movingCurrentBeatTime = track.StartBeatTime;
                            this.movingStartBeatTime = track.StartBeatTime;
                            this.isMovingTrack = true;
                        }
                    }
                }
            }
        }

        private void NoteRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.isPicking && !this.isPlaying)
            {
                if (sender is Rectangle rectangle)
                {
                    if (rectangle.DataContext is Note note)
                    {
                        if (!note.IsPicked)
                        {
                            this.noteDrawer.ClearRectState(rectangle);
                            this.highLightNotes.Remove(note);
                        }
                    }
                }
            }
        }

        private void NoteRectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.isPicking && !this.isPlaying)
            {
                if (sender is Rectangle rectangle)
                {
                    if (rectangle.DataContext is Note note)
                    {
                        if (!note.IsPicked)
                        {
                            this.noteDrawer.RectHighLight(rectangle);
                            this.highLightNotes.Add(note);
                        }
                    }
                }
            }
        }

        private void TrackRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.isPicking && !this.isPlaying)
            {
                if (sender is Rectangle rectangle)
                {
                    if (rectangle.DataContext is Track track)
                    {
                        if (!track.IsPicked)
                        {
                            this.noteDrawer.ClearRectState(rectangle);
                            this.highLightTracks.Remove(track);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 当鼠标移入轨道时
        /// </summary>
        private void TrackRectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.isPicking && !this.isPlaying)
            {
                if (sender is Rectangle rectangle)
                {
                    if (rectangle.DataContext is Track track)
                    {
                        if (!track.IsPicked)
                        {
                            this.noteDrawer.RectHighLight(rectangle);
                            this.highLightTracks.Add(track);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 强制触发鼠标在某个轨道或音符内的事件
        /// </summary>
        private void ForceTriggerTrackOrNoteMouseEnter()
        {
            Point mousePosition = Mouse.GetPosition(TrackCanvas);
            IInputElement hitElement = TrackCanvas.InputHitTest(mousePosition);
            if (hitElement is Rectangle rectangle)
            {
                if (rectangle.DataContext is Track track)
                {
                    this.TrackRectangle_MouseEnter(rectangle, null);
                }
                else if (rectangle.DataContext is Note note)
                {
                    this.NoteRectangle_MouseEnter(rectangle, null);
                }
            }
        }
        
        /// <summary>
        /// 清除所有矩形的高光状态
        /// </summary>
        public void ClearAllRectHighLight()
        {
            foreach (Track track in highLightTracks)
            {
                this.noteDrawer.ClearRectState(track.Rectangle);
            }
            foreach (Note note in highLightNotes)
            {
                this.noteDrawer.ClearRectState(note.Rectangle);
            }
        }

        /// <summary>
        /// 更改音量，0-1之间
        /// </summary>
        public void OnMusicVolumeChanged(float musicVolume)
        {
            this.musicPlayer.SetVolume(musicVolume);
        }

        public void OnNoteVolumeChanged(float noteVolume)
        {
            this.hitSoundPlayer.SetVolume(noteVolume);
        }

        public void OnGlobalVolumeChanged(float globalVolume)
        {
            this.audioMixer.SetVolume(globalVolume);
        }

        /// <summary>
        /// 当列宽变化时
        /// </summary>
        public void OnColumnWidthChanged()
        {
            this.DrawTrackGrid();
            this.noteDrawer.RedrawWhenColumnWidthChanged();
            this.columnLabelDrawer.RedrawWhenColumnWidthChanged();
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
            this.TrackEditBoard.TrackCanvasFloor.Width = this.ChartEditModel.TotalWidth + 200;
            this.TrackEditBoard.TrackCanvasFloor.Height = this.ChartEditModel.TotalHeight + this.TrackCanvasViewer.ActualHeight;
            Canvas.SetTop(this.TrackCanvas, this.TrackCanvasViewer.ActualHeight * Common.JudgeLineRateOp);
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
        /// 查找多选框中的音符
        /// </summary>
        private void SelectBoxNotes()
        {
            Rect selectBoxRect = this.selectBoxDrawer.GetRect();
            // 检查已选中的音符是否还在多选框内
            this.selectBoxPickedNotes.RemoveWhere(note =>
            {
                Rect noteRect = note.GetRect();
                if (!selectBoxRect.IntersectsWith(noteRect))
                {
                    note.IsPicked = false;
                    this.noteDrawer.ClearRectState(note.Rectangle);
                    return true;
                }
                return false;
            });
            // 确定左右边界的列数
            int leftColumnIndex = Math.Max(0, (int)(selectBoxRect.X / this.ChartEditModel.ColumnWidth));
            int rightColumnIndex = Math.Min(this.ChartEditModel.ColumnNum - 1, (int)((selectBoxRect.X + selectBoxRect.Width) / this.ChartEditModel.ColumnWidth));
            if (leftColumnIndex >= this.ChartEditModel.ColumnNum || rightColumnIndex < 0) return;
            // 确定上下边界的时间
            BeatTime bottomBeatTime = new BeatTime(this.ChartEditModel.Divide);
            bottomBeatTime.UpdateFromJudgeLineOffset(Math.Max(0, selectBoxRect.Top), this.ChartEditModel.RowWidth);
            BeatTime topBeatTime = new BeatTime(this.ChartEditModel.Divide);
            topBeatTime.UpdateFromJudgeLineOffset(Math.Min(this.ChartEditModel.TotalHeight, selectBoxRect.Bottom), this.ChartEditModel.RowWidth);
            if (bottomBeatTime.Beat >= this.ChartEditModel.BeatNum || topBeatTime.Beat < 0) return;
            // 搜索与多选框相交的音符
            for (int i = leftColumnIndex; i <= rightColumnIndex; i++)
            {
                SkipList<BeatTime, Track> skipList = this.ChartEditModel.TrackSkipLists[i];
                SkipListNode<BeatTime, Track> currentTrackNode = skipList.GetPreNode(bottomBeatTime);
                if (currentTrackNode == skipList.Head) currentTrackNode = currentTrackNode.Next[0];
                while (currentTrackNode != null)
                {
                    Track track = currentTrackNode.Value;
                    // 轨道超过搜索范围则退出
                    if (track.StartBeatTime.IsLaterThan(topBeatTime)) break;
                    Rect trackRect = track.GetRect();

                    if (selectBoxRect.IntersectsWith(trackRect))
                    {
                        SkipListNode<BeatTime, Note> currentNoteNode = track.NoteSkipList.GetPreNode(bottomBeatTime);
                        if (currentNoteNode == track.NoteSkipList.Head) currentNoteNode = currentNoteNode.Next[0];
                        while (currentNoteNode != null)
                        {
                            Note note = currentNoteNode.Value;
                            // 超过搜索范围则退出
                            if (note.StartBeatTime.IsLaterThan(topBeatTime)) break;
                            currentNoteNode = currentNoteNode.Next[0];
                            if (this.selectBoxPickedNotes.Contains(note) || this.ChartEditModel.PickedNotes.Contains(note)) continue;
                            Rect noteRect = note.GetRect();
                            if (selectBoxRect.IntersectsWith(noteRect))
                            {
                                note.IsPicked = true;
                                this.selectBoxPickedNotes.Add(note);
                                this.noteDrawer.RectPicked(note.Rectangle);
                            }
                        }
                        SkipListNode<BeatTime, HoldNote> currentHoldNoteNode = track.HoldNoteSkipList.GetPreNode(bottomBeatTime);
                        if (currentHoldNoteNode == track.HoldNoteSkipList.Head) currentHoldNoteNode = currentHoldNoteNode.Next[0];
                        while (currentHoldNoteNode != null)
                        {
                            HoldNote holdNote = currentHoldNoteNode.Value;
                            if (holdNote.StartBeatTime.IsLaterThan(topBeatTime)) break;
                            currentHoldNoteNode = currentHoldNoteNode.Next[0];
                            if (this.selectBoxPickedNotes.Contains(holdNote) || this.ChartEditModel.PickedNotes.Contains(holdNote)) continue;
                            Rect noteRect = holdNote.GetRect();
                            if (selectBoxRect.IntersectsWith(noteRect))
                            {
                                holdNote.IsPicked = true;
                                this.selectBoxPickedNotes.Add(holdNote);
                                this.noteDrawer.RectPicked(holdNote.Rectangle);
                            }
                        }
                    }
                    currentTrackNode = currentTrackNode.Next[0];
                }
            }
        }

        public void OnScrollChanged()
        {
            // 更新列标签位置
            this.columnLabelDrawer.UpdateOffset();
        }

        /// <summary>
        /// 删除被选中的音符
        /// </summary>
        public void DeletePickedNotes()
        {
            foreach (Note note in this.ChartEditModel.PickedNotes)
            {
                this.noteDrawer.RemoveNote(note);
                this.ChartEditModel.DeleteNote(note);
            }
            this.ChartEditModel.PickedNotes.Clear();
        }

        /// <summary>
        /// 删除被选中的轨道
        /// </summary>
        public void DeletePickedTrack()
        {
            this.noteDrawer.RemoveTrack(this.ChartEditModel.PickedTrack);
            this.ChartEditModel.DeleteTrack(this.ChartEditModel.PickedTrack);
            this.ChartEditModel.PickedTrack = null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.audioMixer.RemoveAllMixerInput();
            this.hitSoundPlayer.Dispose();
            this.musicPlayer.Dispose();
            this.audioMixer.Dispose();
            GC.Collect();
            Console.WriteLine(logTag + "占用文件已释放");
        }
    }
}
