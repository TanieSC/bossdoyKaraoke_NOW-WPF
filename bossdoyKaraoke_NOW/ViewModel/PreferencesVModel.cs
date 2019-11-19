using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using Implementation;
using Un4seen.Bass;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.EqualizerEnum;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class PreferencesVModel : IPreferencesVModel, INotifyPropertyChanged
    {
        private TitleTextModel _setTitleText;
        private DefaultVideoBGModel _defaultVideoBG;
        private AudioOutputDeviceModel _audioOutputDevice;
        private EqualizerModel _equalizer;
        private bool _isPresetLoaded = false;


        //Prefs Tab
        private Window _prefsWindow;
        private Dictionary<int, BASS_DEVICEINFO> _deviceInfos;
        private int _selectedDevice;
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
        private int _eqSelectedPreset;
        private string _titleText = "";
        private string _backGroundVideoPath = "";
        private System.Windows.Forms.Panel _panelPreviewScreen;
        private float _preAmp = 0f;
        private ICommand _selectedDeviceCommand;
        private ICommand _closingCommand;
        private ICommand _eqLoadedCommand;
        private ICommand _eqSelectedPresetCommand;
        private ICommand _eqEnabledCommand;
        private ICommand _titleTextCommand;
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
        private ICommand _selectBGVideoCommand;
        private ICommand _videoPreviewSreenLoadedCommand;
        private ICommand _viewPreviousVideoBGCommand;
        private ICommand _viewNextVideoBGCommand;
        private ICommand _applyVideoCommand;


        public Dictionary<int, BASS_DEVICEINFO> DeviceInfos
        {
            get
            {
                return _deviceInfos;
            }

            set
            {
                _deviceInfos = value;
                OnPropertyChanged();
            }
        }

        public int SelectedDevice
        {
            get
            {
                return _selectedDevice;
            }

            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

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

        public int EQSelectedPreset
        {
            get
            {
                return _eqSelectedPreset;
            }

            set
            {
                _eqSelectedPreset = value;
                OnPropertyChanged();
            }
        }

        public string TitleText
        {
            get
            {
                return _titleText;
            }

            set
            {
                _titleText = value;
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

        public string BackGroundVideoPath
        {
            get
            {
                return _backGroundVideoPath;
            }

            set
            {
                _backGroundVideoPath = value;
                OnPropertyChanged();
            }
        }

        public PreferencesVModel()
        {
            try
            {
                _setTitleText = TitleTextModel.Instance;
                TitleText = _setTitleText.TitleText;

                _defaultVideoBG = DefaultVideoBGModel.Instance;
                _audioOutputDevice = AudioOutputDeviceModel.Instance;
                DeviceInfos = _audioOutputDevice.DeviceInfos;
                SelectedDevice = _audioOutputDevice.SelectedDevice;

                _equalizer = EqualizerModel.Instance;
                EQPresets = _equalizer.EQPresets;
                EQEnabled = _equalizer.EQEnabled;
                EQSelectedPreset = _equalizer.EQSelectedPreset;           
            }
            catch
            {
            }
        }

        public ICommand LoadedCommand
        {
            get
            {
                return _selectedDeviceCommand ?? (_selectedDeviceCommand = new RelayCommand(x =>
                {
                    _prefsWindow = x as Window;
                }));
            }
        }

        public ICommand SelectedDeviceCommand
        {
            get
            {
                return _selectedDeviceCommand ?? (_selectedDeviceCommand = new RelayCommand(x =>
                {
                    var selectedDevice = (x as ComboBox).SelectedIndex;
                    _audioOutputDevice.SelectedDevice = selectedDevice;
                    _audioOutputDevice.SetAudioOutputDevice();
                }));
            }
        }

        public ICommand ClosingCommand
        {
            get
            {
                return _closingCommand ?? (_closingCommand = new RelayCommand(x =>
                {
                    _defaultVideoBG.StopPreviewVideoBG();
                    _setTitleText.UpdateTitleText();
                    Worker.DoWork(NewTask.SAVE_EQ_SETTINGS);
                }));
            }
        }

        //public ICommand SaveSettingsCommand
        //{
        //    get
        //    {
        //        return _closingCommand ?? (_closingCommand = new RelayCommand(x =>
        //        {

        //        }));
        //    }
        //}

        public ICommand EQLoadedCommand
        {
            get
            {
                return _eqLoadedCommand ?? (_eqLoadedCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        var eqPanel = ((x as StackPanel).Children[1] as StackPanel).Children[1] as StackPanel;
                        _sliderPreAmp = (eqPanel.Children[0] as Slider);
                        _sliderEq0 = (eqPanel.Children[1] as Slider);
                        _sliderEq1 = (eqPanel.Children[2] as Slider);
                        _sliderEq2 = (eqPanel.Children[3] as Slider);
                        _sliderEq3 = (eqPanel.Children[4] as Slider);
                        _sliderEq4 = (eqPanel.Children[5] as Slider);
                        _sliderEq5 = (eqPanel.Children[6] as Slider);
                        _sliderEq6 = (eqPanel.Children[7] as Slider);
                        _sliderEq7 = (eqPanel.Children[8] as Slider);
                        _sliderEq8 = (eqPanel.Children[9] as Slider);
                        _sliderEq9 = (eqPanel.Children[10] as Slider);

                        SetUIEQPreset();
                    }
                }));
            }
        }

        public ICommand EQSelectedPresetCommand
        {
            get
            {
                return _eqSelectedPresetCommand ?? (_eqSelectedPresetCommand = new RelayCommand(x =>
                {
                if (x != null)
                {
                    var selectedPreset = (x as ComboBox).SelectedIndex;
                    var preset = ((KeyValuePair<int, Preset>)(x as ComboBox).SelectedItem).Key;

                    _isPresetLoaded = true;

                        if (preset != -1)
                        {
                            if (_equalizer.EQPreset != null) _equalizer.EQPreset.Dispose();

                            _equalizer.EQPreset = new Equalizer(EQPresets[preset]);
                            _equalizer.EQ0 = (float)_equalizer.EQPreset.Bands[0].Amplitude;
                            _equalizer.EQ1 = (float)_equalizer.EQPreset.Bands[1].Amplitude;
                            _equalizer.EQ2 = (float)_equalizer.EQPreset.Bands[2].Amplitude;
                            _equalizer.EQ3 = (float)_equalizer.EQPreset.Bands[3].Amplitude;
                            _equalizer.EQ4 = (float)_equalizer.EQPreset.Bands[4].Amplitude;
                            _equalizer.EQ5 = (float)_equalizer.EQPreset.Bands[5].Amplitude;
                            _equalizer.EQ6 = (float)_equalizer.EQPreset.Bands[6].Amplitude;
                            _equalizer.EQ7 = (float)_equalizer.EQPreset.Bands[7].Amplitude;
                            _equalizer.EQ8 = (float)_equalizer.EQPreset.Bands[8].Amplitude;
                            _equalizer.EQ9 = (float)_equalizer.EQPreset.Bands[9].Amplitude;
                            _equalizer.PreAmp = (float)_equalizer.EQPreset.Preamp;
                            _equalizer.EQSelectedPreset = selectedPreset;

                            SetUIEQPreset();
                            Worker.DoWork(NewTask.LOAD_EQ_PRESET);
                        }
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

                        Worker.DoWork(NewTask.EQ_ENABLED);
                    }
                }));
            }
        }

        public ICommand TitleTextCommand
        {
            get
            {
                return _titleTextCommand ?? (_titleTextCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                       // IntroText = (x as TextBox).Text;
                       _setTitleText.TitleText = (x as TextBox).Text;
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQPreamp);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand0);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand1);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand2);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand3);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand4);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand5);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand6);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand7);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand8);
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

                        if (!_isPresetLoaded)
                            Worker.DoWork(NewTask.UPDATE_EQ_SETTINGS, NewPreset.AudioEQBand9);
                    }
                }));
            }
        }

        public ICommand SelectBGVideoCommand
        {
            get
            {
                return _selectBGVideoCommand ?? (_selectBGVideoCommand = new RelayCommand(x =>
                {
                    var fbd = new System.Windows.Forms.FolderBrowserDialog();
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderName = Path.GetFileName(fbd.SelectedPath);
                        BackGroundVideoPath = fbd.SelectedPath;

                        var isIfExist = _defaultVideoBG.GetVideoBG(BackGroundVideoPath);

                        if (isIfExist)
                        {
                            _defaultVideoBG.SetDefaultVideoBG(_panelPreviewScreen.Handle);
                        }
                    }
                }));
            }
        }

        public ICommand VideoPreviewSreenLoadedCommand
        {
            get
            {
                return _videoPreviewSreenLoadedCommand ?? (_videoPreviewSreenLoadedCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        var formHost = x as WindowsFormsHost;
                        _panelPreviewScreen = formHost.Child as System.Windows.Forms.Panel;

                        BackGroundVideoPath = _defaultVideoBG.VideoPathDir;

                        var isIfExist = _defaultVideoBG.GetVideoBG(BackGroundVideoPath);

                        if (isIfExist)
                        {
                            _defaultVideoBG.SetDefaultVideoBG(_panelPreviewScreen.Handle);
                        }                    
                    }
                }));
            }
        }

        public ICommand ViewNextVideoBGCommand
        {
            get
            {
                return _viewNextVideoBGCommand ?? (_viewNextVideoBGCommand = new RelayCommand(x =>
                {
                    _defaultVideoBG.ViewNextVideoBG();
                }));
            }
        }

        public ICommand ViewPreviousVideoBGCommand
        {
            get
            {
                return _viewPreviousVideoBGCommand ?? (_viewPreviousVideoBGCommand = new RelayCommand(x =>
                {
                    _defaultVideoBG.ViewPreviousVideoBG();
                }));
            }
        }

        public ICommand ApplyVideoCommand
        {
            get
            {
                return _applyVideoCommand ?? (_applyVideoCommand = new RelayCommand(x =>
                {
                    _defaultVideoBG.LoadDefaultVideoBG(BackGroundVideoPath);
                    _prefsWindow.Close();
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void SetUIEQPreset()
        {
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
            PreAmp = _equalizer.PreAmp;

            _sliderPreAmp.Value = PreAmp * 10;
            _sliderEq0.Value = EQ0 * 10;
            _sliderEq1.Value = EQ1 * 10;
            _sliderEq2.Value = EQ2 * 10;
            _sliderEq3.Value = EQ3 * 10;
            _sliderEq4.Value = EQ4 * 10;
            _sliderEq5.Value = EQ5 * 10;
            _sliderEq6.Value = EQ6 * 10;
            _sliderEq7.Value = EQ7 * 10;
            _sliderEq8.Value = EQ8 * 10;
            _sliderEq9.Value = EQ9 * 10;

            _isPresetLoaded = false;
        }
    }
}
