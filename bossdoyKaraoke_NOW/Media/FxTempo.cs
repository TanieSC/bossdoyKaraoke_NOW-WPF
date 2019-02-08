using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace bossdoyKaraoke_NOW.Media
{
    class FxTempo : IFxTempo
    {
        private float _key = 0f;
        private float _tempo = 0f;
        private int _channelTempo = 0;
        private static FxTempo _instance;


        public static FxTempo Instance
        {
            get
            {                
               _instance = new FxTempo();
                return _instance;
            }
        }

        public int CreateTempo(int Channel)
        {
                _channelTempo = BassFx.BASS_FX_TempoCreate(Channel, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_SAMPLE_FLOAT);
                return _channelTempo;
        }

        public float Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                Bass.BASS_ChannelSetAttribute(_channelTempo, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, value);
            }
        }

        public float Tempo
        {
            get
            {
                return _tempo;
            }
            set
            {
                _tempo = value;
                Bass.BASS_ChannelSetAttribute(_channelTempo, BASSAttribute.BASS_ATTRIB_TEMPO, value);
            }
        }
    }
}
