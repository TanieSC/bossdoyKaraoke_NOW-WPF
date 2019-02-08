using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bossdoyKaraoke_NOW.Model;

namespace bossdoyKaraoke_NOW.Media
{
    public interface ISongsSourceTreeView
    {
        List<List<TrackInfo>> Songs { get; }
        List<List<TrackInfo>> Favorites { get; }
        List<TrackInfo> SongsQueue { get; }
        List<TrackInfo> FilteredSongs { get; set; }
        List<ISongsSourceTreeView> ItemSource { get; }
        StackPanel Title { get; set; }
        ObservableCollection<ISongsSourceTreeViewChild> Items { get; set; }
        void LoadSongCollections();
        SongsSourceListViewItems LoadSelectedSongs(int selectedSongs);
        //AsyncVirtualizingCollection<TrackInfo> LoadSelectedSongsAsync(int selectedSongs);
        void LoadFilteredSongsAsync(FrameworkElement context, string searchString);
    }

    public interface ISongsSourceTreeViewChild
    {
        StackPanel Title { get; set; }
    }
}
