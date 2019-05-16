using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using static bossdoyKaraoke_NOW.Enums.PlayerState;
using static bossdoyKaraoke_NOW.Media.VlcSync;

namespace bossdoyKaraoke_NOW.Media
{
    class Vlc : PlayerBase
    { 
        IMediaPlayerFactory _factory;
        protected IVideoPlayer _player;
        IMemoryRenderer _memRender;

        IMediaList _media_list, _media_list_preview;
        IMediaListPlayer _list_player, _list_preview_player;

        IMedia _media, _media_preview;

        string _path1 = FilePath + @"VIDEO_NATURE\1.vob";
        string _path2 = FilePath + @"VIDEO_NATURE\2.vob";
        string _path3 = FilePath + @"VIDEO_NATURE\3.vob";
        string[] _videoPath;
        string _videoDir;

        private SYNCPROC _syncProc;
        private float _volume = 75f;
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
        private Bitmap _bitmapVideo;
        private int _videoWidth;
        private int _videoHeight;
        private byte[] _byteArrayBitmap;

        public int VideoWidth { get { return _videoWidth; } }
        public int VideoHeight { get { return _videoHeight; } }
        public string TimeElapsed { get; private set; }
        public string TimeDuration { get; private set; }
        public string GetTimeDuration { get; private set; }
        public float PlayerPosition { get; private set; }
      
        public Bitmap BitmapVideo
        {
            get
            {
                lock (_locker)
                {
                    return _bitmapVideo; 
                }
            }
        }

        public byte[] ByteArrayBitmap { get { return _byteArrayBitmap; } }

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
                "--plugin-path=./plugins"
                //"--audio-filter=equalizer",
                //"--equalizer-preamp=11.9",
               // "--equalizer-bands=0 0 0 0 0 0 0 0 0 0"
            };

            _factory = new MediaPlayerFactory(args);
            _player = _factory.CreatePlayer<IVideoPlayer>();
            _media_list = _factory.CreateMediaList<IMediaList>();
            _media_list_preview = _factory.CreateMediaList<IMediaList>();

            //string[] path = new string[] { _path1, _path2, _path3 };

            //for (int i = 0; i < path.Length; i++)
            //{
            //    _media = _factory.CreateMedia<IMediaFromFile>(path[i]);
            //    _media_list.Add(_media);
            //}

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
            _list_player.InnerPlayer.Mute = true;
            //Background Video

            //Preview Background video ==========
            _list_preview_player = _factory.CreateMediaListPlayer<IMediaListPlayer>(_media_list_preview);
            _list_preview_player.PlaybackMode = PlaybackMode.Loop;
            _list_preview_player.InnerPlayer.Mute = true;
            //Preview Background video

            if (_videoDir == string.Empty || !Directory.Exists(_videoDir))
                _videoDir = FilePath + @"VIDEO_NATURE\";

            GetVideoBG(_videoDir);
            SetAudioOutputDevice();

            LoadDefaultVideoBG();

        }

        public bool GetVideoBG(string sDir)
        {
            bool isVideoFound = false;

            if (sDir != string.Empty && Directory.Exists(sDir))
            {
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
            if (_player.IsPlaying) Stop();

            if (_videoDir != string.Empty && _videoPath != null)
            {
                if (_media_list.Count() > 0) _media_list.Clear();

                for (int i = 0; i < _videoPath.Length; i++)
                {
                    _media_preview = _factory.CreateMedia<IMediaFromFile>(_videoPath[i]);
                    _media_list.Add(_media_preview);
                }

                _media_preview.Parse(true);
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

        public bool PlayVideoke(string filePath, SYNCPROC syncProc )
        {
            if (!File.Exists(filePath)) return false;

            _syncProc = syncProc;

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
            foreach (AudioOutputModuleInfo module in _factory.AudioOutputModules)
            {
                List<AudioOutputDeviceInfo> info = _factory.GetAudioOutputDevices(module).ToList();
            }
        }

        public void SetDefaultVideoBG(IntPtr handle)
        {
            throw new NotImplementedException();
        }

        public void ViewNextVideoBG()
        {
            throw new NotImplementedException();
        }

        public void ViewPreviousVideoBG()
        {
            throw new NotImplementedException();
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

        private void MemRenderSetCallBack( IMemoryRenderer memRender)
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
                _volume = value;
                _player.Volume = (int)value;
            }
        }

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
