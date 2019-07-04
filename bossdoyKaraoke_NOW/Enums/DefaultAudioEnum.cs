using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class DefaultAudioEnum
    {
        private static DefaultAudioOutput defaultAudioOutput;

        /// <summary>
        /// Enums to set default audio interface
        /// </summary>
        public enum DefaultAudioOutput
        {
            Bass,
            Wasapi,
            Asio
        }
    }
}
