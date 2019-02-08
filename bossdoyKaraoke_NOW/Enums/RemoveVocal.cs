using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class RemoveVocal
    {
        private static ChannelSelected _channelSelected;
        public enum ChannelSelected
        {
            None,
            Right,
            Left
        }

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
