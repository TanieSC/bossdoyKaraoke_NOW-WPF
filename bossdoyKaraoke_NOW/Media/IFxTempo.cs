using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Media
{
    public interface IFxTempo
    {
        int CreateTempo(int channel);
        float Key { get; set; }
        float Tempo { get; set; }
    }
}
