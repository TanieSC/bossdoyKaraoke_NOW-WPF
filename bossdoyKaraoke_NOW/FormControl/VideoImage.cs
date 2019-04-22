using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using bossdoyKaraoke_NOW.Graphic;
using bossdoyKaraoke_NOW.Media;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using static bossdoyKaraoke_NOW.Enums.PlayerState;
using D2D = SharpDX.Direct2D1;

namespace bossdoyKaraoke_NOW.FormControl
{
    public class VideoImage : D2dImageSource
    {
        private struct StringSize
        {
            public float Width;
            public float Height;
        }

        private static Player _player;
        private D2D.Bitmap _bmp;
        private D2D.Bitmap _cdgbmp;
        private D2D.Bitmap1 _cdgTarget;
        private RawRectangleF _videoBitmapRectangle;
        private D2D.RoundedRectangle _roundedRecReserve;
        private D2D.RoundedRectangle _roundedRecNextSong;
        private D2D.SolidColorBrush _textBrush;
        private D2D.SolidColorBrush _roundedRecOutColor;
        private D2D.SolidColorBrush _roundedRecInColor;
        private TextFormat _textFormat10;
        private TextFormat _textFormat15;
        private TextLayout _textLayout;
        private D2D.Effects.Shadow _shadowEffects;
        private D2D.Effects.AffineTransform2D _affineTransformEffect;
        private D2D.Effects.Composite _compositeEffect;
        private ISongsSource _songsSource = SongsSource.Instance;

        private bool _isFullScreen;
        private float _fontSize10 = 10f;
        private float _fontSize15 = 15f;
        private float _fontSize30 = 30f;

        public VideoImage(bool isFullScreen = false)
        {
            _player = Player.Instance;
            _isFullScreen = isFullScreen;
        }

        public override void Render()
        {
            LoadResources();

            _player.DbLevel();

            if (_player.IsPlayingBass)
            {
                if (_player.CDGmp3 != null)
                {
                    var cdgbmp = _player.CDGmp3.RGBImage as WriteableBitmap;
                    _player.CDGmp3.renderAtPosition(_player.CdgRenderAtPosition);
                    _cdgbmp = GraphicUtil.ConvertToSharpDXBitmap(RenderContext.CdgContext, cdgbmp);

                    var width = this.Width / 1.5f;
                    var height = this.Height / 1.5f;
                    var left = (this.Width / 2) - (width / 2);
                    var top = this.Height - height;
                    var right = (this.Width / 2) + (width / 2);
                    var bottom = this.Height;
                    var _cdgBitmapRectangle = new RawRectangleF(left, top, right, bottom);

                    RenderContext.CdgContext.Target = _cdgTarget;
                    RenderContext.CdgContext.BeginDraw();
                    RenderContext.CdgContext.Clear(SharpDX.Color.Transparent);
                    RenderContext.CdgContext.DrawBitmap(_cdgbmp, _cdgBitmapRectangle, 1.0f, D2D.BitmapInterpolationMode.Linear);
                    RenderContext.CdgContext.EndDraw();
                }

               // _player.DbLevel();
            }

            if (_player.IsPlayingVlc)
            {
                _player.VlcRenderAtPosition();
            }

            var nextsong = _player.GetNextTrackInfo();
            //Just guesing font size
            _fontSize10 = _videoBitmapRectangle.Bottom / 40;
            _fontSize15 = _videoBitmapRectangle.Bottom / 20;
            _fontSize30 = _videoBitmapRectangle.Bottom / 10;

            RenderContext.VideoContext.BeginDraw();
            RenderContext.VideoContext.Clear(SharpDX.Color.Transparent);

            if (_bmp != null)
            {
                RenderContext.VideoContext.DrawBitmap(_bmp, _videoBitmapRectangle, 1.0f, D2D.BitmapInterpolationMode.Linear);
            }
            else
            {
                var stringSize0 = MeasureStringDX("Video file not found!", _videoBitmapRectangle.Right, _textFormat15);
                RenderContext.VideoContext.DrawTextLayout(new Vector2((_videoBitmapRectangle.Right / 2) - (stringSize0.Width / 2), (_videoBitmapRectangle.Bottom / 2) - (stringSize0.Height / 2)), _textLayout, _textBrush);

            }

            RenderContext.VideoContext.DrawImage(_compositeEffect, new RawVector2(0, 0), D2D.InterpolationMode.Linear, D2D.CompositeMode.Xor);
            RenderContext.VideoContext.DrawBitmap(_cdgTarget, 1f, D2D.BitmapInterpolationMode.Linear);

            string reservedSong = "R".PadRight(2) + _songsSource.SongQueueCount;
            var stringSize1 = MeasureStringDX(reservedSong, _videoBitmapRectangle.Right, _textFormat15);

            _roundedRecReserve = new D2D.RoundedRectangle()
            {
                Rect = new RawRectangleF((_videoBitmapRectangle.Right - 10) - (stringSize1.Width + 10), _videoBitmapRectangle.Top + 10, _videoBitmapRectangle.Right - 10, stringSize1.Height + 10),
                RadiusX = stringSize1.Height / 8,
                RadiusY = stringSize1.Height / 8
            };

            RenderContext.VideoContext.DrawRoundedRectangle(_roundedRecReserve, _roundedRecOutColor, 10f);
            RenderContext.VideoContext.FillRoundedRectangle(_roundedRecReserve, _roundedRecInColor);
            RenderContext.VideoContext.DrawTextLayout(new Vector2(_roundedRecReserve.Rect.Left + 5, _roundedRecReserve.Rect.Top), _textLayout, _textBrush);

            if (_isFullScreen)
            {
                if (nextsong != "" || nextsong != string.Empty)
                {
                    string song = "Next song: " + nextsong;
                    var stringSize2 = MeasureStringDX(song, _videoBitmapRectangle.Right, _textFormat10);

                    _roundedRecNextSong = new D2D.RoundedRectangle()
                    {
                        Rect = new RawRectangleF(_videoBitmapRectangle.Left + 10, _videoBitmapRectangle.Top + 10, _roundedRecReserve.Rect.Left - 15, (stringSize1.Height + 10)),
                        RadiusX = stringSize2.Height / 8,
                        RadiusY = stringSize2.Height / 8
                    };

                    RenderContext.VideoContext.DrawRoundedRectangle(_roundedRecNextSong, _roundedRecOutColor, 10f);
                    RenderContext.VideoContext.FillRoundedRectangle(_roundedRecNextSong, _roundedRecInColor);
                    RenderContext.VideoContext.DrawTextLayout(new Vector2(_roundedRecNextSong.Rect.Left + 5, _roundedRecNextSong.Rect.Top + (stringSize2.Height / 1.5f)), _textLayout, _textBrush);
                }
            }

            RenderContext.VideoContext.EndDraw();

            UnloadResources();
        }

        private void LoadResources()
        {
            _videoBitmapRectangle = new RawRectangleF(0, 0, this.Width, this.Height);

            byte[] bmpBytes = _player.VlcPlayer.ByteArrayBitmap;
            _bmp = GraphicUtil.ConvertToSharpDXBitmap(RenderContext.VideoContext, bmpBytes, _player.VlcPlayer.VideoWidth, _player.VlcPlayer.VideoHeight);

            _cdgTarget = new D2D.Bitmap1(RenderContext.CdgContext, new Size2((int)_videoBitmapRectangle.Right, (int)_videoBitmapRectangle.Bottom), GraphicUtil.BitmapProps1);

            //// Create image shadow effect
            _shadowEffects = new D2D.Effects.Shadow(RenderContext.CdgContext);
            _shadowEffects.SetInput(0, _cdgTarget, true);

            // Create image transform effect
            _affineTransformEffect = new D2D.Effects.AffineTransform2D(RenderContext.CdgContext);
            _affineTransformEffect.SetInputEffect(0, _shadowEffects);
            _affineTransformEffect.TransformMatrix = Matrix3x2.Translation(0, 0);
            // Create composite effect
            _compositeEffect = new D2D.Effects.Composite(RenderContext.CdgContext);
            _compositeEffect.InputCount = 2;
            _compositeEffect.SetInputEffect(0, _shadowEffects);
            _compositeEffect.SetInputEffect(1, _affineTransformEffect);
            _compositeEffect.SetInput(2, _cdgTarget, true);

            _textBrush = new D2D.SolidColorBrush(RenderContext.VideoContext, new Color(103, 58, 183)); //new Color(128, 0, 255));
            _roundedRecOutColor = new D2D.SolidColorBrush(RenderContext.VideoContext, new Color(32, 117, 81)); //new Color(227,227,227));
            _roundedRecInColor = new D2D.SolidColorBrush(RenderContext.VideoContext, new Color(240, 240, 240)); // new Color(234, 137, 6));
            _textFormat10 = new TextFormat(RenderContext.DWFactory, "Arial", FontWeight.Bold, FontStyle.Normal, _fontSize10);
            _textFormat15 = new TextFormat(RenderContext.DWFactory, "Arial", FontWeight.UltraBold, FontStyle.Normal, _fontSize15);
 
        }

        private void UnloadResources()
        {
            if (_bmp != null)
                _bmp.Dispose();

            if (_cdgbmp != null)
                _cdgbmp.Dispose();

            _cdgTarget.Dispose();
            _shadowEffects.Dispose();
            _affineTransformEffect.Dispose();
            _compositeEffect.Dispose();
            _textBrush.Dispose();
            _roundedRecInColor.Dispose();
            _roundedRecOutColor.Dispose();
            _textFormat10.Dispose();
            _textFormat15.Dispose();
            _textLayout.Dispose();
        }

        private StringSize MeasureStringDX(string Message, float Width, TextFormat format)
        {
            _textLayout = new TextLayout(RenderContext.DWFactory, Message, format, Width, format.FontSize);            
            return new StringSize() { Width = _textLayout.Metrics.Width , Height = _textLayout.Metrics.Height };
        }

        public override void D2dImageSource_Disposed(object sender, EventArgs e)
        {
            UnloadResources();

            if (this.RenderContext != null)
            {
                this.RenderContext.Dispose();
                this.RenderContext = null;
            }
        }
    }
}
