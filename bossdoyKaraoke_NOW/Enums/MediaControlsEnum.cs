using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class MediaControlsEnum
    {
        private static Control _currentControl;

        public enum Control
        {
            PlayPause,
            Next,
            VolumeUp,
            VolumeDown,
            MuteUnmute,
            KeyUp,
            KeyDown,
            TempoUp,
            TempoDown
        }

        public static Control CurrentControl
        {
            get
            {
                return _currentControl;
            }
            set
            {
                _currentControl = value;
            }

        }
    }
}
