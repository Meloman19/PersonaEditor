using PersonaEditorLib.Extension;
using PersonaEditorLib.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Utilities
{
    public static class ImageHelper
    {
        public static List<Color> ReadPalette(BinaryReader reader, int Count)
        {
            List<Color> Colors = new List<Color>();
            for (int i = 0; i < Count; i++)
                Colors.Add(new Color()
                {
                    R = reader.ReadByte(),
                    G = reader.ReadByte(),
                    B = reader.ReadByte(),
                    A = reader.ReadByte()
                });
            return Colors;
        }

        public static byte[] GetPalette(BitmapPalette bitmapPalette)
        {
            if (bitmapPalette == null)
                return new byte[0];

            var colors = bitmapPalette.Colors;

            byte[] returned = new byte[colors.Count * 4];

            int index = 0;
            foreach (var color in colors)
            {
                returned[index++] = color.R;
                returned[index++] = color.G;
                returned[index++] = color.B;
                returned[index++] = color.A;
            }

            return returned;
        }

        public static BitmapPalette PalleteAddColor(BitmapPalette bitmapPalette, PixelFormat pixelFormat)
        {
            var colors = bitmapPalette.Colors.ToList();

            if (pixelFormat == PixelFormats.Indexed8)
                for (int i = colors.Count; i < 256; i++)
                    colors.Add(Colors.White);
            else
                return bitmapPalette;

            return new BitmapPalette(colors);
        }

        public static int GetStride(PixelFormat pixelFormat, int width)
        {
            return (pixelFormat.BitsPerPixel * width + 7) / 8;
        }

        public static int GetStride(PixelBaseFormat pixelBaseFormat, int width)
        {
            return (PixelFormatHelper.BitsPerPixel(pixelBaseFormat) * width + 7) / 8;
        }

    }
}