using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media.Quantization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    public class Bitmap
    {
        private BitmapDataBase bitmapDataBase = null;

        public Bitmap(int width, int height, PixelFormat pixelFormat, byte[] imageData, Color[] palette)
        {
            if (imageData == null)
                throw new Exception("1");
            if (pixelFormat.IsIndexed)
            {
                if (palette == null)
                    throw new Exception("2");
                else if (palette.Length > (int)Math.Pow(2, pixelFormat.BitsPerPixel))
                    throw new Exception("3");
            }

            int stride = ImageHelper.GetStride(pixelFormat, width);
            if (stride * height != imageData.Length)
                throw new Exception("4");

            if (pixelFormat.IsIndexed)
                bitmapDataBase = new BitmapDataIndexed(width, height, pixelFormat, imageData.Copy(), palette);
            else
                bitmapDataBase = new BitmapData(width, height, pixelFormat, imageData.Copy());
        }

        public Bitmap(int width, int height, Color[] pixels)
        {
            if (pixels == null)
                throw new Exception("1");
            if (width * height != pixels.Length)
                throw new Exception("2");

            bitmapDataBase = new BitmapDataUndefined(width, height, pixels);
        }

        internal Bitmap(BitmapDataBase bitmapDataBase)
        {
            this.bitmapDataBase = bitmapDataBase;
        }

        public Bitmap ConvertTo(PixelFormat dstFormat, Color[] palette)
        {
            if (palette == null)
                return new Bitmap(bitmapDataBase.ConvertTo(dstFormat));
            else
                return new Bitmap(bitmapDataBase.ConvertTo(dstFormat, palette));
        }

        public Bitmap Copy()
        {
            return new Bitmap(bitmapDataBase.Copy());
        }

        public void CopyData(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new Exception("");

            byte[] temp = bitmapDataBase.GetData();
            if (offset < 0 && buffer.Length - offset < temp.Length)
                throw new Exception("");

            Buffer.BlockCopy(temp, 0, buffer, offset, temp.Length);
        }

        public byte[] CopyData()
        {
            byte[] temp = bitmapDataBase.GetData();
            byte[] buffer = new byte[temp.Length];
            Buffer.BlockCopy(temp, 0, buffer, 0, temp.Length);
            return buffer;
        }

        internal byte[] InternalGetData() => bitmapDataBase.GetData();

        public Color[] CopyPixels()
            => bitmapDataBase.GetPixels();

        public Color[] CopyPalette()
            => bitmapDataBase.GetPalette()?.Copy();

        #region Public Properties

        public int Width
            => bitmapDataBase.Width;

        public int Height
            => bitmapDataBase.Height;

        public PixelFormat PixelFormat
            => bitmapDataBase.PixelFormat;

        #endregion
    }
}