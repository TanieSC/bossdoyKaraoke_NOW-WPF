using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IPreferencesModel
    {
        string IntroText { get; set; }
        bool EQEnabled { get; set; }
        DataTable EQPreset { get; set; }
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
        ICommand EQEnabledCommand { get; }
        ICommand IntroTextCommand { get; }
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

    }
}
