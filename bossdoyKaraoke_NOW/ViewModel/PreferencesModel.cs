using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class PreferencesModel : IPreferencesModel
    {
        //General Tab
        private float _EQ0;
        private float _EQ1;
        private float _EQ2;
        private float _EQ3;
        private float _EQ4;
        private float _EQ5;
        private float _EQ6;
        private float _EQ7;
        private float _EQ8;
        private float _EQ9;
        private bool _eqEnabled;
        private string _infoText;
        private float _preAmp;


        public float EQ0
        {
            get
            {
                return _EQ0;
            }

            set
            {
                _EQ0 = value;
            }
        }

        public float EQ1
        {
            get
            {
                return _EQ1;
            }

            set
            {
                _EQ1 = value;
            }
        }

        public float EQ2
        {
            get
            {
                return _EQ2;
            }

            set
            {
                _EQ2 = value;
            }
        }

        public float EQ3
        {
            get
            {
                return _EQ3;
            }

            set
            {
                _EQ3 = value;
            }
        }

        public float EQ4
        {
            get
            {
                return _EQ4;
            }

            set
            {
                _EQ4 = value;
            }
        }

        public float EQ5
        {
            get
            {
                return _EQ5;
            }

            set
            {
                _EQ5 = value;
            }
        }

        public float EQ6
        {
            get
            {
                return _EQ6;
            }

            set
            {
                _EQ6 = value;
            }
        }

        public float EQ7
        {
            get
            {
                return _EQ7;
            }

            set
            {
                _EQ7 = value;
            }
        }

        public float EQ8
        {
            get
            {
                return _EQ8;
            }

            set
            {
                _EQ8 = value;
            }
        }

        public float EQ9
        {
            get
            {
                return _EQ9;
            }

            set
            {
                _EQ9 = value;
            }
        }

        public bool EQEnabled
        {
            get
            {
                return _eqEnabled;
            }

            set
            {
                value = _eqEnabled;
            }
        }

        public string IntroText
        {
            get
            {
                return _infoText;
            }

            set
            {
                value = _infoText;
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
                value = _preAmp;
            }
        }
    }
}
