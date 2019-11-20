using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Misc;
using bossdoyKaraoke_NOW.Model;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Misc.GlobalHotkeyService;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class MainMenuVModel : IMainMenuVModel
    {
        private MainWindow _parent;
        private GlobalHotkeyService _ctlO;
        private GlobalHotkeyService _ctlA;
        private GlobalHotkeyService _ctlP;
        private GlobalHotkeyService _ctlE;

        // private Preferences prefs = new Preferences();
        private ISongsSource _songsSource = SongsSource.Instance;
        ITreeViewModelChild sender = new TreeViewModelChild();
        private ICommand _loadedCommand;
        private ICommand _addSongsCommand;
        private ICommand _exitApplicationCommand;
        private ICommand _openCommand;
        private ICommand _clientConnectShowCommand;
        private ICommand _preferencesShowCommand;

        public ICommand LoadedCommand
        {
            get
            {
                return _loadedCommand ?? (_loadedCommand = new RelayCommand(x =>
                {
                    _ctlO = new GlobalHotkeyService(Key.O, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlA = new GlobalHotkeyService(Key.A, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlP = new GlobalHotkeyService(Key.P, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlE = new GlobalHotkeyService(Key.E, KeyModifier.Ctrl, OnHotKeyHandler);

                    _parent = x as MainWindow;

                }));
            }
        }

        public ICommand AddSongsCommand
        {
            get
            {
                return _addSongsCommand ?? (_addSongsCommand = new RelayCommand(x =>
                {
                    AddSongs();
                }));
            }
        }

        public ICommand ExitApplicationCommand
        {
            get
            {
                return _exitApplicationCommand ?? (_exitApplicationCommand = new RelayCommand(x =>
                {
                    ExitApplication();
                }));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return _openCommand ?? (_openCommand = new RelayCommand(x =>
                {
                    OpenFile();
                }));
            }
        }

        public ICommand ClientConnectShowCommand
        {
            get
            {
                return _clientConnectShowCommand ?? (_clientConnectShowCommand = new RelayCommand(x =>
                {
                    ClientConnect cc = new ClientConnect();
                   // _parent = (((((x as MenuItem).Parent as MenuItem).Parent as Menu).Parent as DockPanel).Parent as Grid).Parent as MainWindow;
                    cc.Owner = _parent;
                    cc.Topmost = true;
                    cc.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    cc.Show();
                    cc.Activate();
                }));
            }
        }

        public ICommand PreferencesShowCommand
        {
            get
            {
                return _preferencesShowCommand ?? (_preferencesShowCommand = new RelayCommand(x =>
                {
                    PreferencesShow();
                }));
            }
        }

        private void OnHotKeyHandler(GlobalHotkeyService hotKey)
        {
            if (hotKey.KeyModifiers == KeyModifier.Ctrl)
            {
                switch (hotKey.Key)
                {
                    case Key.O:
                        OpenFile();
                        break;
                    case Key.A:
                        AddSongs();
                        break;
                    case Key.P:
                        PreferencesShow();
                        break;
                    case Key.E:
                        ExitApplication();
                        break;
                }
            }
        }

        private void AddSongs()
        {
            sender.CurrentTask = NewTask.ADD_NEW_SONGS;
            _songsSource.AddNewSongs(sender);
        }

        private void OpenFile()
        {
            //_songsSource.AddNewSong();

            TrackInfoModel sender = null;

            CurrentTask = NewTask.ADD_TO_QUEUE;
            Worker.DoWork(CurrentTask, sender);

            Console.WriteLine("OpenCommand");
        }

        private void ExitApplication()
        {
            _ctlO.Dispose();
            _ctlA.Dispose();
            _ctlP.Dispose();
            _ctlE.Dispose();
            System.Windows.Application.Current.Shutdown();
            // Environment.Exit(0);
        }

        private void PreferencesShow()
        {
            Preferences prefs = new Preferences();
            // _parent = (((((x as MenuItem).Parent as MenuItem).Parent as Menu).Parent as DockPanel).Parent as Grid).Parent as MainWindow;
            prefs.Owner = _parent;
            prefs.Topmost = true;
            prefs.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            prefs.Show();
            prefs.Activate();
        }
    }
}
