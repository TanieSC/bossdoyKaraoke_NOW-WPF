using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Media;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace bossdoyKaraoke_NOW.Media
{
    class Mixer : IMixer
    {
        private int _mixerChannel = 0;
        private static Mixer _instance;
        public static Mixer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Mixer();
                }
                return _instance;
            }
        }

        public int MixerStreamCreate(int samplerate)
        {
            BASSFlag mixerFlags = BASSFlag.BASS_SAMPLE_FLOAT;
            _mixerChannel = BassMix.BASS_Mixer_StreamCreate(samplerate, 2, mixerFlags);

            return _mixerChannel;
        }

        public void StreamAddChannel(int channel, SYNCPROC trackSync)
        {
            BassMix.BASS_Mixer_StreamAddChannel(_mixerChannel, channel, BASSFlag.BASS_MIXER_PAUSE | BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_MIXER_DOWNMIX);
            
            // an BASS_SYNC_END is used to trigger the next track in the playlist (if no POS sync was set)
            BassMix.BASS_Mixer_ChannelSetSync(channel, BASSSync.BASS_SYNC_END, 0L, trackSync, new IntPtr(1));
        }
    }
}
