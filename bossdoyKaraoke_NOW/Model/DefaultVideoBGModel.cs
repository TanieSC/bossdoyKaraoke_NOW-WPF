using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Misc;

namespace bossdoyKaraoke_NOW.Model
{
    class DefaultVideoBGModel
    {
        private static DefaultVideoBGModel _instance;
        private Vlc _vlcPlayer;

        public static DefaultVideoBGModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefaultVideoBGModel();
                }
                return _instance;
            }
        }

        public bool GetVideoBG(string sDir)
        {
            _vlcPlayer = Vlc.Instance;

            return _vlcPlayer.GetVideoBG(sDir);
        }

        public void SetDefaultVideoBG(IntPtr handle)
        {
            _vlcPlayer.SetDefaultVideoBG(handle);
        }

        public void LoadDefaultVideoBG(string videoPath)
        {
            _vlcPlayer.LoadDefaultVideoBG();
            AppConfig.Set("BackGroundVideoDir", videoPath);
        }

        public void StopPreviewVideoBG()
        {
            _vlcPlayer = Vlc.Instance;
            _vlcPlayer.StopPreviewVideoBG();
        }
    }
}
