using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class MainMenuVModel : IMainMenuVModel
    {
        // private Preferences prefs = new Preferences();
        private ISongsSource _songsSource = SongsSource.Instance;
        ITreeViewModelChild sender = new TreeViewModelChild();
        private ICommand _addSongsCommand;
        private ICommand _exitApplicationCommand;
        private ICommand _openCommand;
        private ICommand _preferencesShowCommand;

        public ICommand AddSongsCommand
        {
            get
            {
                return _addSongsCommand ?? (_addSongsCommand = new RelayCommand(x =>
                {
                    
                    sender.CurrentTask = NewTask.ADD_NEW_SONGS;
                    _songsSource.AddNewSongs(sender);
                }));
            }
        }

        public ICommand ExitApplicationCommand
        {
            get
            {
                return _exitApplicationCommand ?? (_exitApplicationCommand = new RelayCommand(x =>
                {
                    System.Windows.Application.Current.Shutdown();
                   // Environment.Exit(0);
                }));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return _openCommand ?? (_openCommand = new RelayCommand(x =>
                {
                    //_songsSource.AddNewSong();
                   
                    TrackInfoModel sender = null;

                    CurrentTask = NewTask.ADD_TO_QUEUE;
                    Worker.DoWork(CurrentTask, sender);

                }));
            }
        }

        public ICommand PreferencesShowCommand
        {
            get
            {
                return _preferencesShowCommand ?? (_preferencesShowCommand = new RelayCommand(x =>
                {
                    Preferences prefs = new Preferences();
                    prefs.Topmost = true;
                    prefs.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    prefs.Show();
                    prefs.Activate();
                }));
            }
        }
    }
}
