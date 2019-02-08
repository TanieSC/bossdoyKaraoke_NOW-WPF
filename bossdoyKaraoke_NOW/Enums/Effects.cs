using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class Effects
    {
        private static Load Fx;

        public enum Load
        {
            EQ1_0,
            EQ1_1,
            EQ1_2,
            COMPRESSOR1_0,
            COMPRESSOR1_1,
            COMPRESSOR1_2,
            EQ7_0,
            EQ7_1,
            EQ7_2,
            EQ7_PHONE,
            DeEsser0_0,
            DeEsser0_1,
            DeEsser0_2,
            REVERB,
            CHANNETSTRIP
        }

        public static Load GetorSetFx
        {
            get
            {
                return Fx;
            }
            set
            {
                Fx = value;
            }
        }
    }
}
