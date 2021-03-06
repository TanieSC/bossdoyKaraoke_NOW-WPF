﻿using System;
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
        private static Vlc _vlcPlayer;

        public static DefaultVideoBGModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefaultVideoBGModel();
                }

                _vlcPlayer = Vlc.Instance;

                return _instance;
            }
        }

        public string VideoPathDir { get { return _vlcPlayer.VideoPathDir; } }

        public bool GetVideoBG(string sDir)
        {
            //_vlcPlayer = Vlc.Instance;

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
            //_vlcPlayer = Vlc.Instance;
            _vlcPlayer.StopPreviewVideoBG();
        }

        public void ViewNextVideoBG()
        {
            _vlcPlayer.ViewNextVideoBG();
        }

        public void ViewPreviousVideoBG()
        {
            _vlcPlayer.ViewPreviousVideoBG();
        }
    }
}
