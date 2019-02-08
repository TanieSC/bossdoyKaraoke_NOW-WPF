using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class DefaultAudio
    {
        private static DefaultAudioOutput defaultAudioOutput;
        public enum DefaultAudioOutput
        {
            Bass,
            Wasapi,
            Asio
        }
    }
}
