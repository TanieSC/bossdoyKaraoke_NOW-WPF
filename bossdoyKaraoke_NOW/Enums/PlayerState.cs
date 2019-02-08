using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class PlayerState
    {
        private static PlayState _currentState;

        public enum PlayState
        {
            Playing,
            Paused,
            Stopped
        }

        /// <summary>
        /// 
        /// </summary>
        public static PlayState CurrentPlayState
        {
            get
            {
                return _currentState;
            }
            set
            {
                _currentState = value;
            }

        }
    }
}
