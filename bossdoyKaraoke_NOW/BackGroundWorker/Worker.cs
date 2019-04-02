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
using static bossdoyKaraoke_NOW.Enums.KaraokeNowFiles;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.BackGroundWorker
{
    class Worker
    {
        private const int _favoritesIndex = 1;
        private const int _myComputerIndex = 2;
        private static readonly Queue<QueueItem<NewTask>> _workerQueue = new Queue<QueueItem<NewTask>>();
        private static ListView _listViewElement;
        private static TreeView _treeViewElement;
        private static TrackInfo _trackInfo;
        private static int _senderID;
        private static string _filePath;
        private static string _searchFilter;
        private static ITreeViewModelChild _treeViewModelChild;


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
        /// <param name="treeViewModelChild">The TreeviewItem that contains inforamtion used for adding/removing treeview items.</param>
        public static void DoWork(NewTask newTask, ITreeViewModelChild treeViewModelChild)
        {
            _treeViewModelChild = treeViewModelChild;

            if (treeViewModelChild != null)
                _senderID = treeViewModelChild.ID;

            RunWorker(newTask);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="filter">Filter song by typing string on the search box</param>
        public static void DoWork(NewTask newTask, string filter)
        {
            // RunWorker(newTask, null, 0, "", filter);
            _searchFilter = filter;
            RunWorker(newTask);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="trackInfo">Contains the song information for adding, removing, and playing</param>
        public static void DoWork(NewTask newTask, TrackInfo trackInfo)
        {
            _trackInfo = trackInfo;
            // RunWorker(newTask, trackInfo);
            RunWorker(newTask);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="senderID">Song index from treeview used for loading song to listview</param>
        public static void DoWork(NewTask newTask, int senderID)
        {
            //RunWorker(newTask, null, senderID);
            _senderID = senderID;
            RunWorker(newTask);
        }

        /// <summary>
        /// Run the new task in background
        /// </summary>
        /// <param name="newTask">Task to run</param>
        /// <param name="senderID">Song index from the list used for loading song to listview</param>
        /// <param name="filePath">The path folder of the song, used for adding new songs to the collections</param>
        public static void DoWork(NewTask newTask, int senderID, string filePath)
        {
            //RunWorker(newTask, null, senderID, filePath);
            _senderID = senderID;
            _filePath = filePath;
            RunWorker(newTask);
        }

        private static void RunWorker(NewTask newTask)//, TrackInfo trackInfo = null, int senderID = 0, string filePath = "", string filter = "")
        {           
            Player player = Player.Instance;
            ISongsSource songsSource = SongsSource.Instance;//player.SongsSrc;
            IMediaControls mediaControls = MediaControls.Instance;
            ISearchBoxModel searchBoxModel = SearchBoxModel.Instance;
            ITreeViewDialogModel dialog = TreeViewDialogModel.Instance;
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
                         songsSource.DirSearchSongs(_filePath);
                         break;
                     case NewTask.ADD_NEW_FAVORITES:
                         CurrentTask = NewTask.ADD_NEW_FAVORITES;
                         songsSource.CreateFavorites(_treeViewModelChild);
                         break;
                     case NewTask.ADD_TO_QUEUE:
                         CurrentTask = NewTask.ADD_TO_QUEUE;

                         songQueueTitle = songsSource.AddToQueue(_trackInfo);

                         if (songsSource.SongQueueCount == 1 && CurrentPlayState == PlayState.Stopped)
                         {
                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(_trackInfo.FilePath);
                             else
                                 player.LoadVideokeFile(_trackInfo.FilePath);

                             MediaControls.Instance.IconPlayPause = PackIconKind.Pause;
                         }
                         break;
                     case NewTask.ADD_FAVORITES_TO_QUEUE:
                         CurrentTask = NewTask.ADD_FAVORITES_TO_QUEUE;

                         var previousCount = songsSource.SongQueueCount;

                         songsSource.AddFavoritesToSongQueue(_senderID);

                         if (songsSource.SongQueueCount > 0 && CurrentPlayState == PlayState.Stopped)
                         {
                             songsSource.PlayFirstSongInQueue();

                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(songsSource.SongsQueue[0].FilePath);
                             else
                                 player.LoadVideokeFile(songsSource.SongsQueue[0].FilePath);

                             MediaControls.Instance.IconPlayPause = PackIconKind.Pause;
                         }

                         songsSource.LoadSongsInQueue(previousCount);
                         break;
                     case NewTask.ADD_TO_QUEUE_AS_NEXT:
                         CurrentTask = NewTask.ADD_TO_QUEUE_AS_NEXT;
                         songQueueTitle = songsSource.AddToQueueAsNext(_trackInfo);
                         break;
                     case NewTask.ADD_PLAYEDSONGS_TO_FAVORITES:
                         CurrentTask = NewTask.ADD_PLAYEDSONGS_TO_FAVORITES;
                         songsSource.CreateFavoritesPlayedSongs(_treeViewModelChild);
                         break;
                     case NewTask.REMOVE_FROM_QUEUE:
                         songQueueTitle = songsSource.RemoveFromQueue(_trackInfo);
                         break;
                     case NewTask.REMOVE_SELECTED_FAVORITE:
                         songsSource.RemoveSelectedFavorite(_trackInfo, _treeViewModelChild);
                         break;
                     case NewTask.REMOVE_SELECTED_SONG:
                         songsSource.RemoveSelectedSong(_trackInfo, _senderID);
                         break;
                     case NewTask.LOAD_QUEUE_SONGS:
                         CurrentTask = NewTask.LOAD_QUEUE_SONGS;

                         songsSource.PlayFirstSongInQueue();

                         if (songsSource.SongQueueCount > 0 && CurrentPlayState == PlayState.Stopped)
                         {
                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(songsSource.SongsQueue[0].FilePath);
                             else
                                 player.LoadVideokeFile(songsSource.SongsQueue[0].FilePath);

                             MediaControls.Instance.IconPlayPause = PackIconKind.Pause;

                             songsSource.LoadSongsInQueue();                          
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
                         searchBoxModel.ItemId = _senderID;
                         break;
                     case NewTask.SEARCH_LISTVIEW:
                         //CurrentTask = NewTask.SEARCH_LISTVIEW;
                         filteredSong = searchBoxModel.FilteredSong(_searchFilter);
                         break;
                     case NewTask.REMOVE_FAVORITES:
                         CurrentTask = NewTask.REMOVE_FAVORITES;
                         songsSource.RemoveTreeViewItem(Create.Favorites, _treeViewModelChild);
                         break;
                     case NewTask.REMOVE_SONGS:
                         CurrentTask = NewTask.REMOVE_SONGS;
                         songsSource.RemoveTreeViewItem(Create.NewSongs, _treeViewModelChild);
                         break;
                 }

                 return new {WorkerTask = currentTask, Duration = songQueueTitle, Filter = filteredSong };
             },
            args =>  // RunWorkerCompleted
            {
                var currentTask = args.Result.WorkerTask;
                var songQueueTitle = args.Result.Duration;
                var parentTreeview = _treeViewElement.Items[0] as ITreeViewModel;
                var filteredSong = args.Result.Filter;


                switch (currentTask)
                {
                    case NewTask.ADD_NEW_SONGS:
                        if (songsSource.Songs != null)
                            _listViewElement.ItemsSource = songsSource.Songs[_senderID];

                        dialog.ShowDialog = false;
                        songsSource.ItemSource[_myComputerIndex].Items[0].IsProgressVisible = System.Windows.Visibility.Hidden;
                        break;
                    case NewTask.ADD_NEW_FAVORITES:
                        dialog.ShowDialog = false;
                        break;
                    case NewTask.ADD_TO_QUEUE:
                    case NewTask.ADD_TO_QUEUE_AS_NEXT:
                    case NewTask.REMOVE_FROM_QUEUE:
                        parentTreeview.Title = songQueueTitle;

                        if (currentTask == NewTask.ADD_TO_QUEUE || currentTask == NewTask.ADD_TO_QUEUE_AS_NEXT)
                        {
                            _trackInfo.IsSelected = false;
                        }

                        if (currentTask == NewTask.REMOVE_FROM_QUEUE)
                        {
                            _listViewElement.ItemsSource = songsSource.SongsQueue;
                        }
                        break;
                    case NewTask.LOAD_QUEUE_SONGS:
                    case NewTask.ADD_FAVORITES_TO_QUEUE:
                    case NewTask.EMPTY_QUEUE_LIST:
                        dialog.ShowDialog = false;
                        _listViewElement.ItemsSource = songsSource.SongsQueue;

                        if (currentTask == NewTask.EMPTY_QUEUE_LIST)
                            parentTreeview.Title = songQueueTitle;
                        break;
                    case NewTask.ADD_PLAYEDSONGS_TO_FAVORITES:
                    case NewTask.LOAD_FAVORITES:
                    case NewTask.REMOVE_SELECTED_FAVORITE:
                        if (songsSource.Favorites.Count != 0)
                            _listViewElement.ItemsSource = songsSource.Favorites[_senderID];
                        break;
                    case NewTask.LOAD_SONGS:
                    case NewTask.REMOVE_SELECTED_SONG:
                        if (songsSource.Songs.Count != 0)
                            _listViewElement.ItemsSource = songsSource.Songs[_senderID];
                        break;
                    case NewTask.SEARCH_LISTVIEW:
                        _listViewElement.ItemsSource = filteredSong;                        
                        break;
                    case NewTask.REMOVE_FAVORITES:
                        songsSource.ItemSource[_favoritesIndex].Items.Remove(_treeViewModelChild);
                        songsSource.Favorites[_senderID].Clear();
                        break;
                    case NewTask.REMOVE_SONGS:
                        songsSource.ItemSource[_myComputerIndex].Items.Remove(_treeViewModelChild);
                        songsSource.Songs[_senderID].Clear();
                        break;
                }
            });
        }
    }
}