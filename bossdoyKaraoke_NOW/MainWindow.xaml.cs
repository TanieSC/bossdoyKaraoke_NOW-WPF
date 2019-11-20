using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using bossdoyKaraoke_NOW.Misc;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using static bossdoyKaraoke_NOW.Misc.GlobalHotkeyService;
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
        private VideoImage _videoImage;
        //GlobalHotkeyService _ctlO;

        public MainWindow()
        {
            InitInBackground();
            InitializeComponent();
            _player.AppMainWindowHandle = new WindowInteropHelper(this).Handle;
            _videoImage = new VideoImage();
            main_video_screen.Child = _videoImage;

         //   if (_ctlO != null) _ctlO.Dispose();

         // _ctlO = new GlobalHotkeyService(Key.O, KeyModifier.Ctrl, OnHotKeyHandler);

         //   if (_ctlO != null) _ctlO.Dispose();

        }

        private void OnHotKeyHandler(GlobalHotkeyService hotKey)
        {
            if (hotKey.KeyModifiers == KeyModifier.Ctrl)
            {
                switch (hotKey.Key)
                {
                    case Key.O:
                        Console.WriteLine("Key O");
                        break;
                    case Key.A:

                        break;
                    case Key.P:

                        break;
                    case Key.E:

                        break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _videoImage.Dispose();
            DeviceManager.Instance.Dispose();

            //Restore Previous Settings, ie, Go To Sleep Again
            SystemState.RestoreDisplaySettings();

        }

        private void InitInBackground()
        {
            _player = Player.Instance;
            App.SplashScreen.LoadComplete();
            //Prevent system sleep
            SystemState.KeepDisplayActive();
        }

        //[DllImport("user32.dll")]
        //private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        //[DllImport("user32.dll")]
        //private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        //private const int HOTKEY_ID = 9000;

        ////Modifiers:
        //private const uint MOD_NONE = 0x0000; //(none)
        //private const uint MOD_ALT = 0x0001; //ALT
        //private const uint MOD_CONTROL = 0x0002; //CTRL
        //private const uint MOD_SHIFT = 0x0004; //SHIFT
        //private const uint MOD_WIN = 0x0008; //WINDOWS
        ////CAPS LOCK:
        //private const uint VK_CAPITAL = 0x14;

        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _source = PresentationSource.FromVisual(this) as HwndSource;
            _source.AddHook(WndProc);

           // RegisterHotKey(_source.Handle, HOTKEY_ID, MOD_CONTROL, VK_CAPITAL); //CTRL + CAPS_LOCK
           // RegisterHotKey(_source.Handle, 1, MOD_CONTROL, 0x45);
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

            //const int WM_HOTKEY = 0x0312;
            //switch (msg)
            //{
            //    case WM_HOTKEY:
            //        switch (wParam.ToInt32())
            //        {
            //            case HOTKEY_ID:
            //                int vkey = (((int)lParam >> 16) & 0xFFFF);
            //                if (vkey == VK_CAPITAL)
            //                {
            //                    Console.WriteLine("CapsLock was pressed");
            //                }
            //                handled = true;
            //                break;
            //            case 1:
            //                int vkeys = (((int)lParam >> 16) & 0xFFFF);
            //                if (vkeys == 0x45)
            //                {
            //                    Console.WriteLine("E was pressed");
            //                }
            //                    break;
            //        }
            //        break;
            //}

            return IntPtr.Zero;
        }


        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(WndProc);
           // UnregisterHotKey(_source.Handle, HOTKEY_ID);
            _source.Dispose();
            base.OnClosed(e);
        }
    }
}
