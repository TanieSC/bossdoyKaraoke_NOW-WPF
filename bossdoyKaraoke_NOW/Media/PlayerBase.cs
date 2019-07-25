using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Media
{
    abstract class PlayerBase
    {
        private static string _filePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\karaokeNow\";
        private static HashSet<string> _extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cdg", ".vob", ".mp4", ".flv", ".avi", ".mov", ".mkv", ".mpg", ".wav", ".wmv" }; // ".vob", ".mp4", ".flv", ".avi", ".mov", ".mkv", ".mpg", ".wav", ".wmv"

        public static string FilePath { get { return _filePath; } }
        public static HashSet<string> Entensions { get { return _extensions; } }
        public static string Filter
        {
            get
            {
                string extns = string.Join<string>(";", _extensions).Replace(".", "*.");
                string allextn = "Media files (" + extns + ")|" + extns;
                string extn = "";

                foreach (string extension in _extensions)
                {
                    string s = extension.Replace(".", "*.");
                    extn += extension.Replace(".", "").Substring(0, 1).ToUpper() + extension.Substring(2) + " file (" + s + ")|" + s + "|";
                }

                return extn + allextn;
            }
        }
        public abstract float Volume { get; set; }
        public abstract void KeyMinus();
        public abstract void KeyPlus();
        public abstract void Mute();
        public abstract void UnMute();
        public abstract void Pause();
        public abstract void Play();
        public abstract void Stop();
        public abstract void TempoMinus();
        public abstract void TempoPlus();
    }
}
