using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IMediaControls
    {
        string SongTitle { get; set; }
        string SongArtist { get; set; }
        int VolumeValue { get; set; }
        string VocalChannel { get; set; }
        string ElapsedTime { get; set; }
        string RemainingTime { get; set; }
        double ProgressValue { get; set; }
        string Tempo { get; set; }
        string Key { get; set; }
        bool EnableControl { get; set; }
        double KeyTempoOpacity { get; set; }
        PackIconKind IconPlayPause { get; set; }
        PackIconKind IconMuteUnMute { get; set; }
        IMediaControls Controls { get; }
        ICommand Loaded { get; }
        ICommand VocalChannelCommand { get; }
        ICommand AddNewScreenCommand { get; }
        ICommand PlayPauseCommand { get; }
        ICommand PlayNextCommand { get; }
        ICommand MuteUnMuteCommand { get; }
        ICommand ShowVolumeControlCommand { get; }
        ICommand HideVolumeControlCommand { get; }
        ICommand VolumeSliderCommand { get; }
        ICommand KeyPlusCommand { get; }
        ICommand KeyMinusCommand { get; }
        ICommand TempoPlusCommand { get; }
        ICommand TempoMinusCommand { get; }


    }
}
