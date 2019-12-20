﻿using System;
using System.Windows;
using System.Windows.Input;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Misc;
using bossdoyKaraoke_NOW.Model;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.HotKeyEnum;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class MainMenuVModel : IMainMenuVModel
    {
        private MainWindow _parent;
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
        private bool _isMute;

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
                    _ctlSpceBar = new GlobalHotkeyService(Key.Space, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlN = new GlobalHotkeyService(Key.N, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlUP = new GlobalHotkeyService(Key.Up, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlDwn = new GlobalHotkeyService(Key.Down, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlM = new GlobalHotkeyService(Key.M, KeyModifier.Ctrl, OnHotKeyHandler);
                    _ctlAltUp = new GlobalHotkeyService(Key.Up, KeyModifier.Ctrl | KeyModifier.Alt, OnHotKeyHandler);
                    _ctlAltDwn = new GlobalHotkeyService(Key.Down, KeyModifier.Ctrl | KeyModifier.Alt, OnHotKeyHandler);
                    _ctlShftUp = new GlobalHotkeyService(Key.Up, KeyModifier.Ctrl | KeyModifier.Shift, OnHotKeyHandler);
                    _ctlShftDwn = new GlobalHotkeyService(Key.Down, KeyModifier.Ctrl | KeyModifier.Shift, OnHotKeyHandler);

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
                    cc.WindowStartupLocation = WindowStartupLocation.CenterOwner;
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

        private void AddSongs()
        {
            sender.CurrentTask = NewTask.ADD_NEW_SONGS;
            _songsSource.AddNewSongs(sender);
        }

        private void OpenFile()
        {
            //_songsSource.AddNewSong();
            if (Mouse.OverrideCursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Wait;

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

        private void PreferencesShow()
        {
            Preferences prefs = new Preferences();
            // _parent = (((((x as MenuItem).Parent as MenuItem).Parent as Menu).Parent as DockPanel).Parent as Grid).Parent as MainWindow;
            prefs.Owner = _parent;
            prefs.Topmost = true;
            prefs.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            prefs.Show();
            prefs.Activate();
        }
    }
}
