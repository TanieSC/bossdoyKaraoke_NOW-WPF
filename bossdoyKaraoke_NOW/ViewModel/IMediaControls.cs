using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IMediaControls
    {
        string SongTitle { get; set; }
        string SongArtist { get; set; }
        string ElapsedTime { get; set; }
        string RemainingTime { get; set; }
        double ProgressValue { get; set; }
        string Tempo { get; set; }
        string Key { get; set; }
        bool EnableTempoKeyPanel { get; set; }
        double KeyTempoOpacity { get; set; }
        IMediaControls Controls { get; }
        ICommand Loaded { get; }
        ICommand KeyPlusCommand { get; }

    }
}
