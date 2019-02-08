using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class VideoSize
    {
        private static VideoSizeSelected vidoeSize;

        public enum VideoSizeSelected
        {
            Default,
            Good,
            Best
        }

        public static VideoSizeSelected SizeSelected
        {
            get {
                return vidoeSize;
            }
            set
            {
                vidoeSize = value;
            }
        }
    }
}
