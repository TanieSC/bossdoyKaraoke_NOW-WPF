using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class TreeViewDialogModel : ITreeViewDialogModel, INotifyPropertyChanged
    {
        private static TreeViewDialogModel _instance;
        private string _dialogStatus;
        private bool _showDialog;

        //public List<ITreeViewModel> ItemSource { get; }

        public bool ShowDialog
        {
            get
            {
               return _showDialog;
            }
            set
            {
                _showDialog = value;
                OnPropertyChanged();
            }
        }

        public string DialogStatus
        {
            get
            {
                return _dialogStatus;
            }
            set
            {
                _dialogStatus = value;
                OnPropertyChanged();
            }
        }

        public static TreeViewDialogModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TreeViewDialogModel();
                }
                return _instance;
            }
        }

        public TreeViewDialogModel()
        {
            _instance = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
