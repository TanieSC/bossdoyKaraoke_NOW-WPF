using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface ITreeViewDialogModel
    {
        bool ShowDialog { get; set; }
        string DialogStatus { get; set; }
        bool AcceptEnabled { get; }
        ITreeViewModelChild AddFavoritesSender { get; set; }
        Visibility AddingStatus { get; set; }
        Visibility LoadingStatus { get; set; }
        ICommand AddFavoritesTitleCommand { get; }
        ICommand AcceptCommand { get; }
    }
}
