using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Declarations.Events;

namespace bossdoyKaraoke_NOW.Media
{
    public interface IVlc : IDisposable
    {
        Bitmap BitmapVideo { get; }
        int VideoWidth { get; }
        int VideoHeight { get; }
        byte[] ByteArrayBitmap { get; }
        bool GetVideoBG(string sDir);
        void LoadDefaultVideoBG();
        bool PlayVideoke(string filePath);
        void SetAudioOutputDevice();
        void SetDefaultVideoBG(IntPtr handle);
        void ViewNextVideoBG();
        void ViewPreviousVideoBG();
        void GetDuration(string filePath);

        //Events
        //void Events_PlayerStopped(object sender, EventArgs e);
        //void Events_MediaEnded(object sender, EventArgs e);
        //void Events_TimeChanged(object sender, MediaPlayerTimeChanged e);
        //void Events_PlayerPositionChanged(object sender, MediaPlayerPositionChanged e);
        //void Events_GetTimeDuration(object sender, MediaDurationChange e);

    }
}
