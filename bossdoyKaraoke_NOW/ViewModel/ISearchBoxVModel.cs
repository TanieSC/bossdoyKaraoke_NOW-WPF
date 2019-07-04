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
    interface ISearchBoxVModel
    {
        ObservableCollection<TrackInfoModel> Items { get; set; }
        ObservableCollection<TrackInfoModel> FilteredSong(string filter);
        int ItemId { get; set; }
        ICommand SearchSongsCommand { get; }
    }
}
