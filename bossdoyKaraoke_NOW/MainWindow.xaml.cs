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
        private List<Window> _fullScreen = new List<Window>();

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

        private void UpdateFullScreenCount()
        {
            if (_fullScreen != null)
                full_screen_count.Badge = _fullScreen.Count();
            else
                full_screen_count.Badge = "0";
        }

        private void dual_Screen_Click(object sender, RoutedEventArgs e)
        {
            //Thread thread = new Thread(() =>
            //{
            FullScreen fullScreen = new FullScreen();

            _fullScreen.Add(fullScreen);

            //    // Create our context, and install it:
            //    SynchronizationContext.SetSynchronizationContext(
            //        new DispatcherSynchronizationContext(
            //            Dispatcher.CurrentDispatcher));

            fullScreen.Loaded += (sender1, e1) =>
             {
                 // full_screen_count.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(UpdateFullScreenCount));
                 UpdateFullScreenCount();
             };
            fullScreen.Closed += (sender2, e2) =>
             {
                 //fullScreen.Dispatcher.InvokeShutdown();
                 _fullScreen.Remove(fullScreen);
                 UpdateFullScreenCount();
                 //  full_screen_count.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ThreadStart(UpdateFullScreenCount));
             };

                fullScreen.Show();
                fullScreen.Activate(); 

            //    Dispatcher.Run();

            //});

            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
        }
    }

    //public class VideoImage : D2dImageSource
    //{
    //    private static Player _player;
    //    private D2D.Bitmap bmp;
    //    private D2D.Bitmap _cdgbmp;
    //    private D2D.Bitmap1 _cdgTarget;
    //    private RawRectangleF _videoBitmapRectangle;
    //    private RawRectangleF _destCdgRectangle;
    //    private D2D.Effects.Shadow _shadowEffects;
    //    private D2D.Effects.AffineTransform2D _affineTransformEffect;
    //    private D2D.Effects.Composite _compositeEffect;

    //    private ISongsSource _songsSource = SongsSource.Instance;
    //    long cdgLength;
    //    DateTime startTime = DateTime.Now;
    //    DateTime endTime;
    //    long millisecondsRemaining;
 
    //    public VideoImage()
    //    {
    //        _player = Player.Instance;
    //        string s1 = @"D:\Downloads\SunFly Karaoke\Filipino Karaoke\FLIPN976-01 - Banawa, Carol - Iingatan Ka\FLIPN976-01 - Banawa, Carol - Iingatan Ka.cdg";
    //        string s2 = @"D:\New folder (2) - Copy\02 WAITING FOR TONIGHT-JENNIFER LOPEZ.CDG";

    //        _player.LoadCDGFile(s2);

    //        cdgLength = _player.CDGmp3.getTotalDuration();
    //        endTime = startTime.AddMilliseconds(_player.CDGmp3.getTotalDuration());
    //        millisecondsRemaining = cdgLength;
    //    }

    //    public override void Render()
    //    {
    //        millisecondsRemaining = (long)endTime.Subtract(DateTime.Now).TotalMilliseconds;
    //        long pos = cdgLength - millisecondsRemaining;
    //        _player.CDGmp3.renderAtPosition(pos);


    //        LoadResources();

    //        var width = this.Width / 1.5f;
    //        var height = this.Height / 1.5f;
    //        var left = (this.Width / 2) - (width / 2);
    //        var top = this.Height - height;
    //        var right = (this.Width / 2) + (width / 2);
    //        var bottom = this.Height;
    //        var _cdgBitmapRectangle = new RawRectangleF(left, top, right, bottom);

    //        RenderContext.CdgContext.Target = _cdgTarget;
    //        RenderContext.CdgContext.BeginDraw();
    //        RenderContext.CdgContext.Clear(SharpDX.Color.Transparent);
    //        RenderContext.CdgContext.DrawBitmap(_cdgbmp, _cdgBitmapRectangle, 1.0f, D2D.BitmapInterpolationMode.Linear);
    //        RenderContext.CdgContext.EndDraw();


    //        RenderContext.VideoContext.BeginDraw();
    //        RenderContext.VideoContext.Clear(SharpDX.Color.Transparent);
  
    //         RenderContext.VideoContext.DrawBitmap(bmp, _videoBitmapRectangle, 1.0f, D2D.BitmapInterpolationMode.Linear);
    //         RenderContext.VideoContext.DrawImage(_compositeEffect, new RawVector2(0, 0), D2D.InterpolationMode.Linear, D2D.CompositeMode.Xor);
    //         RenderContext.VideoContext.DrawBitmap(_cdgTarget, 1f, D2D.BitmapInterpolationMode.Linear);
    //         RenderContext.VideoContext.EndDraw();

    //        UnloadResources();

    //    }

    //    private void LoadResources()
    //    {
    //        var ff = this.Parent.Controls;
    //        var fff = ff[0].Parent;
    //        // _videoBitmapRectangle = new RawRectangleF((this.Parent.Width / 2) - (390 / 2), this.Top, (this.Parent.Width / 2) + (390 / 2), this.Height);
    //        _videoBitmapRectangle = new RawRectangleF(0, 0, this.Width, this.Height);

    //        byte[] bmpBytes = _player.VlcPlayer.ByteArrayBitmap;
    //        bmp = GraphicUtil.ConvertToSharpDXBitmap(RenderContext.VideoContext, bmpBytes, _player.VlcPlayer.VideoWidth, _player.VlcPlayer.VideoHeight);

    //        _cdgbmp = GraphicUtil.ConvertToSharpDXBitmap(RenderContext.CdgContext, _player.CDGmp3.RGBImage);
    //        _cdgTarget = new D2D.Bitmap1(RenderContext.CdgContext, new Size2((int)_videoBitmapRectangle.Right, (int)_videoBitmapRectangle.Bottom),  GraphicUtil.BitmapProps1);

    //        //// Create image shadow effect
    //         _shadowEffects = new D2D.Effects.Shadow(RenderContext.CdgContext);
    //        _shadowEffects.SetInput(0, _cdgTarget, true);

    //        // Create image transform effect
    //        _affineTransformEffect = new D2D.Effects.AffineTransform2D(RenderContext.CdgContext);
    //        _affineTransformEffect.SetInputEffect(0, _shadowEffects);
    //        _affineTransformEffect.TransformMatrix = Matrix3x2.Translation(0, 0);
    //        // Create composite effect
    //        _compositeEffect = new D2D.Effects.Composite(RenderContext.CdgContext);
    //        _compositeEffect.InputCount = 2;
    //        _compositeEffect.SetInputEffect(0, _shadowEffects);
    //        _compositeEffect.SetInputEffect(1, _affineTransformEffect);
    //        _compositeEffect.SetInput(2, _cdgTarget, true);
    //    }

    //    private void UnloadResources()
    //    {
    //        bmp.Dispose();
    //        _cdgbmp.Dispose();
    //        _cdgTarget.Dispose();
    //        _shadowEffects.Dispose();
    //        _affineTransformEffect.Dispose();
    //        _compositeEffect.Dispose();
    //    }

    //    public override void D2dImageSource_Disposed(object sender, EventArgs e)
    //    {
    //        if (this.RenderContext != null)
    //        {
    //            this.RenderContext.Dispose();
    //            this.RenderContext = null;
    //        }
    //    }

    //}
}
