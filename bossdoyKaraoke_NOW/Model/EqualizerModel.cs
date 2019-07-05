using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Enums;
using Implementation;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.Misc;
using static bossdoyKaraoke_NOW.Enums.EqualizerEnum;

namespace bossdoyKaraoke_NOW.Model
{
    class EqualizerModel
    {
        private static EqualizerModel _instance;
        //public static BandValue[] ArrBandValue = new BandValue[12];
        // private BandValue _bandValue;
        private DSP_Gain _dsp_gain;
        private int _handle = -1;
        private int _fxHandle = -1;
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

        private float[] _centers =
        {
            31.0f,
            63.0f,
            125.0f,
            250.0f,
            500.0f,
            1000.0f,
            2000.0f,
            4000.0f,
            8000.0f,
            16000.0f
        };

        //public int Handle
        //{
        //    get
        //    {
        //        return _handle;
        //    }

        //    set
        //    {
        //        _handle = value;
        //    }
        //}

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
            EQ0 = AppConfig.Get<float>(NewPreset.AudioEQBand0);
            EQ1 = AppConfig.Get<float>(NewPreset.AudioEQBand1);
            EQ2 = AppConfig.Get<float>(NewPreset.AudioEQBand2);
            EQ3 = AppConfig.Get<float>(NewPreset.AudioEQBand3);
            EQ4 = AppConfig.Get<float>(NewPreset.AudioEQBand4);
            EQ5 = AppConfig.Get<float>(NewPreset.AudioEQBand5);
            EQ6 = AppConfig.Get<float>(NewPreset.AudioEQBand6);
            EQ7 = AppConfig.Get<float>(NewPreset.AudioEQBand7);
            EQ8 = AppConfig.Get<float>(NewPreset.AudioEQBand8);
            EQ9 = AppConfig.Get<float>(NewPreset.AudioEQBand9); 
            EQEnabled = AppConfig.Get<bool>(NewPreset.AudioEQEnabled);
            PreAmp = AppConfig.Get<float>(NewPreset.AudioEQPreamp);
            EQSelectedPreset =  AppConfig.Get<int>(NewPreset.AudioEQPreset);
        }

        public void SetupEQ(int handle)
        {
            _handle = handle;

            if (EQEnabled)
            {
                if (handle != -1)
                {
                    
                    BASS_BFX_PEAKEQ eq = new BASS_BFX_PEAKEQ();
                    BASS_BFX_COMPRESSOR2 comp = new BASS_BFX_COMPRESSOR2();
                    BASS_BFX_VOLUME preamp = new BASS_BFX_VOLUME();

                   _fxHandle = Bass.BASS_ChannelSetFX(handle, BASSFXType.BASS_FX_BFX_PEAKEQ, 1);

                    if (_dsp_gain != null) _dsp_gain.Dispose();

                    _dsp_gain = new DSP_Gain(handle, 2);
                    _dsp_gain.Gain_dBV = PreAmp;

                    eq.fQ = 0f;
                    eq.fBandwidth = 0.6f;
                    eq.lChannel = BASSFXChan.BASS_BFX_CHANALL;

                    //for (int i = 0; i < _centers.Length; i++)
                    //{
                    //    eq.lBand = i;
                    //    eq.fCenter = _centers[i];
                    //    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    //}

                    eq.lBand = 0;
                    eq.fCenter = _centers[0];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand0, EQ0);

                    eq.lBand = 1;
                    eq.fCenter = _centers[1];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand1, EQ1);

                    eq.lBand = 2;
                    eq.fCenter = _centers[2];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand2, EQ2);

                    eq.lBand = 3;
                    eq.fCenter = _centers[3];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand3, EQ3);

                    eq.lBand = 4;
                    eq.fCenter = _centers[4];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand4, EQ4);

                    eq.lBand = 5;
                    eq.fCenter = _centers[5];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand5, EQ5);

                    eq.lBand = 6;
                    eq.fCenter = _centers[6];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand6, EQ6);

                    eq.lBand = 7;
                    eq.fCenter = _centers[7];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand7, EQ7);

                    eq.lBand = 8;
                    eq.fCenter = _centers[8];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand8, EQ8);

                    eq.lBand = 9;
                    eq.fCenter = _centers[9];
                    Bass.BASS_FXSetParameters(_fxHandle, eq);
                    UpdateEQBass(NewPreset.AudioEQBand0, EQ9);
                }
            }
        }

        public void UpdateEQBassPreamp(float eqGain)
        {
            _dsp_gain.Gain_dBV = eqGain;
            Console.WriteLine("PreAmp");
        }

        public void UpdateEQBass(NewPreset band, float eqGain)
        {
            BASS_BFX_PEAKEQ eq = new BASS_BFX_PEAKEQ();

            if (_fxHandle == -1) return;

            eq.lBand = (int)band;
            Bass.BASS_FXGetParameters(_fxHandle, eq);
            eq.fGain = eqGain;
            Bass.BASS_FXSetParameters(_fxHandle, eq);

            Console.WriteLine(eq.lBand + " : " + eqGain + " : " + eq.fCenter);

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
