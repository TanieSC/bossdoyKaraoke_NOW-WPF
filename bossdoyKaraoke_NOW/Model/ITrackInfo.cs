using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
//using System.Windows.Forms;
using Un4seen.Bass.AddOn.Tags;

namespace bossdoyKaraoke_NOW.Model
{
    public interface ITrackInfo
    {
        string ID { get; set; }
        string Name { get; set; }
        string Artist { get; set; }
        string Duration { get; set; }
        string FilePath { get; set; }
        TAG_INFO Tags { get; set; }
    }
}
