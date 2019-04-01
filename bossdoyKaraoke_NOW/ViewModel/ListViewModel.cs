using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private ICommand _addToQueueDblClkCommand;// _previewMouseDoubleClick;
        private ICommand _loadedCommand;
        private ICommand _contextMenuLoadedCommand;
        private ICommand _addToQueueCommand;
        private ICommand _addToQueueAsNextCommand;
        private ICommand _removeCommand;

        public ObservableCollection<TrackInfo> Items { get; private set; }

        public ListViewModel()
        {
            Items =  _songsSource.Songs.Count > 0 ? _songsSource.Songs[0] : new ObservableCollection<TrackInfo>();
        }

        public ICommand AddToQueueDblClkCommand// PreviewMouseDoubleClick
        {
            get
            {
                return _addToQueueDblClkCommand ?? (_addToQueueDblClkCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        AddToQueue(x as TrackInfo);
                    }
                }));
            }
        }

        public ICommand LoadedCommand
        {
            get
            {
                return _loadedCommand ?? (_loadedCommand = new RelayCommand(x =>
                {
                    Worker.ListViewElement = x as ListView;
                }));
            }
        }

        public ICommand ContextMenuLoadedCommand
        {
            get
            {
                return _contextMenuLoadedCommand ?? (_contextMenuLoadedCommand = new RelayCommand(x =>
                {
                    if (x != null)
                        EnableDisableMenuItem(x as ContextMenu);
                }));
            }
        }

        public ICommand AddToQueueCommand
        {
            get
            {
                return _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(x =>
                {
                    if (x != null)
                        AddToQueue(x as TrackInfo);
                }));
            }
        }

        public ICommand AddToQueueAsNextCommand
        {
            get
            {
                return _addToQueueAsNextCommand ?? (_addToQueueAsNextCommand = new RelayCommand(x =>
                {
                    if (x != null)
                        AddToQueueAsNext(x as TrackInfo);
                }));
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        RemoveSong(x as TrackInfo);
                    }
                }));
            }
        }

        private void EnableDisableMenuItem(ContextMenu sender)
        {
            var contextMenu = sender as ContextMenu;
           // var play = contextMenu.Items[0] as MenuItem;
            var addToQueue = contextMenu.Items[0] as MenuItem;
            var addToQueueAsNext = contextMenu.Items[1] as MenuItem;
            var removeFromQueue = contextMenu.Items[3] as MenuItem;

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
                   // play.IsEnabled = true;
                    addToQueue.IsEnabled = true;

                    if (CurrentPlayState == PlayState.Stopped)
                        addToQueueAsNext.IsEnabled = false;
                    else
                        addToQueueAsNext.IsEnabled = true;

                   // removeFromQueue.IsEnabled = false;
                    break;
            }
        }

        private void AddToQueue(TrackInfo sender)
        {
            CurrentTask = NewTask.ADD_TO_QUEUE;
            sender.IsSelected = true;
            Worker.DoWork(CurrentTask, sender);
        }

        private void AddToQueueAsNext(TrackInfo sender)
        {
            CurrentTask = NewTask.ADD_TO_QUEUE_AS_NEXT;
            sender.IsSelected = true;
            Worker.DoWork(CurrentTask, sender);
        }

        private void RemoveSong(TrackInfo sender)
        {
            if (CurrentTask == NewTask.LOAD_QUEUE_SONGS)
            {
                //CurrentTask = NewTask.REMOVE_FROM_QUEUE;
                Worker.DoWork(NewTask.REMOVE_FROM_QUEUE, sender);
            }
            else if (CurrentTask == NewTask.LOAD_FAVORITES)
            {
                //CurrentTask = NewTask.REMOVE_SELECTED_FAVORITE;
                Worker.DoWork(NewTask.REMOVE_SELECTED_FAVORITE, sender);
            }
            else if (CurrentTask == NewTask.LOAD_SONGS)
            {
                //CurrentTask = NewTask.REMOVE_SELECTED_SONG;
                Worker.DoWork(NewTask.REMOVE_SELECTED_SONG, sender);
            }
            
            //CurrentTask = NewTask.REMOVE_FROM_QUEUE;
            //Worker.DoWork(CurrentTask, sender);
        }
    }
}