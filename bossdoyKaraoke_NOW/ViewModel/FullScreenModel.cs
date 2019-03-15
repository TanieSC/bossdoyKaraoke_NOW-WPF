using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using bossdoyKaraoke_NOW.FormControl;
using bossdoyKaraoke_NOW.Interactivity;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class FullScreenModel : IFullScreenModel
    {
        private Window _fullScreen;
        private VideoImage _videoImage;
        private ICommand _closingCommmand;
        private ICommand _loadedCommmand;
        private ICommand _sizeChangedCommmand;

        public FullScreenModel()
        {
        }

        public ICommand LoadedCommmand
        {
            get
            {
                return _loadedCommmand ?? (_loadedCommmand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _videoImage = new VideoImage(true);
                        _fullScreen = x as Window;
                        ((_fullScreen.Content as Grid).Children[0] as WindowsFormsHost).Child = _videoImage;

                        _videoImage.MouseDoubleClick += _videoImage_MouseDoubleClick;

                        // AutoSizeWindow();
                        _fullScreen.Topmost = true;
                    }
                }));
            }
        }

        public ICommand SizeChangedCommmand
        {
            get
            {
                return _sizeChangedCommmand ?? (_sizeChangedCommmand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        if (_fullScreen.WindowState == WindowState.Maximized)
                        {
                            _fullScreen.WindowStyle = WindowStyle.None;
                            _fullScreen.Topmost = true;
                        }
                        else
                            _fullScreen.WindowStyle = WindowStyle.SingleBorderWindow;
                    }
                }));
            }
        }

        public ICommand ClosingCommmand
        {
            get
            {
                return _closingCommmand ?? (_closingCommmand = new RelayCommand(x =>
                {
                    if (x != null)
                    {
                        _videoImage.MouseDoubleClick -= _videoImage_MouseDoubleClick;
                        _videoImage.Dispose();
                        _fullScreen.Hide();
                    }
                }));
            }
        }

        private void _videoImage_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            AutoSizeWindow();
        }

        private void AutoSizeWindow()
        {
            if (_fullScreen.WindowState == WindowState.Normal)
            {
                _fullScreen.WindowStyle = WindowStyle.None;
                _fullScreen.WindowState = WindowState.Maximized;
                _fullScreen.Topmost = true;
            }
            else
            {
                _fullScreen.WindowState = WindowState.Normal;
                _fullScreen.Topmost = true;
                _fullScreen.Width = 500;
                _fullScreen.Height = 350;
            }
        }
    }
}
