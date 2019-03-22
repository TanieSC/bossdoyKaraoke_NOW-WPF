using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using static bossdoyKaraoke_NOW.Enums.KaraokeNowFiles;

namespace bossdoyKaraoke_NOW.Media
{
    public interface ISongsSource
    {
        List<ITreeViewModel> ItemSource { get; }
        List<ObservableCollection<TrackInfo>> Songs { get; }
        List<ObservableCollection<TrackInfo>> Favorites { get; }
        ObservableCollection<TrackInfo> SongsQueue { get; set; }
        int SongQueueCount { get; }
        bool IsCdgFileType{ get; set; }
        void LoadSongCollections();
        void PlayFirstSongInQueue();
        void LoadSongsInQueue(int songQueuePreviousCount = 0);
        void AddFavoritesToSongQueue(int senderId);
        void DirSearchSongs(string sDir);
        string AddToQueue(TrackInfo sender);
        string AddToQueueAsNext(TrackInfo sender);
        string RemoveFromQueue(TrackInfo sender);
        string EmptyQueueList();
        void CreateFavorites(ITreeViewModelChild sender);
        void RemoveTreeViewItem(Create create, ITreeViewModelChild sender);
        void PreProcessFiles(string mediaFileName);
    }

}
