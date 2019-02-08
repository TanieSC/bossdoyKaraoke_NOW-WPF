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
            byte[] wbBytes = new byte[stream.Length];
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

            return wbBytes;
        }

        public static unsafe WriteableBitmap StreamToBitmap(ref Stream stream, int width, int height)
        {

            WriteableBitmap wbBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            try
            {
                wbBitmap.Lock();
                stream.Seek(0, SeekOrigin.Begin);
                byte[] myBytes = new byte[stream.Length];
                stream.Read(myBytes, 0, Convert.ToInt32(stream.Length));
                IntPtr buff = wbBitmap.BackBuffer;
                // unsafe
                // {
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
                //}

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
    }
}
