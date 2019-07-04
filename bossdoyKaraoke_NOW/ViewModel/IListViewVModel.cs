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
    public interface IListViewVModel
    {
        ObservableCollection<TrackInfoModel> Items { get; }
        ICommand AddToQueueDblClkCommand { get; }
        ICommand LoadedCommand { get; }
        ICommand ContextMenuLoadedCommand { get; }
        ICommand AddToQueueCommand { get; }
        ICommand AddToQueueAsNextCommand { get; }
        ICommand RemoveCommand { get; }
    }
}
