using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpDX;
using D2D = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;

namespace bossdoyKaraoke_NOW.Graphic
{
    class GraphicUtil
    {
        private static System.Drawing.FontFamily _fontFamily;
        private static DataStream _dataStream;
        public static readonly DXGI.Format DXGIFormat = DXGI.Format.B8G8R8A8_UNorm;
        public static readonly D2D.PixelFormat D2PixelFormat = new D2D.PixelFormat(DXGIFormat, D2D.AlphaMode.Premultiplied);
        public static D2D.BitmapProperties1 BitmapProps1 = new D2D.BitmapProperties1(D2PixelFormat, 96, 96, D2D.BitmapOptions.Target);
        
        //public static byte[] BitmapToByteArray(Bitmap bitmap)
        //{

        //    BitmapData bmpdata = null;

        //    try
        //    {
        //        bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        //        int numbytes = bmpdata.Stride * bitmap.Height;
        //        byte[] bytedata = new byte[numbytes];
        //        IntPtr ptr = bmpdata.Scan0;

        //        Marshal.Copy(ptr, bytedata, 0, numbytes);

        //        return bytedata;
        //    }
        //    finally
        //    {
        //        if (bmpdata != null)
        //            bitmap.UnlockBits(bmpdata);
        //    }

        //}

        //Not working
        public static byte[] StreamToBytes(ref Stream stream, int width, int height)
        { 
            WriteableBitmap wbBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            byte[] myBytes = new byte[stream.Length];
            try
            {
                wbBitmap.Lock();
                stream.Seek(0, SeekOrigin.Begin);
                //byte[] myBytes = new byte[stream.Length];
                stream.Read(myBytes, 0, Convert.ToInt32(stream.Length));
                IntPtr buff = wbBitmap.BackBuffer;
                unsafe
                {
                    byte* p = (byte*)buff.ToPointer();
                    //Get start index of the specified pixel
                    int i = ((1 * width) + 1) * 4;
                    byte b = myBytes[i];
                    byte g = myBytes[i + 1];
                    byte r = myBytes[i + 2];
                    byte a = myBytes[i + 3];

                    for (int n = 0; n < stream.Length; n += 4)
                    {
                        if (myBytes[n + 3] == a && myBytes[n + 2] == r && myBytes[n + 1] == g && myBytes[n] == b)
                        {
                            for (var j = n; j < n + 4; j++)
                            {
                                myBytes[j] = 0;
                            }
                        }

                        p[n] = myBytes[n];
                        p[n + 1] = myBytes[n + 1];
                        p[n + 2] = myBytes[n + 2];
                        p[n + 3] = myBytes[n + 3];
                    }

                }

                wbBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                wbBitmap.Unlock();

            }
            catch
            {

            }
            
            return myBytes;// wbBytes;
        }

        public static WriteableBitmap StreamToBitmap(ref Stream stream, int width, int height)
        {

            WriteableBitmap wbBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            try
            {
                wbBitmap.Lock();
                stream.Seek(0, SeekOrigin.Begin);
                byte[] myBytes = new byte[stream.Length];
                stream.Read(myBytes, 0, Convert.ToInt32(stream.Length));
                IntPtr buff = wbBitmap.BackBuffer;
                unsafe
                {
                    byte* p = (byte*)buff.ToPointer();
                    //Get start index of the specified pixel
                    int i = ((1 * width) + 1) * 4;
                    byte b = myBytes[i];
                    byte g = myBytes[i + 1];
                    byte r = myBytes[i + 2];
                    byte a = myBytes[i + 3];

                    for (int n = 0; n < stream.Length; n += 4)
                    {
                        if (myBytes[n + 3] == a && myBytes[n + 2] == r && myBytes[n + 1] == g && myBytes[n] == b)
                        {
                            for (var j = n; j < n + 4; j++)
                            {
                                myBytes[j] = 0;
                            }
                        }

                        p[n] = myBytes[n];
                        p[n + 1] = myBytes[n + 1];
                        p[n + 2] = myBytes[n + 2];
                        p[n + 3] = myBytes[n + 3];
                    }
                }

                wbBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                wbBitmap.Unlock();
            }
            catch
            {

            }

            return wbBitmap;
        }

        public static D2D.Bitmap ConvertToSharpDXBitmap(D2D.DeviceContext context, System.Drawing.Bitmap bmp)
        {
            D2D.Bitmap Image = null;
            try
            {
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

                _dataStream = new DataStream(bmpData.Scan0, bmpData.Stride * bmpData.Height, true, false);
                D2D.BitmapProperties d2dBitmapProperties = new D2D.BitmapProperties();
                d2dBitmapProperties.PixelFormat = D2PixelFormat;

                Image = new D2D.Bitmap(context, new Size2(bmpData.Width, bmpData.Height), _dataStream, bmpData.Stride, d2dBitmapProperties);
                _dataStream.Dispose();
                bmp.UnlockBits(bmpData);
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                

            }
            return Image;

        }


        public static D2D.Bitmap ConvertToSharpDXBitmap(D2D.DeviceContext target, WriteableBitmap wbBitmap)
        {

            D2D.Bitmap Image = null;
            try
            {
                wbBitmap.Lock();
                IntPtr buff = wbBitmap.BackBuffer;
                _dataStream = new DataStream(wbBitmap.BackBuffer,  wbBitmap.BackBufferStride * (int)wbBitmap.Height, true, false);
                D2D.BitmapProperties d2dBitmapProperties = new D2D.BitmapProperties();
                d2dBitmapProperties.PixelFormat = D2PixelFormat;

                Image = new D2D.Bitmap(target, new Size2((int)wbBitmap.Width, (int)wbBitmap.Height), _dataStream, wbBitmap.BackBufferStride, d2dBitmapProperties);
                _dataStream.Dispose();
                wbBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)Image.Size.Width, (int)Image.Size.Height)); //(int)wbBitmap.Width, (int)wbBitmap.Height));
                wbBitmap.Unlock();
                wbBitmap = null;
            }
            catch (Exception ex)
            {

            }

            return Image;
        }

        public static D2D.Bitmap ConvertToSharpDXBitmap(D2D.DeviceContext target, byte[] byteBitmap, int width, int height)
        {
            D2D.Bitmap Image = null;
            if (byteBitmap == null) return Image;

            try
            {
                
                //int bpp = LogPictureBox.Image.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;
                //int Stride = (LogPictureBox.Image.Width +
                //             (LogPictureBox.Image.Width % (4 * (4 - (bpp / 8))))) * bpp;
                //int LogSize = Stride * LogPictureBox.Image.Height;

                int stride = (width + (height % (4 * (4 - (4 / 8))))) * 4;
                GCHandle handle = GCHandle.Alloc(byteBitmap, GCHandleType.Pinned);
                IntPtr iptr = Marshal.UnsafeAddrOfPinnedArrayElement(byteBitmap, 0);

                _dataStream = new DataStream(iptr, stride * height, true, false);

                D2D.BitmapProperties d2dBitmapProperties = new D2D.BitmapProperties();
                d2dBitmapProperties.PixelFormat = D2PixelFormat;
                Image = new D2D.Bitmap(target, new Size2(width, height), _dataStream, stride, d2dBitmapProperties);
                _dataStream.Dispose();

                iptr = IntPtr.Zero;
                handle.Free();

            }
            catch (Exception ex)
            {

            }

            return Image;
        }


        //Dont know how to draw this on SharpDx so i'm using system drawing to draw and convet it to SharpDX Bitmap.
        public static SharpDX.Direct2D1.Bitmap DrawString(D2D.DeviceContext target, string textString, int width, int height, float fontSize15, float fontSize30)
        {
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(width, height, gr);

            gr.Dispose();
            _fontFamily = new System.Drawing.FontFamily("Arial");
            gr = System.Drawing.Graphics.FromImage(bm);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            var strformat = new System.Drawing.StringFormat
            {
                Alignment = System.Drawing.StringAlignment.Center,
                LineAlignment = System.Drawing.StringAlignment.Center
            };

            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;


            if (textString == string.Empty || textString == null)
            {
                string b = "BossDoy KaraokeNow";
                var stringSize = MeasureString(b, fontSize15);
                path.AddString(b, _fontFamily, (int)System.Drawing.FontStyle.Bold, fontSize15, new System.Drawing.Point((bm.Width / 2), (bm.Height / 2) - ((int)stringSize.Height) / 2), strformat);
                path.AddString("Select a song", _fontFamily,
                (int)System.Drawing.FontStyle.Bold, fontSize30, new System.Drawing.Point(bm.Width / 2, (bm.Height / 2) + ((int)stringSize.Height) / 2), strformat);
            }
            else
            {

                //string[] intro = textString.Split(new char[] { '/' }, StringSplitOptions.None);
                //var stringSize = MeasureString(intro[0], fontSize15);

                //path.AddString(intro[0], _fontFamily, (int)System.Drawing.FontStyle.Bold, fontSize15, new System.Drawing.Point((bm.Width / 2), (bm.Height / 2) - ((int)stringSize.Height) / 2), strformat);
                //if (intro.Length > 1)
                //{
                //    path.AddString(intro[1], _fontFamily,
                //     (int)System.Drawing.FontStyle.Bold, fontSize30, new System.Drawing.Point(bm.Width / 2, (bm.Height / 2) + ((int)stringSize.Height) / 2), strformat);

                //}
                //else
                //{
                //    path.AddString("Select a song", _fontFamily,
                //    (int)System.Drawing.FontStyle.Bold, fontSize30, new System.Drawing.Point(bm.Width / 2, (bm.Height / 2) + ((int)stringSize.Height) / 2), strformat);
                //}

                var stringSize = MeasureString(textString, fontSize15);

                path.AddString(textString, _fontFamily, 
                    (int)System.Drawing.FontStyle.Bold, fontSize15, new System.Drawing.Point(bm.Width / 2, (bm.Height / 2) - ((int)stringSize.Height) / 2), strformat);

                path.AddString("Select a song", _fontFamily,
                    (int)System.Drawing.FontStyle.Bold, fontSize30, new System.Drawing.Point(bm.Width / 2, (bm.Height / 2) + ((int)stringSize.Height) / 2), strformat);
            }

            System.Drawing.Pen penOut = new System.Drawing.Pen(System.Drawing.Color.FromArgb(32, 117, 81), (fontSize30 / 4));
            penOut.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            gr.DrawPath(penOut, path);

            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(240, 240, 240), (fontSize30 / 8));
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            gr.DrawPath(pen, path);
            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(103, 58, 183));
            gr.FillPath(brush, path);

            path.Dispose();
            penOut.Dispose();
            pen.Dispose();
            brush.Dispose();
            gr.Dispose();
            _fontFamily.Dispose();

            return ConvertToSharpDXBitmap(target, bm);

        }

        private static System.Drawing.SizeF MeasureString(string txtstring, float fontSize)
        {
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            var stringSize = gr.MeasureString(txtstring, new System.Drawing.Font(_fontFamily, fontSize));
            gr.Dispose();
            return stringSize;
        }
    }
}
