using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media.Quantization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    class BitmapDataIndexed : BitmapDataBase
    {
        public byte[] data;
        public Color[] palette;

        public BitmapDataIndexed(int width, int height, PixelFormat pixelFormat, byte[] data, Color[] palette) : base(width, height, pixelFormat)
        {
            if (!pixelFormat.IsIndexed)
                throw new Exception("BitmapDataIndexed: pixelformat isn't indexed");
            if (palette == null)
                throw new Exception("BitmapDataIndexed: palette is null");

            this.palette = new Color[(int)Math.Pow(2, pixelFormat.BitsPerPixel)];

            this.data = data;           
            palette.CopyTo(this.palette, 0);
        }

        public override BitmapDataBase ConvertTo(PixelFormat dstFormat)
        {
            if (dstFormat == PixelFormat)
                return Copy();
            else if (dstFormat.Format == PixelFormatEnum.Undefined)
            {
                var dataInd2Color = PixelConverters.GetDataIndexedToColorConverter(PixelFormat);
                if (dataInd2Color != null)
                    return new BitmapDataUndefined(Width, Height, dataInd2Color(data, palette, Width));
                else
                    throw new Exception($"BitmapData: convert to {dstFormat} error. DataToColorConverter undefined.");
            }
            else if (dstFormat.IsIndexed)
            {
                if (PixelFormat.BitsPerPixel <= dstFormat.BitsPerPixel)
                {
                    var data2data = PixelConverters.GetDataToDataConverter(PixelFormat, dstFormat);
                    if (data2data != null)
                        return new BitmapDataIndexed(Width, Height, dstFormat, data2data(data), palette);
                    else
                    {
                        var dataInd2dataInd = PixelConverters.DataIndexedToDataIndexed(PixelFormat, dstFormat);
                        if (dataInd2dataInd != null)
                            return new BitmapDataIndexed(Width, Height, dstFormat, dataInd2dataInd(data, Width), palette);
                    }
                }

                var data2colorindexed = PixelConverters.GetDataIndexedToColorConverter(PixelFormat);
                if (data2colorindexed != null)
                {
                    IQuantization quantization = new WuAlphaColorQuantizer();

                    if (quantization.StartQuantization(data2colorindexed(data, palette, Width), Convert.ToInt32(Math.Pow(2, dstFormat.BitsPerPixel))))
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
                    throw new Exception($"BitmapDataIndexed: convert to {dstFormat} error. DataToColorConverter undefined.");
            }
            else
            {
                var dataInd2data = PixelConverters.GetDataIndexedToDataConverter(PixelFormat, dstFormat);
                if (dataInd2data == null)
                {
                    var color2data = PixelConverters.GetColorToDataConverter(dstFormat);
                    var data2color = PixelConverters.GetDataIndexedToColorConverter(PixelFormat);
                    if (data2color != null & color2data != null)
                        return new BitmapData(Width, Height, dstFormat, color2data(data2color(data, palette, Width)));
                    else
                        throw new Exception($"BitmapData: convert to {dstFormat} error. ColorToDataConverter or DataToColorConverter undefined.");
                }
                else
                    return new BitmapData(Width, Height, dstFormat, dataInd2data(data, palette));
            }
        }

        public override BitmapDataBase ConvertTo(PixelFormat dstFormat, Color[] palette)
        {
            if (dstFormat.IsIndexed)
            {
                int maxcolor = (int)Math.Pow(2, dstFormat.BitsPerPixel);
                if (maxcolor >= palette.Length)
                {
                    var data2color = PixelConverters.GetDataIndexedToColorConverter(PixelFormat);
                    if (data2color != null)
                    {
                        var indexes = ImageHelper.GetIndexes(data2color(data, palette, Width), palette, dstFormat, Width);
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
            return new BitmapDataIndexed(Width, Height, PixelFormat, data.Copy(), palette);
        }

        public override byte[] GetData()
        {
            return data;
        }

        public override Color[] GetPalette()
        {
            return palette;
        }

        public override Color[] GetPixels()
        {
            var data2color = PixelConverters.GetDataIndexedToColorConverter(PixelFormat);
            if (data2color != null)
                return data2color(data, palette, Width);
            else
                throw new Exception("BitmapData: GetPixels error. DataIndexedToColorConverter undefined.");
        }
    }
}