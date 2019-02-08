using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using bossdoyKaraoke_NOW.Enums;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.BackGroundWorker
{
    class Worker
    {
        private static readonly Queue<QueueItem<NewTask>> _workerQueue = new Queue<QueueItem<NewTask>>();

        public static void DoWork(ItemsControl itemsControl, NewTask newTask)
        {
            RunWorker(itemsControl, newTask);
        }

        public static void DoWork(ItemsControl itemsControl, NewTask newTask, TrackInfo trackInfo)
        {
            RunWorker(itemsControl, newTask, trackInfo);
        }

        public static void DoWork(ItemsControl itemsControl, NewTask newTask, int senderID)
        {
            RunWorker(itemsControl, newTask, null, senderID);
        }

        public static void DoWork(ItemsControl itemsControl, NewTask newTask, int senderID, string filePath)
        {
            RunWorker(itemsControl, newTask, null, senderID, filePath);
        }

        private static void RunWorker(ItemsControl itemsControl, NewTask newTask, TrackInfo trackInfo = null, int senderID = 0, string filePath = "")
        {
            
            Player player = Player.Instance;
            ISongsSource songsSource = player.Songs_Source;
            IMediaControls mediaControls = MediaControls.Instance;
            QueuedBackgroundWorker.QueueWorkItem(
            _workerQueue,
            newTask,
             args =>  // DoWork
             {
                 var currentTask = args.Argument;// as string;
                 string duration = string.Empty;

                 switch (currentTask)
                 {
                     case NewTask.ADD_NEW_SONGS:
                         CurrentTask = NewTask.ADD_NEW_SONGS;
                         songsSource.DirSearchSongs(filePath);
                         break;
                     case NewTask.ADD_TO_QUEUE:
                         CurrentTask = NewTask.ADD_TO_QUEUE;
                         duration = songsSource.AddToQueue(trackInfo);
                         if (songsSource.SongsQueue.Count == 1 && CurrentPlayState == PlayState.Stopped)
                         {
                             if(songsSource.IsCdgFileType)
                                 player.LoadCDGFile(trackInfo.FilePath);
                             else
                                 player.LoadVideokeFile(trackInfo.FilePath);
                         }
                         break;
                     case NewTask.ADD_TO_QUEUE_AS_NEXT:
                         CurrentTask = NewTask.ADD_TO_QUEUE_AS_NEXT;
                         duration = songsSource.AddToQueueAsNext(trackInfo);
                         break;
                     case NewTask.REMOVE_FROM_QUEUE:
                         CurrentTask = NewTask.REMOVE_FROM_QUEUE;
                         duration = songsSource.RemoveFromQueue(trackInfo);
                         break;
                     case NewTask.LOAD_CDG_FILE: //Not in use
                         CurrentTask = NewTask.LOAD_CDG_FILE;

                         break;
                     case NewTask.LOAD_QUEUE_SONGS:
                         CurrentTask = NewTask.LOAD_QUEUE_SONGS;
                         if (songsSource.SongsQueue.Count > 0 && CurrentPlayState == PlayState.Stopped)
                         {
                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(trackInfo.FilePath);
                             else
                                 player.LoadVideokeFile(trackInfo.FilePath);
                         }
                         break;
                     case NewTask.EMPTY_QUEUE_LIST:
                         CurrentTask = NewTask.EMPTY_QUEUE_LIST;
                         songsSource.EmptyQueueList();
                         break;
                     case NewTask.LOAD_FAVORITES:
                         CurrentTask = NewTask.LOAD_FAVORITES;
                         break;
                     case NewTask.LOAD_SONGS:
                         CurrentTask = NewTask.LOAD_SONGS;
                         break;

                 }
                 //}
                 return new { ID = senderID, Duration = duration };
             },
            args =>  // RunWorkerCompleted
            {
                var currentID = args.Result.ID;
                var duration = args.Result.Duration;

                switch (CurrentTask)
                {
                    case NewTask.ADD_NEW_SONGS:
                        var myComputerIndex = 2;
                        if (songsSource.Songs != null)
                            itemsControl.ItemsSource = songsSource.Songs[currentID];

                        songsSource.ItemSource[myComputerIndex].Items[0].IsProgressVisible = System.Windows.Visibility.Hidden;
                        break;
                    case NewTask.ADD_TO_QUEUE:
                    case NewTask.ADD_TO_QUEUE_AS_NEXT:
                    case NewTask.REMOVE_FROM_QUEUE:
                       // var count = songsSource.SongsQueue.Count;
                        var parent = itemsControl.Items[0] as ITreeViewModel;
                        parent.Title = duration; //"Song Queue (" + count + "-[" + duration + "])";
                        break;
                    case NewTask.LOAD_QUEUE_SONGS:
                    case NewTask.EMPTY_QUEUE_LIST:
                        var tempSongQueue = songsSource.SongsQueue;
                        if (tempSongQueue.Count > 0)
                            tempSongQueue.RemoveAt(0);
                        itemsControl.ItemsSource = tempSongQueue;
                        break;
                    case NewTask.LOAD_FAVORITES:
                        if (songsSource.Favorites != null)
                            itemsControl.ItemsSource = songsSource.Favorites[currentID];
                        break;
                    case NewTask.LOAD_SONGS:
                        if (songsSource.Songs != null)
                            itemsControl.ItemsSource = songsSource.Songs[currentID];
                        break;
                }

            });
        }
    }
}
