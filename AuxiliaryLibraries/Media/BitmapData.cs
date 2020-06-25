using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media.Quantization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    class BitmapData : BitmapDataBase
    {
        public byte[] data;

        public BitmapData(int width, int height, PixelFormat pixelFormat, byte[] data) : base(width, height, pixelFormat)
        {
            if (pixelFormat.IsIndexed)
                throw new Exception("BitmapData: pixelformat is indexed");

            this.data = data;
        }

        public override BitmapDataBase ConvertTo(PixelFormat dstFormat)
        {
            if (dstFormat.Format == PixelFormatEnum.Undefined)
            {
                var data2color = PixelConverters.GetDataToColorConverter(PixelFormat);
                if (data2color != null)
                    return new BitmapDataUndefined(Width, Height, data2color(data));
                else
                    throw new Exception($"BitmapData: convert to {dstFormat} error. DataToColorConverter undefined.");
            }
            else if (dstFormat.Format == PixelFormat.Format)
                return Copy();
            else if (dstFormat.IsIndexed)
            {
                var data2color = PixelConverters.GetDataToColorConverter(PixelFormat);
                if (data2color != null)
                {
                    IQuantization quantization = new WuAlphaColorQuantizer();

                    if (quantization.StartQuantization(data2color(data), Convert.ToInt32(Math.Pow(2, dstFormat.BitsPerPixel))))
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
                    throw new Exception($"BitmapData: convert to {dstFormat} error. DataToColorConverter undefined.");
            }
            else
            {
                var data2data = PixelConverters.GetDataToDataConverter(PixelFormat, dstFormat);
                if (data2data == null)
                {
                    var data2color = PixelConverters.GetDataToColorConverter(PixelFormat);
                    var color2data = PixelConverters.GetColorToDataConverter(dstFormat);
                    if (data2color != null & color2data != null)
                        return new BitmapData(Width, Height, dstFormat, color2data(data2color(data)));
                    else
                        throw new Exception($"BitmapData: convert to {dstFormat} error. ColorToDataConverter or DataToColorConverter undefined.");
                }
                else
                    return new BitmapData(Width, Height, dstFormat, data2data(data));
            }
        }

        public override BitmapDataBase ConvertTo(PixelFormat dstFormat, Color[] palette)
        {
            if (dstFormat.IsIndexed)
            {
                int maxcolor = (int)Math.Pow(2, dstFormat.BitsPerPixel);
                if (maxcolor >= palette.Length)
                {
                    var data2color = PixelConverters.GetDataToColorConverter(PixelFormat);
                    if (data2color != null)
                    {
                        var indexes = ImageHelper.GetIndexes(data2color(data), palette, dstFormat, Width);
                        return new BitmapDataIndexed(Width, Height, dstFormat,
                            ImageHelper.IndexesToData(indexes, dstFormat.BitsPerPixel, Width),
                            palette);
                    }
                    else
                        throw new Exception($"BitmapData: convert to {dstFormat} error. DataToColorConverter undefined.");
                }
                else
                    throw new Exception($"BitmapData: convert to {dstFormat} error. Palette lenght ({palette.Length}) is less than pixel format's maxcolor ({maxcolor}).");
            }
            else
                throw new Exception($"BitmapData: convert to {dstFormat} error. Destination PixelFormat not indexed.");
        }

        public override BitmapDataBase Copy()
        {
            return new BitmapData(Width, Height, PixelFormat, data.Copy());
        }

        public override byte[] GetData()
        {
            return data;
        }

        public override Color[] GetPalette()
        {
            return null;
        }

        public override Color[] GetPixels()
        {
            var data2color = PixelConverters.GetDataToColorConverter(PixelFormat);
            if (data2color != null)
                return data2color(data);
            else
                throw new Exception("BitmapData: GetPixels error. DataToColorConverter undefined.");
        }
    }
}