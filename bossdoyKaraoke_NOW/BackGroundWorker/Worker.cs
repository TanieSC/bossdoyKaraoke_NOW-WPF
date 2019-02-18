﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using bossdoyKaraoke_NOW.Enums;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.BackGroundWorker
{
    class Worker
    {
        private static readonly Queue<QueueItem<NewTask>> _workerQueue = new Queue<QueueItem<NewTask>>();
        private static ListView _listViewElement;
        private static TreeView _treeViewElement;

        public static TreeView TreeViewElement { get { return _treeViewElement; } set { _treeViewElement = value; } }
        public static ListView ListViewElement { get { return _listViewElement; } set { _listViewElement = value; } }

        public static void DoWork(NewTask newTask)
        {
            RunWorker(newTask);
        }

        public static void DoWork(NewTask newTask, TrackInfo trackInfo)
        {
            RunWorker(newTask, trackInfo);
        }

        public static void DoWork(NewTask newTask, int senderID)
        {
            RunWorker(newTask, null, senderID);
        }

        public static void DoWork(NewTask newTask, int senderID, string filePath)
        {
            RunWorker(newTask, null, senderID, filePath);
        }

        private static void RunWorker(NewTask newTask, TrackInfo trackInfo = null, int senderID = 0, string filePath = "")
        {
            
            Player player = Player.Instance;
            ISongsSource songsSource = player.Songs_Source;
            IMediaControls mediaControls = MediaControls.Instance;
            QueuedBackgroundWorker.QueueWorkItem(
            _workerQueue,
            newTask,
             args =>  // DoWork
             {
                 var currentTask = args.Argument;
                 string songQueueTitle = string.Empty;

                 switch (currentTask)
                 {
                     case NewTask.ADD_NEW_SONGS:
                         CurrentTask = NewTask.ADD_NEW_SONGS;
                         songsSource.DirSearchSongs(filePath);
                         break;
                     case NewTask.ADD_TO_QUEUE:
                         CurrentTask = NewTask.ADD_TO_QUEUE;
                         songQueueTitle = songsSource.AddToQueue(trackInfo);
                         if (songsSource.SongsQueue.Count == 1 && CurrentPlayState == PlayState.Stopped)
                         {
                             if(songsSource.IsCdgFileType)
                                 player.LoadCDGFile(trackInfo.FilePath);
                             else
                                 player.LoadVideokeFile(trackInfo.FilePath);

                             MediaControls.Instance.IconPlayPause = PackIconKind.Pause;
                         }
                         break;
                     case NewTask.ADD_TO_QUEUE_AS_NEXT:
                         CurrentTask = NewTask.ADD_TO_QUEUE_AS_NEXT;
                         songQueueTitle = songsSource.AddToQueueAsNext(trackInfo);
                         break;
                     case NewTask.REMOVE_FROM_QUEUE:
                         CurrentTask = NewTask.REMOVE_FROM_QUEUE;
                         songQueueTitle = songsSource.RemoveFromQueue(trackInfo);
                         break;
                     case NewTask.LOAD_CDG_FILE: //Not in use
                         CurrentTask = NewTask.LOAD_CDG_FILE;
                         break;
                     case NewTask.LOAD_QUEUE_SONGS:
                         CurrentTask = NewTask.LOAD_QUEUE_SONGS;
                         if (songsSource.SongsQueue.Count > 0 && CurrentPlayState == PlayState.Stopped)
                         {
                             songsSource.PlayFirstSongInQueue();

                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(trackInfo.FilePath);
                             else
                                 player.LoadVideokeFile(trackInfo.FilePath);

                             songsSource.LoadSongsInQueue();

                             MediaControls.Instance.IconPlayPause = PackIconKind.Pause;
                         }
                         break;
                     case NewTask.EMPTY_QUEUE_LIST:
                         CurrentTask = NewTask.EMPTY_QUEUE_LIST;
                         songQueueTitle = songsSource.EmptyQueueList();
                         break;
                     case NewTask.LOAD_FAVORITES:
                         CurrentTask = NewTask.LOAD_FAVORITES;
                         break;
                     case NewTask.LOAD_SONGS:
                         CurrentTask = NewTask.LOAD_SONGS;
                         break;

                 }

                 return new { ID = senderID, Duration = songQueueTitle };
             },
            args =>  // RunWorkerCompleted
            {
                var currentID = args.Result.ID;
                var songQueueTitle = args.Result.Duration;
                var parentTreeview = _treeViewElement.Items[0] as ITreeViewModel;

                switch (CurrentTask)
                {
                    case NewTask.ADD_NEW_SONGS:
                        var myComputerIndex = 2;
                        if (songsSource.Songs != null)
                            _listViewElement.ItemsSource = songsSource.Songs[currentID];

                        songsSource.ItemSource[myComputerIndex].Items[0].IsProgressVisible = System.Windows.Visibility.Hidden;
                        break;
                    case NewTask.ADD_TO_QUEUE:
                    case NewTask.ADD_TO_QUEUE_AS_NEXT:
                    case NewTask.REMOVE_FROM_QUEUE:
                        parentTreeview.Title = songQueueTitle;

                        if (CurrentTask == NewTask.REMOVE_FROM_QUEUE)
                        {
                            _listViewElement.ItemsSource = songsSource.SongsQueue;
                        }
                        break;
                    case NewTask.LOAD_QUEUE_SONGS:
                    case NewTask.EMPTY_QUEUE_LIST:
                        _listViewElement.ItemsSource = songsSource.SongsQueue; ;

                        if (CurrentTask == NewTask.EMPTY_QUEUE_LIST)
                            parentTreeview.Title = songQueueTitle;
                        break;
                    case NewTask.LOAD_FAVORITES:
                        if (songsSource.Favorites != null)
                            _listViewElement.ItemsSource = songsSource.Favorites[currentID];
                        break;
                    case NewTask.LOAD_SONGS:
                        if (songsSource.Songs != null)
                            _listViewElement.ItemsSource = songsSource.Songs[currentID];
                        break;
                }

            });
        }
    }
}
