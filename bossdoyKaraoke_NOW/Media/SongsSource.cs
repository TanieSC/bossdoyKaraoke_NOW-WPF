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
        private string _mediaFileName;
        private string _mp3FileName;
        private string _songQueueTitle = string.Empty;
        private double _totalDuration = 0.0;

        private bool _isCdgFileType;
        private bool _isAddingToQueue;

        private RootNode _rootNode;
        private List<ITreeViewModel> _itemSource;
        private static SongsSource _instance;
        private List<ObservableCollection<TrackInfo>> _songs = new List<ObservableCollection<Model.TrackInfo>>();
        private List<ObservableCollection<TrackInfo>> _favorites;
        private List<TrackInfo> _songsQueue;
        private CDGFile _cdgMp3;
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
       // public CDGFile CDGMp3 { get { return _cdgMp3; } set { _cdgMp3 = value; } }
        public bool IsCdgFileType { get { return _isCdgFileType; } set { _isCdgFileType = value; } }

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

                                    if (_songsQueue.Count > 0)
                                    {
                                        for (int i = 0; i < _songsQueue.Count; i++)
                                        {
                                            if (i > 0)
                                            {
                                                AddRemoveFromQueue(_songsQueue[i], true);
                                                _songsQueue[i].Duration = _trackInfo.Duration;
                                            }
                                        }

                                        //_isAddingToQueue = false;
                                        //PreProcessFiles(_songsQueue[0].FilePath);
                                        AddRemoveFromQueue(_songsQueue[0], true);
                                        _songsQueue[0].Tags = _trackInfo.Tags;
                                    }
                                    else
                                    {
                                        _songQueueTitle = "Song Queue (Empty)";
                                    }

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
            int count = 1;
            var files = new ObservableCollection<TrackInfo>(Directory.EnumerateFiles(sDir, "*.*", SearchOption.AllDirectories)
                  .Where(s => _extensions.Contains(Path.GetExtension(s))).Select(s =>
                  {
                      //TrackInfo ti;
                      //string file = GetExtPatern(s);
                      //if (file.EndsWith(".mp3"))
                      //{
                      //    ti = new TrackInfo(file, count++);
                      //    return ti;

                      //}
                      //else
                          return trackInfo(s, count++);
                  }).ToList());

            _songs.Add(files);
        }

        public string AddToQueue(TrackInfo sender)
        {
            try
            {
                lock (_songsQueue)
                {
                    AddRemoveFromQueue(sender, true);

                    if (_trackInfo != null)
                        _songsQueue.Add(_trackInfo);

                    WriteToQueueList();
                }
            }
            catch (Exception ex)
            {
                
            }

            return _songQueueTitle; //string.Format("{0}", Utils.FixTimespan(_totalDuration, "HHMMSS"));
        }

        public string AddToQueueAsNext(TrackInfo sender)
        {
            try
            {
                lock (_songsQueue)
                {
                     AddRemoveFromQueue(sender, true);

                    if (_trackInfo != null)
                        _songsQueue.Insert(0, _trackInfo);

                    WriteToQueueList();
                }
            }
            catch (Exception ex)
            {

            }

            return string.Format("{0}", Utils.FixTimespan(_totalDuration, "HHMMSS"));
        }

        public string RemoveFromQueue(TrackInfo sender)
        {
            AddRemoveFromQueue(sender);
            _songsQueue.Remove(sender);
           // WriteToQueueList();
            return string.Format("{0}", Utils.FixTimespan(_totalDuration, "HHMMSS"));
        }

        public void EmptyQueueList()
        {
            _songsQueue.Clear();
            WriteToQueueList();
        }

        /// <summary>
        /// Check for .cdg file if it exist, then replace the file extension to .mp3
        /// </summary>
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

                    Console.WriteLine(_isCdgFileType + " false");
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

                    Console.WriteLine(_isCdgFileType + " true");
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void AddRemoveFromQueue(TrackInfo sender, bool isAdding = false)
        {
            if (isAdding) //For adding songs to SongQueue
            {
                _isAddingToQueue = true;
                PreProcessFiles(sender.FilePath);
                _isAddingToQueue = false;

                if (!File.Exists(_mp3FileName))
                {
                    if (CurrentTask == NewTask.LOAD_QUEUE_SONGS)
                    {
                        _trackInfo = null;
                        _songsQueue.Remove(sender);
                    }
                    else
                    {
                        MessageBox.Show("Cannot find " + Path.GetFileName(_mp3FileName) + " file to play.");
                        _trackInfo = null;
                    }
                    return;
                }

                if (CurrentTask != NewTask.LOAD_QUEUE_SONGS)
                {
                    int count = _songsQueue.Count + 1;
                    sender.ID = count.ToString();
                }

                if (GetExtPatern(_mp3FileName).EndsWith(".mp3"))
                {
                    _trackInfo = new TrackInfo(sender);
                    if (CurrentPlayState == PlayState.Playing)
                    {
                        _totalDuration += _trackInfo.Tags.duration;
                        _songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + Utils.FixTimespan(_totalDuration, "HHMMSS") + "])";

                    }

                    if (CurrentTask == NewTask.LOAD_QUEUE_SONGS && CurrentPlayState == PlayState.Stopped)
                    {
                        _totalDuration += _trackInfo.Tags.duration;
                        // _songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + Utils.FixTimespan(_totalDuration, "HHMMSS") + "])";
                    }

                    if (!_isCdgFileType && CurrentPlayState == PlayState.Stopped)
                        _isCdgFileType = true;
                }
                else
                {
                    _trackInfo = new TrackInfo();
                    Vlc.Instance.GetDuration(_mp3FileName);

                    double vlcTimeDuration = GetVlcTimeOrDuration(Convert.ToDouble(Vlc.Instance.GetTimeDuration));

                    if (CurrentPlayState == PlayState.Playing)
                    {
                        _totalDuration += vlcTimeDuration;
                        _songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + Utils.FixTimespan(_totalDuration, "HHMMSS") + "])";

                    }

                    if (CurrentTask == NewTask.LOAD_QUEUE_SONGS && CurrentPlayState == PlayState.Stopped)
                    {
                        _totalDuration += vlcTimeDuration;
                        //_songQueueTitle = "Song Queue (" + _songsQueue.Count + "-[" + Utils.FixTimespan(_totalDuration, "HHMMSS") + "])";
                    }

                    if (!_isCdgFileType && CurrentPlayState == PlayState.Stopped)
                        IsCdgFileType = false;

                    _trackInfo.ID = sender.ID;
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
        private void WriteToQueueList()
        {
            try
            {
                var file = _filePath + "SonQueueList.que";
                var songsPath = _songsQueue.Select(s => s.FilePath).ToArray();
                Directory.CreateDirectory(_filePath);

                File.WriteAllLines(file, songsPath);
            }
            catch (Exception ex)
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
            int count = 1;

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
        /// 
        /// </summary>
        /// <param name="file_extensions"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        private double GetVlcTimeOrDuration(double timeOrDuration)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(timeOrDuration);
            return t.TotalSeconds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private TrackInfo trackInfo(string file, int count, int duration = 0)
        {
            string SongTitle = "";
            string SongArtist = "";
            string pattern = @"-\s+|–\s+"; //@"-\s+|–\s+|-|–";

            try
            {
                // string extPattern = ".cdg|.mp4";
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
