using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace bossdoyKaraoke_NOW.Graphic
{
    class OutlineTextRender : CallbackBase, TextRenderer
    {
        readonly SharpDX.Direct2D1.Factory _d2DFactory;
        readonly SharpDX.Direct2D1.DeviceContext _renderTarget;

        public OutlineTextRender(SharpDX.Direct2D1.Factory d2DFactory, SharpDX.Direct2D1.DeviceContext renderTarget)
        {
            _d2DFactory = d2DFactory;
            _renderTarget = renderTarget;
        }

        public Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
        {
            var pathGeometry = new PathGeometry(_d2DFactory);
            var geometrySink = pathGeometry.Open();

            var fontFace = glyphRun.FontFace;

            if (glyphRun.Indices.Length > 0)
                fontFace.GetGlyphRunOutline(glyphRun.FontSize, glyphRun.Indices, glyphRun.Advances, glyphRun.Offsets, glyphRun.Indices.Length, glyphRun.IsSideways, glyphRun.BidiLevel % 2 != 0, geometrySink);
            geometrySink.Close();
            geometrySink.Dispose();
            fontFace.Dispose();

            var matrix = new Matrix3x2()
            {
                M11 = 1,
                M12 = 0,
                M21 = 0,
                M22 = 1,
                M31 = baselineOriginX,
                M32 = baselineOriginY
            };

            var transformedGeometry = new TransformedGeometry(_d2DFactory, pathGeometry, matrix);

            var brushOuterColor = (Color4)Color.Green;
            var brushInnerColor = (Color4)Color.White;
            var brushTextColor = (Color4)Color.Purple;

            var brushOuter = new SolidColorBrush(_renderTarget, brushOuterColor);
            var brushInner = new SolidColorBrush(_renderTarget, brushInnerColor);
            var brushText = new SolidColorBrush(_renderTarget, brushTextColor);

            _renderTarget.DrawGeometry(transformedGeometry, brushOuter, 10);
            _renderTarget.DrawGeometry(transformedGeometry, brushInner, 6);
            _renderTarget.FillGeometry(transformedGeometry, brushText);

            pathGeometry.Dispose();
            transformedGeometry.Dispose();
            brushOuter.Dispose();
            brushInner.Dispose();
            brushText.Dispose();

            return Result.Ok;
        }

        public Result DrawInlineObject(object clientDrawingContext, float originX, float originY, InlineObject inlineObject, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
        {
            throw new NotImplementedException();
        }

        public Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, ComObject clientDrawingEffect)
        {
            throw new NotImplementedException();
        }

        public Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, ComObject clientDrawingEffect)
        {
            throw new NotImplementedException();
        }

        public RawMatrix3x2 GetCurrentTransform(object clientDrawingContext)
        {
            return _renderTarget.Transform;
        }

        public float GetPixelsPerDip(object clientDrawingContext)
        {
            return _renderTarget.PixelSize.Width / 96f;
        }

        public bool IsPixelSnappingDisabled(object clientDrawingContext)
        {
            return false;
        }      
    }
}
