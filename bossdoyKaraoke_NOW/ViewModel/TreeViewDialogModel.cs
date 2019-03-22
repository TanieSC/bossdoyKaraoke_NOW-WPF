﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Media;
using MaterialDesignThemes.Wpf;
using static bossdoyKaraoke_NOW.Enums.BackGroundWorker;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class TreeViewDialogModel : ITreeViewDialogModel, INotifyPropertyChanged
    {
        private static TreeViewDialogModel _instance;
        private TextBox _favoritesTitle;
        private string _dialogStatus;
        private bool _showDialog;
        private bool _acceptEnabled;
        private Color color = (Color)ColorConverter.ConvertFromString("#DD000000");
        private Visibility _addingStatus = Visibility.Collapsed;
        private Visibility _loadingStatus = Visibility.Collapsed;
        private ICommand _addFavoritesTitleCommand;
        private ICommand _acceptCommand;

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

        public bool AcceptEnabled {
            get
            {
                return _acceptEnabled;
            }
            private set
            {
                _acceptEnabled = value;
                OnPropertyChanged();
            }
        }

        public ITreeViewModelChild AddFavoritesSender { get; set; }

        public Visibility AddingStatus
        {
            get
            {
                return _addingStatus;
            }

            set
            {
                _addingStatus = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadingStatus
        {
            get
            {
                return _loadingStatus;
            }

            set
            {
                _loadingStatus = value;
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

        public ICommand AddFavoritesTitleCommand
        {
            get
            {
                return _addFavoritesTitleCommand ?? (_addFavoritesTitleCommand = new RelayCommand(x =>
                {
                    _favoritesTitle = x as TextBox;

                    if (!string.IsNullOrWhiteSpace(_favoritesTitle.Text))
                    {
                        AcceptEnabled = true;
                    }
                    else
                    {
                        AcceptEnabled = false;
                    }

                }));
            }
        }

        public ICommand AcceptCommand
        {
            get
            {
                return _acceptCommand ?? (_acceptCommand = new RelayCommand(x =>
                {
                    if (!(bool)x) return;

                    var favoritesIndex = 1;
                    var items = SongsSource.Instance.ItemSource[favoritesIndex].Items;
                    var favorites = SongsSource.Instance.Favorites != null ? SongsSource.Instance.Favorites.Count : items.Count - 1;

                    //for(int i = 0; i < items.Count; i++)
                    //{
                    //    if (_favoritesTitle.Text == items[i].Title)
                    //    {
                    //        _favoritesTitle.Text = _favoritesTitle.Text + " " + (i + 1).ToString();
                    //    } 
                    //}
                    //int n = 0;
                    //string newName = "";
                    //for (int i = 0; i < items.Count; i++)
                    //{
                    //    do
                    //    {
                    //        n++;
                    //        newName = string.Format("{0}({1})", _favoritesTitle.Text, n);
                    //    }
                    //    while (newName == items[i].Title);
                    //}

                    items.Insert(0, new TreeViewModelChild() { PackIconKind = PackIconKind.Favorite, Foreground = new SolidColorBrush(color), Title = newName, ID = favorites, IsProgressVisible = Visibility.Hidden, CurrentTask = NewTask.LOAD_FAVORITES });
                    _favoritesTitle.Text = string.Empty;
                    Worker.DoWork(AddFavoritesSender.CurrentTask, AddFavoritesSender);

                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
