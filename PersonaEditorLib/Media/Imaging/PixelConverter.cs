using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace PersonaEditorLib.Media.Imaging
{
    public static class PixelConverter
    {
        private static List<(PixelBaseFormat, PixelBaseFormat, Func<byte[], byte[]>)> Converters = new List<(PixelBaseFormat, PixelBaseFormat, Func<byte[], byte[]>)>()
        {
            ( PixelBaseFormat.Rgba32PS2,   PixelBaseFormat.Bgra32,      ConvertRgba32PS2ToBgra32         ),
            ( PixelBaseFormat.Bgra32,      PixelBaseFormat.Rgba32PS2,   ConvertBgra32ToRgba32PS2         ),
            ( PixelBaseFormat.Argb32,      PixelBaseFormat.Bgra32,      ConvertAbgr32ToBgra32            ),
            ( PixelBaseFormat.Indexed4PS2, PixelBaseFormat.Indexed4,    ConvertIndexed4PS2ToFromIndexed4 ),
            ( PixelBaseFormat.Indexed4,    PixelBaseFormat.Indexed4PS2, ConvertIndexed4PS2ToFromIndexed4 ),
            ( PixelBaseFormat.Indexed4,    PixelBaseFormat.Indexed8,    ConvertIndexed4ToIndexed8        ),
            ( PixelBaseFormat.Indexed4PS2, PixelBaseFormat.Indexed8,    ConvertIndexed4PS2ToIndexed8     )
        };

        private static Dictionary<PixelBaseFormat, Func<byte[], Color[], Color[]>> Byte2ColorConverters = new Dictionary<PixelBaseFormat, Func<byte[], Color[], Color[]>>()
        {
            { PixelBaseFormat.Rgba32PS2,   ConvertColorsFromRgba32PS2   },
            { PixelBaseFormat.Bgra32,      ConvertColorsFromBgra32      },
            { PixelBaseFormat.Indexed4,    ConvertColorsFromIndexed4    },
            { PixelBaseFormat.Indexed4PS2, ConvertColorsFromIndexed4PS2 },
            { PixelBaseFormat.Indexed8,    ConvertColorsFromIndexed8    }
        };

        private static Dictionary<PixelBaseFormat, Func<Color[], byte[]>> Color2ByteConverters = new Dictionary<PixelBaseFormat, Func<Color[], byte[]>>()
        {
            { PixelBaseFormat.Bgra32,    ConvertColorsToBgra32   },
            { PixelBaseFormat.Rgba32PS2, ConvertColorsToRgba32PS2 }
        };

        public static Func<byte[], byte[]> GetDataConverter(PixelBaseFormat src, PixelBaseFormat dst)
        {
            var index = Converters.FindIndex(x => x.Item1 == src && x.Item2 == dst);
            if (index >= 0)
                return Converters[index].Item3;
            else
                return null;
        }

        public static Func<byte[], Color[], Color[]> GetDataColorConverter(PixelBaseFormat src)
        {
            if (Byte2ColorConverters.ContainsKey(src))
                return Byte2ColorConverters[src];
            else
                return null;
        }

        public static Func<Color[], byte[]> GetColorDataConverter(PixelBaseFormat src)
        {
            if (Color2ByteConverters.ContainsKey(src))
                return Color2ByteConverters[src];
            else
                return null;
        }

        #region Data Converters

        private static byte[] ConvertRgba32PS2ToBgra32(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 4)
            {
                returned[i] = data[i + 2];
                returned[i + 1] = data[i + 1];
                returned[i + 2] = data[i];
                returned[i + 3] = PixelHelper.ConvertAlphaToPC(data[i + 3]);
            }

            return returned;
        }

        private static byte[] ConvertBgra32ToRgba32PS2(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 4)
            {
                returned[i] = data[i + 2];
                returned[i + 1] = data[i + 1];
                returned[i + 2] = data[i];
                returned[i + 3] = PixelHelper.ConvertAlphaToPS2(data[i + 3]);
            }

            return returned;
        }

        private static byte[] ConvertAbgr32ToBgra32(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 4)
            {
                returned[i] = data[i + 3];
                returned[i + 1] = data[i + 2];
                returned[i + 2] = data[i + 1];
                returned[i + 3] = data[i];
            }

            return returned;
        }

        private static byte[] ConvertIndexed4PS2ToFromIndexed4(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                returned[i] = PixelHelper.ReverseByte(data[i]);

            return returned;
        }

        private static byte[] ConvertIndexed4PS2ToIndexed8(byte[] data)
        {
            int size = data.Length * 2;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < size; i++, k += 2)
            {
                returned[k] = Convert.ToByte(data[i] & 0x0F);
                returned[k] = Convert.ToByte(data[i] >> 4);
            }

            return returned;
        }

        private static byte[] ConvertIndexed4ToIndexed8(byte[] data)
        {
            int size = data.Length * 2;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < size; i++, k += 2)
            {
                returned[k] = Convert.ToByte(data[i] >> 4);
                returned[k] = Convert.ToByte(data[i] & 0x0F);
            }

            return returned;
        }

        #endregion Data Converters

        #region Data to Color Converters

        private static Color[] ConvertColorsFromBgra32(byte[] data, Color[] palette)
        {
            int size = data.Length / 4;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Color.FromArgb(
                    data[k + 3],
                    data[k + 2],
                    data[k + 1],
                    data[k]);


            return returned;
        }

        private static Color[] ConvertColorsFromRgba32PS2(byte[] data, Color[] palette)
        {
            int size = data.Length / 4;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Color.FromArgb(
                    PixelHelper.ConvertAlphaToPC(data[k + 3]),
                    data[k],
                    data[k + 1],
                    data[k + 2]);

            return returned;
        }

        private static Color[] ConvertColorsFromIndexed8(byte[] data, Color[] palette)
        {
            int size = data.Length;
            Color[] returned = new Color[size];

            for (int i = 0; i < size; i++)
                returned[i] = palette[data[i]];

            return returned;
        }

        private static Color[] ConvertColorsFromIndexed4(byte[] data, Color[] palette)
        {
            int size = data.Length * 2;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < size; i += 2, k++)
            {
                int ind1 = data[k] >> 4;
                int ind2 = data[k] & 0x0F;

                returned[i] = palette[ind1];
                returned[i + 1] = palette[ind2];
            }

            return returned;
        }

        private static Color[] ConvertColorsFromIndexed4PS2(byte[] data, Color[] palette)
        {
            int size = data.Length * 2;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < size; i += 2, k++)
            {
                int ind1 = data[k] & 0x0F;
                int ind2 = data[k] >> 4;

                returned[i] = palette[ind1];
                returned[i + 1] = palette[ind2];
            }

            return returned;
        }

        #endregion Color to Data Converters

        #region Color to Data Converters

        private static byte[] ConvertColorsToBgra32(Color[] colours)
        {
            int size = colours.Length * 4;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < colours.Length; i++, k += 4)
            {
                returned[k] = colours[i].B;
                returned[k + 1] = colours[i].G;
                returned[k + 2] = colours[i].R;
                returned[k + 3] = colours[i].A;
            }

            return returned;
        }

        private static byte[] ConvertColorsToRgba32PS2(Color[] colors)
        {
            int size = colors.Length * 4;
            byte[] returned = new byte[size];

            for (int i = 0, k = 0; i < colors.Length; i++, k += 4)
            {
                returned[k] = colors[i].R;
                returned[k + 1] = colors[i].G;
                returned[k + 2] = colors[i].B;
                returned[k + 3] = PixelHelper.ConvertAlphaToPS2(colors[i].A);
            }

            return returned;
        }

        #endregion Data to Color Converters
    }
}