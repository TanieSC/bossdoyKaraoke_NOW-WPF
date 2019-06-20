using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface IPreferencesModel
    {
        string IntroText { get; set; }
        bool EQEnabled { get; set; }
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

    }
}
