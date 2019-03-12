using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class MediaControls : IMediaControls, INotifyPropertyChanged
    {
        private static MediaControls _instance;
        private List<Window> _fullScreen = new List<Window>();
        private string _songTitle;
        private string _songArtist;
        private int _volumeValue = 50;
        private int _openScreen = 0;
        private string _vocalChannel = "BAL";
        private string _elapsedTime = "00:00:00";
        private string _remainingTime = "00:00:00";
        private double _progressValue = 0;
        private string _tempo = "0%";
        private string _key = "0";
        private bool _enableTempoKeyPanel;
        private bool _isMute;
        private double _keyTempoOpacity = 1;
        private StackPanel _audio_panel;
        private StackPanel _dual_screen_panel;
        private StackPanel _tempo_key_panel;
        private StackPanel _song_info_panel;
        private Popup _volumeControl;
        private PackIconKind _iconPlayPause = PackIconKind.Play;
        private PackIconKind _iconMuteUnMute = PackIconKind.VolumeHigh;
        private IMediaControls _controls;
        private ICommand _loaded;
        private ICommand _vocalChannelCommand;
        private ICommand _addNewScreenCommand;
        private ICommand _tempoPlusCommand;
        private ICommand _tempoMinusCommand;
        private ICommand _keyPlusCommand;
        private ICommand _keyMinusCommand;
        private ICommand _playPuaseCommand;
        private ICommand _playNextCommand;
        private ICommand _muteUnMuteCommand;
        private ICommand _showVolumeControlCommand;
        private ICommand _hideVolumeControlCommand;
        private ICommand _volumeSliderCommand;

        public PackIconKind IconPlayPause
        {
            get
            {
                return _iconPlayPause;
            }
            set
            {
                _iconPlayPause = value;
                OnPropertyChanged();
            }
        }

        public PackIconKind IconMuteUnMute
        {
            get
            {
                return _iconMuteUnMute;
            }
            set
            {
                _iconMuteUnMute = value;
                OnPropertyChanged();
            }
        }

        public static MediaControls Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MediaControls();
                }
                return _instance;
            }
        }

        public MediaControls()
        {
            _instance = this;
        }

        public int VolumeValue
        {
            get
            {
                return _volumeValue;
            }

            set
            {
                _volumeValue = value;
                OnPropertyChanged();
            }
        }
        
        public string VocalChannel
        {
            get
            {
                return _vocalChannel;
            }

            set
            {
                _vocalChannel = value;
                OnPropertyChanged();
            }
        }

        public int AddNewSrceen
        {
            get
            {
                return _openScreen;
            }

            set
            {
                _openScreen = value;
                OnPropertyChanged();
            }
        }

        public string ElapsedTime
        {
            get
            {
                return _elapsedTime;
            }

            set
            {
                _elapsedTime = value;
                OnPropertyChanged();
            }
        }

        public string RemainingTime
        {
            get
            {
                return _remainingTime;
            }

            set
            {
                _remainingTime = value;
                OnPropertyChanged();
            }
        }

        public string SongArtist
        {
            get
            {
                return _songArtist;
            }

            set
            {
                _songArtist = value;
                OnPropertyChanged();
            }
        }

        public string SongTitle
        {
            get
            {
                return _songTitle;
            }

            set
            {
                _songTitle = value;
                OnPropertyChanged();
            }
        }

        public double ProgressValue
        {
            get
            {
                return _progressValue;
            }

            set
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        public string Tempo
        {
            get
            {
                return _tempo;
            }

            set
            {
                _tempo = value;
                OnPropertyChanged();
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public bool EnableTempoKeyPanel
        {
            get
            {
                return _enableTempoKeyPanel;
            }

            set
            {
                _enableTempoKeyPanel = value;
                OnPropertyChanged();
            }
        }

        public double KeyTempoOpacity
        {
            get
            {
                return _keyTempoOpacity;
            }

            set
            {
                _keyTempoOpacity = value;
                OnPropertyChanged();
            }
        }

        public IMediaControls Controls
        {
            get
            {
                return _controls;
            }
            set
            {
                _controls = value;
            }
        }

        public ICommand Loaded
        {
            get
            {
                return _loaded ?? (_loaded = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        var dockPanel = x as DockPanel;
                        var colorZone = (dockPanel.Children[2] as ColorZone).Content as DockPanel;
                        //  _controls = dockPanel.DataContext as IMediaControls;

                        //  _audio_panel = dockPanel.Children[0] as StackPanel;
                        //  _dual_screen_panel = dockPanel.Children[1] as StackPanel;
                        //  _tempo_key_panel = dockPanel.Children[2] as StackPanel;
                        //  _song_info_panel = dockPanel.Children[3] as StackPanel;

                        // // _tempo_key_panel.IsEnabled = false;
                        ////  KeyTempoOpacity = 0.25;
                    }
                }));
            }
        }

        public ICommand VocalChannelCommand
        {
            get
            {
                return _vocalChannelCommand ?? (_vocalChannelCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.RemoveVocalLeftRight();
                    }
                }));
            }
        }

        public ICommand AddNewScreenCommand
        {
            get
            {
                return _addNewScreenCommand ?? (_addNewScreenCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        AddNewScreen();
                    }
                }));
            }
        }

        public ICommand TempoPlusCommand
        {
            get
            {
                return _tempoPlusCommand ?? (_tempoPlusCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.TempoPlus();
                    }
                }));
            }
        }

        public ICommand TempoMinusCommand
        {
            get
            {
                return _tempoMinusCommand ?? (_tempoMinusCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.TempoMinus();
                    }
                }));
            }
        }

        public ICommand KeyPlusCommand
        {
            get
            {
                return _keyPlusCommand ?? (_keyPlusCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.KeyPlus();
                    }
                }));      
            }
        }

        public ICommand KeyMinusCommand
        {
            get
            {
                return _keyMinusCommand ?? (_keyMinusCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.KeyMinus();
                    }
                }));
            }
        }

        public ICommand PlayPauseCommand
        {
            get
            {
                return _playPuaseCommand ?? (_playPuaseCommand = new RelayCommand(x =>
                {
                    if (x != null)
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
                }));
            }
        }

        public ICommand PlayNextCommand
        {
            get
            {
                return _playNextCommand ?? (_playNextCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.PlayNext();
                    }
                }));
            }
        }

        public ICommand MuteUnMuteCommand
        {
            get
            {
                return _muteUnMuteCommand ?? (_muteUnMuteCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
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
                    }
                }));
            }
        }

        public ICommand ShowVolumeControlCommand
        {
            get
            {
                return _showVolumeControlCommand ?? (_showVolumeControlCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _volumeControl = x as Popup;
                        _volumeControl.PopupAnimation = PopupAnimation.Fade;
                        _volumeControl.HorizontalOffset = 10;
                        _volumeControl.VerticalOffset = 5;
                        _volumeControl.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                        if (!_volumeControl.IsOpen)
                        {
                            _volumeControl.IsOpen = true;
                            _volumeControl.MouseLeave += _volumeControl_MouseLeave;
                        }
                    }
                }));
            }
        }

        public ICommand HideVolumeControlCommand
        {
            get
            {
                return _hideVolumeControlCommand ?? (_hideVolumeControlCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        var parent = ((x as Popup).Parent as DockPanel).Parent as Grid;
                        if (parent.IsMouseOver)
                        {
                            _volumeControl.PopupAnimation = PopupAnimation.Fade;
                            _volumeControl.IsOpen = false;
                        }
                    }
                }));
            }
        }

        public ICommand VolumeSliderCommand
        {
            get
            {
                return _volumeSliderCommand ?? (_volumeSliderCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        Player.Instance.Volume = (float)(x as Slider).Value;
                    }
                }));
            }
        }

        private void _volumeControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //var parent = ((((((sender as Popup).Parent as Grid)
            //    .Children[1] as DockPanel)
            //    .Children[1] as ColorZone).Content as DockPanel)
            //    .Children[0] as StackPanel)
            //    .Children[2] as Button;
            //if (!parent.IsMouseOver)
            //{
            _volumeControl.IsOpen = false;
            // }
        }

        private void AddNewScreen()
        {
            //Thread thread = new Thread(() =>
            //{
            FullScreen fullScreen = new FullScreen();

            _fullScreen.Add(fullScreen);

            //    // Create our context, and install it:
            //    SynchronizationContext.SetSynchronizationContext(
            //        new DispatcherSynchronizationContext(
            //            Dispatcher.CurrentDispatcher));

            fullScreen.Loaded += (sender1, e1) =>
            {
                // full_screen_count.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(UpdateFullScreenCount));
                UpdateFullScreenCount();
            };
            fullScreen.Closed += (sender2, e2) =>
            {
                //fullScreen.Dispatcher.InvokeShutdown();
                _fullScreen.Remove(fullScreen);
                UpdateFullScreenCount();
                //  full_screen_count.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(UpdateFullScreenCount));
            };

            fullScreen.Show();
            fullScreen.Activate();

            //    Dispatcher.Run();

            //});

            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
        }

        private void UpdateFullScreenCount()
        {
            if (_fullScreen != null)
                AddNewSrceen = _fullScreen.Count();
            else
                AddNewSrceen = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
