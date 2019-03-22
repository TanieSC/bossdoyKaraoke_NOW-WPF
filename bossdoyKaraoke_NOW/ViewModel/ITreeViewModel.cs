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
        PackIconKind PackIconKind { get; set; }
        SolidColorBrush Foreground { get; set; }
        string Title { get; set; }
        NewTask CurrentTask { get; set; }
        ICommand Loaded { get; }
        ICommand ContextMenuLoaded { get; }
        ICommand SelectionChangedCommand { get; }
        ICommand CreateFavoritesCommand { get; }
        ICommand AddFavoritesToSongQueueCommand { get; }
        ICommand RemoveTreeViewItemCommand { get; }
    }

    public interface ITreeViewModelChild
    {
        PackIconKind PackIconKind { get; set; }
        SolidColorBrush Foreground { get; set; }
        string Title { get; set; }
        int ID { get; set; }
        Visibility IsProgressVisible { get; set; }
        NewTask CurrentTask { get; set; }
        ICommand SelectionChangedCommand { get; }
    }
}
