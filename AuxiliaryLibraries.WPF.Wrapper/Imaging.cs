using AuxiliaryLibraries.Media;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AuxiliaryLibraries.WPF.Wrapper
{
    public static class Imaging
    {
        public static BitmapSource GetBitmapSource(this PixelMap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            var data = EncodingHelper.ToBgra32(bitmap.Pixels);
            return BitmapSource.Create(bitmap.Width, bitmap.Height,
                96, 96,
                PixelFormats.Bgra32,
                null,
                data,
                ImageHelper.GetStride(PixelFormats.Bgra32.BitsPerPixel, bitmap.Width));
        }

        public static PixelMap GetBitmap(this BitmapSource bitmapSource)
        {
            if (bitmapSource.Format != PixelFormats.Bgra32)
                bitmapSource = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);

            var pixels = DecodingHelper.FromBgra32(bitmapSource.GetData());
            return new PixelMap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, pixels);
        }
    }
}