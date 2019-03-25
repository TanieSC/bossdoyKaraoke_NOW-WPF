using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class KaraokeNowFiles
    {
        private static Create _createNew;

        public enum Create
        {
            Favorites,
            FromPlayedSongs,
            SongQueueList,
            NewSongs
        }

        /// </summary>
        public static Create CreateNew
        {
            get
            {
                return _createNew;
            }
            set
            {
                _createNew = value;
            }

        }
    }
}
