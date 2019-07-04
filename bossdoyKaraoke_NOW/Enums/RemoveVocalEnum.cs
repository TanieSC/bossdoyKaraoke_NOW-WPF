using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class RemoveVocalEnum
    {
        private static ChannelSelected _channelSelected;

        /// <summary>
        /// Enums to set and remove vocal from a channel
        /// </summary>
        public enum ChannelSelected
        {
            None,
            Right,
            Left
        }

        /// <summary>
        /// Get or Set Channel for removing vocal 
        /// </summary>
        public static ChannelSelected Channel
        {
            get
            {
                return _channelSelected;
            }
            set
            {
                _channelSelected = value;
            }
        }
    }
}
