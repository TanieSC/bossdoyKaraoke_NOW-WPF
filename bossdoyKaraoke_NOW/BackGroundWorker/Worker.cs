using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using bossdoyKaraoke_NOW.Enums;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.EqualizerEnum;
using static bossdoyKaraoke_NOW.Enums.KaraokeNowFiles;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;

namespace bossdoyKaraoke_NOW.BackGroundWorker
{
    class Worker
    {
        private const int _favoritesIndex = 1;
        private const int _myComputerIndex = 2;
        private static readonly Queue<QueueItem<NewTask>> _workerQueue = new Queue<QueueItem<NewTask>>();
        private static ListView _listViewElement;
        private static TreeView _treeViewElement;
        private static TrackInfoModel _trackInfo;
        private static NewPreset _equalizerPreset;
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
        public static void DoWork(NewTask newTask, TrackInfoModel trackInfo)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newTask"></param>
        /// <param name="equalizer"></param>
        public static void DoWork(NewTask newTask, NewPreset equalizerPreset)
        {
            _equalizerPreset = equalizerPreset;
            RunWorker(newTask);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newTask"></param>
        private static void RunWorker(NewTask newTask)//, TrackInfo trackInfo = null, int senderID = 0, string filePath = "", string filter = "")
        {           
            Player player = Player.Instance;
            EqualizerModel equalizer = EqualizerModel.Instance;
            ISongsSource songsSource = SongsSource.Instance;//player.SongsSrc;
            IMediaControlsVModel mediaControls = MediaControlsVModel.Instance;
            ISearchBoxVModel searchBoxModel = SearchBoxVModel.Instance;
            ITreeViewDialogVModel dialog = TreeViewDialogVModel.Instance;
            QueuedBackgroundWorker.QueueWorkItem(
            _workerQueue,
            newTask,
             args =>  // DoWork
             {
                 var currentTask = args.Argument;
                 string songQueueTitle = string.Empty;
                 ObservableCollection<TrackInfoModel> filteredSong = new ObservableCollection<TrackInfoModel>();

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

                         if (_trackInfo == null)
                         {
                             Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

                             openFileDialog.Filter = PlayerBase.Filter; //"Media files (*.cdg;*.mp3;*.mp4)|*.cdg;*.mp3;*.mp4|Cdg files (*.cdg)|*.cdg|Mp3 files (*.mp3)|*.mp3|Mp4 files (*.mp4)|*.mp4";

                             if (openFileDialog.ShowDialog() == true)
                             {
                                 string filename = openFileDialog.FileName;
                                 _trackInfo = songsSource.trackInfo(filename, songsSource.SongQueueCount + 1);

                                 //filename = Regex.Replace(filename, "\\.mp3$", ".cdg", RegexOptions.IgnoreCase);

                                 //if (File.Exists(filename))
                                 //{
                                 //    _trackInfo.FilePath = filename;
                                 //}
                             }
                             else
                             {
                                 break;
                             }
                         }

                         songQueueTitle = songsSource.AddToQueue(_trackInfo);

                         if (songsSource.SongQueueCount == 1 && CurrentPlayState == PlayState.Stopped)
                         {
                             if (songsSource.IsCdgFileType)
                                 player.LoadCDGFile(_trackInfo.FilePath);
                             else
                                 player.LoadVideokeFile(_trackInfo.FilePath);

                             MediaControlsVModel.Instance.IconPlayPause = PackIconKind.Pause;
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

                             MediaControlsVModel.Instance.IconPlayPause = PackIconKind.Pause;
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
                         songsSource.RemoveSelectedSong(_trackInfo, _treeViewModelChild);
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

                             MediaControlsVModel.Instance.IconPlayPause = PackIconKind.Pause;

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
                     case NewTask.EQ_ENABLED:
                         CurrentTask = NewTask.EQ_ENABLED;
                         player.VlcPlayer.Volume = player.Volume;
                         equalizer.EnableEQ();
                         break;
                     case NewTask.LOAD_EQ_PRESET:
                         CurrentTask = NewTask.LOAD_EQ_PRESET;
                         equalizer.UpdateEQBassPreamp(equalizer.PreAmp);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand0, equalizer.EQ0);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand1, equalizer.EQ1);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand2, equalizer.EQ2);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand3, equalizer.EQ3);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand4, equalizer.EQ4);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand5, equalizer.EQ5);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand6, equalizer.EQ6);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand7, equalizer.EQ7);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand8, equalizer.EQ8);
                         equalizer.UpdateEQBass(NewPreset.AudioEQBand9, equalizer.EQ9);

                         equalizer.UpdateEQVlc();
                         equalizer.SaveEQSettings();
                         break;
                     case NewTask.UPDATE_EQ_SETTINGS:
                         CurrentTask = NewTask.UPDATE_EQ_SETTINGS;

                         equalizer.EQSelectedPreset = -1;

                         switch (_equalizerPreset)
                         {
                             case NewPreset.AudioEQBand0:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand0, equalizer.EQ0);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand1:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand1, equalizer.EQ1);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand2:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand2, equalizer.EQ2);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand3:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand3, equalizer.EQ3);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand4:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand4, equalizer.EQ4);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand5:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand5, equalizer.EQ5);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand6:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand6, equalizer.EQ6);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand7:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand7, equalizer.EQ7);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand8:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand8, equalizer.EQ8);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQBand9:
                                 equalizer.UpdateEQBass(NewPreset.AudioEQBand9, equalizer.EQ9);
                                 equalizer.UpdateEQVlc();
                                 break;
                             case NewPreset.AudioEQPreamp:
                                 equalizer.UpdateEQBassPreamp(equalizer.PreAmp);
                                 equalizer.UpdateEQVlcPreamp(equalizer.PreAmp);
                                 break;
                         }
                         break;
                     case NewTask.SAVE_EQ_SETTINGS:
                         CurrentTask = NewTask.SAVE_EQ_SETTINGS;
                         equalizer.SaveEQSettings();
                         break;
                 }

                 return new {WorkerTask = currentTask, Duration = songQueueTitle, Filter = filteredSong };
             },
            args =>  // RunWorkerCompleted
            {
                var currentTask = args.Result.WorkerTask;
                var songQueueTitle = args.Result.Duration;
                var parentTreeview = _treeViewElement.Items[0] as ITreeViewVModel;
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
                        if (_trackInfo != null)
                        {
                            parentTreeview.Title = songQueueTitle;

                            if (currentTask == NewTask.ADD_TO_QUEUE || currentTask == NewTask.ADD_TO_QUEUE_AS_NEXT)
                            {
                                _trackInfo.IsSelected = false;
                            }

                            if (currentTask == NewTask.REMOVE_FROM_QUEUE)
                            {
                                _listViewElement.ItemsSource = songsSource.SongsQueue;
                            }
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
                        //songsSource.Songs.Se
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
                    case NewTask.UPDATE_EQ_SETTINGS:
                        break;
                    case NewTask.LOAD_EQ_PRESET:
                        break;
                }
            });
        }
    }
}