using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
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

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        public static void DoWork(NewTask newTask)
        {
            RunWorker(newTask);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="filter">Filter song by typing string on the search box</param>
        public static void DoWork(NewTask newTask, string filter)
        {
            RunWorker(newTask, null, 0, "", filter);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="trackInfo">Contains the song information for adding, removing, and playing</param>
        public static void DoWork(NewTask newTask, TrackInfo trackInfo)
        {
            RunWorker(newTask, trackInfo);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="senderID">Song index from the list used for loading song to listview</param>
        public static void DoWork(NewTask newTask, int senderID)
        {
            RunWorker(newTask, null, senderID);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="senderID">Song index from the list used for loading song to listview</param>
        /// <param name="filePath">The path folder on the song, used for adding new songs to the collections</param>
        public static void DoWork(NewTask newTask, int senderID, string filePath)
        {
            RunWorker(newTask, null, senderID, filePath);
        }

        private static void RunWorker(NewTask newTask, TrackInfo trackInfo = null, int senderID = 0, string filePath = "", string filter = "")
        {
            
            Player player = Player.Instance;
            ISongsSource songsSource = SongsSource.Instance;//player.SongsSrc;
            IMediaControls mediaControls = MediaControls.Instance;
            ISearchBoxModel searchBoxModel = SearchBoxModel.Instance;
            QueuedBackgroundWorker.QueueWorkItem(
            _workerQueue,
            newTask,
             args =>  // DoWork
             {
                 var currentTask = args.Argument;
                 string songQueueTitle = string.Empty;
                 ObservableCollection<TrackInfo> filteredSong = new ObservableCollection<TrackInfo>();

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
                             if (songsSource.IsCdgFileType)
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

                         songsSource.PlayFirstSongInQueue();

                         if (songsSource.SongsQueue.Count > 0 && CurrentPlayState == PlayState.Stopped)
                         {
                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(songsSource.SongsQueue[0].FilePath);
                             else
                                 player.LoadVideokeFile(songsSource.SongsQueue[0].FilePath);

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
                         searchBoxModel.ItemId = senderID;
                         break;
                     case NewTask.SEARCH_LISTVIEW:
                         CurrentTask = NewTask.SEARCH_LISTVIEW;
                         filteredSong = searchBoxModel.FilteredSong(filter);
                         break;
                 }

                 return new { ID = senderID, Duration = songQueueTitle, Filter = filteredSong };
             },
            args =>  // RunWorkerCompleted
            {
                var currentID = args.Result.ID;
                var songQueueTitle = args.Result.Duration;
                var parentTreeview = _treeViewElement.Items[0] as ITreeViewModel;
                var filteredSong = args.Result.Filter;

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
                        if (songsSource.SongsQueue.Count != 0)
                            _listViewElement.ItemsSource = songsSource.SongsQueue;

                        if (CurrentTask == NewTask.EMPTY_QUEUE_LIST)
                            parentTreeview.Title = songQueueTitle;
                        break;
                    case NewTask.LOAD_FAVORITES:
                        if (songsSource.Favorites != null)
                            _listViewElement.ItemsSource = songsSource.Favorites[currentID];
                        break;
                    case NewTask.LOAD_SONGS:
                        if (songsSource.Songs.Count != 0)
                            _listViewElement.ItemsSource = songsSource.Songs[currentID];
                        break;
                    case NewTask.SEARCH_LISTVIEW:
                       // CurrentTask = NewTask.LOAD_SONGS;
                        _listViewElement.ItemsSource = filteredSong;                        
                        break;
                }
            });
        }
    }
}
