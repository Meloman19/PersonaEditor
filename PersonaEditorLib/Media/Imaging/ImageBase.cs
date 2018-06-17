using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Media.Imaging
{
    public class ImageBase
    {
        private const string className = "ImageBase";

        protected byte[] imageData = null;
        protected int width = 0;
        protected int height = 0;
        protected int stride => Utilities.ImageHelper.GetStride(PixelFormat, Width);
        protected BitmapPalette bitmapPalette = null;
        protected PixelBaseFormat pixelFormat = PixelBaseFormat.Unknown;

        public int Width => width;
        public int Height => height;
        public int LengthData => imageData.Length;
        public BitmapPalette Palette => bitmapPalette;
        public PixelBaseFormat PixelFormat => pixelFormat;

        public ImageBase(int width, int height, PixelBaseFormat imageFormat, byte[] imageData,
            PixelBaseFormat paletteFormat = PixelBaseFormat.Unknown, byte[] paletteData = null) : this(width, height, imageFormat, imageData)
        {
            if (paletteData != null)
            {
                var paletteConverter = PixelConverter.GetDataColorConverter(paletteFormat);
                if (paletteConverter != null)
                    bitmapPalette = new BitmapPalette(paletteConverter(paletteData, null));
                else
                    throw new ArgumentException("Wrong palette pixel format");
            }
        }

        public ImageBase(int width, int height, PixelBaseFormat imageFormat, byte[] imageData,
            BitmapPalette bitmapPalette) : this(width, height, imageFormat, imageData)
        {
            if (imageFormat.IsIndexed() && bitmapPalette == null)
                throw new ArgumentNullException("paletteData", "imageFormat is Indexed");

            this.bitmapPalette = new BitmapPalette(bitmapPalette?.Colors.ToArray());
        }

        public ImageBase(BitmapSource bitmapSource)
        {
            width = bitmapSource.PixelWidth;
            height = bitmapSource.PixelHeight;
            pixelFormat = PixelFormatHelper.ConvertFromSystem(bitmapSource.Format);

            var LengthData = height * width * pixelFormat.BitsPerPixel() / 8;

            imageData = new byte[LengthData];
            bitmapSource.CopyPixels(imageData, stride, 0);
            bitmapPalette = bitmapSource.Palette;
        }

        protected ImageBase(int width, int height, PixelBaseFormat imageFormat, byte[] imageData)
        {
            this.width = width;
            this.height = height;
            pixelFormat = imageFormat;
            this.imageData = imageData ?? throw new ArgumentNullException("imageData");

            //if (LengthData != imageData.Length)
            //    throw new ArgumentException($"{className}:Wrong image data length");
        }

        protected ImageBase()
        {

        }

        public BitmapSource GetBitmapSource()
        {
            PixelBaseFormat PixelFormat = this.PixelFormat;
            byte[] data = imageData;
            if (PixelFormat.IsCompressed())
                ImageDecompress.DDSDecompress(width, height, imageData, PixelFormat, out data, out PixelFormat);

            PixelFormat pixelFormat = PixelFormatHelper.ConvertToSystem(PixelFormat);
            if (pixelFormat == PixelFormats.Default)
            {
                pixelFormat = PixelFormatHelper.CompatibilityFormat(PixelFormat);
                if (pixelFormat == PixelFormats.Default)
                    throw new Exception("ImageBase: can't create image");
                else
                {
                    var converter = PixelConverter.GetDataConverter(PixelFormat, PixelFormatHelper.ConvertFromSystem(pixelFormat));
                    data = converter(imageData);
                }
            }

            var bitmapSource = BitmapSource.Create(Width, Height, 96, 96, pixelFormat, Palette,
                data, Utilities.ImageHelper.GetStride(pixelFormat, Width));

            if (bitmapSource.CanFreeze)
                bitmapSource.Freeze();

            return bitmapSource;
        }

        public byte[] GetImageData()
        {
            byte[] returned = new byte[imageData.Length];
            Buffer.BlockCopy(imageData, 0, returned, 0, imageData.Length);
            return returned;
        }

        public byte[] GetPaletteData(PixelBaseFormat dstPixelFormat)
        {
            if (bitmapPalette == null)
                return new byte[0];
            else
            {
                var pixelConverter = PixelConverter.GetColorDataConverter(dstPixelFormat);
                if (pixelConverter == null)
                    throw new Exception($"{className}: converter doesn't exist");
                else
                    return pixelConverter(bitmapPalette.Colors.ToArray());
            }
        }

        //private byte[] getPaletteData(PixelBaseFormat dstPixelFormat = PixelBaseFormat.Unknown)
        //{
        //    if (paletteData != null)
        //    {
        //        if (dstPixelFormat == PaletteFormat || dstPixelFormat == PixelBaseFormat.Unknown)
        //            return paletteData;
        //        else
        //        {
        //            var pixelConverter = PixelConverter.GetDataConverter(PaletteFormat, dstPixelFormat);
        //            if (pixelConverter == null)
        //                throw new Exception($"{className}: converter doesn't exist");
        //            else
        //                return pixelConverter(paletteData);
        //        }
        //    }
        //    else
        //    {
        //        if (dstPixelFormat == PaletteFormat || dstPixelFormat == PixelBaseFormat.Unknown)
        //        {
        //            var pixelConverter = PixelConverter.GetColorDataConverter(PaletteFormat);
        //            if (pixelConverter == null)
        //                throw new Exception($"{className}: converter doesn't exist");
        //            else
        //                paletteData = pixelConverter(palette.Colors.ToArray());

        //            return paletteData;
        //        }
        //        else
        //        {
        //            var pixelConverter = PixelConverter.GetColorDataConverter(dstPixelFormat);
        //            if (pixelConverter == null)
        //                throw new Exception($"{className}: converter doesn't exist");
        //            else
        //                return pixelConverter(palette.Colors.ToArray());
        //        }
        //    }
        //}
    }
}