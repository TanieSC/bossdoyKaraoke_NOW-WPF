using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface ITreeViewModel
    {
        ObservableCollection<ITreeViewModelChild> Items { get; }
        List<ITreeViewModel> ItemSource { get; }
        //StackPanel Title { get; set; }
        PackIconKind PackIconKind { get; set; }
        SolidColorBrush Foreground { get; set; }
        string Title { get; set; }
        //string Name { get; set; }
       // int ID { get; set; }
        NewTask CurrentTask { get; set; }
        ICommand Loaded { get; }
        ICommand ContextMenuLoaded { get; }
        ICommand SelectionChangedCommand { get; }
    }

    public interface ITreeViewModelChild
    {
        // StackPanel Title { get; }
        PackIconKind PackIconKind { get; set; }
        SolidColorBrush Foreground { get; set; }
        string Title { get; set; }
       // string Name { get; set; }
        int ID { get; set; }
        Visibility IsProgressVisible { get; set; }
        NewTask CurrentTask { get; set; }
        ICommand Loaded { get; }
        ICommand SelectionChangedCommand { get; }
       // ListViewModelItems ListItems  { get; }
       // ListViewModelItems ShowSelectedSongs(ITreeViewModelChild sender);
    }
}
