using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using bossdoyKaraoke_NOW.FormControl;
using bossdoyKaraoke_NOW.Graphic;
using bossdoyKaraoke_NOW.Media;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using D2D = SharpDX.Direct2D1;

namespace bossdoyKaraoke_NOW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Player _player;
        public  VideoImage _videoImage;

        public MainWindow()
        {
            LoadInBackground();
            InitializeComponent();
            _player.AppMainWindowHandle = new WindowInteropHelper(this).Handle;
            _videoImage = new VideoImage();
            main_video_screen.Child = _videoImage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _videoImage.Dispose();
            DeviceManager.Instance.Dispose();
        }

        private void LoadInBackground()
        {
            _player = Player.Instance;
            App.SplashScreen.LoadComplete();
        }
    }
}
