using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Model
{
    class Equalizer
    {
        public static BandValue[] ArrBandValue = new BandValue[12];
        private BandValue _bandValue;


        public class BandValue
        {
            public int Handle { get; set; }

            public float Gain { get; set; }
            public float PreAmp { get; set; }
            public int PreSet { get; set; }
        }
    }
}
