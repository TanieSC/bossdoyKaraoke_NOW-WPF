using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Model;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IListViewModel
    {
        ObservableCollection<TrackInfo> Items { get; }
        ICommand PreviewMouseDoubleClick { get; }
        ICommand Loaded { get; }
        ICommand ContextMenuLoaded { get; }
        ICommand AddToQueueClick { get; }
        ICommand AddToQueueAsNextClick { get; }
        ICommand RemoveFromQueueClick { get; }
        //void LoadSelectedItem(object sender);
        //void LoadSelectedItem(string senderName);

        // List<TrackInfo> Items { get; set; }
    }

    public interface IListViewModelItems
    {
        ObservableCollection<TrackInfo> Items { get; }
        //ICommand DoubleClickCommand { get; }
        //ICommand LoadInitialSongs { get; }
    }
}
