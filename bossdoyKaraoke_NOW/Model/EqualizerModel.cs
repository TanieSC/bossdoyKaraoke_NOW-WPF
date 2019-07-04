using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Enums;
using Implementation;

namespace bossdoyKaraoke_NOW.Model
{
    class EqualizerModel
    {
        private static EqualizerModel _instance;
        public static BandValue[] ArrBandValue = new BandValue[12];
        private BandValue _bandValue;

        private bool _eqEnabled = false;
        private Dictionary<int, Preset> _eqPresets;
        private Equalizer _eqPreset;
        private int _eqSelectedPreset;
        private float _preAmp = 0f;
        private float _eq0 = 0f;
        private float _eq1 = 0f;
        private float _eq2 = 0f;
        private float _eq3 = 0f;
        private float _eq4 = 0f;
        private float _eq5 = 0f;
        private float _eq6 = 0f;
        private float _eq7 = 0f;
        private float _eq8 = 0f;
        private float _eq9 = 0f;

        public bool EQEnabled
        {
            get
            {
                return _eqEnabled;
            }

            set
            {
                _eqEnabled = value;
            }
        }

        public Dictionary<int, Preset> EQPresets
        {
            get
            {
                return _eqPresets;
            }

            set
            {
                _eqPresets = value;
            }
        }

        public Implementation.Equalizer EQPreset
        {
            get
            {
                return _eqPreset;
            }

            set
            {
                _eqPreset = value;
            }
        }

        public int EQSelectedPreset
        {
            get
            {
                return _eqSelectedPreset;
            }

            set
            {
                _eqSelectedPreset = value;
            }
        }

        public float PreAmp
        {
            get
            {
                return _preAmp;
            }

            set
            {
                _preAmp = value;
            }
        }

        public float EQ0
        {
            get
            {
                return _eq0;
            }

            set
            {
                _eq0 = value;
            }
        }

        public float EQ1
        {
            get
            {
                return _eq1;
            }

            set
            {
                _eq1 = value;
            }
        }

        public float EQ2
        {
            get
            {
                return _eq2;
            }

            set
            {
                _eq2 = value;
            }
        }

        public float EQ3
        {
            get
            {
                return _eq3;
            }

            set
            {
                _eq3 = value;
            }
        }

        public float EQ4
        {
            get
            {
                return _eq4;
            }

            set
            {
                _eq4 = value;
            }
        }

        public float EQ5
        {
            get
            {
                return _eq5;
            }

            set
            {
                _eq5 = value;
            }
        }

        public float EQ6
        {
            get
            {
                return _eq6;
            }

            set
            {
                _eq6 = value;
            }
        }

        public float EQ7
        {
            get
            {
                return _eq7;
            }

            set
            {
                _eq7 = value;
            }
        }

        public float EQ8
        {
            get
            {
                return _eq8;
            }

            set
            {
                _eq8 = value;
            }
        }

        public float EQ9
        {
            get
            {
                return _eq9;
            }

            set
            {
                _eq9 = value;
            }
        }


        public static EqualizerModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EqualizerModel();
                }
                return _instance;
            }
        }

        public EqualizerModel()
        {
            var b = EqualizerEnum.Band.AudioEQBand0;
            EQ0 = AppConfig.Get<float>("AudioEQBand0");
            EQ1 = AppConfig.Get<float>("AudioEQBand1");
            EQ2 = AppConfig.Get<float>("AudioEQBand2");
            EQ3 = AppConfig.Get<float>("AudioEQBand3");
            EQ4 = AppConfig.Get<float>("AudioEQBand4");
            EQ5 = AppConfig.Get<float>("AudioEQBand5");
            EQ6 = AppConfig.Get<float>("AudioEQBand6");
            EQ7 = AppConfig.Get<float>("AudioEQBand7");
            EQ8 = AppConfig.Get<float>("AudioEQBand8");
            EQ9 = AppConfig.Get<float>("AudioEQBand9"); 
            EQEnabled = AppConfig.Get<bool>("AudioEQEnabled");
            PreAmp = AppConfig.Get<float>("AudioEQPreamp");
            EQSelectedPreset =  AppConfig.Get<int>("AudioEQPreset");
        }

        public void SaveEQSettings(string eqBand)
        {

        }

        public class BandValue
        {
            public int Handle { get; set; }
            public float Gain { get; set; }
            public float PreAmp { get; set; }
            public int PreSet { get; set; }
        }
    }
}
