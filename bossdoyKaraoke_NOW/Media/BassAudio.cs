using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.Media
{
    class BassAudio : PlayerBase //, IBassAudio
    {
        private static string _startupPath = AppDomain.CurrentDomain.BaseDirectory;
        private static IntPtr _appManWindow;
        private static int _mixerChannel;
        private static IMixer _mixer;
        private bool _mute;
        private float _playerVolume = 0.5f;

        public IMixer BassMixer { get { return _mixer; } }
        public static int MixerChannel { get { return _mixerChannel; } }
        public int Channel { get; private set; }
        public IFxTempo FXTempo { get; private set; }
        public TAG_INFO Tags { get; set; }
        public long TrackLength { get; private set; }
        public SYNCPROC TrackSync { get; set; }
        public int NextTrackSync { get; set; }
        public override float Volume
        {
            get
            {
                return _playerVolume;
            }
            set
            {
                _playerVolume = value;
                Bass.BASS_ChannelSetAttribute(this.Channel, BASSAttribute.BASS_ATTRIB_VOL, value);
            }
        }

        public BassAudio()
        {
            
        }

        public static bool Initialize(IntPtr appMainWindow)
        {
            _mixer = Mixer.Instance;
            _appManWindow = appMainWindow;

            // string targetPath = string.Empty;

            var assemblyFolder = Path.Combine(_startupPath, (Utils.Is64Bit ? "x64" : "x86"));// + "\\");

            var use64 = Utils.Is64Bit;
            var appAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var appFolder = new FileInfo(appAssembly.Location).Directory.FullName;
            var binariesPath = Path.Combine(appFolder, (Utils.Is64Bit ? "x64" : "x86"));
            //if (Utils.Is64Bit)
            //    targetPath = Path.Combine(_startupPath, "x64"); 
            //else
            //    targetPath = Path.Combine(_startupPath, "x86");


            // var s = Bass.LoadMe(assemblyFolder);
            //  var f = BassMix.LoadMe(assemblyFolder);
            BassNet.OmitCheckVersion = true;
            BassNet.Registration("tanie_calacar@yahoo.com", "2X183372334322");
            var b = Bass.LoadMe(binariesPath);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 200);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 20);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true);

            BASS_DEVICEINFO info = new BASS_DEVICEINFO();
            for (int n = 0; Bass.BASS_GetDeviceInfo(n, info); n++)
            {

                if (info.IsEnabled && info.IsDefault)
                {
                    Console.WriteLine(info.ToString());
                    // m_defaultdevicelongname = info.name;
                    Bass.BASS_SetDevice(n);
                    //  m_defaultdevice = n;
                }

                if (!Bass.BASS_Init(n, 44100, BASSInit.BASS_DEVICE_DEFAULT, _appManWindow))
                {
                    var error = Bass.BASS_ErrorGetCode();
                    //MessageBox.Show(error.ToString(), "Bass_Init error!");
                     return false;
                }
            }


            Bass.BASS_SetVolume(0.3051406f);
   

            Console.WriteLine("BASS_SetVolume: " + b);

            // already create a mixer
            _mixerChannel = _mixer.MixerStreamCreate(44100);

            if (_mixerChannel == 0)
            {
                var error = Bass.BASS_ErrorGetCode();
                // MessageBox.Show(error.ToString(), "Could not create mixer!");
                Bass.BASS_Free();
                return false;
            }

            return true;

        }

        public void CreateStream()
        {
            if (!File.Exists(Tags.filename)) { throw new FileNotFoundException(Tags.filename); }

            FXTempo = FxTempo.Instance;
            Channel = Bass.BASS_StreamCreateFile(Tags.filename, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
            Channel = FXTempo.CreateTempo(Channel);

            if (Channel != 0)
            {
                _mixer.StreamAddChannel(Channel, TrackSync);
                TrackLength = Bass.BASS_ChannelGetLength(Channel);
            }

            CurrentPlayState = PlayState.Stopped;
        }

        public void Dispose()
        {
            if (this.Channel != 0)
            {
                Bass.BASS_StreamFree(this.Channel);
                Bass.BASS_Free();
            }
            else
                Bass.BASS_Free();
        }

        public override void KeyMinus()
        {
            try
            {
                if (FXTempo.Key != -6)
                {
                    FXTempo.Key -= 1f;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void KeyPlus()
        {
            try
            {
                if (FXTempo.Key != 6)
                {
                    FXTempo.Key += 1f;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void Mute()
        {
            if (CurrentPlayState == PlayState.Playing)
            {
                //if (!_mute)
                //{
                    Bass.BASS_ChannelSetAttribute(this.Channel, BASSAttribute.BASS_ATTRIB_VOL, 0f);
                
                  //  _mute = true;
                //}
                //else
                //{
                  //  Bass.BASS_ChannelSetAttribute(this.Channel, BASSAttribute.BASS_ATTRIB_VOL, _playerVolume);
                  //  _mute = false;
               // }
            }
        }

        public override void UnMute()
        {
            if (CurrentPlayState == PlayState.Playing)
            {
                Bass.BASS_ChannelSetAttribute(this.Channel, BASSAttribute.BASS_ATTRIB_VOL, _playerVolume);
            }
        }

        public override void Pause()
        {
            if (CurrentPlayState == PlayState.Playing)
            {
                CurrentPlayState = PlayState.Paused;
                BassMix.BASS_Mixer_ChannelPause(this.Channel);
            }
        }

        public override void Play()
        {
            if (CurrentPlayState != PlayState.Playing)
            {
                CurrentPlayState = PlayState.Playing;
                BassMix.BASS_Mixer_ChannelPlay(this.Channel);
            }
        }

        public override void Stop()
        {
            if (CurrentPlayState == PlayState.Playing)
            {
                CurrentPlayState = PlayState.Stopped;
                Bass.BASS_StreamFree(this.Channel);
            }
        }

        public override void TempoMinus()
        {
            try
            {
                if (FXTempo.Tempo != -50)
                {
                    FXTempo.Tempo -= 5f;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void TempoPlus()
        {
            try
            {
                if (FXTempo.Tempo != 50)
                {
                    FXTempo.Tempo += +5f;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
