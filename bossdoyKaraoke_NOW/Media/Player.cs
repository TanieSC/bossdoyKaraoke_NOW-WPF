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
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;
using static bossdoyKaraoke_NOW.Enums.PlayerState;
using static bossdoyKaraoke_NOW.Enums.RemoveVocal;

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

        public bool IsPlayingBass { get { return _isPlayingBass; } }
        public bool IsPlayingVlc { get { return _isPlayingVlc; } }
        public IntPtr AppMainWindowHandle;
        public Vlc VlcPlayer;
        public CDGFile CDGmp3;
        public string GetNextSongInfo { get { return _getNestSongInfo; } }

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
                    VlcPlayer.Volume = (value != 0 ? (value + 25) : value);

                MediaControls.Instance.VolumeValue = (int)value;

                if (_volume <= 0)
                {
                    MediaControls.Instance.IconMuteUnMute = PackIconKind.VolumeMute;
                }
                else
                {
                    MediaControls.Instance.IconMuteUnMute = PackIconKind.VolumeHigh;
                }
            }
        }

        public long BassChannelPosition
        {
            get
            {
                _bassChannelPosition = BassMix.BASS_Mixer_ChannelGetPosition(_currentTrack.Channel);
                return _bassChannelPosition;
            }
        }

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

                    MediaControls.Instance.ElapsedTime = Utils.FixTimespan(_bassChannelInSeconds, "HHMMSS");
                    MediaControls.Instance.RemainingTime = Utils.FixTimespan(Bass.BASS_ChannelBytes2Seconds(_currentTrack.Channel, _currentTrack.TrackLength - _bassChannelPosition), "HHMMSS");

                    bpp = _currentTrack.TrackLength / _progressBarMaximum;
                    MediaControls.Instance.ProgressValue = Math.Round(_bassChannelPosition / bpp);

                }
                else
                    _renderAtPosition = 0;

                return _renderAtPosition;
            }
        }

        public void VlcRenderAtPosition()
        {

            double timeElapsed = Convert.ToDouble(VlcPlayer.TimeElapsed);
            double timeRemain = Convert.ToDouble(VlcPlayer.TimeDuration);
            MediaControls.Instance.ElapsedTime = TimeSpan.FromMilliseconds(timeElapsed).ToString(@"hh\:mm\:ss"); //.Substring(0, 8);
            MediaControls.Instance.RemainingTime = TimeSpan.FromMilliseconds(timeRemain - timeElapsed).ToString(@"hh\:mm\:ss");//.Substring(0, 8);

            double bpp = 0;
            bpp = (int)Math.Round(VlcPlayer.PlayerPosition * _progressBarMaximum);
            MediaControls.Instance.ProgressValue = bpp;

        }

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

        public Player()
        {
            App.SplashScreen.AddMessage("Initializing Bass Audio");
            //Initialized Bass Un4seen
            InitBass();
            Thread.Sleep(1000);
            App.SplashScreen.AddMessage("Initializing VLC Player");
            //Initialized nVLC
            VlcPlayer = Vlc.Instance;
            Thread.Sleep(1000);
            App.SplashScreen.AddMessage("Loading Song Collections");
            //Create Instance of song source                                                                                                                             
            _songsSource = SongsSource.Instance;
            _songsSource.LoadSongCollections();

            //Vocal Channel default value is Balance = ChannelSelected.None)
            Channel = ChannelSelected.Right;
        }

        public void LoadCDGFile(string cdgFileName)
        {
            CDGmp3 = new CDGFile(cdgFileName);
            AddToBassMixer();
            PlayNextTrack();
            _isPlayingBass = true;
            _isPlayingVlc = false;
            MediaControls.Instance.EnableControl = true;
            MediaControls.Instance.KeyTempoOpacity = 1f;
        }

        public void LoadVideokeFile(string videokeFileName)
        {
            CDGmp3 = null;
            VlcPlayer.Volume = (Volume != 0 ? (Volume + 25) : Volume);
            PlayNextTrack();
            _isPlayingVlc = true;
            _isPlayingBass = false;
            MediaControls.Instance.EnableControl = false;
            MediaControls.Instance.KeyTempoOpacity = 0.25f;
        }

        public override void KeyMinus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.KeyMinus();
                MediaControls.Instance.Key = string.Format("{0}", _currentTrack.FXTempo.Key);
            }
        }

        public override void KeyPlus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.KeyPlus();
                MediaControls.Instance.Key = string.Format("{0}", _currentTrack.FXTempo.Key);
            }
        }

        public override void TempoMinus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.TempoMinus();
                MediaControls.Instance.Tempo = string.Format("{0}", _currentTrack.FXTempo.Tempo + "%");
            }
        }

        public override void TempoPlus()
        {
            if (_isPlayingBass)
            {
                _currentTrack.TempoPlus();
                MediaControls.Instance.Tempo = string.Format("{0}", _currentTrack.FXTempo.Tempo + "%");
            }
        }

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

            MediaControls.Instance.IconMuteUnMute = PackIconKind.VolumeMute;
        }

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

            MediaControls.Instance.IconMuteUnMute = PackIconKind.VolumeHigh;
        }

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

            MediaControls.Instance.IconPlayPause = PackIconKind.Play;
        }

        public override void Play()
        {
            //if (CurrentPlayState == PlayState.Stopped && _songsSource.SongsQueue.Count > 0)
            //{
            //    CurrentTask = NewTask.ADD_TO_QUEUE;
            //    Worker.DoWork(CurrentTask, _songsSource.SongsQueue[0]);
            //}

           // if (CurrentPlayState == PlayState.Paused || CurrentPlayState == PlayState.Playing)
           // {
                if (_isPlayingBass)
                {
                    _currentTrack.Play();
                }

                if (_isPlayingVlc)
                {
                    VlcPlayer.Play();
                }
            //}

            MediaControls.Instance.IconPlayPause = PackIconKind.Pause;
        }

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

            MediaControls.Instance.ElapsedTime = "00:00:00";
            MediaControls.Instance.RemainingTime = "00:00:00";
            MediaControls.Instance.IconPlayPause = PackIconKind.Play;
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
                        MediaControls.Instance.VocalChannel = "BAL";
                        Channel = ChannelSelected.Right;

                        // bt.RemoveVocalLeftOrRight(ChannelSelected.None);
                        break;
                    case ChannelSelected.Right: // Remove Right Vocal
                        Bass.BASS_ChannelRemoveFX(BassAudio.MixerChannel, _fxMix);
                        _duplicateChannel = new BASS_BFX_MIX(BASSFXChan.BASS_BFX_CHAN1, BASSFXChan.BASS_BFX_CHAN1);
                        _fxMix = Bass.BASS_ChannelSetFX(BassAudio.MixerChannel, BASSFXType.BASS_FX_BFX_MIX, 0);
                        Bass.BASS_FXSetParameters(_fxMix, _duplicateChannel);
                        MediaControls.Instance.VocalChannel = "RGT";
                        Channel = ChannelSelected.Left;

                        // bt.RemoveVocalLeftOrRight(ChannelSelected.Right);
                        break;
                    case ChannelSelected.Left: // Remove Left Vocal 
                        Bass.BASS_ChannelRemoveFX(BassAudio.MixerChannel, _fxMix);
                        _duplicateChannel = new BASS_BFX_MIX(BASSFXChan.BASS_BFX_CHAN2, BASSFXChan.BASS_BFX_CHAN2);
                        _fxMix = Bass.BASS_ChannelSetFX(BassAudio.MixerChannel, BASSFXType.BASS_FX_BFX_MIX, 0);
                        Bass.BASS_FXSetParameters(_fxMix, _duplicateChannel);
                        MediaControls.Instance.VocalChannel = "LFT";
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
            try
            {
                string showNextTrack = MediaControls.Instance.RemainingTime.Trim();

                if (showNextTrack != string.Empty || showNextTrack != "")
                {
                    // int minute = Convert.ToInt32(showNextTrack.Substring(3, 2));
                    // int second = Convert.ToInt32(showNextTrack.Substring(6, 2));
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetNextTrackInfo");
            }

            return _getNestSongInfo;
        }

        /// <summary>
        /// Next button = Play the next track if Song Queue is not empty
        /// </summary>
        public void PlayNext()
        {
            try
            {
                lock (_songsSource.SongsQueue)
                {
                    if (_songsSource.SongQueueCount > 0)
                    {
                        if (_currentTrack != null)
                            Bass.BASS_ChannelSlideAttribute(_currentTrack.Channel, BASSAttribute.BASS_ATTRIB_VOL, -1f, 2000);

                        _songsSource.PreProcessFiles(_songsSource.SongsQueue[0].FilePath);

                        if (_songsSource.IsCdgFileType)
                            LoadCDGFile(_songsSource.SongsQueue[0].FilePath);
                        else
                            LoadVideokeFile(_songsSource.SongsQueue[0].FilePath);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

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
                MediaControls.Instance.VUmeterColorL = "#FFF0F0F0"; //off
            }
            else if ((int)dbLevelL < -20)
            {
                MediaControls.Instance.VUmeterColorL = "#FF00EE55";
            }
            else if ((int)dbLevelL < -17)
            {
                MediaControls.Instance.VUmeterColorL = "#FF00EE55"; 
            }
            else if ((int)dbLevelL < -15)
            {
                MediaControls.Instance.VUmeterColorL = "#FF5AF700"; 
            }
            else if ((int)dbLevelL < -12)
            {
                MediaControls.Instance.VUmeterColorL = "#FF5DFF00";
            }
            else if ((int)dbLevelL < -10)
            {
                MediaControls.Instance.VUmeterColorL = "#FF82EE00";
            }
            else if ((int)dbLevelL < -8)
            {
                MediaControls.Instance.VUmeterColorR = "#FFA4F100";
            }
            else if ((int)dbLevelL < -6)
            {
                MediaControls.Instance.VUmeterColorR = "#FFBAFF00";
            }
            else if ((int)dbLevelL < -3)
            {
                MediaControls.Instance.VUmeterColorR = "#FFBAFF00";
            }
            else if ((int)dbLevelL < 0)
            {
                MediaControls.Instance.VUmeterColorR = "#FFD1FF00";
            }
            else if ((int)dbLevelL < 1)
            {
                MediaControls.Instance.VUmeterColorL = "#FFE3EE00";
            }
            else if ((int)dbLevelL < 2)
            {
                MediaControls.Instance.VUmeterColorL = "#FFEEE300";
            }
            else if ((int)dbLevelL < 3)
            {
                MediaControls.Instance.VUmeterColorL = "#FFEEC300";
            }
            else if ((int)dbLevelL < 4)
            {
                MediaControls.Instance.VUmeterColorL = "#FFEE6100";
            }


            if ((int)dbLevelR < -25)
            {
                MediaControls.Instance.VUmeterColorR = "#FFF0F0F0"; //off
            }
            else if ((int)dbLevelR < -20)
            {
                MediaControls.Instance.VUmeterColorR = "#FF00EE55";
            }
            else if ((int)dbLevelR < -17)
            {
                MediaControls.Instance.VUmeterColorR = "#FF00EE55";
            }
            else if ((int)dbLevelR < -15)
            {
                MediaControls.Instance.VUmeterColorR = "#FF5AF700";
            }
            else if ((int)dbLevelR < -12)
            {
                MediaControls.Instance.VUmeterColorR = "#FF5DFF00";
            }
            else if ((int)dbLevelR < -10)
            {
                MediaControls.Instance.VUmeterColorR = "#FF82EE00";
            }
            else if ((int)dbLevelR < -8)
            {
                MediaControls.Instance.VUmeterColorR = "#FFA4F100";
            }
            else if ((int)dbLevelR < -6)
            {
                MediaControls.Instance.VUmeterColorR = "#FFBAFF00"; 
            }
            else if ((int)dbLevelR < -3)
            {
                MediaControls.Instance.VUmeterColorR = "#FFBAFF00"; 
            }
            else if ((int)dbLevelR < 0)
            {
                MediaControls.Instance.VUmeterColorR = "#FFD1FF00";
            }
            else if ((int)dbLevelR < 1)
            {
                MediaControls.Instance.VUmeterColorR = "#FFE3EE00";
            }
            else if ((int)dbLevelR < 2)
            {
                MediaControls.Instance.VUmeterColorR = "#FFEEE300";
            }
            else if ((int)dbLevelR < 3)
            {
                MediaControls.Instance.VUmeterColorR = "#FFEEC300";
            }
            else if ((int)dbLevelR < 4)
            {
                MediaControls.Instance.VUmeterColorR = "#FFEE6100";
            }
        }

        //Private Method ===================================================================================================

        /// <summary>
        /// Initialize Bass audio playback
        /// </summary>
        private void InitBass()
        {
            try
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
            catch (Exception ex)
            {

            }
        }

        private void AddToBassMixer()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("AddToBassMixer");
            }
        }

        private void PlayNextTrack()
        {
            try
            {
                lock (_songsSource.SongsQueue)
                {
                    _getNestSongInfo = "";
                    MediaControls.Instance.VocalChannel = "BAL";
                    Channel = ChannelSelected.Right;

                    if (_songsSource.SongQueueCount > 0)
                    {
                        MediaControls.Instance.SongTitle = _songsSource.SongsQueue[0].Name;
                        MediaControls.Instance.SongArtist = _songsSource.SongsQueue[0].Artist;

                        if (_songsSource.IsCdgFileType)
                        {
                            _previousTrack = _currentTrack;
                            _currentTrack = _track as BassAudio;
                            VlcPlayer.PlayBackGroundVideo();
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
            catch (Exception ex)
            {
                Console.WriteLine("PlayNextTrack");
            }
        }

        private void OnVlcSync()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("OnVlcSync");
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
            try
            {
                if (_songsSource.SongQueueCount <= 0)
                    _currentTrack.NextTrackSync = 0;
                else
                    _currentTrack.NextTrackSync = 1;

                user = new IntPtr(_currentTrack.NextTrackSync);

                if (user.ToInt32() == 0)
                {
                    // END SYNC
                    Application.Current.Dispatcher.BeginInvoke( new Action(PlayNextTrack));
                }
                else
                {
                    // POS SYNC
                    Application.Current.Dispatcher.BeginInvoke( new Action(delegate
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
            catch (Exception ex)
            {
                Console.WriteLine("OnTrackSync");
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
            try
            {
               
            }
            catch (Exception ex)
            {

            }
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
