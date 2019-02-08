using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Model;

namespace bossdoyKaraoke_NOW.Media
{
    public interface ISongsSourceListView
    {
        //string IDNumber { get; }
        //string Name { get; }
        //string Artist { get; }
        //string Duration { get; }
        //string Path { get; }
        //ObservableCollection<TrackInfo> ItemSource { get; }
        //ObservableCollection<ISongsSourceListViewChild> Items { get; set; }
        //void LoadSongCollections(ISongsSourceTreeView songsSource, int defaultSong);

        List<TrackInfo> Items { get; }
        void AddRange(List<TrackInfo> items);
        void Add(TrackInfo item);
    }

    public interface ISongsSourceListViewItems
    {
        List<TrackInfo> Items { get; }
    }
}
