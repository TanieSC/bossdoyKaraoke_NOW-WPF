using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Un4seen.Bass;
//using System.Windows.Forms;
using Un4seen.Bass.AddOn.Tags;

namespace bossdoyKaraoke_NOW.Model
{
    [Serializable]
    public class TrackInfoModel : ITrackInfoModel, INotifyPropertyChanged
    {
        private bool _isSelected = false;

        public string ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public string FilePath { get; set; }
        public TAG_INFO Tags { get; set; }
        public bool IsSelected { get { return _isSelected; } set { _isSelected = value; OnPropertyChanged(); } }

        public TrackInfoModel()
        {
        }

        public TrackInfoModel(TrackInfoModel trackInfo)
        {

            string filename = Regex.Replace(trackInfo.FilePath, "\\.cdg$", ".mp3", RegexOptions.IgnoreCase);

            Tags = BassTags.BASS_TAG_GetFromFile(filename);

            if (Tags == null)
            {
               // throw new ArgumentException("File not valid!");
               // MessageBox.Show("File not valid or missing " + Path.GetFileName(filename) + " file.");
                return;
            }

            Artist = trackInfo.Artist;
            Duration = Utils.FixTimespan(Tags.duration, "HHMMSS");
            FilePath = trackInfo.FilePath;
            ID = trackInfo.ID;
            Type = trackInfo.Type;
            Name = trackInfo.Name;


            //Tags = BassTags.BASS_TAG_GetFromFile(filename);
            //Duration = Tags.duration.ToString();

            //if (Tags == null)
            //    throw new ArgumentException("File not valid!");
        }

        public TrackInfoModel(string filepath, int songId)
        {

            string filename = Regex.Replace(filepath, "\\.mp3$", ".cdg", RegexOptions.IgnoreCase);

            Tags = BassTags.BASS_TAG_GetFromFile(filepath);

            if (Tags == null)
                return;

            Artist = Tags.artist;
            Duration = Tags.duration.ToString();
            FilePath = filename;
            ID = songId.ToString();
            Name = Tags.title;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
