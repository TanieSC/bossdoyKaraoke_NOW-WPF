﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;

namespace bossdoyKaraoke_NOW.Media
{
    public interface ISongsSource
    {
        List<ITreeViewModel> ItemSource { get; }
        List<ObservableCollection<TrackInfo>> Songs { get; }
        List<ObservableCollection<TrackInfo>> Favorites { get; }
        ObservableCollection<TrackInfo> SongsQueue { get; set; }
       // CDGFile CDGMp3 { get; set; }
        bool IsCdgFileType{ get; set; }
        void LoadSongCollections();
        void DirSearchSongs(string sDir);
        string AddToQueue(TrackInfo sender);
        string AddToQueueAsNext(TrackInfo sender);
        string RemoveFromQueue(TrackInfo sender);
        void EmptyQueueList();
        void PreProcessFiles(string mediaFileName);
    }

}