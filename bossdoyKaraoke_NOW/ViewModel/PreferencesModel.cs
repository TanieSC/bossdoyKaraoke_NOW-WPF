using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using Implementation;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class PreferencesModel : IPreferencesModel, INotifyPropertyChanged
    {
        private Model.Equalizer _equalizer;

        //General Tab
        private float _EQ0 = 0f;
        private float _EQ1 = 0f;
        private float _EQ2 = 0f;
        private float _EQ3 = 0f;
        private float _EQ4 = 0f;
        private float _EQ5 = 0f;
        private float _EQ6 = 0f;
        private float _EQ7 = 0f;
        private float _EQ8 = 0f;
        private float _EQ9 = 0f;
        private Slider _sliderPreAmp;
        private Slider _sliderEq0;
        private Slider _sliderEq1;
        private Slider _sliderEq2;
        private Slider _sliderEq3;
        private Slider _sliderEq4;
        private Slider _sliderEq5;
        private Slider _sliderEq6;
        private Slider _sliderEq7;
        private Slider _sliderEq8;
        private Slider _sliderEq9;
        private bool _eqEnabled = false;
        private Dictionary<int, Preset> _eqPresets;
        //private int _eqSelectedPreset;
        private string _infoText = "";
        private float _preAmp = 0f;
        private ICommand _eqLoadedCommand;
        private ICommand _eqEnabledCommand;
        private ICommand _infoTextCommand;
        private ICommand _preAmpCommand;
        private ICommand _eq0Command;
        private ICommand _eq1Command;
        private ICommand _eq2Command;
        private ICommand _eq3Command;
        private ICommand _eq4Command;
        private ICommand _eq5Command;
        private ICommand _eq6Command;
        private ICommand _eq7Command;
        private ICommand _eq8Command;
        private ICommand _eq9Command;

        public float EQ0
        {
            get
            {
                return _EQ0;
            }

            set
            {
                _EQ0 = value;
                OnPropertyChanged();
            }
        }

        public float EQ1
        {
            get
            {
                return _EQ1;
            }

            set
            {
                _EQ1 = value;
                OnPropertyChanged();
            }
        }

        public float EQ2
        {
            get
            {
                return _EQ2;
            }

            set
            {
                _EQ2 = value;
                OnPropertyChanged();
            }
        }

        public float EQ3
        {
            get
            {
                return _EQ3;
            }

            set
            {
                _EQ3 = value;
                OnPropertyChanged();
            }
        }

        public float EQ4
        {
            get
            {
                return _EQ4;
            }

            set
            {
                _EQ4 = value;
                OnPropertyChanged();
            }
        }

        public float EQ5
        {
            get
            {
                return _EQ5;
            }

            set
            {
                _EQ5 = value;
                OnPropertyChanged();
            }
        }

        public float EQ6
        {
            get
            {
                return _EQ6;
            }

            set
            {
                _EQ6 = value;
                OnPropertyChanged();
            }
        }

        public float EQ7
        {
            get
            {
                return _EQ7;
            }

            set
            {
                _EQ7 = value;
                OnPropertyChanged();
            }
        }

        public float EQ8
        {
            get
            {
                return _EQ8;
            }

            set
            {
                _EQ8 = value;
                OnPropertyChanged();
            }
        }

        public float EQ9
        {
            get
            {
                return _EQ9;
            }

            set
            {
                _EQ9 = value;
                OnPropertyChanged();
            }
        }

        public bool EQEnabled
        {
            get
            {
                return _eqEnabled;
            }

            set
            {
                _eqEnabled = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<int, Preset> EQPresets
        {
            get
            {
                return _eqPresets;
            }

            set
            {
                _eqPresets = value;
                OnPropertyChanged();
            }
        }

        //public int EQSelectedPreset
        //{
        //    get
        //    {
        //        return _eqSelectedPreset;
        //    }

        //    set
        //    {
        //        _eqSelectedPreset = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string IntroText
        {
            get
            {
                return _infoText;
            }

            set
            {
                _infoText = value;
                OnPropertyChanged();
            }
        }

        public float PreAmp
        {
            get
            {
                return _preAmp;
            }

            set
            {
                _preAmp = value;
                OnPropertyChanged();
            }
        }


        public PreferencesModel()
        {
            try
            {
                _equalizer = Model.Equalizer.Instance;
                _eqPresets = _equalizer.EQPresets;
                EQ0 = _equalizer.EQ0;
                EQ1 = _equalizer.EQ1;
                EQ2 = _equalizer.EQ2;
                EQ3 = _equalizer.EQ3;
                EQ4 = _equalizer.EQ4;
                EQ5 = _equalizer.EQ5;
                EQ6 = _equalizer.EQ6;
                EQ7 = _equalizer.EQ7;
                EQ8 = _equalizer.EQ8;
                EQ9 = _equalizer.EQ9;
            }
            catch
            {
            }
        }
        
        public ICommand EQLoadedCommand
        {
            get
            {
                return _eqLoadedCommand ?? (_eqLoadedCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        var eqPanel = ((x as StackPanel).Children[1] as StackPanel).Children[1] as StackPanel;
                       // (eqPanel.Children[0] as Slider).Value = PreAmp;
                       //                                                                                                   (eqPanel.Children[1] as Slider).Value = EQ0 * 10;

                    }
                }));
            }
        }

        public ICommand EQEnabledCommand
        {
            get
            {
                return _eqEnabledCommand ?? (_eqEnabledCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQEnabled = (bool)(x as CheckBox).IsChecked;
                        EQEnabled = _equalizer.EQEnabled;
                    }
                }));
            }
        }

        public ICommand IntroTextCommand
        {
            get
            {
                return _infoTextCommand ?? (_infoTextCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        IntroText = (x as TextBox).Text;
                    }
                }));
            }
        }

        public ICommand PreAmpCommand
        {
            get
            {
                return _preAmpCommand ?? (_preAmpCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.PreAmp = (float)(x as Slider).Value / 10;
                        PreAmp = _equalizer.PreAmp;
                    }
                }));
            }
        }

        public ICommand EQ0Command
        {
            get
            {
                return _eq0Command ?? (_eq0Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ0 = (float)(x as Slider).Value / 10;
                        EQ0 = _equalizer.EQ0;
                    }
                }));
            }
        }

        public ICommand EQ1Command
        {
            get
            {
                return _eq1Command ?? (_eq1Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ1 = (float)(x as Slider).Value / 10;
                        EQ1 = _equalizer.EQ1;
                    }
                }));
            }
        }

        public ICommand EQ2Command
        {
            get
            {
                return _eq2Command ?? (_eq2Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ2 = (float)(x as Slider).Value / 10;
                        EQ2 = _equalizer.EQ2;
                    }
                }));
            }
        }

        public ICommand EQ3Command
        {
            get
            {
                return _eq3Command ?? (_eq3Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ3 = (float)(x as Slider).Value / 10;
                        EQ3 = _equalizer.EQ3;
                    }
                }));
            }
        }

        public ICommand EQ4Command
        {
            get
            {
                return _eq4Command ?? (_eq4Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ4 = (float)(x as Slider).Value / 10;
                        EQ4 = _equalizer.EQ4;
                    }
                }));
            }
        }

        public ICommand EQ5Command
        {
            get
            {
                return _eq5Command ?? (_eq5Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ5 = (float)(x as Slider).Value / 10;
                        EQ5 = _equalizer.EQ5;
                    }
                }));
            }
        }

        public ICommand EQ6Command
        {
            get
            {
                return _eq6Command ?? (_eq6Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ6 = (float)(x as Slider).Value / 10;
                        EQ6 = _equalizer.EQ6;
                    }
                }));
            }
        }

        public ICommand EQ7Command
        {
            get
            {
                return _eq7Command ?? (_eq7Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ7 = (float)(x as Slider).Value / 10;
                        EQ7 = _equalizer.EQ7;
                    }
                }));
            }
        }

        public ICommand EQ8Command
        {
            get
            {
                return _eq8Command ?? (_eq8Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ8 = (float)(x as Slider).Value / 10;
                        EQ8 = _equalizer.EQ8;
                    }
                }));
            }
        }

        public ICommand EQ9Command
        {
            get
            {
                return _eq9Command ?? (_eq9Command = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _equalizer.EQ9 = (float)(x as Slider).Value / 10;
                        EQ9 = _equalizer.EQ9;
                    }
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        /*
         <ComboBox 
   x:Name="myComboBox" 
   DisplayMemberPath="Name"
   SelectedValuePath="id"
   ItemsSource="{Binding myDataTable}"      
   SelectedValue="{Binding theID, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" 
>*/
    }
}
