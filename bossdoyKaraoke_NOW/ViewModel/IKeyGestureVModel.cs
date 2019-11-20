using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bossdoyKaraoke_NOW.ViewModel
{
    interface IKeyGestureVModel
    {
        ICommand KeyGestureCommand { get; }
    }
}
