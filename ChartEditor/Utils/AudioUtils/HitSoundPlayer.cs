using ChartEditor.Models;
using ChartEditor.Utils.MusicUtils;
using ChartEditor.ViewModels;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChartEditor.Utils.AudioUtils
{
    /// <summary>
    /// 打击音效播放器
    /// </summary>
    public class HitSoundPlayer
    {
        private static string logTag = "[HitSoundPlayer]";

        private ChartEditModel ChartEditModel;

        private MusicPlayer MusicPlayer;

        private List<int> hitSoundPool0 = new List<int>();

        private List<int> hitSoundPool1 = new List<int>();

        private List<int> hitSoundPool2 = new List<int>();

        private const int poolSize = 20;

        private float volume = 0.5f;

        private bool isPlaying = false;

        private Thread playThread;

        private ManualResetEvent playEvent = new ManualResetEvent(false);

        List<SkipListNode<BeatTime, Track>> trackNodes = new List<SkipListNode<BeatTime, Track>>();

        List<SkipListNode<BeatTime, Note>> noteNodes = new List<SkipListNode<BeatTime, Note>>();

        List<SkipListNode<BeatTime, HoldNote>> holdNoteNodes = new List<SkipListNode<BeatTime, HoldNote>>();

        public HitSoundPlayer(ChartEditModel chartEditModel, MusicPlayer musicPlayer)
        {
            this.ChartEditModel = chartEditModel;
            this.MusicPlayer = musicPlayer;
            this.volume = chartEditModel.NoteVolume / 100;
            // 初始化所有播放节点
            foreach (var trackList in this.ChartEditModel.TrackSkipLists)
            {
                var trackNode = trackList.FirstNode;
                this.trackNodes.Add(trackNode);
                if (trackNode == null)
                {
                    this.noteNodes.Add(null);
                    this.holdNoteNodes.Add(null);
                }
                else
                {
                    this.noteNodes.Add(trackNode.Value.NoteSkipList.FirstNode);
                    this.holdNoteNodes.Add(trackNode.Value.HoldNoteSkipList.FirstNode);
                }
            }
            // 初始化音频池
            for (int i = 0; i < poolSize; i++)
            {
                int stream = Bass.CreateStream(Common.HitSoundPaths[0], Flags: BassFlags.Default);
                if (stream == 0) Console.WriteLine(logTag + "音效加载失败：" + Bass.LastError);
                hitSoundPool0.Add(stream);
                stream = Bass.CreateStream(Common.HitSoundPaths[1], Flags: BassFlags.Default);
                if (stream == 0) Console.WriteLine(logTag + "音效加载失败：" + Bass.LastError);
                hitSoundPool1.Add(stream);
                stream = Bass.CreateStream(Common.HitSoundPaths[2], Flags: BassFlags.Default);
                if (stream == 0) Console.WriteLine(logTag + "音效加载失败：" + Bass.LastError);
                hitSoundPool2.Add(stream);
            }
            this.StartPlayLoop();
        }

        private void PlayLoop()
        {
            while (true)
            {
                this.playEvent.WaitOne();
                if (!this.isPlaying) break;

                // 当前音乐播放时间
                int currentTime = (int)((this.MusicPlayer.GetMusicTime() - this.ChartEditModel.ChartInfo.Delay) * 1000);
                
                for (int i = 0; i < this.ChartEditModel.ColumnNum; i++)
                {
                    var trackNode = this.trackNodes[i];

                    if (trackNode == null) continue;
                    else
                    {
                        var noteNode = this.noteNodes[i];
                        // 判断note是否到达播放时间
                        if (noteNode != null)
                        {
                            if (noteNode.Value.StartTime <= currentTime)
                            {
                                switch (noteNode.Value.Type)
                                {
                                    case NoteType.Tap: this.PlayHitSound(0); break;
                                    case NoteType.Flick: this.PlayHitSound(2); break;
                                    case NoteType.Catch: this.PlayHitSound(1); break;
                                }
                                this.noteNodes[i] = noteNode.Next[0];
                            }
                        }
                        var holdNoteNode = this.holdNoteNodes[i];
                        if (holdNoteNode != null)
                        {
                            if (holdNoteNode.Value.StartTime <= currentTime)
                            {
                                this.PlayHitSound(0);
                                this.holdNoteNodes[i] = holdNoteNode.Next[0];
                            }
                        }
                        if (this.noteNodes[i] == null && this.holdNoteNodes[i] == null)
                        {
                            trackNode = trackNode.Next[0];
                            this.trackNodes[i] = trackNode;
                            if (trackNode != null)
                            {
                                this.noteNodes[i] = trackNode.Value.NoteSkipList.FirstNode;
                                this.holdNoteNodes[i] = trackNode.Value.HoldNoteSkipList.FirstNode;
                            }
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 重置所有节点
        /// </summary>
        public void ResetPlayNodes()
        {
            for (int i = 0; i < this.ChartEditModel.ColumnNum; i++)
            {
                var trackList = this.ChartEditModel.TrackSkipLists[i];
                var trackNode = trackList.FirstNode;
                this.trackNodes[i] = trackNode;

                if (trackNode == null)
                {
                    this.noteNodes[i] = null;
                    this.holdNoteNodes[i] = null;
                }
                else
                {
                    this.noteNodes[i] = trackNode.Value.NoteSkipList.FirstNode;
                    this.holdNoteNodes[i] = trackNode.Value.HoldNoteSkipList.FirstNode;
                }
            }
        }

        /// <summary>
        /// 播放前定位所有节点
        /// </summary>
        public void SetPlayNodes()
        {
            BeatTime currentBeatTime = this.ChartEditModel.CurrentBeat;
            // 是否是起始点
            bool isBeginning = currentBeatTime.IsZero();

            for (int i = 0; i < this.ChartEditModel.ColumnNum; i++)
            {
                var trackList = this.ChartEditModel.TrackSkipLists[i];
                var trackNode = trackList.GetPreNode(currentBeatTime);
                if (trackNode == trackList.Head) trackNode = trackNode.Next[0];
                if (trackNode != null && trackNode.Value.EndBeatTime.IsEarlierThan(currentBeatTime)) trackNode = trackNode.Next[0];
                this.trackNodes[i] = trackNode;
                if (trackNode == null)
                {
                    this.noteNodes[i] = null;
                    this.holdNoteNodes[i] = null;
                }
                else
                {
                    if (isBeginning)
                    {
                        this.noteNodes[i] = trackNode.Value.NoteSkipList.TryGetNodeOrNext(currentBeatTime);
                        this.holdNoteNodes[i] = trackNode.Value.HoldNoteSkipList.TryGetNodeOrNext(currentBeatTime);
                    }
                    else
                    {
                        this.noteNodes[i] = trackNode.Value.NoteSkipList.TryGetNext(currentBeatTime);
                        this.holdNoteNodes[i] = trackNode.Value.HoldNoteSkipList.TryGetNext(currentBeatTime);
                    }
                }
            }
        }

        public void StartPlayLoop()
        {
            this.isPlaying = true;
            if (this.playThread == null)
            {
                this.playThread = new Thread(PlayLoop)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                this.playThread.Start();
            }
            else this.ResumePlayLoop();
        }

        public void PausePlayLoop()
        {
            this.playEvent.Reset();
        }

        public void ResumePlayLoop()
        {
            this.playEvent.Set();
        }

        public void StopPlayLoop()
        {
            isPlaying = false;
            playEvent.Set();
            playThread.Join();
        }

        /// <summary>
        /// 播放音效，0 - Tap, 1 - Catch, 2 - Flick
        /// </summary>
        public void PlayHitSound(int hitSoundType)
        {
            int soundHandle = GetAvailableHandle(hitSoundType);
            if (soundHandle != 0)
            {
                Bass.ChannelSetAttribute(soundHandle, ChannelAttribute.Volume, this.volume);
                Bass.ChannelPlay(soundHandle);
            }
            else
            {
                Console.WriteLine(logTag + "音效池无可用音效");
            }
        }

        /// <summary>
        /// 从池中获取一个可用的音效句柄
        /// </summary>
        private int GetAvailableHandle(int hitSoundType)
        {
            List<int> pool = null;
            switch (hitSoundType)
            {
                case 0: pool = this.hitSoundPool0; break;
                case 1: pool = this.hitSoundPool1; break;
                case 2: pool = this.hitSoundPool2; break;
            }
            if (pool == null) return 0;

            foreach (int handle in pool)
            {
                if (Bass.ChannelIsActive(handle) == PlaybackState.Stopped)
                {
                    return handle;
                }
            }
            return 0;
        }

        public void SetVolume(float volume)
        {
            if (volume < 0 || volume > 1) return;
            this.volume = volume;
        }

        public void Dispose()
        {
            this.StopPlayLoop();
            foreach (int handle in hitSoundPool0)
            {
                Bass.StreamFree(handle);
            }
            foreach (int handle in hitSoundPool1)
            {
                Bass.StreamFree(handle);
            }
            foreach (int handle in hitSoundPool2)
            {
                Bass.StreamFree(handle);
            }
            Bass.Free();
        }
    }
}
