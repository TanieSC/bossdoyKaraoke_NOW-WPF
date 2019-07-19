using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using MaterialDesignThemes.Wpf;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;
using static bossdoyKaraoke_NOW.Enums.RemoveVocalEnum;

namespace bossdoyKaraoke_NOW.Media
{
    class Player : PlayerBase
    {
        private ISongsSource _songsSource;
        private BassAudio _track = null;
        private BassAudio _currentTrack = null;
        private BassAudio _previousTrack = null;
        private SYNCPROC _mixerStallSync;
        private BASS_BFX_MIX _duplicateChannel;
        private int _fxMix = 0;
        private bool _isBassInitialized;
        private float _volume = 50f;
        private bool _isPlayingBass;
        private bool _isPlayingVlc;
        private long _bassChannelPosition;
        private double _bassChannelInSeconds;
        private long _renderAtPosition;
        private string _getNestSongInfo = string.Empty;
        private double _progressBarMaximum = 2000;
        private DispatcherTimer _vlcVolumeSlideAttribute;
        private int _vlcVolumeCounter;
        private float _plus20Volume = 20f;
        
        public bool IsPlayingBass { get { return _isPlayingBass; } }
        public bool IsPlayingVlc { get { return _isPlayingVlc; } }
        public IntPtr AppMainWindowHandle;
        public Vlc VlcPlayer;
        public CDGFile CDGmp3;
        public string GetNextSongInfo { get { return _getNestSongInfo; } }

        /// <summary>
        /// 
        /// </summary>
        public override float Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                _volume = value;

                if (_isPlayingBass)
                    _currentTrack.Volume = value * 0.01f;

                if (_isPlayingVlc)
                    VlcPlayer.Volume = (value != 0 ? (value + _plus20Volume) : value);

                MediaControlsVModel.Instance.VolumeValue = (int)value;

                if (_volume <= 0)
                {
                    MediaControlsVModel.Instance.IconMuteUnMute = PackIconKind.VolumeMute;
                }
                else
                {
                    MediaControlsVModel.Instance.IconMuteUnMute = PackIconKind.VolumeHigh;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long BassChannelPosition
        {
            get
            {
                _bassChannelPosition = BassMix.BASS_Mixer_ChannelGetPosition(_currentTrack.Channel);
                return _bassChannelPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public long CdgRenderAtPosition
        {
            get
            {
                double bpp = 0;

                if (_currentTrack != null)
                {
                    _bassChannelPosition = BassMix.BASS_Mixer_ChannelGetPosition(_currentTrack.Channel);
                    _bassChannelInSeconds = Bass.BASS_ChannelBytes2Seconds(_currentTrack.Channel, _bassChannelPosition);
                    _renderAtPosition = Convert.ToInt64(_bassChannelInSeconds * 1000);

                    MediaControlsVModel.Instance.ElapsedTime = Utils.FixTimespan(_bassChannelInSeconds, "HHMMSS");
                    MediaControlsVModel.Instance.RemainingTime = Utils.FixTimespan(Bass.BASS_ChannelBytes2Seconds(_currentTrack.Channel, _currentTrack.TrackLength - _bassChannelPosition), "HHMMSS");

                    bpp = _currentTrack.TrackLength / _progressBarMaximum;
                    MediaControlsVModel.Instance.ProgressValue = Math.Round(_bassChannelPosition / bpp);

                }
                else
                    _renderAtPosition = 0;

                return _renderAtPosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void VlcRenderAtPosition()
        {

            double timeElapsed = Convert.ToDouble(VlcPlayer.TimeElapsed);
            double timeRemain = Convert.ToDouble(VlcPlayer.TimeDuration);
            MediaControlsVModel.Instance.ElapsedTime = TimeSpan.FromMilliseconds(timeElapsed).ToString(@"hh\:mm\:ss"); //.Substring(0, 8);
            MediaControlsVModel.Instance.RemainingTime = TimeSpan.FromMilliseconds(timeRemain - timeElapsed).ToString(@"hh\:mm\:ss");//.Substring(0, 8);

            double bpp = 0;
            bpp = (int)Math.Round(VlcPlayer.PlayerPosition * _progressBarMaximum);
            MediaControlsVModel.Instance.ProgressValue = bpp;
        }

        /// <summary>
        /// 
        /// </summary>
        private static Player _instance;
        public static Player Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Player();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Player()
        {
            App.SplashScreen.AddMessage("Loading Default Configuration");
            //Initialized Default Configuration
            AppConfig.Initialize();

            if (!AppConfig.Get<bool>("IsDefaultInit"))
            {
                AppConfig.Set("IsDefaultInit", "true");
                AppConfig.SetFxDefaultSettings("DEFAudioEQBand");
                AppConfig.SetFxDefaultSettings("DEFAudioEQEnabled");
                AppConfig.SetFxDefaultSettings("DEFAudioEQPreset");
                AppConfig.SetFxDefaultSettings("DEFAudioEQPreamp");
            }
            Thread.Sleep(1000);
            App.SplashScreen.AddMessage("Initializing Bass Audio");
            //Initialized Bass Un4seen
            InitBass();
            Thread.Sleep(1000);
            App.SplashScreen.AddMessage("Initializing nVLC Components");
            //Initialized nVLC
            VlcPlayer = Vlc.Instance;
            Thread.Sleep(1000);
            App.SplashScreen.AddMessage("Loading Song Collections");
            //Create Instance of song source                                                                                                                             
            _songsSource = SongsSource.Instance;
            _songsSource.LoadSongCollections();

            //Vocal Channel default value is Balance = ChannelSelected.None)
            Channel = ChannelSelected.Right;

            _vlcVolumeSlideAttribute = new DispatcherTimer();
            _vlcVolumeSlideAttribute.Tick += _vlcVolumeSlideAttribute_Tick;
            _vlcVolumeSlideAttribute.IsEnabled = false;
            _vlcVolumeSlideAttribute.Interval = TimeSpan.FromMilliseconds(100);
        }

        //for test 
        public void LoadFile(string filename)
        {
            if (_songsSource.IsCdgFileType)
                LoadCDGFile(filename);
            else
                LoadVideokeFile(filename);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cdgFileName"></param>
        public void LoadCDGFile(string cdgFileName)
        {
            CDGmp3 = new CDGFile(cdgFileName);
            AddToBassMixer();
            PlayNextTrack();
            _isPlayingBass = true;
            _isPlayingVlc = false;
            MediaControlsVModel.Instance.EnableControl = true;
            MediaControlsVModel.Instance.KeyTempoOpacity = 1f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="videokeFileName"></param>
        public void LoadVideokeFile(string videokeFileName)
        {
            CDGmp3 = null;
            PlayNextTrack();
            _isPlayingVlc = true;
            _isPlayingBass = false;
            MediaControlsVModel.Instance.EnableControl = false;
            MediaControlsVModel.Instance.KeyTempoOpacity = 0.25f;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void KeyMinus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.KeyMinus();
                MediaControlsVModel.Instance.Key = string.Format("{0}", _currentTrack.FXTempo.Key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void KeyPlus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.KeyPlus();
                MediaControlsVModel.Instance.Key = string.Format("{0}", _currentTrack.FXTempo.Key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void TempoMinus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.TempoMinus();
                MediaControlsVModel.Instance.Tempo = string.Format("{0}", _currentTrack.FXTempo.Tempo + "%");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void TempoPlus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.TempoPlus();
                MediaControlsVModel.Instance.Tempo = string.Format("{0}", _currentTrack.FXTempo.Tempo + "%");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Mute()
        {
            if (_isPlayingBass)
            {
                _currentTrack.Mute();
            }

            if (_isPlayingVlc)
            {
                VlcPlayer.Mute();
            }

            MediaControlsVModel.Instance.IconMuteUnMute = PackIconKind.VolumeMute;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UnMute()
        {
            if (_isPlayingBass)
            {
                _currentTrack.UnMute();
            }

            if (_isPlayingVlc)
            {
                VlcPlayer.UnMute();
            }

            MediaControlsVModel.Instance.IconMuteUnMute = PackIconKind.VolumeHigh;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Pause()
        {
            if (_isPlayingBass)
            {
                _currentTrack.Pause();
            }

            if (_isPlayingVlc)
            {
                VlcPlayer.Pause();
            }

            MediaControlsVModel.Instance.IconPlayPause = PackIconKind.Play;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Play()
        {
            if (_isPlayingBass)
            {
                _currentTrack.Play();
            }

            if (_isPlayingVlc)
            {
                VlcPlayer.Play();
            }

            MediaControlsVModel.Instance.IconPlayPause = PackIconKind.Pause;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            if (_isPlayingBass)
            {
                _currentTrack.Stop();
                CDGmp3 = null;
                _track = null;
                _currentTrack = null;
                _previousTrack = null;
                VlcPlayer.PlayBackGroundVideo();
            }

            if (_isPlayingVlc)
            {
                VlcPlayer.Stop();
            }

            _songsSource.IsCdgFileType = false;
            _isPlayingBass = false;
            _isPlayingVlc = false;

            MediaControlsVModel.Instance.ElapsedTime = "00:00:00";
            MediaControlsVModel.Instance.RemainingTime = "00:00:00";
            MediaControlsVModel.Instance.IconPlayPause = PackIconKind.Play;
        }

        /// <summary>
        /// Remove left/right vocal on audio track with seperate vocal track  
        /// </summary>
        public void RemoveVocalLeftRight()
        {
            if (_isPlayingBass)
            {
                switch (Channel)
                {
                    case ChannelSelected.None: // Center no vocal removed
                        Bass.BASS_ChannelRemoveFX(BassAudio.MixerChannel, _fxMix);
                        MediaControlsVModel.Instance.VocalChannel = "BAL";
                        Channel = ChannelSelected.Right;

                        // bt.RemoveVocalLeftOrRight(ChannelSelected.None);
                        break;
                    case ChannelSelected.Right: // Remove Right Vocal
                        Bass.BASS_ChannelRemoveFX(BassAudio.MixerChannel, _fxMix);
                        _duplicateChannel = new BASS_BFX_MIX(BASSFXChan.BASS_BFX_CHAN1, BASSFXChan.BASS_BFX_CHAN1);
                        _fxMix = Bass.BASS_ChannelSetFX(BassAudio.MixerChannel, BASSFXType.BASS_FX_BFX_MIX, 0);
                        Bass.BASS_FXSetParameters(_fxMix, _duplicateChannel);
                        MediaControlsVModel.Instance.VocalChannel = "RGT";
                        Channel = ChannelSelected.Left;

                        // bt.RemoveVocalLeftOrRight(ChannelSelected.Right);
                        break;
                    case ChannelSelected.Left: // Remove Left Vocal 
                        Bass.BASS_ChannelRemoveFX(BassAudio.MixerChannel, _fxMix);
                        _duplicateChannel = new BASS_BFX_MIX(BASSFXChan.BASS_BFX_CHAN2, BASSFXChan.BASS_BFX_CHAN2);
                        _fxMix = Bass.BASS_ChannelSetFX(BassAudio.MixerChannel, BASSFXType.BASS_FX_BFX_MIX, 0);
                        Bass.BASS_FXSetParameters(_fxMix, _duplicateChannel);
                        MediaControlsVModel.Instance.VocalChannel = "LFT";
                        Channel = ChannelSelected.None;

                        // bt.RemoveVocalLeftOrRight(ChannelSelected.Left);
                        break;
                }
            }
        }

        /// <summary>
        /// Get the next tack to play and display on top of the screen window for 30 sec.
        /// </summary>
        public string GetNextTrackInfo()
        {

            string showNextTrack = MediaControlsVModel.Instance.RemainingTime.Trim();

            //Hack just to make sure vlc volume and EQ setting is applied because nvlc will not update it 
            string setVlcVolume = MediaControlsVModel.Instance.ElapsedTime.Trim();

            if (showNextTrack != string.Empty || showNextTrack != "")
            {
                double second = TimeSpan.Parse(showNextTrack).TotalSeconds;

                if (second == 30)
                {
                    lock (_songsSource.SongsQueue)
                    {
                        if (_songsSource.SongQueueCount > 0)
                        {
                            _songsSource.PreProcessFiles(_songsSource.SongsQueue[0].FilePath);
                            string nextSong = _songsSource.SongsQueue[0].Name + "[ " + _songsSource.SongsQueue[0].Artist + " ]";
                            _getNestSongInfo = nextSong;
                        }
                    }
                }
            }

            if (setVlcVolume != string.Empty || setVlcVolume != "")
            {
                double second = TimeSpan.Parse(setVlcVolume).TotalSeconds;

                if (second == 1 && _isPlayingVlc)
                {
                    EqualizerModel.Instance.SetupEQ(-1);
                    VlcPlayer.Volume = Volume != 0 ? (Volume + _plus20Volume) : Volume;
                }
            }

            return _getNestSongInfo;
        }

        /// <summary>
        /// Next button = Play the next track if Song Queue is not empty
        /// </summary>
        public void PlayNext()
        {

            lock (_songsSource.SongsQueue)
            {
                if (_songsSource.SongQueueCount > 0)
                {

                    if (_currentTrack != null)
                        Bass.BASS_ChannelSlideAttribute(_currentTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, 2000);

                    if (_previousTrack != null)
                        Bass.BASS_StreamFree(_previousTrack.Channel);

                    _songsSource.PreProcessFiles(_songsSource.SongsQueue[0].FilePath);

                    VlcVolumeSlideAttribute();

                    //if (_songsSource.IsCdgFileType)
                    //    LoadCDGFile(_songsSource.SongsQueue[0].FilePath);
                    //else
                    //    LoadVideokeFile(_songsSource.SongsQueue[0].FilePath);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void DbLevel()
        {
            double dbLevelL = 0.0;
            double dbLevelR = 0.0;

            RMS(out dbLevelL, out dbLevelR);

            // Raise the level with factor 1.5 so that the VUMeter shows more activity
            dbLevelL += Math.Abs(dbLevelL * 0.5);
            dbLevelR += Math.Abs(dbLevelR * 0.5);

            if ((int)dbLevelL < -25)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFF0F0F0"; //off
            }
            else if ((int)dbLevelL < -20)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FF00EE55";
            }
            else if ((int)dbLevelL < -17)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FF00EE55"; 
            }
            else if ((int)dbLevelL < -15)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FF5AF700"; 
            }
            else if ((int)dbLevelL < -12)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FF5DFF00";
            }
            else if ((int)dbLevelL < -10)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FF82EE00";
            }
            else if ((int)dbLevelL < -8)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFA4F100";
            }
            else if ((int)dbLevelL < -6)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFBAFF00";
            }
            else if ((int)dbLevelL < -3)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFBAFF00";
            }
            else if ((int)dbLevelL < 0)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFD1FF00";
            }
            else if ((int)dbLevelL < 1)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFE3EE00";
            }
            else if ((int)dbLevelL < 2)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFEEE300";
            }
            else if ((int)dbLevelL < 3)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFEEC300";
            }
            else if ((int)dbLevelL < 4)
            {
                MediaControlsVModel.Instance.VUmeterColorL = "#FFEE6100";
            }


            if ((int)dbLevelR < -25)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFF0F0F0"; //off
            }
            else if ((int)dbLevelR < -20)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FF00EE55";
            }
            else if ((int)dbLevelR < -17)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FF00EE55";
            }
            else if ((int)dbLevelR < -15)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FF5AF700";
            }
            else if ((int)dbLevelR < -12)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FF5DFF00";
            }
            else if ((int)dbLevelR < -10)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FF82EE00";
            }
            else if ((int)dbLevelR < -8)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFA4F100";
            }
            else if ((int)dbLevelR < -6)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFBAFF00"; 
            }
            else if ((int)dbLevelR < -3)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFBAFF00"; 
            }
            else if ((int)dbLevelR < 0)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFD1FF00";
            }
            else if ((int)dbLevelR < 1)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFE3EE00";
            }
            else if ((int)dbLevelR < 2)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFEEE300";
            }
            else if ((int)dbLevelR < 3)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFEEC300";
            }
            else if ((int)dbLevelR < 4)
            {
                MediaControlsVModel.Instance.VUmeterColorR = "#FFEE6100";
            }
        }

        //Private Method ===================================================================================================

        /// <summary>
        /// Initialize Bass audio playback
        /// </summary>
        private void InitBass()
        {
            CurrentPlayState = PlayState.Stopped;
            _isBassInitialized = BassAudio.Initialize(AppMainWindowHandle);

            if (_isBassInitialized)
            {
                _mixerStallSync = new SYNCPROC(OnMixerStall);
                Bass.BASS_ChannelSetSync(BassAudio.MixerChannel, BASSSync.BASS_SYNC_STALL, 0L, _mixerStallSync, IntPtr.Zero);

                Bass.BASS_ChannelPlay(BassAudio.MixerChannel, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _vlcVolumeSlideAttribute_Tick(object sender, EventArgs e)
        {
            _vlcVolumeCounter--;

            if (VlcPlayer.Volume > 0)
                VlcPlayer.Volume = _vlcVolumeCounter;

            if (_vlcVolumeCounter <= 0)
            {
                _vlcVolumeSlideAttribute.Stop();

                if (_songsSource.IsCdgFileType)
                    LoadCDGFile(_songsSource.SongsQueue[0].FilePath);
                else
                    LoadVideokeFile(_songsSource.SongsQueue[0].FilePath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void VlcVolumeSlideAttribute()
        {
            int vlcVolume = (int)Volume + (int)_plus20Volume;
            int interval = 2000 / vlcVolume;
            _vlcVolumeCounter = vlcVolume;
            _vlcVolumeSlideAttribute.Interval = TimeSpan.FromMilliseconds(interval);
            _vlcVolumeSlideAttribute.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddToBassMixer()
        {
            lock (_songsSource.SongsQueue)
            {
                _track = new BassAudio();
                _track.Tags = _songsSource.SongsQueue[0].Tags as TAG_INFO;

                _track.TrackSync = new SYNCPROC(OnTrackSync);
                _track.Volume = Volume * 0.01f;
                _track.CreateStream();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PlayNextTrack()
        {
            lock (_songsSource.SongsQueue)
            {
                _getNestSongInfo = "";
                MediaControlsVModel.Instance.VocalChannel = "BAL";
                Channel = ChannelSelected.Right;

                if (_songsSource.SongQueueCount > 0)
                {
                    MediaControlsVModel.Instance.SongTitle = _songsSource.SongsQueue[0].Name;
                    MediaControlsVModel.Instance.SongArtist = _songsSource.SongsQueue[0].Artist;

                    if (_songsSource.IsCdgFileType)
                    {
                        _previousTrack = _currentTrack;
                        _currentTrack = _track as BassAudio;
                        VlcPlayer.PlayBackGroundVideo();

                        EqualizerModel.Instance.SetupEQ(_currentTrack.Channel);

                        _currentTrack.Play();

                    }
                    else
                    {
                        VlcPlayer.PlayVideoke(_songsSource.SongsQueue[0].FilePath, new VlcSync.SYNCPROC(OnVlcSync));
                    }

                    _songsSource.RemoveFromQueue(_songsSource.SongsQueue[0], true);

                }
                else
                {
                    Stop();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnVlcSync()
        {
            if (_songsSource.SongQueueCount <= 0)
            {
                // END SYNC
                Application.Current.Dispatcher.BeginInvoke(new Action(PlayNextTrack));
            }
            else
            {
                // POS SYNC
                Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    // this code runs on the UI thread!
                    if (_songsSource.IsCdgFileType)
                        LoadCDGFile(_songsSource.SongsQueue[0].FilePath);
                    else
                        LoadVideokeFile(_songsSource.SongsQueue[0].FilePath);

                    // and fade out and stop the 'previous' track (for 2 seconds)
                    if (_previousTrack != null)
                        Bass.BASS_ChannelSlideAttribute(_previousTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, 2000);
                }));
            }
        }

        // Bass Operation ###################################

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void OnTrackSync(int handle, int channel, int data, IntPtr user)
        {
            if (_songsSource.SongQueueCount <= 0)
                _currentTrack.NextTrackSync = 0;
            else
                _currentTrack.NextTrackSync = 1;

            user = new IntPtr(_currentTrack.NextTrackSync);

            if (user.ToInt32() == 0)
            {
                // END SYNC
                Application.Current.Dispatcher.BeginInvoke(new Action(PlayNextTrack));
            }
            else
            {
                // POS SYNC
                Application.Current.Dispatcher.BeginInvoke(new Action(delegate
               {
                   // this code runs on the UI thread!
                   if (_songsSource.IsCdgFileType)
                       LoadCDGFile(_songsSource.SongsQueue[0].FilePath);
                   else
                       LoadVideokeFile(_songsSource.SongsQueue[0].FilePath);

                   // and fade out and stop the 'previous' track (for 2 seconds)
                   if (_previousTrack != null)
                       Bass.BASS_ChannelSlideAttribute(_previousTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, 2000);

                   // set vlc volume to 0 when not playing videoke file
                   //VlcPlayer.Volume = 0f;

               }));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="user"></param>
        private void OnMixerStall(int handle, int channel, int data, IntPtr user)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                // this code runs on the UI thread!
                if (data == 0)
                {
                    // mixer stalled
                    if (!_isPlayingBass && !_isPlayingVlc)
                    {
                        _track = null;
                        _currentTrack = null;
                        _previousTrack = null;

                        //Console.WriteLine("mixer stalled");
                    }
                }
            }));
        }


        /// <summary>
        /// Get the dblevel per channel
        /// </summary>
        /// <param name="dbLevelL">Left channel</param>
        /// <param name="dbLevelR">Right channel</param>
        private void RMS(out double dbLevelL, out double dbLevelR)
        {

            int peakL = 0;
            int peakR = 0;
            double dbLeft = 0.0;
            double dbRight = 0.0;

            int level = 0;

            //if (Player.IsAsioInitialized)
            //{
            //    float fpeakL = BassAsio.BASS_ASIO_ChannelGetLevel(false, BassAsioDevice.asioOuputChannel);
            //    float fpeakR = BassAsio.BASS_ASIO_ChannelGetLevel(false, BassAsioDevice.asioOuputChannel + 1);
            //    dbLeft = 20.0 * Math.Log10(fpeakL);
            //    dbRight = 20.0 * Math.Log10(fpeakR);
            //}

            //else if (Player.IsWasapiInitialized)
            //{
            //    level = BassWasapi.BASS_WASAPI_GetLevel();
            //}
            //else
                level = Bass.BASS_ChannelGetLevel(BassAudio.MixerChannel);


           // if (Player.IsBassInitialized || Player.IsWasapiInitialized)
          //  {
                peakL = Un4seen.Bass.Utils.LowWord32(level); // the left level
                peakR = Un4seen.Bass.Utils.HighWord32(level); // the right level

                dbLeft = Un4seen.Bass.Utils.LevelToDB(peakL, 65535);
                dbRight = Un4seen.Bass.Utils.LevelToDB(peakR, 65535);
           // }

            dbLevelL = dbLeft;
            dbLevelR = dbRight;
        }
    }
}
