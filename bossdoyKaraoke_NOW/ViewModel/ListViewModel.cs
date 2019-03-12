using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using bossdoyKaraoke_NOW.Model;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;
using static bossdoyKaraoke_NOW.Enums.PlayerState;

namespace bossdoyKaraoke_NOW.ViewModel
{
    public class ListViewModel : IListViewModel
    {
        private ISongsSource _songsSource = SongsSource.Instance;
        private ICommand _previewMouseDoubleClick;
        private ICommand _loaded;
        private ICommand _contextMenuLoaded;
        private ICommand _addToQueueClick;
        private ICommand _addToQueueAsNextClick;
        private ICommand _removeFromQueue;

        public ObservableCollection<TrackInfo> Items { get; private set; }

        public ListViewModel()
        {
            Items =  _songsSource.Songs.Count > 0 ? _songsSource.Songs[0] : new ObservableCollection<TrackInfo>();
        }

        public ICommand PreviewMouseDoubleClick
        {
            get
            {
                return _previewMouseDoubleClick ?? (_previewMouseDoubleClick = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        AddToQueue(x as TrackInfo);
                    }
                }));
            }
        }

        public ICommand Loaded
        {
            get
            {
                return _loaded ?? (_loaded = new RelayCommand(x =>
                {
                    Worker.ListViewElement = x as ListView;
                }));
            }
        }

        public ICommand ContextMenuLoaded
        {
            get
            {
                return _contextMenuLoaded ?? (_contextMenuLoaded = new RelayCommand(x =>
                {
                    if (x != null)
                        EnableDisableMenuItem(x as ContextMenu);
                }));
            }
        }

        public ICommand AddToQueueClick
        {
            get
            {
                return _addToQueueClick ?? (_addToQueueClick = new RelayCommand(x =>
                {
                    if (x != null)
                        AddToQueue(x as TrackInfo);
                }));
            }
        }

        public ICommand AddToQueueAsNextClick
        {
            get
            {
                return _addToQueueAsNextClick ?? (_addToQueueAsNextClick = new RelayCommand(x =>
                {
                    if (x != null)
                        AddToQueueAsNext(x as TrackInfo);
                }));
            }
        }

        public ICommand RemoveFromQueueClick
        {
            get
            {
                return _removeFromQueue ?? (_removeFromQueue = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        RemoveFromQueue(x as TrackInfo);
                    }
                }));
            }
        }

        private void EnableDisableMenuItem(ContextMenu sender)
        {
            var contextMenu = sender as ContextMenu;
            var play = contextMenu.Items[0] as MenuItem;
            var addToQueue = contextMenu.Items[1] as MenuItem;
            var addToQueueAsNext = contextMenu.Items[2] as MenuItem;
            var removeFromQueue = contextMenu.Items[4] as MenuItem;

            switch (CurrentTask) //CurrentTask is already set from selecting treeview item so we are just getting the enum value
            {
                case NewTask.LOAD_QUEUE_SONGS:
                    addToQueue.IsEnabled = false;
                    addToQueueAsNext.IsEnabled = false;
                    removeFromQueue.IsEnabled = true;
                    break;
                case NewTask.LOAD_FAVORITES:
                case NewTask.LOAD_SONGS:
                case NewTask.ADD_TO_QUEUE:
                case NewTask.ADD_TO_QUEUE_AS_NEXT:
                case NewTask.SEARCH_LISTVIEW:
                    play.IsEnabled = true;
                    addToQueue.IsEnabled = true;

                    if (CurrentPlayState == PlayState.Stopped)
                        addToQueueAsNext.IsEnabled = false;
                    else
                        addToQueueAsNext.IsEnabled = true;

                    removeFromQueue.IsEnabled = false;
                    break;
            }
        }

        private void AddToQueue(TrackInfo sender)
        {
            CurrentTask = NewTask.ADD_TO_QUEUE;
            Worker.DoWork(CurrentTask, sender);
        }

        private void AddToQueueAsNext(TrackInfo sender)
        {
            CurrentTask = NewTask.ADD_TO_QUEUE_AS_NEXT;
            Worker.DoWork(CurrentTask, sender);
        }

        private void RemoveFromQueue(TrackInfo sender)
        {
            CurrentTask = NewTask.REMOVE_FROM_QUEUE;
            Worker.DoWork(CurrentTask, sender);
        }
    }
}
