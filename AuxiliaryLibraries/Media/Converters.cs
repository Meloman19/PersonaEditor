using System;
using System.Collections.Generic;
using System.Drawing;

namespace AuxiliaryLibraries.Media
{
    public delegate Color[] DataToColorDelegate(byte[] data);
    public delegate Color[] DataIndexedToColorDelegate(byte[] data, Color[] palette, int width);
    public delegate byte[] ColorToDataDelegate(Color[] pixels);
    public delegate byte[] DataToDataDelegate(byte[] data);
    public delegate byte[] DataIndexedToDataIndexedDelegate(byte[] data, int width);
    public delegate byte[] DataIndexedToDataDelegate(byte[] data, Color[] palette);

    public static class PixelConverters
    {
        #region DataToColor

        private static Dictionary<PixelFormat, DataToColorDelegate> data2colorDic = new Dictionary<PixelFormat, DataToColorDelegate>()
        {
            { PixelFormats.Argb32,          Argb32          },
            { PixelFormats.Bgra32,          Bgra32          },
            { PixelFormats.Rgba32,          Rgba32          },
            { PixelFormats.Rgba32PS2,       Rgba32PS2       }
        };

        private static Dictionary<PixelFormat, DataIndexedToColorDelegate> data2colorIndexedDic = new Dictionary<PixelFormat, DataIndexedToColorDelegate>()
        {
            { PixelFormats.Indexed4,        Indexed4        },
            { PixelFormats.Indexed4Reverse, Indexed4Reverse },
            { PixelFormats.Indexed8,        Indexed8        }
        };

        /// <summary>
        /// Get data to color converter for not indexed format.
        /// </summary>
        /// <param name="pixelFormat"></param>
        /// <returns>Return data to color converter for not indexed format. Else return null</returns>
        public static DataToColorDelegate GetDataToColorConverter(PixelFormat pixelFormat)
        {
            if (data2colorDic.ContainsKey(pixelFormat))
                return data2colorDic[pixelFormat];
            else
                return null;
        }

        /// <summary>
        /// Get data to color converter for indexed format.
        /// </summary>
        /// <param name="pixelFormat"></param>
        /// <returns>Return data to color converter for indexed format. Else return null</returns>
        public static DataIndexedToColorDelegate GetDataIndexedToColorConverter(PixelFormat pixelFormat)
        {
            if (data2colorIndexedDic.ContainsKey(pixelFormat))
                return data2colorIndexedDic[pixelFormat];
            else
                return null;
        }

        private static Color[] Indexed4(byte[] data, Color[] palette, int width)
        {
            Color[] returned = null;
            if (width % 2 == 0)
            {
                int size = data.Length * 2;
                returned = new Color[size];

                for (int i = 0, k = 0; i < size; i += 2, k++)
                {
                    int ind1 = data[k] >> 4;
                    int ind2 = data[k] & 0x0F;

                    returned[i] = palette[ind1];
                    returned[i + 1] = palette[ind2];
                }
            }
            else
            {
                int height = data.Length / (width + 1);
                int size = height * width;
                returned = new Color[size];

                int tempWidth = width - 1;
                for (int x = 0, y = 0, i = 0; y < height; y++)
                {
                    for (; x < tempWidth; x += 2, i++)
                    {
                        int ind1 = data[i] >> 4;
                        int ind2 = data[i] & 0x0F;

                        returned[x] = palette[ind1];
                        returned[x + 1] = palette[ind2];
                    }

                    returned[x] = palette[data[i] >> 4];
                    i++;
                    x++;
                    tempWidth += width;
                }
            }

            return returned;
        }

        private static Color[] Indexed4Reverse(byte[] data, Color[] palette, int width)
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

        private static Color[] Indexed8(byte[] data, Color[] palette, int width)
        {
            int size = data.Length;
            Color[] returned = new Color[size];

            for (int i = 0; i < size; i++)
                returned[i] = palette[data[i]];

            return returned;
        }

        private static Color[] Argb32(byte[] data)
        {
            int size = data.Length / 4;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Color.FromArgb(
                    data[k],
                    data[k + 1],
                    data[k + 2],
                    data[k + 3]);

            return returned;
        }

        private static Color[] Bgra32(byte[] data)
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

        private static Color[] Rgba32(byte[] data)
        {
            int size = data.Length / 4;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Color.FromArgb(
                    data[k + 3],
                    data[k],
                    data[k + 1],
                    data[k + 2]);

            return returned;
        }

        private static Color[] Rgba32PS2(byte[] data)
        {
            int size = data.Length / 4;
            Color[] returned = new Color[size];

            for (int i = 0, k = 0; i < returned.Length; i++, k += 4)
                returned[i] = Color.FromArgb(
                    BitHelper.AlphaPS2ToPC[data[k + 3]],
                    data[k],
                    data[k + 1],
                    data[k + 2]);

            return returned;
        }

        #endregion DataToColor

        #region ColorToData

        private static Dictionary<PixelFormat, ColorToDataDelegate> color2dataDic = new Dictionary<PixelFormat, ColorToDataDelegate>()
        {
            { PixelFormats.Bgra32,    ConvertColorsToBgra32    },
            { PixelFormats.Rgba32PS2, ConvertColorsToRgba32PS2 }
        };

        public static ColorToDataDelegate GetColorToDataConverter(PixelFormat src)
        {
            if (color2dataDic.ContainsKey(src))
                return color2dataDic[src];
            else
                return null;
        }

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
                returned[k + 3] = BitHelper.AlphaPCToPS2[colors[i].A];
            }

            return returned;
        }

        #endregion ColorToData

        #region DataToData

        private static List<(PixelFormat, PixelFormat, DataToDataDelegate)> data2dataDic = new List<(PixelFormat, PixelFormat, DataToDataDelegate)>()
        {
            ( PixelFormats.Rgba32PS2,       PixelFormats.Bgra32,          Rgba32PS2ToBgra32         ),
            ( PixelFormats.Bgra32,          PixelFormats.Rgba32PS2,       Bgra32ToRgba32PS2         ),
            ( PixelFormats.Argb32,          PixelFormats.Bgra32,          Abgr32ToBgra32            ),
            ( PixelFormats.Indexed4Reverse, PixelFormats.Indexed4,        Indexed4ReverseToFromIndexed4 ),
            ( PixelFormats.Indexed4,        PixelFormats.Indexed4Reverse, Indexed4ReverseToFromIndexed4 ),
        };

        private static List<(PixelFormat, PixelFormat, DataIndexedToDataDelegate)> dataInd2dataDic = new List<(PixelFormat, PixelFormat, DataIndexedToDataDelegate)>()
        {

        };

        private static List<(PixelFormat, PixelFormat, DataIndexedToDataIndexedDelegate)> dataInd2dataInd2Dic = new List<(PixelFormat, PixelFormat, DataIndexedToDataIndexedDelegate)>()
        {

            ( PixelFormats.Indexed4,        PixelFormats.Indexed8, Indexed4ToIndexed8        ),
            ( PixelFormats.Indexed4Reverse, PixelFormats.Indexed8, Indexed4ReverseToIndexed8 )
        };

        public static DataToDataDelegate GetDataToDataConverter(PixelFormat src, PixelFormat dst)
        {
            var index = data2dataDic.FindIndex(x => x.Item1 == src && x.Item2 == dst);
            if (index >= 0)
                return data2dataDic[index].Item3;
            else
                return null;
        }

        public static DataIndexedToDataDelegate GetDataIndexedToDataConverter(PixelFormat src, PixelFormat dst)
        {
            var index = dataInd2dataDic.FindIndex(x => x.Item1 == src && x.Item2 == dst);
            if (index >= 0)
                return dataInd2dataDic[index].Item3;
            else
                return null;
        }

        public static DataIndexedToDataIndexedDelegate DataIndexedToDataIndexed(PixelFormat src, PixelFormat dst)
        {
            var index = dataInd2dataInd2Dic.FindIndex(x => x.Item1 == src && x.Item2 == dst);
            if (index >= 0)
                return dataInd2dataInd2Dic[index].Item3;
            else
                return null;
        }

        private static byte[] Rgba32PS2ToBgra32(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 4)
            {
                returned[i] = data[i + 2];
                returned[i + 1] = data[i + 1];
                returned[i + 2] = data[i];
                returned[i + 3] = BitHelper.AlphaPS2ToPC[data[i + 3]];
            }

            return returned;
        }

        private static byte[] Bgra32ToRgba32PS2(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i += 4)
            {
                returned[i] = data[i + 2];
                returned[i + 1] = data[i + 1];
                returned[i + 2] = data[i];
                returned[i + 3] = BitHelper.AlphaPCToPS2[data[i + 3]];
            }

            return returned;
        }

        private static byte[] Abgr32ToBgra32(byte[] data)
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

        private static byte[] Indexed4ReverseToFromIndexed4(byte[] data)
        {
            byte[] returned = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                returned[i] = PixelFormatHelper.ReverseByte(data[i]);

            return returned;
        }

        private static byte[] Indexed4ReverseToIndexed8(byte[] data, int width)
        {
            int height = 0;
            int tempWidth = 0;
            bool tempBool = false;

            if (width % 2 == 0)
            {
                height = data.Length * 2 / width;
                tempBool = false;
                tempWidth = width;
            }
            else
            {
                height = data.Length * 2 / (width + 1);
                tempBool = true;
                tempWidth = width - 1;
            }

            int len = data.Length;
            byte[] returned = new byte[height * width];

            for (int src = 0, dst = 0; src < len;)
            {
                for (; dst < tempWidth; src++, dst += 2)
                {
                    returned[dst] = (byte)(data[src] & 0xF);
                    returned[dst + 1] = (byte)(data[src] >> 4);

                    if (returned[dst] > 0 |returned[dst + 1] > 0)
                    {

                    }
                }

                if (tempBool)
                {
                    returned[dst] = (byte)(data[src] & 0xF);
                    dst++;
                    src++;
                }

                tempWidth += width;
            }

            return returned;
        }

        private static byte[] Indexed4ToIndexed8(byte[] data, int width)
        {
            int height = 0;
            int tempWidth = 0;
            bool tempBool = false;

            if (width % 2 == 0)
            {
                height = data.Length / width;
                tempBool = false;
                tempWidth = width;
            }
            else
            {
                height = data.Length / (width + 1);
                tempBool = true;
                tempWidth = width - 1;
            }

            int len = height * width;
            byte[] returned = new byte[len];

            for (int src = 0, dst = 0; src < len;)
            {
                for (; dst < tempWidth; src++, dst += 2)
                {
                    returned[dst] = (byte)(data[src] >> 4);
                    returned[dst + 1] = (byte)(data[src] & 0x0F);
                }

                if (tempBool)
                {
                    returned[dst] = (byte)(data[src] >> 4);
                    dst++;
                    src++;

                }

                tempWidth += width;
            }

            return returned;
        }

        #endregion DataToData
    }
}