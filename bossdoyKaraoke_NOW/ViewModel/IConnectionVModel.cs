using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IConnectionVModel
    {
        PackIconKind IconClientConnect { get; }
        string ClientConnectIP { get; }
        ICommand LoadedCommand { get; }
        ICommand ClientConnectCommand { get; }
    }
}
