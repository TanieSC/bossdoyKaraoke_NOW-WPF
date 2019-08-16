using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Media;
using Un4seen.Bass;

namespace bossdoyKaraoke_NOW.Model
{
    class AudioOutputDeviceModel
    {
        private static AudioOutputDeviceModel _instance;
        private Vlc _vlcPlayer;
        private Dictionary<int,BASS_DEVICEINFO> _deviceInfos;
        private int _selectedDevice;

        public Dictionary<int, BASS_DEVICEINFO> DeviceInfos
        {
            get
            {
                return _deviceInfos;
            }

            set
            {
                _deviceInfos = value;
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
            }
        }

        public static AudioOutputDeviceModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AudioOutputDeviceModel();
                }
                return _instance;
            }
        }

        public void SetAudioOutputDevice()
        {
            _vlcPlayer = Vlc.Instance;
            BassAudio.SetAudioOutputDevice();
            _vlcPlayer.SetAudioOutputDevice();
        }

    }
}
