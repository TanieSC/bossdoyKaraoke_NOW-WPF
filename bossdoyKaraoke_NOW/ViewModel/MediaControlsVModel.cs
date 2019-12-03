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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Misc;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class MediaControlsVModel : IMediaControlsVModel, INotifyPropertyChanged
    {
        private static MediaControlsVModel _instance;
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
        private bool _enableControl = false;
        private bool _isMute;
        private double _keyTempoOpacity = 0.25;
        private StackPanel _audio_panel;
        private StackPanel _dual_screen_panel;
        private StackPanel _tempo_key_panel;
        private StackPanel _song_info_panel;
        private Popup _volumeControl;
        private string _vuMeterValue = "99.9";
        private string _vuMeterColorL = "#FFF0F0F0";
        private string _vuMeterColorR = "#FFF0F0F0";
        private bool _playerStatus = false;
        private PackIconKind _iconPlayPause = PackIconKind.Play;
        private PackIconKind _iconMuteUnMute = PackIconKind.VolumeHigh;
        private IMediaControlsVModel _controls;
        private ICommand _loadedCommand;
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

        public static MediaControlsVModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MediaControlsVModel();
                }
                return _instance;
            }
        }

        public MediaControlsVModel()
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
        
        public string VUmeterValue
        {
            get
            {
                return _vuMeterValue;
            }

            set
            {
                _vuMeterValue = value;
                OnPropertyChanged();
            }
        }

        public string VUmeterColorL
        {
            get
            {
                return _vuMeterColorL;
            }

            set
            {
                _vuMeterColorL = value;
                OnPropertyChanged();
            }
        }

        public string VUmeterColorR
        {
            get
            {
                return _vuMeterColorR;
            }

            set
            {
                _vuMeterColorR = value;
                OnPropertyChanged();
            }
        }

        public bool PlayerStatus
        {
            get
            {
                return _playerStatus;
            }

            set
            {
                _playerStatus = value;
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

        public int NewScreenCount
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

        public bool EnableControl
        {
            get
            {
                return _enableControl;
            }

            set
            {
                _enableControl = value;
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

        public IMediaControlsVModel Controls
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

        public ICommand LoadedCommand
        {
            get
            {
                return _loadedCommand ?? (_loadedCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        //var dockPanel = x as DockPanel;
                        //var colorZone = (dockPanel.Children[2] as ColorZone).Content as DockPanel;
                        //  _controls = dockPanel.DataContext as IMediaControls;

                        //  _audio_panel = dockPanel.Children[0] as StackPanel;
                        //  _dual_screen_panel = dockPanel.Children[1] as StackPanel;
                        //  _tempo_key_panel = dockPanel.Children[2] as StackPanel;
                        //  _song_info_panel = dockPanel.Children[3] as StackPanel;

                        // // _tempo_key_panel.IsEnabled = false;
                        //  KeyTempoOpacity = 0.25;
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
                        //Worker.ListViewElement.VerticalAlignment = VerticalAlignment.Bottom;
                        //Worker.ListViewElement.VerticalContentAlignment = VerticalAlignment.Bottom;
                        //Worker.ListViewElement.Width = 250;
                        //Grid.SetColumn(Worker.ListViewElement, 0);                      
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
                        _volumeControl.VerticalAlignment = VerticalAlignment.Bottom;

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
            _volumeControl.IsOpen = false;
            _volumeControl.MouseLeave -= _volumeControl_MouseLeave;
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
                // fullScreen.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(UpdateFullScreenCount));
                UpdateFullScreenCount();
            };
            fullScreen.Closed += (sender2, e2) =>
            {
                //fullScreen.Dispatcher.InvokeShutdown();
                _fullScreen.Remove(fullScreen);
                UpdateFullScreenCount();
                //  fullScreen.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(UpdateFullScreenCount));
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
                NewScreenCount = _fullScreen.Count();
            else
                NewScreenCount = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
