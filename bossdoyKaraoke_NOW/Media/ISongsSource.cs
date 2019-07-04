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
        List<ITreeViewVModel> ItemSource { get; }
        List<ObservableCollection<TrackInfoModel>> Songs { get; }
        List<ObservableCollection<TrackInfoModel>> Favorites { get; }
        ObservableCollection<TrackInfoModel> SongsQueue { get; set; }
        int SongQueueCount { get; }
        int PlayedSongsCount { get; }
        bool IsCdgFileType{ get; set; }
        void LoadSongCollections();
        void PlayFirstSongInQueue();
        void LoadSongsInQueue(int songQueuePreviousCount = 0);
        void AddFavoritesToSongQueue(int senderId);
        void CreateFavoritesPlayedSongs(ITreeViewModelChild sender);
        //void CreateFavoritesSongQueue();
        void DirSearchSongs(string sDir);
        void AddNewSongs(ITreeViewModelChild sender);
        string AddToQueue(TrackInfoModel sender);
        string AddToQueueAsNext(TrackInfoModel sender);
        string RemoveFromQueue(TrackInfoModel sender, bool fromPlayNextTrack = false);
        void RemoveSelectedFavorite(TrackInfoModel trackInfo, ITreeViewModelChild sender);
        void RemoveSelectedSong(TrackInfoModel trackInfo, ITreeViewModelChild sender);
        string EmptyQueueList();
        void CreateFavorites(ITreeViewModelChild sender);
        void RemoveTreeViewItem(Create create, ITreeViewModelChild sender);
        void PreProcessFiles(string mediaFileName);
        string CheckFilenameExist(Create createFile, string filename);
    }

}
