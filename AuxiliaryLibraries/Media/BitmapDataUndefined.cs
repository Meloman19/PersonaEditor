using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media.Quantization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    class BitmapDataUndefined : BitmapDataBase
    {
        Color[] pixels;

        public BitmapDataUndefined(int width, int height, Color[] pixels) : base(width, height, PixelFormats.Undefined)
        {
            this.pixels = pixels;
        }

        public override BitmapDataBase ConvertTo(PixelFormat dstFormat)
        {
            if (dstFormat.Format == PixelFormat.Format)
                return Copy();
            else if (dstFormat.IsIndexed)
            {
                IQuantization quantization = new WuAlphaColorQuantizer();

                if (quantization.StartQuantization(pixels, Convert.ToInt32(Math.Pow(2, dstFormat.BitsPerPixel))))
                {
                    var newData = quantization.QuantData;

                    if (dstFormat.Format == PixelFormatEnum.Indexed4Reverse)
                    {
                        var data2data = PixelConverters.GetDataToDataConverter(PixelFormats.Indexed4, PixelFormats.Indexed4Reverse);
                        newData = data2data(newData);
                    }

                    return new BitmapDataIndexed(Width, Height, dstFormat, newData, quantization.QuantPalette);
                }
                else
                    throw new Exception($"BitmapData: convert to {dstFormat} error. Quantization don't work.");
            }
            else
            {
                var color2data = PixelConverters.GetColorToDataConverter(dstFormat);
                if (color2data != null)
                    return new BitmapData(Width, Height, dstFormat, color2data(pixels));
                else
                    throw new Exception($"BitmapData: convert to {dstFormat} error. ColorToDataConverter undefined.");
            }
        }

        public override BitmapDataBase ConvertTo(PixelFormat dstFormat, Color[] palette)
        {
            if (dstFormat.IsIndexed)
            {
                int maxcolor = (int)Math.Pow(2, dstFormat.BitsPerPixel);
                if (maxcolor >= palette.Length)
                {
                    var indexes = ImageHelper.GetIndexes(pixels, palette, dstFormat, Width);
                    return new BitmapDataIndexed(Width, Height, dstFormat,
                        ImageHelper.IndexesToData(indexes, dstFormat.BitsPerPixel, Width),
                        palette);
                }
                else
                    throw new Exception($"BitmapData: convert to {dstFormat} error. Palette lenght ({palette.Length}) is less than pixel format's maxcolor ({maxcolor}).");
            }
            else
                throw new Exception($"BitmapData: convert to {dstFormat} error. Destination PixelFormat not indexed.");
        }

        public override BitmapDataBase Copy()
        {
            return new BitmapDataUndefined(Width, Height, pixels.Copy());
        }

        public override byte[] GetData()
        {
            return null;
        }

        public override Color[] GetPalette()
        {
            return null;
        }

        public override Color[] GetPixels()
        {
            return pixels.Copy();
        }
    }
}
