﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class MediaControls : IMediaControls, INotifyPropertyChanged
    {
        private static MediaControls _instance;
        private string _songTitle;
        private string _songArtist;
        private string _elapsedTime = "00:00:00";
        private string _remainingTime = "00:00:00";
        private double _progressValue = 0;
        private string _tempo = "0%";
        private string _key = "0";
        private bool _enableTempoKeyPanel;
        private double _keyTempoOpacity = 1;
        private StackPanel _audio_panel;
        private StackPanel _dual_screen_panel;
        private StackPanel _tempo_key_panel;
        private StackPanel _song_info_panel;
        private IMediaControls _controls;
        private ICommand _loaded;
        private ICommand _tempoPlusCommand;
        private ICommand _tempoMinusCommand;
        private ICommand _keyPlusCommand;
        private ICommand _keyMinusCommand;

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
                        _controls = dockPanel.DataContext as IMediaControls;

                        _audio_panel = dockPanel.Children[0] as StackPanel;
                        _dual_screen_panel = dockPanel.Children[1] as StackPanel;
                        _tempo_key_panel = dockPanel.Children[2] as StackPanel;
                        _song_info_panel = dockPanel.Children[3] as StackPanel;

                        _tempo_key_panel.IsEnabled = false;
                        KeyTempoOpacity = 0.25;
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


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}