using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Misc;
using bossdoyKaraoke_NOW.ViewModel;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.HotKeyEnum;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;

namespace bossdoyKaraoke_NOW.Model
{
    class MainMenuModel
    {
        private MainWindow _parent;
        private static MainMenuModel _instance;
        private bool _isMute;
        private GlobalHotkeyService _ctlO; // Open File
        private GlobalHotkeyService _ctlA; //Add Songs
        private GlobalHotkeyService _ctlP; //Preferences form
        private GlobalHotkeyService _ctlE; //Exit
        private GlobalHotkeyService _ctlSpceBar; //PlayPause
        private GlobalHotkeyService _ctlN; //Next
        private GlobalHotkeyService _ctlUP; //Volume UP
        private GlobalHotkeyService _ctlDwn; // Volume Down
        private GlobalHotkeyService _ctlM; // Mute/UnMute 
        private GlobalHotkeyService _ctlAltUp; // Key Up
        private GlobalHotkeyService _ctlAltDwn; // Key Down
        private GlobalHotkeyService _ctlShftUp; // Tempo Up
        private GlobalHotkeyService _ctlShftDwn; // Tempo Down

        public GlobalHotkeyService CtlO { get { return _ctlO; } set { _ctlO = value; } } // Open File
        public GlobalHotkeyService CtlA { get { return _ctlA; } set { _ctlA = value; } } //Add Songs
        public GlobalHotkeyService CtlP { get { return _ctlP; } set { _ctlP = value; } } //Preferences form
        public GlobalHotkeyService CtlE { get { return _ctlE; } set { _ctlE = value; } } //Exit
        public GlobalHotkeyService CtlSpceBar { get { return _ctlSpceBar; } set { _ctlSpceBar = value; } } //PlayPause
        public GlobalHotkeyService CtlN { get { return _ctlN; } set { _ctlN = value; } } //Next
        public GlobalHotkeyService CtlUP { get { return _ctlUP; } set { _ctlUP = value; } } //Volume UP
        public GlobalHotkeyService CtlDwn { get { return _ctlDwn; } set { _ctlDwn = value; } } // Volume Down
        public GlobalHotkeyService CtlM { get { return _ctlM; } set { _ctlM = value; } } // Mute/UnMute 
        public GlobalHotkeyService CtlAltUp { get { return _ctlAltUp; } set { _ctlAltUp = value; } } // Key Up
        public GlobalHotkeyService CtlAltDwn { get { return _ctlAltDwn; } set { _ctlAltDwn = value; } } // Key Down
        public GlobalHotkeyService CtlShftUp { get { return _ctlShftUp; } set { _ctlShftUp = value; } } // Tempo Up
        public GlobalHotkeyService CtlShftDwn { get { return _ctlShftDwn; } set { _ctlShftDwn = value; } } // Tempo Down

        public MainWindow ParentWindow { get { return _parent; } set { _parent = value; } }

        private ISongsSource _songsSource = SongsSource.Instance;
        private ITreeViewModelChild _sender = new TreeViewModelChild();



        public static MainMenuModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainMenuModel();
                }
                return _instance;
            }
        }

        public MainMenuModel()
        {
            _ctlO = new GlobalHotkeyService(Key.O, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlA = new GlobalHotkeyService(Key.A, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlP = new GlobalHotkeyService(Key.P, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlE = new GlobalHotkeyService(Key.E, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlSpceBar = new GlobalHotkeyService(Key.Space, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlN = new GlobalHotkeyService(Key.N, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlUP = new GlobalHotkeyService(Key.Up, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlDwn = new GlobalHotkeyService(Key.Down, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlM = new GlobalHotkeyService(Key.M, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlAltUp = new GlobalHotkeyService(Key.Up, KeyModifier.Ctrl | KeyModifier.Alt, OnHotKeyHandler);
            _ctlAltDwn = new GlobalHotkeyService(Key.Down, KeyModifier.Ctrl | KeyModifier.Alt, OnHotKeyHandler);
            _ctlShftUp = new GlobalHotkeyService(Key.Up, KeyModifier.Ctrl | KeyModifier.Shift, OnHotKeyHandler);
            _ctlShftDwn = new GlobalHotkeyService(Key.Down, KeyModifier.Ctrl | KeyModifier.Shift, OnHotKeyHandler);

        }

        public void OnHotKeyHandler(GlobalHotkeyService hotKey)
        {
            switch (hotKey.KeyModifiers)
            {
                case KeyModifier.None:
                    if (hotKey.Key == Key.Space)
                    {
                        if (CurrentPlayState == PlayState.Paused)
                        {
                            Player.Instance.Play();
                        }
                        else
                        {
                            Player.Instance.Pause();
                        }
                    }
                    break;
                case KeyModifier.Ctrl:
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
                        case Key.N:
                            Player.Instance.PlayNext();
                            break;
                        case Key.Up:
                            if (Player.Instance.Volume < 100)
                                Player.Instance.Volume += 1;
                            break;
                        case Key.Down:
                            if (Player.Instance.Volume > 0)
                                Player.Instance.Volume -= 1;
                            break;
                        case Key.M:
                            if (!_isMute)
                            {
                                Player.Instance.Mute();
                                _isMute = true;
                            }
                            else
                            {
                                Player.Instance.UnMute();
                                _isMute = false;
                            }
                            break;
                        case Key.Space:
                            if (CurrentPlayState == PlayState.Paused)
                            {
                                Player.Instance.Play();
                            }
                            else
                            {
                                Player.Instance.Pause();
                            }
                            break;
                    }
                    break;
                case KeyModifier.Ctrl | KeyModifier.Alt:
                    if (hotKey.Key == Key.Up)
                    {
                        Player.Instance.KeyPlus();
                    }
                    else if (hotKey.Key == Key.Down)
                    {
                        Player.Instance.KeyMinus();
                    }
                    break;
                case KeyModifier.Ctrl | KeyModifier.Shift:
                    if (hotKey.Key == Key.Up)
                    {
                        Player.Instance.TempoPlus();
                    }
                    else if (hotKey.Key == Key.Down)
                    {
                        Player.Instance.TempoMinus();
                    }
                    break;
            }
        }

        public void AddSongs()
        {
            _sender.CurrentTask = NewTask.ADD_NEW_SONGS;
            _songsSource.AddNewSongs(_sender);
        }

        public void OpenFile()
        {
            //_songsSource.AddNewSong();
            if (Mouse.OverrideCursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Wait;

            TrackInfoModel sender = null;

            CurrentTask = NewTask.ADD_TO_QUEUE;
            Worker.DoWork(CurrentTask, sender);

            Console.WriteLine("OpenCommand");
        }

        public void ExitApplication()
        {
            _ctlO.Dispose();
            _ctlA.Dispose();
            _ctlP.Dispose();
            _ctlE.Dispose();
            _ctlSpceBar.Dispose();
            _ctlN.Dispose();
            _ctlUP.Dispose();
            _ctlDwn.Dispose();
            _ctlM.Dispose();
            _ctlAltUp.Dispose();
            _ctlAltDwn.Dispose();
            _ctlShftUp.Dispose();
            _ctlShftDwn.Dispose();

            Application.Current.Shutdown();
            // Environment.Exit(0);
        }

        public void PreferencesShow()
        {
            Preferences prefs = new Preferences();
            // _parent = (((((x as MenuItem).Parent as MenuItem).Parent as Menu).Parent as DockPanel).Parent as Grid).Parent as MainWindow;
            prefs.Owner = ParentWindow;
            prefs.Topmost = true;
            prefs.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            prefs.Show();
            prefs.Activate();
        }
    }
}
