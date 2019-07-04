using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class EqualizerEnum
    {
        private static Band _currentBand;

        public enum Band
        {
            AudioEQBand0,
            AudioEQBand1,
            AudioEQBand2,
            AudioEQBand3,
            AudioEQBand4,
            AudioEQBand5,
            AudioEQBand6,
            AudioEQBand7,
            AudioEQBand8,
            AudioEQBand9,
            AudioEQEnabled,
            AudioEQPreamp,
            AudioEQPreset
        }

        public static Band CurrentBand
        {
            get
            {
                return _currentBand;
            }
            set
            {
                _currentBand = value;
            }

        }
    }
}
