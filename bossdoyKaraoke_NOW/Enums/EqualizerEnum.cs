using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class EqualizerEnum
    {
        private static NewPreset _currentPreset;

        public enum NewPreset
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

        public static NewPreset CurrentPreset
        {
            get
            {
                return _currentPreset;
            }
            set
            {
                _currentPreset = value;
            }

        }

        //public string GetPreset(Preset preset)
        //{        
        //    return Enum.GetName(typeof(EqualizerEnum), preset);
        //}
    }
}
