using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Misc;

namespace bossdoyKaraoke_NOW.Model
{
    class TitleTextModel
    {
        private static TitleTextModel _instance;
        private string _titleText;

        public static TitleTextModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TitleTextModel();
                }

                return _instance;
            }
        }

        //public TitleTextModel()
        //{
        //    TitleText = AppConfig.Get<string>("TitleText");
        //}


        public string TitleText
        {
            get
            {
                return _titleText;
            }

            set
            {
                _titleText = value;
            }
        }

        public void UpdateTitleText()
        {
           AppConfig.Set("TitleText", _titleText);
        }
    }
}
