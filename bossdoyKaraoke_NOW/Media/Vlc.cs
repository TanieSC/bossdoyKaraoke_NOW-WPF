using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using bossdoyKaraoke_NOW.Misc;
using bossdoyKaraoke_NOW.Model;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;
using static bossdoyKaraoke_NOW.Media.VlcSync;

namespace bossdoyKaraoke_NOW.Media
{
    class Vlc : PlayerBase
    {
        IMediaPlayerFactory _factory;
        IVideoPlayer _player;
        IMemoryRenderer _memRender;

        IMediaList _media_list, _media_list_preview;
        IMediaListPlayer _list_player, _list_preview_player;

        IMedia _media, _media_preview;

        string _path1 = FilePath + @"VIDEO_NATURE\1.vob";
        string _path2 = FilePath + @"VIDEO_NATURE\2.vob";
        string _path3 = FilePath + @"VIDEO_NATURE\3.vob";
        string[] _videoPath;
        string _videoDir;

        [StructLayout(LayoutKind.Sequential)]
        private struct AudioOutputDevice
        {
            public AudioOutputModuleInfo Module;
            public List<AudioOutputDeviceInfo> Devices;
        }

        private List<AudioOutputDevice> _audioOutputDevices;
        private AudioOutputDeviceModel _audioOutputDevice;
        private EqualizerModel _equalizer;
        private SYNCPROC _syncProc;
        private string _filePath;
        private float _volume = 50f;
        private float _plus20Volume = 20f;
        private static Vlc _instance;

        public static Vlc Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Vlc();
                }
                return _instance;
            }
        }
        private object _locker = new object();
        private int _videoWidth;
        private int _videoHeight;
        private byte[] _byteArrayBitmap;

        public int VideoWidth { get { return _videoWidth; } }
        public int VideoHeight { get { return _videoHeight; } }
        public string TimeElapsed { get; private set; }
        public string TimeDuration { get; private set; }
        public string GetTimeDuration { get; private set; }
        public float PlayerPosition { get; private set; }
        public string VideoPathDir { get { return _videoDir; } }

        public byte[] ByteArrayBitmap
        {
            get
            {
                return _byteArrayBitmap;
            }
        }

        public Vlc()
        {
            string[] args = new string[]
            {
                "-I",
                "dumy",
                "--ignore-config",
                "--no-osd",
                "--disable-screensaver",
                "--file-caching=1000",
                "--plugin-path=./plugins",
                "--audio-filter=equalizer",
                "--equalizer-preamp=0",
                "--equalizer-bands=0 0 0 0 0 0 0 0 0 0"
            };

            _videoDir = AppConfig.Get<string>("BackGroundVideoDir");
            _audioOutputDevice = AudioOutputDeviceModel.Instance;
            _equalizer = EqualizerModel.Instance;

            _factory = new MediaPlayerFactory(args);
            _player = _factory.CreatePlayer<IVideoPlayer>();
            _media_list = _factory.CreateMediaList<IMediaList>();
            _media_list_preview = _factory.CreateMediaList<IMediaList>();

            var eqPresets = new Dictionary<int, Preset>();
            var presets = Equalizer.Presets.ToDictionary(key => key.Index);

            _equalizer.EQPreset = new Equalizer();

            _equalizer.EQPreset.Bands[0].Amplitude = _equalizer.EQ0;
            _equalizer.EQPreset.Bands[1].Amplitude = _equalizer.EQ1;
            _equalizer.EQPreset.Bands[2].Amplitude = _equalizer.EQ2;
            _equalizer.EQPreset.Bands[3].Amplitude = _equalizer.EQ3;
            _equalizer.EQPreset.Bands[4].Amplitude = _equalizer.EQ4;
            _equalizer.EQPreset.Bands[5].Amplitude = _equalizer.EQ5;
            _equalizer.EQPreset.Bands[6].Amplitude = _equalizer.EQ6;
            _equalizer.EQPreset.Bands[7].Amplitude = _equalizer.EQ7;
            _equalizer.EQPreset.Bands[8].Amplitude = _equalizer.EQ8;
            _equalizer.EQPreset.Bands[9].Amplitude = _equalizer.EQ9;
            _equalizer.EQPreset.Preamp = _equalizer.PreAmp;

            if (!_equalizer.EQEnabled)
            {
                _player.SetEqualizer(null);
            }
            else
            {
                _player.SetEqualizer(_equalizer.EQPreset);
            }

            //_vlcEqualizer.Dispose();

            eqPresets.Add(-1, null);

            for (int i = 0; i < presets.Count; i++)
            {
                eqPresets.Add(i, presets[i]);
            }

            _equalizer.EQPresets = eqPresets;

            _player.Volume = (int)_volume;
            _player.Events.PlayerPositionChanged += new EventHandler<MediaPlayerPositionChanged>(Events_PlayerPositionChanged);
            _player.Events.TimeChanged += new EventHandler<MediaPlayerTimeChanged>(Events_TimeChanged);
            _player.Events.MediaEnded += new EventHandler(Events_MediaEnded);
            _player.Events.PlayerStopped += new EventHandler(Events_PlayerStopped);

            //Background Video ==========
            _list_player = _factory.CreateMediaListPlayer<IMediaListPlayer>(_media_list);

            _memRender = _list_player.InnerPlayer.CustomRenderer;
            MemRenderSetCallBack(_memRender);

            // 4:3 aspect ratio resolutions: 640×480, 800×600, 960×720, 1024×768, 1280×960, 1400×1050, 1440×1080 , 1600×1200, 1856×1392, 1920×1440, and 2048×1536
            //16:9 aspect ratio resolutions: 1024×576, 1152×648, 1280×720, 1366×768, 1600×900, 1920×1080, 2560×1440 and 3840×2160

            _list_player.PlaybackMode = PlaybackMode.Loop;
            _list_player.InnerPlayer.Volume = 0;
            _list_player.InnerPlayer.Mute = true;
            //Background Video

            //Preview Background video ==========
            _list_preview_player = _factory.CreateMediaListPlayer<IMediaListPlayer>(_media_list_preview);
            _list_preview_player.PlaybackMode = PlaybackMode.Loop;
            _list_preview_player.InnerPlayer.Volume = 0;
            _list_preview_player.InnerPlayer.Mute = true;
            //Preview Background video

            if (_videoDir == string.Empty || !Directory.Exists(_videoDir))
                _videoDir = FilePath + @"VIDEO_NATURE\";

            GetVideoBG(_videoDir);

            GetAudioOutputDevices();

            LoadDefaultVideoBG();

        }

        public bool GetVideoBG(string sDir)
        {
            bool isVideoFound = false;

            if (sDir != string.Empty && Directory.Exists(sDir))
            {
                _videoDir = sDir;
                _videoPath = Directory.EnumerateFiles(sDir, "*.*", SearchOption.AllDirectories)
                       .Where(s => Entensions.Contains(Path.GetExtension(s)))
                       .Select(s =>
                       {
                           return s;
                       }).ToArray();

                if (_videoPath.Count() > 0)
                {
                    isVideoFound = true;
                }
                else
                {
                    // MessageBox.Show("No supported video file(s) found!");
                }
            }

            return isVideoFound;
        }

        public void LoadDefaultVideoBG()
        {

            if (_player.IsPlaying) _player.Stop();

            if (_videoDir != string.Empty && _videoPath != null)
            {
                if (_media_list.Count() > 0) _media_list.Clear();

                if( _list_player != null )_list_player.InnerPlayer.Dispose();

                for (int i = 0; i < _videoPath.Length; i++)
                {
                    _media_preview = _factory.CreateMedia<IMediaFromFile>(_videoPath[i]);
                    _media_preview.Parse(true);
                    _media_list.Add(_media_preview);
                    _media_preview.Dispose();
                }

                if (_list_player.IsPlaying) _list_player.Stop();
                _list_player.Play();

            }
        }

        public void PlayBackGroundVideo()
        {

            if (_player.IsPlaying)
                _player.Stop();

            _list_player.PlayNext();
        }

        public void UpdateEQ(Equalizer equalizer)
        {
            if (_player.IsPlaying)
            {
                _player.SetEqualizer(equalizer);
            }
        }

        public bool PlayVideoke(string filePath, SYNCPROC syncProc)
        {
            if (!File.Exists(filePath)) return false;

            _syncProc = syncProc;
            _filePath = filePath;

            if (_list_player.IsPlaying)
                _list_player.Stop();

            if (_player.IsPlaying)
                _player.Stop();

            _media = _factory.CreateMedia<IMediaFromFile>(filePath);
            _memRender = _player.CustomRenderer;
            MemRenderSetCallBack(_memRender);

            try
            {
                _media.Events.DurationChanged -= new EventHandler<MediaDurationChange>(Events_DurationChanged);
                _media.Events.StateChanged -= new EventHandler<MediaStateChange>(Events_StateChanged);
                _media.Events.ParsedChanged -= new EventHandler<MediaParseChange>(Events_ParsedChanged);

                _media.Events.DurationChanged += new EventHandler<MediaDurationChange>(Events_DurationChanged);
                _media.Events.StateChanged += new EventHandler<MediaStateChange>(Events_StateChanged);
                _media.Events.ParsedChanged += new EventHandler<MediaParseChange>(Events_ParsedChanged);
            }
            catch (Exception exce)
            {
                _media.Events.DurationChanged += new EventHandler<MediaDurationChange>(Events_DurationChanged);
                _media.Events.StateChanged += new EventHandler<MediaStateChange>(Events_StateChanged);
                _media.Events.ParsedChanged += new EventHandler<MediaParseChange>(Events_ParsedChanged);
            }

            _player.Channel = AudioChannelType.Stereo;
            _player.Open(_media);
            _media.Parse(true);
            Play();

            CurrentPlayState = PlayState.Playing;

            //Console.WriteLine("VLC: " + Volume);
            return true;
        }

        public void SetAudioOutputDevice()
        {
            for (int i = 0; i < _audioOutputDevices.Count; i++)
            {
                var selected = _audioOutputDevice.SelectedDevice;

                foreach (var device in _audioOutputDevices[i].Devices)
                {
                    if (device.Longname == _audioOutputDevice.DeviceInfos[selected].name)
                    {
                        _player.SetAudioOutputModuleAndDevice(_audioOutputDevices[i].Module, device);

                        if (_player.IsPlaying)
                        {
                            var pos = _player.Position;

                            _player.Stop();
                            PlayVideoke(_filePath, _syncProc);
                            _player.Position = pos;
                        }
                    }
                }
            }
        }

        public void SetDefaultVideoBG(IntPtr handle)
        {
            if (_videoDir != string.Empty)
            {

                StopPreviewVideoBG();

               // _media_list_preview = _factory.CreateMediaList<IMediaList>();
               // _list_preview_player = _factory.CreateMediaListPlayer<IMediaListPlayer>(_media_list_preview);
               // _list_preview_player.PlaybackMode = PlaybackMode.Loop;
               // _list_preview_player.InnerPlayer.Volume = 0;
               // _list_preview_player.InnerPlayer.Mute = true;

                for (int i = 0; i < _videoPath.Length; i++)
                {
                    _media_preview = _factory.CreateMedia<IMediaFromFile>(_videoPath[i]);
                    _media_preview.Parse(true);
                    _media_list_preview.Add(_media_preview);
                    _media_preview.Dispose();
                }
                _list_preview_player.InnerPlayer.WindowHandle = handle;
                _list_preview_player.Play();

            }
        }

        public void ViewNextVideoBG()
        {
            if (_list_preview_player != null && _list_preview_player.IsPlaying)
            {
                _list_preview_player.PlayNext();
            }
        }

        public void ViewPreviousVideoBG()
        {
            if (_list_preview_player != null && _list_preview_player.IsPlaying)
            {
                _list_preview_player.PlayPrevios();
            }
        }

        public void StopPreviewVideoBG()
        {
            if (_media_list_preview != null && _media_list_preview.Count() > 0)
            {
                _media_list_preview.Clear();
            }

            if (_list_preview_player != null && _list_preview_player.IsPlaying)
            {
                _list_preview_player.Stop();
                _list_preview_player.InnerPlayer.WindowHandle = IntPtr.Zero;
                _list_preview_player.InnerPlayer.Dispose();
            }
           // if (_media_list_preview != null) _media_list_preview.Dispose();
           // if (_media_preview != null)  _media_preview.Dispose();
           // if(_list_preview_player != null) _list_preview_player.Dispose();
        }

        public void GetDuration(string filePath)
        {
            if (!File.Exists(filePath)) return;

            IMedia media = _factory.CreateMedia<IMediaFromFile>(filePath);
            IVideoPlayer player = _factory.CreatePlayer<IVideoPlayer>();
            _memRender = player.CustomRenderer;
            _memRender.SetFormat(new BitmapFormat(1, 1, ChromaType.RV32));
            media.Events.DurationChanged += new EventHandler<MediaDurationChange>(Events_GetTimeDuration);
            media.Parse(true);
            player.Play();

            Thread.Sleep(100);
            player.Stop();
            player.Dispose();
            _memRender.Dispose();
            media.Dispose();
        }

        private void GetAudioOutputDevices()
        {
            _audioOutputDevices = new List<AudioOutputDevice>();

            foreach (AudioOutputModuleInfo module in _factory.AudioOutputModules)
            {
                var info = _factory.GetAudioOutputDevices(module).ToList();

                if (info.Count > 0)
                {
                    AudioOutputDevice device = new AudioOutputDevice();

                    device.Module = module;
                    device.Devices = info;

                    _audioOutputDevices.Add(device);
                }
            }

            SetAudioOutputDevice();
        }

        private void MemRenderSetCallBack(IMemoryRenderer memRender)
        {
            memRender.SetCallback(delegate (Bitmap frame)
            {
                BitmapData bmpdata = null;
                _videoWidth = frame.Width;
                _videoHeight = frame.Height;

                try
                {
                    bmpdata = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                    int numbytes = bmpdata.Stride * frame.Height;
                    byte[] bytedata = new byte[numbytes];
                    IntPtr ptr = bmpdata.Scan0;
                    Marshal.Copy(ptr, bytedata, 0, numbytes);
                    _byteArrayBitmap = bytedata;
                }
                finally
                {
                    if (bmpdata != null)
                        frame.UnlockBits(bmpdata);
                }
            });

            memRender.SetFormat(new BitmapFormat(1024, 576, ChromaType.RV32));
        }

        private void Events_ParsedChanged(object sender, MediaParseChange e)
        {

        }

        private void Events_StateChanged(object sender, MediaStateChange e)
        {
            //Console.WriteLine("NewState : " + e.NewState);
            if (e.NewState == MediaState.Playing)
            {
            }
        }

        private void Events_DurationChanged(object sender, MediaDurationChange e)
        {
            TimeDuration = Convert.ToDouble(e.NewDuration).ToString();

        }

        private void Events_GetTimeDuration(object sender, MediaDurationChange e)
        {
            GetTimeDuration = Convert.ToDouble(e.NewDuration).ToString();
        }

        private void Events_PlayerStopped(object sender, EventArgs e)
        {
            //_syncProc();           
        }

        private void Events_MediaEnded(object sender, EventArgs e)
        {
            _syncProc();
        }

        private void Events_TimeChanged(object sender, MediaPlayerTimeChanged e)
        {
            TimeElapsed = Convert.ToDouble(e.NewTime).ToString();
        }

        private void Events_PlayerPositionChanged(object sender, MediaPlayerPositionChanged e)
        {
            PlayerPosition = e.NewPosition;// (int)(e.NewPosition * 100);
        }

        public override float Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                if (_equalizer.EQEnabled)
                    _volume = value != 0 ? (value + _plus20Volume) + _equalizer.PreAmp : value;
                else
                    _volume = value != 0 ? (value + _plus20Volume) : value;

                _player.Volume = (int)_volume;

                // Console.WriteLine("Worker VLC2 : " + _player.Volume + " : " + _equalizer.EQEnabled + " : " + _equalizer.PreAmp);
            }
        }

        // public Dictionary<int, Preset> EqPresets { get { return _presets; }  }

        public override void KeyMinus()
        {
            throw new NotImplementedException();
        }

        public override void KeyPlus()
        {
            throw new NotImplementedException();
        }

        public override void Mute()
        {
            //_player.ToggleMute();
            _player.Mute = true;
        }

        public override void UnMute()
        {
            //_player.ToggleMute();
            _player.Mute = false;
        }


        public override void Pause()
        {
            if (_player.IsPlaying)
            {
                CurrentPlayState = PlayState.Paused;
                _player.Pause();
            }
        }

        public override void Play()
        {
            if (!_player.IsPlaying)
            {
                CurrentPlayState = PlayState.Playing;
                _player.Play();
            }
        }

        public override void Stop()
        {
            CurrentPlayState = PlayState.Stopped;
            _player.Stop();
            PlayBackGroundVideo();
        }

        public override void TempoMinus()
        {
            throw new NotImplementedException();
        }

        public override void TempoPlus()
        {
            throw new NotImplementedException();
        }

        ~Vlc()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _factory.Dispose();
                    _player.Dispose();
                    _media.Dispose();
                    _memRender.Dispose();

                    _media_list.Dispose();
                    _list_player.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Vlc() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    class VlcSync
    {
        public delegate void SYNCPROC();
    }
}
