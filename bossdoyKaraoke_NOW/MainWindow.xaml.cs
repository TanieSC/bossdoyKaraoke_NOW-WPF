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
        private const int WM_DISPLAYCHANGE = 0x7E;
        private Player _player;
        public VideoImage _videoImage;

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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DISPLAYCHANGE)
            {
                int lparamInt = lParam.ToInt32();

                uint width = (uint)(lparamInt & 0xffff);
                uint height = (uint)(lparamInt >> 16);

                int monCount = ScreenInformation.GetMonitorCount();
               // int winFormsMonCount = System.Windows.Forms.Screen.AllScreens.Length;
            }

            return IntPtr.Zero;
        }
    }
}
