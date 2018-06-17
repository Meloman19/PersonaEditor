using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Media.Imaging
{
    public static class PixelHelper
    {
        public static byte ConvertAlphaToPC(byte original)
        {
            if ((int)original - 0x80 <= 0)
                return (byte)Math.Round((((float)original / 0x80) * 0xFF));
            else
                return (byte)(0xFF - ((((float)original - 0x80) / 0x80) * 0xFF)); //wrap around
        }

        public static byte ConvertAlphaToPS2(byte original)
        {
            return (byte)Math.Round((((float)original / 0xFF) * 0x80));
        }

        public static byte[] ConvertAlphaToPC(byte[] data)
        {
            for (int i = 3; i < data.Length; i += 4)
                data[i] = ConvertAlphaToPC(data[i]);
            return data;
        }

        public static byte[] ConvertAlphaToPS2(byte[] data)
        {
            for (int i = 3; i < data.Length; i += 4)
                data[i] = ConvertAlphaToPS2(data[i]);
            return data;
        }

        public static byte ReverseByte(byte toReverse)
        {
            int temp = ((toReverse >> 4) & 0xF) + ((toReverse & 0xF) << 4);
            return Convert.ToByte(temp);
        }

        public static byte[] PixelDataConverter(PixelBaseFormat srcPixelFormat, byte[] srcData, PixelBaseFormat dstPixelFormat,
            BitmapPalette srcPalette = null, BitmapPalette dstPalette = null)
        {
            var func = PixelConverter.GetDataConverter(srcPixelFormat, dstPixelFormat);
            if (func != null)
                return func.Invoke(srcData);

            return null;
        }

        public static bool GetColors(PixelBaseFormat pixelBaseFormat, byte[] data, out Color[] colors, BitmapPalette palette = null)
        {
            if (data == null)
            {
                colors = null;
                return false;
            }

            int byteperpixel = (int)System.Math.Ceiling((double)PixelFormatHelper.BitsPerPixel(pixelBaseFormat) / 8);
            int size = data.Length / byteperpixel;
            Color[] returned = new Color[size];

            using (ColorReader colorreader = new ColorReader(data, true))
                for (int i = 0; i < returned.Length; i++)
                {
                    if (pixelBaseFormat == PixelBaseFormat.Bgra32)
                        returned[i] = colorreader.ReadBgra32();
                    else if (pixelBaseFormat == PixelBaseFormat.Rgba32)
                        returned[i] = colorreader.ReadRgba32();
                    else
                    {
                        colors = null;
                        return false;
                    }
                }

            colors = returned;
            return true;
        }
    }
}