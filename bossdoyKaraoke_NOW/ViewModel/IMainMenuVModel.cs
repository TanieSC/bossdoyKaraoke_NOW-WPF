using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IMainMenuVModel
    {
        ICommand OpenCommand { get; }
        ICommand AddSongsCommand { get; }
        ICommand ClientConnectShowCommand { get; }
        ICommand PreferencesShowCommand { get; }
        ICommand ExitApplicationCommand { get; }
    }
}
