﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Implementation;
using Un4seen.Bass;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IPreferencesVModel
    {
        string TitleText { get; set; }
        string BackGroundVideoPath { get; set; }
        Dictionary<int, BASS_DEVICEINFO> DeviceInfos { get; set; }
        int SelectedDevice { get; set; }
        bool EQEnabled { get; set; }
        Dictionary<int, Preset> EQPresets { get; set; }
       int EQSelectedPreset { get; set; }
        float PreAmp { get; set; }
        float EQ0 { get; set; }
        float EQ1 { get; set; }
        float EQ2 { get; set; }
        float EQ3 { get; set; }
        float EQ4 { get; set; }
        float EQ5 { get; set; }
        float EQ6 { get; set; }
        float EQ7 { get; set; }
        float EQ8 { get; set; }
        float EQ9 { get; set; }
        ICommand SelectedDeviceCommand { get; }
        ICommand ClosingCommand { get; }
        ICommand EQLoadedCommand { get; }
        ICommand EQSelectedPresetCommand { get; }
        ICommand EQEnabledCommand { get; }
        ICommand TitleTextCommand { get; }
        ICommand PreAmpCommand { get; }
        ICommand EQ0Command { get; }
        ICommand EQ1Command { get; }
        ICommand EQ2Command { get; }
        ICommand EQ3Command { get; }
        ICommand EQ4Command { get; }
        ICommand EQ5Command { get; }
        ICommand EQ6Command { get; }
        ICommand EQ7Command { get; }
        ICommand EQ8Command { get; }
        ICommand EQ9Command { get; }
        ICommand SelectBGVideoCommand { get; }
        ICommand VideoPreviewSreenLoadedCommand { get; }
        ICommand ViewNextVideoBGCommand { get; }
        ICommand ViewPreviousVideoBGCommand { get; }
        ICommand ApplyVideoCommand { get; }
    }
}
