using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using static bossdoyKaraoke_NOW.Enums.PlayerStateEnum;

namespace bossdoyKaraoke_NOW.Media
{
    public interface IPlayer
    {
        PlayState PlayState { get; }
        int MixerChannel { get; }
        int Channel { get; }
        IFxTempo FXTempo { get; }
        TAG_INFO Tags { get; }
        long TrackLength { get; }
        SYNCPROC TrackSync { get; set; }
        int NextTrackSync { get; set; }
        //float Volume { get; }
        ////float Key { get; set; }
        ////float Tempo { get; set; }
        //void BassInitialize();
        //void CreateStream();
        //void Play();
        //void Pause();
        //void Mute();
        //void Stop();
        //void KeyPlus();
        //void KeyMinus();
        //void TempoPlus();
        //void TempoMinus();
    }
}
