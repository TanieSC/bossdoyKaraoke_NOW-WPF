using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class SearchBoxVModel : ISearchBoxVModel, INotifyPropertyChanged
    {
        private static SearchBoxVModel _instance;
        private ICommand _searchSongsCommand;
        private ISongsSource songsSource = SongsSource.Instance;
        public ObservableCollection<TrackInfoModel> Items { get; set; }
        public int ItemId { get; set; }

        public static SearchBoxVModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SearchBoxVModel();
                }
                return _instance;
            }
        }

        public SearchBoxVModel()
        {
            _instance = this;
        }

        public ObservableCollection<TrackInfoModel> FilteredSong(string filter)
        {
            var data = songsSource.Songs[ItemId].Where(w => w.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1 || w.Artist.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1);

            return new ObservableCollection<TrackInfoModel>(data);
        }

        public ICommand SearchSongsCommand
        {
            get
            {
                return _searchSongsCommand ?? (_searchSongsCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                       // if (CurrentTask == NewTask.LOAD_SONGS)
                       // {
                            //CurrentTask = NewTask.SEARCH_LISTVIEW;
                            Worker.DoWork(NewTask.SEARCH_LISTVIEW, (x as TextBox).Text);
                       // }
                    }
                }));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        //private bool Filter(TrackInfo trackInfo)
        //{
        //    return Search == null
        //        || trackInfo.Name.IndexOf(Search, StringComparison.OrdinalIgnoreCase) != -1
        //        || trackInfo.Artist.IndexOf(Search, StringComparison.OrdinalIgnoreCase) != -1;
        //}
    }
}
