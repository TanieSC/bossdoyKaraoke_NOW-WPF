using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.Media
{
    public interface IBassAudio
    {
        PlayState PlayState{ get; }
        IMixer BassMixer { get; }
        int MixerChannel { get; }
        int Channel { get; }
        IFxTempo FXTempo { get; }
        TAG_INFO Tags { get; }
        long TrackLength { get; }
        SYNCPROC TrackSync { get; set; }
        int NextTrackSync { get; set; }
        void Initialize(IntPtr appMainWindow);
        void CreateStream();
        void Dispose();
    }
}
