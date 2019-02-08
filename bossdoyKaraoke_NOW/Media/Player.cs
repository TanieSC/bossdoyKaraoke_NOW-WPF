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
using bossdoyKaraoke_NOW.Model;
using bossdoyKaraoke_NOW.ViewModel;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.Media
{
    class Player : PlayerBase
    {
        private ObservableCollection<TrackInfo> _songsQueue;
        private ISongsSource _songsSource;
        private BassAudio _track = null;
        private BassAudio _currentTrack = null;
        private BassAudio _previousTrack = null;
        private SYNCPROC _mixerStallSync;
        private bool _isBassInitialized;
        private long _bassChannelPosition;
        private double _bassChannelInSeconds;
        private long _renderAtPosition;
        private int _progressValue;
        private string _getNestSongInfo;
        private double _progressBarMaximum = 2000;

        public IntPtr AppMainWindowHandle;
        public ISongsSource Songs_Source { get { return _songsSource; } }
        public Vlc VlcPlayer;
        public CDGFile CDGmp3;
        public string GetNextSongInfo { get { return _getNestSongInfo; } }

        public override float Volume
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
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

            double timeElapsed = Convert.ToDouble(Vlc.Instance.TimeElapsed);
            double timeRemain = Convert.ToDouble(Vlc.Instance.TimeDuration);
            MediaControls.Instance.ElapsedTime = TimeSpan.FromMilliseconds(timeElapsed).ToString().Substring(0, 8);
            MediaControls.Instance.RemainingTime = TimeSpan.FromMilliseconds(timeRemain - timeElapsed).ToString().Substring(0, 8);

            double bpp = 0;
            bpp = (int)Math.Round(Vlc.Instance.PlayerPosition * _progressBarMaximum);
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
            //SongsSource.Instance.LoadSongCollections();
            _songsSource = SongsSource.Instance;
            _songsSource.LoadSongCollections();
        }

        public void LoadCDGFile(string cdgFileName)
        {
            CDGmp3 = new CDGFile(cdgFileName);

            AddToBassMixer();
            PlayNextTrack();
        }

        public void LoadVideokeFile(string videokeFileName)
        {
            CDGmp3 = null;
            PlayNextTrack();
        }

        public override void KeyMinus()
        {

            if (_songsSource.IsCdgFileType)
            {
                _currentTrack.KeyMinus();
                MediaControls.Instance.Key = string.Format("{0}", _currentTrack.FXTempo.Key);
            }
        }

        public override void KeyPlus()
        {
            if (_songsSource.IsCdgFileType)
            {
                _currentTrack.KeyPlus();
                MediaControls.Instance.Key = string.Format("{0}", _currentTrack.FXTempo.Key);
            }
        }

        public override void TempoMinus()
        {
            if (_songsSource.IsCdgFileType)
            {
                _currentTrack.TempoMinus();
                MediaControls.Instance.Tempo = string.Format("{0}", _currentTrack.FXTempo.Tempo + "%");
            }
        }

        public override void TempoPlus()
        {
            if (_songsSource.IsCdgFileType)
            {
                _currentTrack.TempoPlus();
                MediaControls.Instance.Tempo = string.Format("{0}", _currentTrack.FXTempo.Tempo + "%");
            }
        }

        public override void Mute()
        {
            throw new NotImplementedException();
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override void Play()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            if (_songsSource.IsCdgFileType)
            {
                _currentTrack.Stop();
                CDGmp3 = null;
                _track = null;
                _currentTrack = null;
                _previousTrack = null;
                Vlc.Instance.PlayBackGroundVideo();
            }
            else
            {
                VlcPlayer.Stop();
            }

            _songsSource.IsCdgFileType = false;
        }

        /// <summary>
        /// Get the next tack to play and display on top of the full screen window for 30 sec.
        /// </summary>
        public void GetNextTrackInfo()
        {
            try
            {
                if (_songsSource.SongsQueue.Count > 0)
                {
                    string showNextTrack = MediaControls.Instance.RemainingTime;
                    int minute = Convert.ToInt32(showNextTrack.Substring(3, 2));
                    int second = Convert.ToInt32(showNextTrack.Substring(6, 2));

                    if(minute <= 0 && second == 30)
                        _songsSource.PreProcessFiles(_songsSource.SongsQueue[0].FilePath);

                    if (minute <= 0 && second < 30)
                    {                      
                        string nextSong = _songsSource.SongsQueue[0].Name + "( " + _songsSource.SongsQueue[0].Artist + " )";
                        _getNestSongInfo = nextSong;
                    }
                    else
                        _getNestSongInfo = "";
                }
                else
                    _getNestSongInfo = "";
            }
            catch (Exception ex)
            {


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

                    // m_timerUpdate.Start();
                    Bass.BASS_ChannelPlay(BassAudio.MixerChannel, false);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetMediaControls()
        {
            if (CurrentPlayState == PlayState.Stopped)
                MediaControls.Instance.EnableTempoKeyPanel = false;

            if (_songsSource.IsCdgFileType)
            {
                MediaControls.Instance.EnableTempoKeyPanel = true;
            }
            else
            {
                MediaControls.Instance.EnableTempoKeyPanel = false;
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
                    _track.CreateStream();
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void PlayNextTrack()
        {
            try
            {
                lock (_songsSource.SongsQueue)
                {
                    if (_songsSource.SongsQueue.Count > 0)
                    {
                        _songsQueue = _songsSource.SongsQueue;
                        MediaControls.Instance.SongTitle = _songsQueue[0].Name;
                        MediaControls.Instance.SongArtist = _songsQueue[0].Artist;

                        if (_songsSource.IsCdgFileType)
                        {
                            _previousTrack = _currentTrack;
                            _currentTrack = _track as BassAudio;
                            Vlc.Instance.PlayBackGroundVideo();
                            _currentTrack.Play();

                        }
                        else
                        {
                            Vlc.Instance.PlayVideoke(_songsQueue[0].FilePath, new VlcSync.SYNCPROC(OnVlcSync));
                        }
                    }
                    else
                    {
                        MediaControls.Instance.ElapsedTime = "00:00:00";
                        MediaControls.Instance.RemainingTime = "00:00:00";
                        Stop(); //Stop VLC from playing videoke
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }


        private void OnVlcSync()
        {
            try
            {
                _songsQueue.RemoveAt(0);
                _songsSource.SongsQueue = _songsQueue;
                _songsQueue.Clear();

                if (_songsSource.SongsQueue.Count <= 0)
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
                _songsQueue.RemoveAt(0);
                _songsSource.SongsQueue = _songsQueue;
                _songsQueue.Clear();

                if (_songsSource.SongsQueue.Count <= 0)
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

    }
}
