using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Model;

namespace bossdoyKaraoke_NOW.ViewModel
{
    interface ISearchBoxModel
    {
        ObservableCollection<TrackInfo> Items { get; set; }
        ObservableCollection<TrackInfo> FilteredSong(string filter);
        int ItemId { get; set; }
        ICommand SearchSongsCommand { get; }
    }
}
