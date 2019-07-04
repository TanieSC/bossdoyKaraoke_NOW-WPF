using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class PlayerStateEnum
    {
        private static PlayState _currentState;

        /// <summary>
        /// Enums to Set player state
        /// </summary>
        public enum PlayState
        {
            Playing,
            Paused,
            Stopped
        }

        /// <summary>
        /// Get or Set Player state
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
