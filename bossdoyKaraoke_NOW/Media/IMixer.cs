using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace bossdoyKaraoke_NOW.Media
{
    public interface IMixer
    {
        int MixerStreamCreate(int samplerate);
        void StreamAddChannel(int channel, SYNCPROC trackSync);
    }
}
