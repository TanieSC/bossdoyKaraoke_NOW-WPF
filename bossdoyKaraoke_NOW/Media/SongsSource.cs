using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using MaterialDesignThemes.Wpf;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.KaraokeNowFiles;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;
using static bossdoyKaraoke_NOW.Enums.TreeViewRootItemEnum;

namespace bossdoyKaraoke_NOW.Media
{
    public class SongsSource : ISongsSource
    {
        private const int _songQueueIndex = 0;
        private const int _favoritesIndex = 1;
        private const int _myComputerIndex = 2;
        private Color color = (Color)ColorConverter.ConvertFromString("#DD000000");
        private static string _filePath = PlayerBase.FilePath; // Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\karaokeNow\";
        private static HashSet<string> _extensions = PlayerBase.Entensions; //new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cdg", ".mp4", ".flv" };
        private string _extPattern = HashSetExtensionsToString(_extensions);
        private string _logs = _filePath + @"logs\";
        private string _favoritesPath = _filePath + @"favorites\";
        private string _songsPath = _filePath + @"songs\";
        private string _songQueueList = _filePath + @"SongQueueList.que";
        private string _mediaFileName;
        private string _songQueueTitle = string.Empty;
        private double _totalDuration = 0.0;

        private bool _isCdgFileType;
        private bool _isAddingToQueue;

        private RootNode _rootNode;
        private List<ITreeViewVModel> _itemSource;
        private static SongsSource _instance;
        private List<ObservableCollection<TrackInfoModel>> _songs = new List<ObservableCollection<TrackInfoModel>>();
        private List<ObservableCollection<TrackInfoModel>> _favorites = new List<ObservableCollection<TrackInfoModel>>();
        private List<TrackInfoModel> _playedSongs = new List<TrackInfoModel>();
        private List<TrackInfoModel> _songsQueue;
        private TrackInfoModel _trackInfo;

        public List<ITreeViewVModel> ItemSource { get { return _itemSource; } }
        public List<ObservableCollection<TrackInfoModel>> Songs { get { return _songs; } }
        public List<ObservableCollection<TrackInfoModel>> Favorites { get { return _favorites; } set { _favorites = value; } }
        public ObservableCollection<TrackInfoModel> SongsQueue
        {          
            get
            {
                return new ObservableCollection<TrackInfoModel>(_songsQueue);
            }
            set
            {
                _songsQueue = value.ToList();
            }
        }

        public int SongQueueCount { get { return _songsQueue.Count; } }

        public int PlayedSongsCount { get { return _playedSongs.Count; } }

        /// <summary>
        /// Check method to know if file loaded is CdgMp3 oor Video file
        /// </summary>
        public bool IsCdgFileType
        {
            get
            {
                return _isCdgFileType;
            }
            set
            {
                _isCdgFileType = value;
            }
        }

        public static SongsSource Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SongsSource();
                }
                return _instance;
            }
        }

        public SongsSource()
        {
            _itemSource = new List<ITreeViewVModel>();
        }

        /// <summary>
        /// Method to load all songs collection (e.g Song Queue, Favorites, and Collections from Computer)
        /// </summary>
        public void LoadSongCollections()
        {
            DirectoryInfo directoryInfo;
            DirectorySecurity directorySecurity;
            AccessRule accessRule;
            SecurityIdentifier securityIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            bool directoryExist = false;

            PackIcon icon = new PackIcon();
            icon.Kind = PackIconKind.Music;

            if (Directory.Exists(_filePath))
                directoryExist = true;

            directoryInfo = Directory.CreateDirectory(_filePath);

            if (!directoryExist)
            {
                directoryInfo = Directory.CreateDirectory(_filePath);
                bool modified;
                directorySecurity = directoryInfo.GetAccessControl();
                accessRule = new FileSystemAccessRule(
                        securityIdentifier,
                        FileSystemRights.FullControl,
                        AccessControlType.Allow);
                directorySecurity.ModifyAccessRule(AccessControlModification.Add, accessRule, out modified);
                directoryInfo.SetAccessControl(directorySecurity);
            }

            Directory.CreateDirectory(_favoritesPath);
            Directory.CreateDirectory(_favoritesPath);
            Directory.CreateDirectory(_songsPath);
            if (!File.Exists(_songQueueList))
            {
                using (File.Create(_songQueueList));
            }

            List<string> songs;

            foreach (var node in Enum.GetValues(typeof(RootNode)))
            {
                ITreeViewVModel items = null;

                if (Enum.TryParse(node.ToString().ToUpper(), out _rootNode))
                {
                    switch (_rootNode)
                    {
                        case RootNode.SONG_QUEUE:
                            songs = new List<string>();
                            songs = Directory.EnumerateFiles(_filePath, "*.que", SearchOption.AllDirectories).ToList();
                            CurrentTask = NewTask.LOAD_QUEUE_SONGS;
                            if (songs.Count() > 0)
                            {
                                _songsQueue = TextSearchSongs(songs)[0].ToList();
                                _songQueueTitle = "Song Queue (Empty)";
                            }

                            items = AddTreeViewItems(items, PackIconKind.Music, _songQueueTitle);//_songsQueue.Count > 0 ? "Song Queue (" + _songsQueue.Count + "-[" + Utils.FixTimespan(_totalDuration, "HHMMSS") + "])" : "Song Queue (Empty)");
                            _itemSource.Add(items);
                            break;
                        case RootNode.MY_FAVORITES:
                            songs = new List<string>();
                            songs = Directory.EnumerateFiles(_favoritesPath, "*.fav", SearchOption.AllDirectories).OrderByDescending(file => new FileInfo(file).CreationTime).ToList();
                            CurrentTask = NewTask.LOAD_FAVORITES;
                            if (songs.Count > 0)
                            {
                                _favorites = TextSearchSongs(songs);
                            }

                            items = AddTreeViewItems(items, PackIconKind.FavoriteOutline, "Favorites", PackIconKind.Favorite, songs, PackIconKind.Folder, "Add Favorites");
                            _itemSource.Add(items);
                            break;
                        case RootNode.MY_COMPUTER:
                            songs = new List<string>();
                            songs = Directory.EnumerateFiles(_songsPath, "*.bkN", SearchOption.AllDirectories).OrderByDescending(file => new FileInfo(file).CreationTime).ToList();
                            CurrentTask = NewTask.LOAD_SONGS;
                            if (songs.Count > 0)
                            {
                                _songs = TextSearchSongs(songs);
                            }

                            items = AddTreeViewItems(items, PackIconKind.Monitor, "My Computer", PackIconKind.Music, songs, PackIconKind.Folder, "Add Songs");
                            _itemSource.Add(items);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///  Process the first item to play it automatically
        /// </summary>
        public void PlayFirstSongInQueue()
        {
            if (_songsQueue.Count > 0)
            {              
                _isAddingToQueue = false;
                AddRemoveFromQueue(_songsQueue[0], true);
                if (_trackInfo == null)
                {
                    _songsQueue.Remove(_songsQueue[0]);

                    PlayFirstSongInQueue();
                }
                else
                {
                    _songsQueue[0].Tags = _trackInfo.Tags;
                    _songsQueue[0].Tags.duration = 0.0;
                    //_totalDuration = 0.0;
                }
            }
        }

        /// <summary>
        /// Load the saved songs from SongQueueList.que to our songQueue list
        /// </summary>
        public void LoadSongsInQueue(int songQueuePreviousCount = 0)
        {          
            var songQueue = Worker.TreeViewElement.Items[_songQueueIndex] as ITreeViewVModel;

            lock (_songsQueue)
            {
                if (_songsQueue.Count > 0)
                {
                    for (int i = songQueuePreviousCount; i < _songsQueue.Count; i++)
                    {
                        AddRemoveFromQueue(_songsQueue[i], true);

                        if (_trackInfo == null)
                        {
                            _songsQueue.Remove(_songsQueue[i]);
                            i -= 1;
                        }
                        else
                        {
                            _songsQueue[i] = _trackInfo;
                            _totalDuration += _trackInfo.Tags.duration;
                            _songQueueTitle = "Song Queue (" + (i + 1) + "-[" + TimeSpan.FromSeconds(_totalDuration).ToString(@"d\.hh\:mm\:ss") + "])";
                            songQueue.Title = _songQueueTitle;
                            TreeViewDialogVModel.Instance.DialogStatus = _songQueueTitle;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to load/add favorites to our SongQueue List
        /// </summary>
        /// <param name="senderId">The index of favorites to add</param>
        public void AddFavoritesToSongQueue(int senderId)
        {
            var favorites = _favorites[senderId];

            for (int i = 0; i < favorites.Count; i++)
            {
                _songsQueue.Add(favorites[i]);
            }

            CreateKaraokeNowFiles(Create.SongQueueList);
        }

        /// <summary>
        /// Method to create favorites from played songs.
        /// </summary>
        /// <param name="sender">The index of favorites to update.</param>
        public void CreateFavoritesPlayedSongs(ITreeViewModelChild sender)
        {
            if (_playedSongs.Count <= 0) return;

            CreateKaraokeNowFiles(Create.FromPlayedSongs, sender);
        }

        /// <summary>
        /// Method to create the treeview to display menu, used by LoadSongCollections() method
        /// </summary>
        /// <param name="songsSource">The ITreeViewModel</param>
        /// <param name="kindParent">Icon for parent node on treeview</param>
        /// <param name="parentTitle">Title for parent treeview node</param>
        /// <param name="kindChild">Ocin for child treeview node</param>
        /// <param name="songs">The list of song collections</param>
        /// <param name="kindAddChild">Icon for Add new child treeview node</param>
        /// <param name="addChildTitle">Title for Add new child treeview node</param>
        /// <returns>Returns the created treeview menu in ITreeViewModel form</returns>
        private ITreeViewVModel AddTreeViewItems(ITreeViewVModel songsSource, PackIconKind kindParent, string parentTitle, PackIconKind kindChild = PackIconKind.Null, List<string> songs = null, PackIconKind kindAddChild = PackIconKind.Null, string addChildTitle = null)
        {
            string fileName;
            string fileExt;
            Color color = (Color)ColorConverter.ConvertFromString("#DD000000");
            songsSource = new TreeViewVModel() { PackIconKind = kindParent, Foreground = new SolidColorBrush(color), Title = parentTitle, CurrentTask = CurrentTask };
            
            if (songs != null)
            {
                for (int i = 0; i < songs.Count; i++)
                {
                    fileExt = System.IO.Path.GetExtension(songs[i]);
                    fileName = System.IO.Path.GetFileName(songs[i]).Replace(fileExt, "");
                    
                    songsSource.Items.Add(new TreeViewModelChild() { PackIconKind = kindChild, Foreground = new SolidColorBrush(color), Title = fileName, ID = i, IsProgressVisible = Visibility.Hidden, CurrentTask = CurrentTask });
                }
            }

            if (addChildTitle != null)
            {
                switch (CurrentTask)
                {
                    case NewTask.LOAD_FAVORITES:
                        songsSource.Items.Add(new TreeViewModelChild() { PackIconKind = kindAddChild, Foreground = new SolidColorBrush(color), Title = addChildTitle, IsProgressVisible = Visibility.Hidden, CurrentTask = NewTask.ADD_NEW_FAVORITES });
                        break;
                    case NewTask.LOAD_SONGS:
                        songsSource.Items.Add(new TreeViewModelChild() { PackIconKind = kindAddChild, Foreground = new SolidColorBrush(color), Title = addChildTitle, IsProgressVisible = Visibility.Hidden, CurrentTask = NewTask.ADD_NEW_SONGS });
                        break;
                }
            }

            return songsSource;
        }

        /// <summary>
        /// Select Folder that contains mp3cdg and mp4 from directory to add it to listview and to ProgramData\karaokeNow\songs\filename.bkn
        /// </summary>
        /// <param name="sDir">Folder path</param>
        /// <returns></returns>
        public void DirSearchSongs(string sDir)
        {
            //try
            //{
            //    int count = 1;
            //    var files = new ObservableCollection<TrackInfo>(Directory.EnumerateFiles(sDir, "*.*", SearchOption.AllDirectories)
            //          .Where(s => _extensions.Contains(Path.GetExtension(s))).Select(s =>
            //          {
            //              return trackInfo(s, count++);
            //          }).ToList());

            //    _songs.Add(files);
            //}
            //catch (UnauthorizedAccessException) { }
            //catch (PathTooLongException) { }

            int count = 1;
            DirectoryInfo dir_info = new DirectoryInfo(sDir);
            List<TrackInfoModel> file_list = new List<TrackInfoModel>();

            SearchDirectory(dir_info, file_list, count);
            _songs.Add(new ObservableCollection<TrackInfoModel>(file_list));

            CreateKaraokeNowFiles(Create.NewSongs);

        }

        /// <summary>
        /// Method to add song by clicking Open option on main menu.
        /// </summary>
        public void AddNewSong()
        {
            //TrackInfoModel sender = null;

            //CurrentTask = NewTask.ADD_TO_QUEUE;
            //Worker.DoWork(CurrentTask, sender);
        }

        /// <summary>
        /// Method to add collection of songs from a selected folder.
        /// </summary>
        /// <param name="sender"></param>
        public void AddNewSongs(ITreeViewModelChild sender)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] filePath = new string[] { fbd.SelectedPath };
                string folderName = Path.GetFileName(fbd.SelectedPath);
                var items = _itemSource[_myComputerIndex].Items;
                var songs = _songs.Count;
                var filaname = CheckFileNameExist(Create.NewSongs, folderName);

                items.Insert(0, new TreeViewModelChild() { PackIconKind = PackIconKind.Music, Foreground = new SolidColorBrush(color), Title = filaname, ID = songs, IsProgressVisible = Visibility.Visible, CurrentTask = NewTask.LOAD_SONGS });

                TreeViewDialogVModel.Instance.DialogStatus = "Working on it! Please wait...";
                TreeViewDialogVModel.Instance.AddingStatus = Visibility.Collapsed;
                TreeViewDialogVModel.Instance.LoadingStatus = Visibility.Visible;
                TreeViewDialogVModel.Instance.ShowDialog = true;
                Worker.DoWork(sender.CurrentTask, items[0].ID, fbd.SelectedPath);
            }

            // var items = _itemSource[_myComputerIndex].Items;
            // Worker.DoWork(sender.CurrentTask, items[0].ID, "");
        }

        /// <summary>
        /// Method adding song to song queue 
        /// </summary>
        /// <param name="sender">Contains the information of the selected song</param>
        /// <returns>Returns the total count and total duration of song is song queue</returns>
        public string AddToQueue(TrackInfoModel sender)
        {
            lock (_songsQueue)
            {
                _isAddingToQueue = true;
                AddRemoveFromQueue(sender, true);

                if (_trackInfo != null)
                {
                    _songsQueue.Add(_trackInfo);
                    if (_songsQueue.Count == 1 && CurrentPlayState == PlayState.Stopped)
                    {
                        _totalDuration += _trackInfo.Tags.duration;
                        _songQueueTitle = "Song Queue (Empty)";
                    }
                    else
                    {
                        _totalDuration += _trackInfo.Tags.duration;
                        _songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + TimeSpan.FromSeconds(_totalDuration).ToString(@"d\.hh\:mm\:ss") + "])";
                    }
                }

                //WriteToQueueList();
                CreateKaraokeNowFiles(Create.SongQueueList);
            }

            return _songQueueTitle; //string.Format("{0}", Utils.FixTimespan(_totalDuration, "HHMMSS"));
        }

        /// <summary>
        /// Method inserting song to song queue to be played next 
        /// </summary>
        /// <param name="sender">Contains the information of the selected song</param>
        /// <returns>Returns the total count and total duration of song is song queue</returns>
        public string AddToQueueAsNext(TrackInfoModel sender)
        {
            lock (_songsQueue)
            {
                _isAddingToQueue = true;
                AddRemoveFromQueue(sender, true);

                if (_trackInfo != null)
                {
                    _songsQueue.Insert(0, _trackInfo);

                    _totalDuration += _trackInfo.Tags.duration;
                    _songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + TimeSpan.FromSeconds(_totalDuration).ToString(@"d\.hh\:mm\:ss") + "])";
                }

                // WriteToQueueList();
                CreateKaraokeNowFiles(Create.SongQueueList);
            }

            return _songQueueTitle;
        }

        /// <summary>
        /// Method to remove the selected song from song queue 
        /// </summary>
        /// <param name="sender">Contains the information of the selected song to be removed</param>
        /// <returns></returns>
        public string RemoveFromQueue(TrackInfoModel sender, bool fromPlayNextTrack = false)
        {
            AddRemoveFromQueue(sender);
            _songsQueue.Remove(sender);

            if (_songsQueue.Count <= 0)
            {
                _songQueueTitle = "Song Queue (Empty)";
                _totalDuration = 0.0;
            }
            else
                _songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + TimeSpan.FromSeconds(_totalDuration).ToString(@"d\.hh\:mm\:ss") + "])";

            if (fromPlayNextTrack) //PlayNext Method that does not use background worker, so we are calling a data refresh and update UI.
            {
                _playedSongs.Add(sender);
                (Worker.TreeViewElement.Items[0] as ITreeViewVModel).Title = _songQueueTitle;
                Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    if (CurrentTask == NewTask.LOAD_QUEUE_SONGS)
                        Worker.ListViewElement.ItemsSource = SongsQueue;
                }));
            }

            // WriteToQueueList();
            CreateKaraokeNowFiles(Create.SongQueueList);

            return _songQueueTitle;
        }

        public void RemoveSelectedFavorite(TrackInfoModel trackInfo, ITreeViewModelChild sender)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                sender.CurrentTask = NewTask.REMOVE_SELECTED_FAVORITE;
                _favorites[sender.ID].Remove(trackInfo);
                CreateKaraokeNowFiles(Create.Favorites, sender);
            }));
        }

        public void RemoveSelectedSong(TrackInfoModel trackInfo, ITreeViewModelChild sender)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                sender.CurrentTask = NewTask.REMOVE_SELECTED_SONG;
                _songs[sender.ID].Remove(trackInfo);
                Worker.ListViewElement.Items.Refresh();
                CreateKaraokeNowFiles(Create.NewSongs, sender);
            }));
        }

        /// <summary>
        /// Method to empty the song queue
        /// </summary>
        /// <returns></returns>
        public string EmptyQueueList()
        {
            _songsQueue.Clear();
            _totalDuration = 0.0;
            _songQueueTitle = "Song Queue (Empty)";
            //WriteToQueueList();
            CreateKaraokeNowFiles(Create.SongQueueList);

            return _songQueueTitle;
        }


        public void CreateFavorites(ITreeViewModelChild sender)
        {
            CreateKaraokeNowFiles(Create.Favorites, sender);
        }

        /// <summary>
        /// Remove KaraokeNow files.
        /// </summary>
        /// <param name="create">Remove from Favorites or Songs collections</param>
        /// <param name="sender">The item to remove</param>
        public void RemoveTreeViewItem(Create create, ITreeViewModelChild sender)
        {
            RemoveKaraokeNowFiles(create, sender);
        }

        /// <summary>
        /// Check for file if it exist and tag it if its mp3cdg or video (_isCdgFileType)
        /// </summary>
        /// <param name="mediaFileName">The file to be checked</param>
        public void PreProcessFiles(string mediaFileName)
        {
            //string cdgFileName = "";
            string mp3FileName = "";
            // if (Regex.IsMatch(tbFileName.Text, "\\.zip$"))
            // {
            //     string myTempDir = Path.GetTempPath() + Path.GetRandomFileName();
            //     Directory.CreateDirectory(myTempDir);
            //     mTempDir = myTempDir;
            //     myCDGFileName = Unzip.UnzipMP3GFiles(tbFileName.Text, myTempDir);
            //     goto PairUpFiles;
            // }
            // else 


            if (Regex.IsMatch(mediaFileName, "\\.cdg$", RegexOptions.IgnoreCase))
            {
                //cdgFileName = mediaFileName;
                //_mp3FileName = mediaFileName;
                //mp3FileName = Regex.Replace(mediaFileName, "\\.cdg$", ".mp3", RegexOptions.IgnoreCase);
                _mediaFileName = Regex.Replace(mediaFileName, "\\.cdg$", ".mp3", RegexOptions.IgnoreCase);
                IsCdgFileType = true;
                // _mediaFileName = mp3FileName;
                //goto PairUpCdgMp3;
            }
            else if (Regex.IsMatch(mediaFileName, "\\.mp3$", RegexOptions.IgnoreCase))
            {
                //cdgFileName = mediaFileName;
                _mediaFileName = mediaFileName;
                IsCdgFileType = true;
                // mp3FileName = mediaFileName;
                //goto PairUpCdgMp3;
            }
            else
            {
                _mediaFileName = mediaFileName;
                //mp3FileName = _mediaFileName;
                //if (!_isAddingToQueue)
                IsCdgFileType = false;

               // return;
            }

            //PairUpCdgMp3:           
            //if (File.Exists(_mediaFileName))// mp3FileName))
            //{
            //    //_mediaFileName = mp3FileName;

            //    if (!_isAddingToQueue)
            //        IsCdgFileType = true;
            //    else
            //        IsCdgFileType = false;
            //}
        }

        /// <summary>
        /// Method to check if filename already exist and return a new name.
        /// </summary>
        /// <param name="createFile">Task to run</param>
        /// <param name="filename">The filename for checking</param>
        /// <returns></returns>
        public string CheckFileNameExist(Create createFile, string filename)
        {
            var n = 0;
            var path = string.Empty;
            var name = string.Empty;
            var newName = string.Empty;
            var ext = string.Empty;


            if (createFile == Create.Favorites)
            {
                path = _favoritesPath;
                name = filename;
                ext = ".fav";
            }
            else if (createFile == Create.NewSongs)
            {
                path = _songsPath;
                name = filename;
                ext = ".bkN";
            }

            do
            {
                if (n == 0)
                {
                    newName = name;
                }
                else
                {
                    newName = string.Format("{0}_{1}", name, n);
                }

                n++;
            }
            while (File.Exists(path + newName + ext));


            return newName;
        }


        /// <summary>
        /// Get the cdg file and replace cdg to mp3
        /// </summary>
        /// <param name="fileName">the filename to replace</param>
        /// <returns></returns>
        public string GetExtPatern(string fileName)
        {
            string extPattern = ".cdg$";
            Regex regX = new Regex(extPattern, RegexOptions.IgnoreCase);
            string mediaFileName = regX.Replace(fileName, ".mp3");

            return mediaFileName;
        }


        /// <summary>
        /// Method to add or remove the song in song queue collection if not empty on application start up. "SongQueueList.que"
        /// </summary>
        /// <param name="sender">Contains the information of the song</param>
        /// <param name="isAdding">Check method if adding or removing a song</param>
        private void AddRemoveFromQueue(TrackInfoModel sender, bool isAdding = false)
        {
            if (isAdding) //For adding songs to SongQueue
            {
                var count = 0;
                PreProcessFiles(sender.FilePath);
                _isAddingToQueue = false;

                if (!File.Exists(_mediaFileName))
                {
                    if (CurrentTask == NewTask.LOAD_QUEUE_SONGS)
                    {
                        _trackInfo = null;
                    }
                    else
                    {
                        MessageBox.Show("Cannot find " + Path.GetFileName(_mediaFileName) + " file to play.");
                        _trackInfo = null;
                    }
                    return;
                }

                if (CurrentPlayState == PlayState.Playing)
                {
                    count = _songsQueue.Count + 1;
                }

                if (GetExtPatern(_mediaFileName).EndsWith(".mp3"))
                {
                    _trackInfo = new TrackInfoModel(sender);
                    _trackInfo.ID = count.ToString();

                    if (!IsCdgFileType && CurrentPlayState == PlayState.Stopped)
                        IsCdgFileType = true;
                }
                else
                {
                    _trackInfo = new TrackInfoModel();
                    Vlc.Instance.GetDuration(_mediaFileName);

                    double vlcTimeDuration = GetVlcTimeOrDuration(Convert.ToDouble(Vlc.Instance.GetTimeDuration));

                    if (!IsCdgFileType && CurrentPlayState == PlayState.Stopped)
                        IsCdgFileType = false;

                    _trackInfo.ID = count.ToString();
                    _trackInfo.Type = sender.Type;
                    _trackInfo.Name = sender.Name;
                    _trackInfo.Artist = sender.Artist;
                    _trackInfo.Duration = Utils.FixTimespan(vlcTimeDuration, "HHMMSS");
                    _trackInfo.FilePath = sender.FilePath;
                    _trackInfo.Tags = new TAG_INFO();
                    _trackInfo.Tags.duration = vlcTimeDuration;
                }
            }
            else //Removing song from SongQueue
            {
                _totalDuration -= sender.Tags.duration;
            }
        }

        /// <summary>
        /// Write selected song to SonQueueList.que text file located in ProgramData\karaokeNow directory
        /// </summary>
        private void WriteToQueueList() // not in use
        {
            var songsPath = _songsQueue.Select(s => s.FilePath).ToArray();
            Directory.CreateDirectory(_filePath);
            File.WriteAllLines(_songQueueList, songsPath);
        }

        /// <summary>
        /// Write files to karaokeNow Directory
        /// </summary>
        /// <param name="create">Task to be run</param>
        private void CreateKaraokeNowFiles(Create create, ITreeViewModelChild sender = null)
        {
            var file = Array.Empty<string>();
            string dirPath = string.Empty;
            string filePath = string.Empty;
            string title = string.Empty;
            int itemID = 0;

            switch (create)
            {
                case Create.Favorites:

                    if (sender.CurrentTask != NewTask.ADD_NEW_FAVORITES)
                    {
                        if (sender.CurrentTask == NewTask.REMOVE_SELECTED_FAVORITE) //Delete song from selected favorites
                        {
                            itemID = sender.ID;
                            title = sender.Title + ".fav";
                            file = _favorites[itemID].Select(s => s.FilePath).ToArray();
                        }
                        else //Creates favorites from song colletions
                        {
                            itemID = sender.ID;
                            title = sender.Title + ".fav";
                            file = _songs[itemID].Select(s => s.FilePath).ToArray();
                            _favorites.Add(new ObservableCollection<TrackInfoModel>(_songs[itemID]));
                        }
                    }
                    else // Creates new empty favorites file use for adding song from played song and from songQueue
                    {
                        // 1 = Favorites index in treeview;
                        itemID = _itemSource[_favoritesIndex].Items[0].ID;
                        title = _itemSource[_favoritesIndex].Items[0].Title + ".fav";
                        _favorites.Add(new ObservableCollection<TrackInfoModel>());
                    }

                    Directory.CreateDirectory(_favoritesPath);
                    File.WriteAllLines(_favoritesPath + title, file);
                    break;
                case Create.FromPlayedSongs:
                    file = _playedSongs.Select(s => s.FilePath).ToArray();

                    _favorites[sender.ID] = new ObservableCollection<TrackInfoModel>(_playedSongs);

                    Directory.CreateDirectory(_filePath);
                    File.WriteAllLines(_favoritesPath + sender.Title + ".fav", file);
                    break;
                case Create.NewSongs:
                    // 2 = My Computer index in treeview;
                    if (sender != null)
                    {
                        if (sender.CurrentTask == NewTask.REMOVE_SELECTED_SONG)
                        {
                            itemID = sender.ID;
                            title = sender.Title + ".bkN";
                        }

                        sender.CurrentTask = NewTask.LOAD_SONGS;
                    }
                    else
                    {
                        itemID = _itemSource[_myComputerIndex].Items[0].ID;
                        title = _itemSource[_myComputerIndex].Items[0].Title + ".bkN";
                    }

                    file = _songs[itemID].Select(s => s.FilePath).ToArray();
                    Directory.CreateDirectory(_songsPath);
                    File.WriteAllLines(_songsPath + title, file);
                    break;
                case Create.SongQueueList:
                    file = _songsQueue.Select(s => s.FilePath).ToArray();
                    Directory.CreateDirectory(_filePath);
                    File.WriteAllLines(_songQueueList, file);
                    break;
            }
        }

        private void RemoveKaraokeNowFiles(Create create, ITreeViewModelChild sender)
        {
            string file = string.Empty;

            switch (create)
            {
                case Create.Favorites:
                    file = _favoritesPath + sender.Title + ".fav";
                    if (File.Exists(file))
                        File.Delete(file);
                    break;
                case Create.NewSongs:
                    file = _songsPath + sender.Title + ".bkN";
                    if (File.Exists(file))
                        File.Delete(file);
                    break;
            }
        }

        /// <summary>
        /// Select Folder that contains mp3cdg and video from directory to add it to listview and to ProgramData\karaokeNow\songs\filename.bkn
        /// </summary>
        /// <param name="dir_info">Contains the infomation of the selected directory</param>
        /// <param name="file_list">Add found files to list</param>
        /// <param name="count">Totla number of files in directory</param>
        private void SearchDirectory(DirectoryInfo dir_info, List<TrackInfoModel> file_list, int count)
        {
            try
            {
                foreach (DirectoryInfo subdir_info in dir_info.GetDirectories())
                {
                    SearchDirectory(subdir_info, file_list, count);
                }
            }
            catch
            {
            }
            try
            {
                var data = dir_info.GetFiles()
                     .Where(w => _extensions.Contains(Path.GetExtension(w.FullName)))
                     .Select(s =>
                     {
                         return trackInfo(s.FullName, count++);
                     }).Where(r => r != null ).ToList();

                if (data.Count > 0)
                    file_list.AddRange(data);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Select added file from treeview and load content to litsview. (e.g Song Queue, Favorites, My computer )
        /// </summary>
        /// <param name="sDir">The files from List<string></param>
        /// <returns>Returns the list of song available</returns>
        private List<ObservableCollection<TrackInfoModel>> TextSearchSongs(List<string> sDir)
        {
            int count = CurrentTask == NewTask.LOAD_QUEUE_SONGS ? 0 : 1;

            var AllSongs = new List<ObservableCollection<TrackInfoModel>>();

            for (int i = 0; i < sDir.Count; i++)
            {
                AllSongs.Add(new ObservableCollection<TrackInfoModel>(File.ReadAllLines(sDir[i])
                    .Where(w => _extensions.Contains(Path.GetExtension(w)))
                    .Select(s =>
                    {
                        return trackInfo(s, count++);
                    }).ToList()));

                count = 1;
            }

            return AllSongs;
        }

  
        /// <summary>
        /// The file extensions supported by the player
        /// </summary>
        /// <param name="file_extensions">Supported extensions</param>
        /// <returns>Returns the supported file extensions</returns>
        private static string HashSetExtensionsToString(HashSet<string> file_extensions)
        {

            string fileExt = "";

            foreach (string s in file_extensions)
            {
                fileExt += s.Replace(s, s + "|");
            }

            return fileExt;
        }


        /// <summary>
        /// Get the song duration being played using vlc (e.g mp4 files)
        /// </summary>
        /// <param name="timeOrDuration">Time duration of the file</param>
        /// <returns>Returns the duration of video</returns>
        private double GetVlcTimeOrDuration(double timeOrDuration)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(timeOrDuration);
            return t.TotalSeconds;
        }

        /// <summary>
        /// Method to split and get the title and artist of songs
        /// </summary>
        /// <param name="fileName">The filename of the song</param>
        /// <param name="count">Song count used as song id</param>
        /// <returns>Returns the information of the song</returns>
        public TrackInfoModel trackInfo(string fileName, int count, int duration = 0)
        {
            string SongTitle = "";
            string SongArtist = "";
            string pattern = @"-\s+|–\s+"; //@"-\s+|–\s+|-|–";
            string fName = Path.GetFileName(fileName);
            string extensionType = Path.GetExtension(fName);
            string[] regXpattern = Regex.Split(fName, pattern);

            Regex regX = new Regex(_extPattern, RegexOptions.IgnoreCase);

            switch (regXpattern.Length)
            {
                case 1:
                    SongTitle = regX.Replace(regXpattern[0], "");
                    SongArtist = regX.Replace(regXpattern[0], "");
                    //count++;
                    break;
                case 2:
                    SongTitle = regX.Replace(regXpattern[regXpattern.Length - 1], "");
                    SongArtist = regX.Replace(regXpattern[0], "");
                    //count++;
                    break;
                case 3:
                    SongTitle = regX.Replace(regXpattern[(regXpattern.Length - 1)], "");
                    SongArtist = regX.Replace(regXpattern[(regXpattern.Length - 2)], "");
                    //count++;
                    break;
                case 4:
                    SongTitle = regX.Replace(regXpattern[(regXpattern.Length - 1)], "");
                    SongArtist = regX.Replace(regXpattern[(regXpattern.Length - 2)], "");
                    //count++;
                    break;
                case 5:
                    SongTitle = regX.Replace(regXpattern[(regXpattern.Length - 1)], "");
                    SongArtist = regX.Replace(regXpattern[(regXpattern.Length - 2)], "");
                    //count++;
                    break;
                case 6:
                    SongTitle = regX.Replace(regXpattern[(regXpattern.Length - 1)], "");
                    SongArtist = regX.Replace(regXpattern[(regXpattern.Length - 2)], "");
                    //count++;
                    break;
                case 7:
                    SongTitle = regX.Replace(regXpattern[(regXpattern.Length - 1)], "");
                    SongArtist = regX.Replace(regXpattern[(regXpattern.Length - 2)], "");
                    //count++;
                    break;
                case 8:
                    SongTitle = regX.Replace(regXpattern[(regXpattern.Length - 1)], "");
                    SongArtist = regX.Replace(regXpattern[(regXpattern.Length - 2)], "");
                    //count++;
                    break;
            }

            //if (extensionType.ToLower() == ".mp3")
            //{
            //    fileName = Regex.Replace(fileName, "\\.mp3$", ".cdg", RegexOptions.IgnoreCase);
            //    if (File.Exists(fileName))
            //    {
            //        return null;
            //    }
            //}
            //else if (extensionType.ToLower() == ".cdg")
            //{
            //    fileName = Regex.Replace(fileName, "\\.cdg$", ".mp3", RegexOptions.IgnoreCase);
            //    if (!File.Exists(fileName))
            //    {
            //        return null;
            //    }
            //}


            TrackInfoModel trackInfo = new TrackInfoModel();
            trackInfo.ID = Convert.ToString(count);
            trackInfo.Type = extensionType.ToUpper().Remove(0,1);
            trackInfo.Name = SongTitle;
            trackInfo.Artist = SongArtist;
            trackInfo.Duration = Convert.ToString(duration);
            trackInfo.FilePath = fileName;

            return trackInfo;
        }

    }
}
