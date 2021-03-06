﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorkerEnum;
using static bossdoyKaraoke_NOW.Enums.KaraokeNowFiles;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class TreeViewVModel : ITreeViewVModel, INotifyPropertyChanged
    {
        private string _title;
        private const int _favoritesIndex = 1;
        private const int _myComputerIndex = 2;
        private Color color = (Color)ColorConverter.ConvertFromString("#DD000000");
        private ICommand _loaded;
        private ICommand _contextMenuLoaded;
        private ICommand _selectionChangedCommand;
        private ICommand _createFavoritesCommand;
        private ICommand _addFavoritesToSongQueueCommand;
        private ICommand _createFavoritesPlayedSongsCommand;
        private ICommand _removeTreeViewItemCommand;
        private ICommand _emptyQueueCommand;

        private ISongsSource _songsSource = SongsSource.Instance;
        private static TreeViewItem _selectedItem = new TreeViewItem();
        public ObservableCollection<ITreeViewModelChild> Items { get; set; }
        public List<ITreeViewVModel> ItemSource { get { return _songsSource.ItemSource; } }
        public PackIconKind PackIconKind { get; set; }
        public SolidColorBrush Foreground { get; set; }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        //public string Name { get; set; }
        //public int ID { get; set; }
        public NewTask CurrentTask { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public TreeViewVModel()
        {
            Items = new ObservableCollection<ITreeViewModelChild>();
        }

        public ICommand Loaded
        {
            get
            {
                return _loaded ?? (_loaded = new RelayCommand(x =>
                {
                    Worker.TreeViewElement = x as TreeView;

                    //This will automatically play the song in SongQueue if queue is not empty.
                    if (_songsSource.SongQueueCount > 0)
                    {
                        TreeViewDialogVModel.Instance.DialogStatus = "Song Queue (0-[0.00:00:00]";
                        TreeViewDialogVModel.Instance.AddingStatus = Visibility.Collapsed;
                        TreeViewDialogVModel.Instance.LoadingStatus = Visibility.Visible;
                        TreeViewDialogVModel.Instance.ShowDialog = true;
                        Worker.DoWork(NewTask.LOAD_QUEUE_SONGS, _songsSource.SongsQueue[0]);
                    }
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

        public ICommand SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommand(x =>
                {
                    LoadSelectedItem(x as ITreeViewVModel);
                }));
            }
        }

        public ICommand RemoveTreeViewItemCommand
        {
            get
            {
                return _removeTreeViewItemCommand ?? (_removeTreeViewItemCommand = new RelayCommand(x =>
                {
                    RemoveTreeViewItem(x as ITreeViewModelChild);
                }));
            }
        }

        private void EnableDisableMenuItem(ContextMenu sender)
        {
            var contextMenu = sender as ContextMenu;
            var emptyQueue = contextMenu.Items[0] as MenuItem;
            var shuffle = contextMenu.Items[1] as MenuItem;
            var favorites = contextMenu.Items[2] as MenuItem;
            var addFavorites = favorites.Items[0] as MenuItem;
            var createFavorites = favorites.Items[1] as MenuItem;
            var createFavoritesPlayedSong = favorites.Items[2] as MenuItem;
            var remove = contextMenu.Items[4] as MenuItem;

            var parent = contextMenu.DataContext as ITreeViewVModel;
            var child = contextMenu.DataContext as ITreeViewModelChild;
            var currentTask = CurrentTask;

            if (parent != null)
                currentTask = ((ITreeViewVModel)contextMenu.DataContext).CurrentTask;
            else
                currentTask = ((ITreeViewModelChild)contextMenu.DataContext).CurrentTask;

            //var currentTask = contextMenu.DataContext as ITreeViewModel != null
            //    ? ((ITreeViewModel)contextMenu.DataContext).CurrentTask
            //    : ((ITreeViewModelChild)contextMenu.DataContext).CurrentTask;

            switch (currentTask)
            {
                case NewTask.LOAD_QUEUE_SONGS:
                    if (_songsSource.SongQueueCount > 0)
                    {
                        emptyQueue.IsEnabled = true;
                        shuffle.IsEnabled = true;
                    }
                    else
                    {
                        emptyQueue.IsEnabled = false;
                        shuffle.IsEnabled = false;
                    }
                    favorites.IsEnabled = false;
                    remove.IsEnabled = false;
                    break;
                case NewTask.LOAD_FAVORITES:

                    if (parent != null)
                    {
                        emptyQueue.IsEnabled = false;
                        favorites.IsEnabled = false;
                        shuffle.IsEnabled = false;
                        addFavorites.IsEnabled = false;
                        createFavorites.IsEnabled = false;
                        createFavoritesPlayedSong.IsEnabled = false;
                        remove.IsEnabled = false;
                        return;
                    }

                    emptyQueue.IsEnabled = false;

                    if (_songsSource.Favorites != null)
                    {
                        if (_songsSource.Favorites.Count > 0)
                            shuffle.IsEnabled = true;
                        else
                            shuffle.IsEnabled = false;
                    }
                    else
                        shuffle.IsEnabled = false;

                    favorites.IsEnabled = true;
                    addFavorites.IsEnabled = true;
                    createFavorites.IsEnabled = false;

                    if (_songsSource.PlayedSongsCount > 0)
                        createFavoritesPlayedSong.IsEnabled = true;
                    else
                        createFavoritesPlayedSong.IsEnabled = false;

                    remove.IsEnabled = true;
                    break;
                case NewTask.LOAD_SONGS:

                    if (parent != null)
                    {
                        emptyQueue.IsEnabled = false;
                        favorites.IsEnabled = false;
                        shuffle.IsEnabled = false;
                        addFavorites.IsEnabled = false;
                        createFavorites.IsEnabled = false;
                        createFavoritesPlayedSong.IsEnabled = false;
                        remove.IsEnabled = false;
                        return;
                    }

                    emptyQueue.IsEnabled = false;

                    if (_songsSource.Songs.Count > 0)
                        shuffle.IsEnabled = true;
                    else
                        shuffle.IsEnabled = false;

                    favorites.IsEnabled = true;
                    addFavorites.IsEnabled = false;
                    createFavorites.IsEnabled = true;
                    createFavoritesPlayedSong.IsEnabled = false;
                    remove.IsEnabled = true;
                    break;
            }
        }

        private void LoadSelectedItem(ITreeViewVModel sender)
        {
            Worker.DoWork(sender.CurrentTask);
        }

        public ICommand CreateFavoritesCommand
        {
            get
            {
                return _createFavoritesCommand ?? (_createFavoritesCommand = new RelayCommand(x =>
                {
                    CreateFavorites(x as ITreeViewModelChild);
                }));
            }
        }

        public ICommand AddFavoritesToSongQueueCommand
        {
            get
            {
                return _addFavoritesToSongQueueCommand ?? (_addFavoritesToSongQueueCommand = new RelayCommand(x =>
                {
                    CurrentTask = NewTask.ADD_FAVORITES_TO_QUEUE;
                    var id = (x as ITreeViewModelChild).ID;
                    TreeViewDialogVModel.Instance.DialogStatus = "Song Queue (0-[0.00:00:00]";
                    TreeViewDialogVModel.Instance.AddingStatus = Visibility.Collapsed;
                    TreeViewDialogVModel.Instance.LoadingStatus = Visibility.Visible;
                    TreeViewDialogVModel.Instance.ShowDialog = true;
                    Worker.DoWork(CurrentTask, id);
                }));
            }
        }

        /// <summary>
        /// Method to create favorites from played songs 
        /// </summary>
        public ICommand CreateFavoritesPlayedSongsCommand
        {
            get
            {
                return _createFavoritesPlayedSongsCommand ?? (_createFavoritesPlayedSongsCommand = new RelayCommand(x =>
                {
                    CurrentTask = NewTask.ADD_PLAYEDSONGS_TO_FAVORITES;
                    var sender = x as ITreeViewModelChild;
                    Worker.DoWork(CurrentTask, sender);
                }));
            }
        }

        private void RemoveTreeViewItem(ITreeViewModelChild sender)
        {
            if (sender != null)
            {
                if (sender.CurrentTask == NewTask.LOAD_FAVORITES)
                {
                    CurrentTask = NewTask.REMOVE_FAVORITES;
                    Worker.DoWork(CurrentTask, sender);
                }
                else if (sender.CurrentTask == NewTask.LOAD_SONGS)
                {
                    CurrentTask = NewTask.REMOVE_SONGS;
                    Worker.DoWork(CurrentTask, sender);
                }
            }
        }

        public ICommand EmptyQueueCommand
        {
            get
            {
                return _emptyQueueCommand ?? (_emptyQueueCommand = new RelayCommand(x =>
                {
                    EmptyQueue();
                }));
            }
        }

        private void EmptyQueue()
        {
            if (_songsSource.SongQueueCount > 0)
            {
                CurrentTask = NewTask.EMPTY_QUEUE_LIST;
                Worker.DoWork(CurrentTask);
            }
        }

        /// <summary>
        /// Method to create favorites from collections
        /// </summary>
        /// <param name="sender">Index of the selected treeview item</param>
        private void CreateFavorites(ITreeViewModelChild sender)
        {
            if (sender != null)
            {
                var items = SongsSource.Instance.ItemSource[_favoritesIndex].Items;
                var favorites = SongsSource.Instance.Favorites != null ? SongsSource.Instance.Favorites.Count : items.Count - 1;
                var filaname = SongsSource.Instance.CheckFileNameExist(Create.Favorites, sender.Title);

                //Console.WriteLine("F: " + filaname + " " + sender.Title);

                items.Insert(0, new TreeViewModelChild() { PackIconKind = PackIconKind.Favorite, Foreground = new SolidColorBrush(color), Title = filaname, ID = favorites, IsProgressVisible = Visibility.Hidden, CurrentTask = NewTask.LOAD_FAVORITES });

                sender.Title = filaname;
                Worker.DoWork(NewTask.ADD_NEW_FAVORITES, sender);
            }
        }
    }

    class TreeViewModelChild : ITreeViewModelChild, INotifyPropertyChanged
    {
        private ISongsSource _songsSource = SongsSource.Instance;
        private const int _favoritesIndex = 1;
        private const int _myComputerIndex = 2;
        private Color color = (Color)ColorConverter.ConvertFromString("#DD000000");
        private ICommand _selectionChangedCommand;
        private Visibility _isProgressVisible;
        public PackIconKind PackIconKind { get; set; }
        public SolidColorBrush Foreground { get; set; }
        public string Title { get; set; }
        public int ID { get; set; }
        public Visibility IsProgressVisible
        {
            get
            {
                return _isProgressVisible;
            }
            set
            {
                _isProgressVisible = value;
                OnPropertyChanged();
            }
        }
        public NewTask CurrentTask { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommand(x =>
                {
                    LoadSelectedSongs(x as ITreeViewModelChild);
                }));
            }
        }

        private void LoadSelectedSongs(ITreeViewModelChild sender)
        {

            if (sender.CurrentTask == NewTask.ADD_NEW_FAVORITES)
            {
                TreeViewDialogVModel.Instance.AddingStatus = Visibility.Visible;
                TreeViewDialogVModel.Instance.LoadingStatus = Visibility.Collapsed;
                TreeViewDialogVModel.Instance.ShowDialog = true;
                TreeViewDialogVModel.Instance.AddFavoritesSender = sender;
            }
            else if (sender.CurrentTask == NewTask.ADD_NEW_SONGS)
            {
                _songsSource.AddNewSongs(sender);
            }
            else
            {
                Worker.DoWork(sender.CurrentTask, sender);
            }
        }
    }
}
