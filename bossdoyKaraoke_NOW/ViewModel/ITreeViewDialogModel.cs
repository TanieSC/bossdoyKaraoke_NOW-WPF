using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public interface ITreeViewDialogModel
    {
        bool ShowDialog { get; set; }
        string DialogStatus { get; set; }
        Visibility AddingStatus { get; set; }
        Visibility LoadingStatus { get; set; }
    }
}
