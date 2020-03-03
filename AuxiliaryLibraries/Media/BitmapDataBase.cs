using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    internal abstract class BitmapDataBase
    {
        public int Width { get; }
        public int Height { get; }
        public PixelFormat PixelFormat { get; }

        internal BitmapDataBase(int width, int height, PixelFormat pixelFormat) : this()
        {
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
        }

        private BitmapDataBase() { }

        public abstract BitmapDataBase ConvertTo(PixelFormat dstFormat);

        public abstract BitmapDataBase ConvertTo(PixelFormat dstFormat, Color[] palette);

        public abstract BitmapDataBase Copy();

        public abstract byte[] GetData();

        public abstract Color[] GetPixels();

        public abstract Color[] GetPalette();
    }
}