using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bossdoyKaraoke_NOW.FormControl;
using bossdoyKaraoke_NOW.Graphic;
using bossdoyKaraoke_NOW.Media;
using SharpDX;
using SharpDX.Mathematics.Interop;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace bossdoyKaraoke_NOW
{
    /// <summary>
    /// Interaction logic for FullScreen.xaml
    /// </summary>
    public partial class FullScreen : Window
    {
        private VideoImage _videoImage;

        public FullScreen()
        {
            InitializeComponent();
            _videoImage = new VideoImage();
            full_video_screen.Child = _videoImage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _videoImage.Dispose();
            Hide();
        }

    //    class VideoImage : D2dImageSource
    //    {
    //        private Player _player;
    //        //private WriteableBitmap _wbbmp;
    //        private D2D.Bitmap bmp;
    //        private D2D.Bitmap _cdgbmp;
    //        private D2D.Bitmap1 _cdgTarget;
    //        private RawRectangleF _videoBitmapRectangle;
    //        private WriteableBitmap _wbbmp;
    //        private RawRectangleF _destCdgRectangle;
    //        private D2D.Effects.BitmapSource _bitmapSourceEffect;
    //        private D2D.Effects.Shadow _shadowEffects;
    //        private D2D.Effects.AffineTransform2D _affineTransformEffect;
    //        private D2D.Effects.Composite _compositeEffect;

    //        public VideoImage()
    //        {
    //            _player = Player.Instance;
    //        }

    //        public override void Render()
    //        {

    //            LoadResources();
    //            var width = this.Width / 1.5f; ;
    //            var height = this.Height / 1.5f;
    //            var left = (this.Width / 2) - (width / 2);
    //            var top = this.Height - height;
    //            var right = (this.Width / 2) + (width / 2); ;
    //            var bottom = this.Height;
    //            var _cdgBitmapRectangle = new RawRectangleF(left, top, right, bottom);
                
    //            RenderContext.CdgContext.Target = _cdgTarget;
    //            RenderContext.CdgContext.BeginDraw();
    //            RenderContext.CdgContext.Clear(SharpDX.Color.Transparent);
    //            RenderContext.CdgContext.DrawBitmap(_cdgbmp, _cdgBitmapRectangle, 1.0f, D2D.BitmapInterpolationMode.Linear);
    //            RenderContext.CdgContext.EndDraw();


    //            RenderContext.VideoContext.BeginDraw();
    //            RenderContext.VideoContext.Clear(SharpDX.Color.Transparent);

    //            RenderContext.VideoContext.DrawBitmap(bmp, _videoBitmapRectangle, 1.0f, D2D.BitmapInterpolationMode.Linear);
    //            RenderContext.VideoContext.DrawImage(_compositeEffect, new RawVector2(0, 0), D2D.InterpolationMode.Linear, D2D.CompositeMode.Xor);
    //            RenderContext.VideoContext.DrawBitmap(_cdgTarget, 1f, D2D.BitmapInterpolationMode.Linear);
    //            RenderContext.VideoContext.EndDraw();

    //            UnloadResources();
    //        }

    //        private void LoadResources()
    //        {
    //            _videoBitmapRectangle = new RawRectangleF(0, 0, this.Width, this.Height);
    //            byte[] bmpBytes = _player.VlcPlayer.ByteArrayBitmap;
    //            bmp = GraphicUtil.ConvertToSharpDXBitmap(RenderContext.VideoContext, bmpBytes, _player.VlcPlayer.VideoWidth, _player.VlcPlayer.VideoHeight);

    //            _cdgbmp = GraphicUtil.ConvertToSharpDXBitmap(RenderContext.CdgContext, _player.CDGmp3.RGBImage);
    //            _cdgTarget = new D2D.Bitmap1(RenderContext.CdgContext, new Size2(this.Width, this.Height), GraphicUtil.BitmapProps1);

    //            //// Create image shadow effect
    //            _shadowEffects = new D2D.Effects.Shadow(RenderContext.CdgContext);
    //            _shadowEffects.SetInput(0, _cdgTarget, true);

    //            // Create image transform effect
    //            _affineTransformEffect = new D2D.Effects.AffineTransform2D(RenderContext.CdgContext);
    //            _affineTransformEffect.SetInputEffect(0, _shadowEffects);
    //            _affineTransformEffect.TransformMatrix = Matrix3x2.Translation(0, 0);
    //            // Create composite effect
    //            _compositeEffect = new D2D.Effects.Composite(RenderContext.CdgContext);
    //            _compositeEffect.InputCount = 2;
    //            _compositeEffect.SetInputEffect(0, _shadowEffects);
    //            _compositeEffect.SetInputEffect(1, _affineTransformEffect);
    //            _compositeEffect.SetInput(2, _cdgTarget, true);

    //        }

    //        private void UnloadResources()
    //        {
                
    //            bmp.Dispose();
    //            _cdgbmp.Dispose();
    //            _cdgTarget.Dispose();
    //            _shadowEffects.Dispose();
    //            _affineTransformEffect.Dispose();
    //            _compositeEffect.Dispose();
    //        }

    //        public override void D2dImageSource_Disposed(object sender, EventArgs e)
    //        {
    //            if (this.RenderContext != null)
    //            {
    //                UnloadResources();
    //                //this.RenderContext.Dispose();
    //                //this.RenderContext.VideoContext.Dispose();
    //                //this.RenderContext.CdgContext.Dispose();
    //                //this.RenderContext.DWFactory.Dispose();
    //            }
    //        }
    //    }
    }
}
