using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using MaterialDesignThemes.Wpf;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;
using static bossdoyKaraoke_NOW.Enums.KaraokeNowFiles;
using static bossdoyKaraoke_NOW.Enums.PlayerState;
using static bossdoyKaraoke_NOW.Enums.TreeViewRootItem;

namespace bossdoyKaraoke_NOW.Media
{
    public class SongsSource : ISongsSource
    {
        private static string _filePath = PlayerBase.FilePath; // Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\karaokeNow\";
        private static HashSet<string> _extensions = PlayerBase.Entensions; //new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cdg", ".mp4", ".flv" };
        private string _extPattern = HashSetExtensionsToString(_extensions);
        private string _favoritesPath = _filePath + @"favorites\";
        private string _songsPath = _filePath + @"songs\";
        private string _songQueueList = _filePath + @"SongQueueList.que";
        private string _mp3FileName;
        private string _songQueueTitle = string.Empty;
        private double _totalDuration = 0.0;

        private bool _isCdgFileType;
        private bool _isAddingToQueue;

        private RootNode _rootNode;
        private List<ITreeViewModel> _itemSource;
        private static SongsSource _instance;
        private List<ObservableCollection<TrackInfo>> _songs = new List<ObservableCollection<TrackInfo>>();
        private List<ObservableCollection<TrackInfo>> _favorites = new List<ObservableCollection<TrackInfo>>();
        private List<TrackInfo> _playedSongs = new List<TrackInfo>();
        private List<TrackInfo> _songsQueue;
        private TrackInfo _trackInfo;

        public List<ITreeViewModel> ItemSource { get { return _itemSource; } }
        public List<ObservableCollection<TrackInfo>> Songs { get { return _songs; } }
        public List<ObservableCollection<TrackInfo>> Favorites { get { return _favorites; } set { _favorites = value; } }
        public ObservableCollection<TrackInfo> SongsQueue
        {          
            get
            {
                return new ObservableCollection<TrackInfo>(_songsQueue);
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
            _itemSource = new List<ITreeViewModel>();
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

            try
            {
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
                Directory.CreateDirectory(_songsPath);
                if (!File.Exists(_songQueueList))
                {
                    using (File.Create(_songQueueList));
                }

                List<string> songs;

                foreach (var node in Enum.GetValues(typeof(RootNode)))
                {
                    ITreeViewModel items = null;

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

                                items = AddTreeViewItems(items, PackIconKind.Monitor, "My Computer", PackIconKind.Music, songs, PackIconKind.Folder, "Add Folder");
                                _itemSource.Add(items);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

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
            var songQueue = Worker.TreeViewElement.Items[0] as ITreeViewModel;

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
                            TreeViewDialogModel.Instance.DialogStatus = _songQueueTitle;
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
        private ITreeViewModel AddTreeViewItems(ITreeViewModel songsSource, PackIconKind kindParent, string parentTitle, PackIconKind kindChild = PackIconKind.Null, List<string> songs = null, PackIconKind kindAddChild = PackIconKind.Null, string addChildTitle = null)
        {
            string fileName;
            string fileExt;
            Color color = (Color)ColorConverter.ConvertFromString("#DD000000");
            songsSource = new TreeViewModel() { PackIconKind = kindParent, Foreground = new SolidColorBrush(color), Title = parentTitle, CurrentTask = CurrentTask };
            
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
            List<TrackInfo> file_list = new List<TrackInfo>();

            SearchDirectory(dir_info, file_list, count);
            _songs.Add(new ObservableCollection<TrackInfo>(file_list));

            CreateKaraokeNowFiles(Create.NewSongs);


        }

        /// <summary>
        /// Method adding song to song queue 
        /// </summary>
        /// <param name="sender">Contains the information of the selected song</param>
        /// <returns>Returns the total count and total duration of song is song queue</returns>
        public string AddToQueue(TrackInfo sender)
        {
            try
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
            }
            catch (Exception ex)
            {

            }

            return _songQueueTitle; //string.Format("{0}", Utils.FixTimespan(_totalDuration, "HHMMSS"));
        }

        /// <summary>
        /// Method inserting song to song queue to played next 
        /// </summary>
        /// <param name="sender">Contains the information of the selected song</param>
        /// <returns>Returns the total count and total duration of song is song queue</returns>
        public string AddToQueueAsNext(TrackInfo sender)
        {
            try
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
            }
            catch (Exception ex)
            {

            }

            return _songQueueTitle;
        }

        /// <summary>
        /// Method to remove the selected song from song queue 
        /// </summary>
        /// <param name="sender">Contains the information of the selected song to be removed</param>
        /// <returns></returns>
        public string RemoveFromQueue(TrackInfo sender, bool fromPlayNextTrack = false)
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
                (Worker.TreeViewElement.Items[0] as ITreeViewModel).Title = _songQueueTitle;
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

        public void RemoveSelectedFavorite(TrackInfo trackInfo, int senderId)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                _favorites[senderId].Remove(trackInfo);
            }));            
        }

        public void RemoveSelectedSong(TrackInfo trackInfo, int senderId)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                _songs[senderId].Remove(trackInfo);
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
            try
            {
                string cdgFileName = "";
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
                    cdgFileName = mediaFileName;
                    _mp3FileName = mediaFileName;
                    goto PairUpCdgMp3;
                }
                else
                {
                    _mp3FileName = mediaFileName;
                    if (!_isAddingToQueue)
                        _isCdgFileType = false;
                }

                PairUpCdgMp3:
                string mp3FileName = Regex.Replace(cdgFileName, "\\.cdg$", ".mp3", RegexOptions.IgnoreCase);
                if (File.Exists(mp3FileName))
                {
                    _mp3FileName = mp3FileName;
                    //_mediaFileName = cdgFileName;
                    //// m_TempDir = "";
                    if (!_isAddingToQueue)
                        _isCdgFileType = true;
                }

            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// Method to check if filename already exist and return a new name.
        /// </summary>
        /// <param name="createFile">Task to run</param>
        /// <param name="filename">The filename for checking</param>
        /// <returns></returns>
        public string CheckFilenameExist(Create createFile, string filename)
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

            //switch (createFile)
            //{
            //    case Create.Favorites:
            //        path = _favoritesPath;
            //        name = filename;
            //        ext = ".fav";
            //        break;
            //    case Create.NewSongs:
            //        path = _songsPath;
            //        name = filename;
            //        ext = ".bkN";
            //        break;
            //}

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
        /// Method to add or remove the song in song queue collection if not empty on application start up. "SongQueueList.que"
        /// </summary>
        /// <param name="sender">Contains the information of the song</param>
        /// <param name="isAdding">Check method if adding or removing a song</param>
        private void AddRemoveFromQueue(TrackInfo sender, bool isAdding = false)
        {
            try
            {
                if (isAdding) //For adding songs to SongQueue
                {
                    var count = 0;
                    PreProcessFiles(sender.FilePath);
                    _isAddingToQueue = false;

                    if (!File.Exists(_mp3FileName))
                    {
                        if (CurrentTask == NewTask.LOAD_QUEUE_SONGS)
                        {
                            _trackInfo = null;
                        }
                        else
                        {
                            MessageBox.Show("Cannot find " + Path.GetFileName(_mp3FileName) + " file to play.");
                            _trackInfo = null;
                        }
                        return;
                    }

                    if (CurrentPlayState == PlayState.Playing)
                    {
                        count = _songsQueue.Count + 1;
                    }

                    if (GetExtPatern(_mp3FileName).EndsWith(".mp3"))
                    {
                        _trackInfo = new TrackInfo(sender);
                        _trackInfo.ID = count.ToString();

                        if (!_isCdgFileType && CurrentPlayState == PlayState.Stopped)
                            _isCdgFileType = true;
                    }
                    else
                    {
                        _trackInfo = new TrackInfo();
                        Vlc.Instance.GetDuration(_mp3FileName);

                        double vlcTimeDuration = GetVlcTimeOrDuration(Convert.ToDouble(Vlc.Instance.GetTimeDuration));

                        if (!_isCdgFileType && CurrentPlayState == PlayState.Stopped)
                            IsCdgFileType = false;

                        _trackInfo.ID = count.ToString();
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
            catch (Exception ex)
            {
                Console.WriteLine("AddRemoveFromQueue");
            }
        }

        /// <summary>
        /// Write selected song to SonQueueList.que text file located in ProgramData\karaokeNow directory
        /// </summary>
        private void WriteToQueueList() // not in use
        {
            try
            {
                var songsPath = _songsQueue.Select(s => s.FilePath).ToArray();
                Directory.CreateDirectory(_filePath);
                File.WriteAllLines(_songQueueList, songsPath);
            }
            catch (Exception ex)
            {
            }
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

            try
            {
                switch (create)
                {
                    case Create.Favorites:

                        if (sender.CurrentTask != NewTask.ADD_NEW_FAVORITES) //Creates favorites from song colletions 
                        {
                            itemID = sender.ID;
                            title = sender.Title + ".fav";
                            file = _songs[itemID].Select(s => s.FilePath).ToArray();
                            _favorites.Add(new ObservableCollection<TrackInfo>(_songs[itemID]));
                        }
                        else // Creates new empty favorites file use for adding song from played song and from songQueue
                        {
                            // 1 = Favorites index in treeview;
                            itemID = _itemSource[1].Items[0].ID;
                            title = _itemSource[1].Items[0].Title + ".fav";
                            _favorites.Add(new ObservableCollection<TrackInfo>());
                        }

                        Directory.CreateDirectory(_favoritesPath);
                        File.WriteAllLines(_favoritesPath + title, file);
                        break;
                    case Create.FromPlayedSongs:
                        file = _playedSongs.Select(s => s.FilePath).ToArray();

                        _favorites[sender.ID] = new ObservableCollection<TrackInfo>(_playedSongs);

                        Directory.CreateDirectory(_filePath);
                        File.WriteAllLines(_favoritesPath + sender.Title + ".fav", file);
                        break;
                    case Create.NewSongs:
                        // 2 = My Computer index in treeview;
                        itemID = _itemSource[2].Items[0].ID;
                        title = _itemSource[2].Items[0].Title + ".bkN";
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
            catch (Exception ex)
            {
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
        private void SearchDirectory(DirectoryInfo dir_info, List<TrackInfo> file_list, int count)
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
                     }).ToList();

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
        private List<ObservableCollection<TrackInfo>> TextSearchSongs(List<string> sDir)
        {
            int count = CurrentTask == NewTask.LOAD_QUEUE_SONGS ? 0 : 1;

            var AllSongs = new List<ObservableCollection<TrackInfo>>();
            try
            {

                for (int i = 0; i < sDir.Count; i++)
                {
                    AllSongs.Add(new ObservableCollection<TrackInfo>(File.ReadAllLines(sDir[i])
                        .Where(w => _extensions.Contains(Path.GetExtension(w)))
                        .Select(s =>
                        {
                            return trackInfo(s, count++);
                        }).ToList()));

                    count = 1;
                }
            }
            catch (Exception ex)
            {
                // Logger.LogFile(ex.Message, "", "TextSearchSongs", ex.LineNumber(), this.m_thisControl.Name);

            }

            return AllSongs;
        }

        /// <summary>
        ///  Set media info and display the Title, Artist and time duration to UI
        /// </summary>
        private void SetMediaInfo(IMediaControls controls)
        {

        }

        /// <summary>
        /// Get the cdg file and replace cdg to mp3
        /// </summary>
        /// <param name="fileName">the filename to replace</param>
        /// <returns></returns>
        private string GetExtPatern(string fileName)
        {
            string extPattern = ".cdg$";
            Regex regX = new Regex(extPattern, RegexOptions.IgnoreCase);
            string mediaFileName = regX.Replace(fileName, ".mp3");

            return mediaFileName;
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
        /// <param name="file">The filename of the song</param>
        /// <param name="count">Song count used as song id</param>
        /// <returns>Returns the information of the song</returns>
        private TrackInfo trackInfo(string file, int count, int duration = 0)
        {
            string SongTitle = "";
            string SongArtist = "";
            string pattern = @"-\s+|–\s+"; //@"-\s+|–\s+|-|–";

            try
            {
                string fName = System.IO.Path.GetFileName(file);
                string[] regXpattern = Regex.Split(fName, pattern);
                // var containsSwears = extensions.Any(w => file.Contains(w));

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

            }
            catch (Exception ex)
            {


            }
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.ID = Convert.ToString(count);
            trackInfo.Name = SongTitle;
            trackInfo.Artist = SongArtist;
            trackInfo.Duration = Convert.ToString(duration);
            trackInfo.FilePath = file;

            return trackInfo;
        }

    }
}
