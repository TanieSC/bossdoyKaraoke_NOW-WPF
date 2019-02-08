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
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Model;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.TreeViewRootItem;

namespace bossdoyKaraoke_NOW.Media
{

    class SongsSourceTreeViewChild : ISongsSourceTreeViewChild
    {
        public StackPanel Title { get; set; }
    }

    class SongsSourceTreeView : ISongsSourceTreeView
    {
        private static string _filePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\karaokeNow\";
        private static HashSet<string> _extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cdg", ".mp4", ".flv" };
        private string _extPattern = HashSetExtensionsToString(_extensions);
        private string _favoritesPath = _filePath + @"favorites\";
        private string _songsPath = _filePath + @"songs\";

        private List<List<TrackInfo>> _songs;
        private List<List<TrackInfo>> _favorites;
        private List<TrackInfo> _songsQueue;
        private List<TrackInfo> _filteredSongs;
        private int _selectedSongs;

        private RootNode _rootNode;
        private List<ISongsSourceTreeView> _itemSource;
        private SongsSourceListView _songsSourceListView;
        private static SongsSourceTreeView _instance;

        public List<List<TrackInfo>> Songs { get { return _songs; } }
        public List<List<TrackInfo>> Favorites { get { return _favorites; } }
        public List<TrackInfo> SongsQueue { get { return _songsQueue; } }
        public List<TrackInfo> FilteredSongs
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        public List<ISongsSourceTreeView> ItemSource { get { return _itemSource; } }
        public StackPanel Title { get; set; }
        public ObservableCollection<ISongsSourceTreeViewChild> Items { get; set; }

        public static SongsSourceTreeView Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SongsSourceTreeView();
                }
                return _instance;
            }
        }

        public SongsSourceTreeView()
        {
            Items = new ObservableCollection<ISongsSourceTreeViewChild>();
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

                _itemSource = new List<ISongsSourceTreeView>();
                Directory.CreateDirectory(_favoritesPath);
                Directory.CreateDirectory(_songsPath);
                List<string> songs;

                foreach (var node in Enum.GetValues(typeof(RootNode)))
                {
                    ISongsSourceTreeView collections = null;

                    if (Enum.TryParse(node.ToString().ToUpper(), out _rootNode))
                    {
                        switch (_rootNode)
                        {
                            case RootNode.SONG_QUEUE:
                                songs = new List<string>();
                                songs = Directory.EnumerateFiles(_filePath, "*.que", SearchOption.AllDirectories).ToList();
                                if (songs.Count() > 0)
                                {
                                    _songsQueue = TextSearchSongs(songs)[0];
                                    collections = AddTreeViewItems(collections, PackIconKind.Music, "Song Queue (Empty)", PackIconKind.Null);
                                    _itemSource.Add(collections);

                                }
                                break;
                            case RootNode.MY_FAVORITES:
                                songs = new List<string>();
                                songs = Directory.EnumerateFiles(_favoritesPath, "*.fav", SearchOption.AllDirectories).OrderByDescending(file => new FileInfo(file).CreationTime).ToList();
                                if (songs.Count > 0)
                                {
                                    _favorites = TextSearchSongs(songs);
                                    collections = AddTreeViewItems(collections, PackIconKind.StarOutline, "Favorites", PackIconKind.Star, songs, PackIconKind.Folder, "Add Favorites");
                                    _itemSource.Add(collections);

                                }
                                break;
                            case RootNode.MY_COMPUTER:
                                songs = new List<string>();
                                songs = Directory.EnumerateFiles(_songsPath, "*.bkN", SearchOption.AllDirectories).OrderByDescending(file => new FileInfo(file).CreationTime).ToList();
                                if (songs.Count > 0)
                                {
                                    _songs = TextSearchSongs(songs);
                                    collections = AddTreeViewItems(collections, PackIconKind.Monitor, "My Computer", PackIconKind.Music, songs, PackIconKind.Folder, "Add Folder");
                                    _itemSource.Add(collections);
                                }
                                break;
                        }

                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        public SongsSourceListViewItems LoadSelectedSongs(int selectedSongs)
        {
            SongsSourceListViewItems songsSourceListViewItems = new SongsSourceListViewItems(_songs[selectedSongs]);

            return songsSourceListViewItems;
        }

        //public AsyncVirtualizingCollection<TrackInfo> LoadSelectedSongsAsync(int selectedSongs)
        //{
        //    _selectedSongs = selectedSongs;

        //    _songsSourceListView = new SongsSourceListView(_songs[selectedSongs], _songs[selectedSongs].Count, 0);

        //    return new AsyncVirtualizingCollection<TrackInfo>(_songsSourceListView, 100, 30 * 1000);
        //}

        public void LoadFilteredSongsAsync(FrameworkElement context, string searchString)
        {
             StartTask(context, searchString);
        }

        private readonly Queue<QueueItem<object>> _workerQueue = new Queue<QueueItem<object>>();
        private void StartTask(FrameworkElement context, object songs)
        {
            QueuedBackgroundWorker.QueueWorkItem(
            this._workerQueue,
            songs,
             args =>  // DoWork
             {
                 _filteredSongs = new List<TrackInfo>();

                 var currentTask = args.Argument as string;

                 for (int i = 0; i < _songs.Count; i++)
                 {
                     _filteredSongs.AddRange(_songs[i].Where(s => s.Name.ToLower().Contains(currentTask.ToLower()) || s.Artist.ToLower().Contains(currentTask.ToLower())).ToList());
                 }

                 return new { Message = _filteredSongs, CurrentTask = currentTask };
             },
            args =>  // RunWorkerCompleted
            {
                var msg = args.Result.Message;
                var currentTask = args.Result.CurrentTask;

                if (currentTask != "" || currentTask != string.Empty)
                    context.DataContext = new SongsSourceListViewItems(msg);
                else
                    context.DataContext = new SongsSourceListViewItems(_songs[_selectedSongs]);
            });
        }

        //private readonly Queue<QueueItem<string>> _workerQueue = new Queue<QueueItem<string>>();
        //private void StartTask(FrameworkElement context, string songs)
        //{
        //    QueuedBackgroundWorker.QueueWorkItem(
        //    this._workerQueue,
        //    songs,
        //     args =>  // DoWork
        //     {
        //         _filteredSongs = new List<TrackInfo>();

        //         var currentTask = args.Argument;

        //         for (int i = 0; i < _songs.Count; i++)
        //         {
        //             _filteredSongs.AddRange(_songs[i].Where(s => s.Name.ToLower().Contains(currentTask.ToLower()) || s.Artist.ToLower().Contains(currentTask.ToLower())).ToList());
        //         }

        //         return new { Message = _filteredSongs };
        //     },
        //    args =>  // RunWorkerCompleted
        //    {
        //        var msg = args.Result.Message;

        //        if (songs != "" || songs != string.Empty)
        //            context.DataContext = msg;
        //        else
        //            context.DataContext = _songs[_selectedSongs];
        //    });
        //}

        private ISongsSourceTreeView AddTreeViewItems(ISongsSourceTreeView songsSource, PackIconKind kindParent, string parentTitle, PackIconKind kindChild, List<string> songs = null, PackIconKind kindAddChild = PackIconKind.Null, string addChildTitle = null)
        {
            string fileName;
            string fileExt;

            StackPanel itemParent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 0),
                Name = _rootNode.ToString(),
                Children = { new PackIcon { Kind = kindParent},
                             new TextBlock { Margin = new Thickness(4, 0, 0, 0),
                                 Text = parentTitle }}
            };

            songsSource = new SongsSourceTreeView() { Title = itemParent };

            if (songs != null)
            {

                for (int i = 0; i < songs.Count; i++)
                {
                    fileExt = System.IO.Path.GetExtension(songs[i]);
                    fileName = System.IO.Path.GetFileName(songs[i]).Replace(fileExt, "");

                    StackPanel itemChild = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(0, 0, 0, 0),
                        Name = "song_" + i,
                        Children = { new PackIcon { Kind = kindChild},
                             new TextBlock { Margin = new Thickness(4, 0, 0, 0),
                                 Text = fileName }}
                    };

                    songsSource.Items.Add(new SongsSourceTreeViewChild() { Title = itemChild });
                }

                if (addChildTitle != null)
                {
                    StackPanel itemAddChild = new StackPanel
                    {

                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(0, 0, 0, 0),
                        Name = addChildTitle.Replace(" ", "_"),
                        Children = { new PackIcon { Kind = kindAddChild},
                             new TextBlock { Margin = new Thickness(4, 0, 0, 0),
                                 Text = addChildTitle }}
                    };

                    songsSource.Items.Add(new SongsSourceTreeViewChild() { Title = itemAddChild });
                }
            }
            return songsSource;
        }

        /// <summary>
        /// Select added file from treeview and load content to litsview. (e.g Song Queue, Favorites, My computer )
        /// </summary>
        /// <param name="sDir">selected file from treeview</param>
        /// <returns>Returns the list of song available</returns>
        private List<List<TrackInfo>> TextSearchSongs(List<string> sDir)
        {
            int count = 0;

            List<List<TrackInfo>> AllSongs = new List<List<TrackInfo>>();
            try
            {
                AllSongs.AddRange(sDir.Select(s => File.ReadAllLines(s).Where(w => _extensions.Contains(System.IO.Path.GetExtension(w)))
                .Select(ss =>
                {
                    return trackInfo(ss, count++);

                }).ToList()));

            }
            catch (Exception ex)
            {
                // Logger.LogFile(ex.Message, "", "TextSearchSongs", ex.LineNumber(), this.m_thisControl.Name);

            }

            return AllSongs;
        }


        private ObservableCollection<TrackInfo> ListToObservableCollection(List<TrackInfo> songs)
        {
            ObservableCollection<TrackInfo> myCollection = new ObservableCollection<TrackInfo>(songs);

            return myCollection;
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
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private TrackInfo trackInfo(string file, int count, int duration = 0)
        {
            string SongTitle = "";
            string SongArtist = "";
            string pattern = @"-\s+|–\s+|-|–";

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
