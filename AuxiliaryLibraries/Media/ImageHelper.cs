using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    public static class ImageHelper
    {
        private static int[] Mask = new int[]
        {
            0,
            0x1,    0x3,    0x7,    0xF,
            0x1F,   0x3F,   0x7F,   0xFF,
            0x1FF,  0x3FF,  0x7FF,  0xFFF,
            0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF
        };

        public static int GetStride(PixelFormat pixelFormat, int pixelWidth) => (pixelFormat.BitsPerPixel * pixelWidth + 7) / 8;

        public static int[] GetIndexes(Color[] pixels, Color[] palette, PixelFormat pixelFormat, int pixelWidth)
        {
            Dictionary<Color, int> temp = new Dictionary<Color, int>();

            int[] returned = new int[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                if (temp.ContainsKey(pixels[i]))
                    returned[i] = temp[pixels[i]];
                else
                {
                    float tempDist = float.MaxValue;
                    int tempInd = 0;
                    for (int k = 0; k < palette.Length; k++)
                    {
                        var dist = ColorDistance(palette[k], pixels[i]);
                        if (dist == 0)
                        {
                            tempInd = k;
                            break;
                        }
                        if (dist < tempDist)
                        {
                            tempDist = dist;
                            tempInd = k;
                        }
                    }

                    temp.Add(pixels[i], tempInd);
                    returned[i] = tempInd;
                }
            }

            return returned;
        }

        public static float ColorDistance(Color col1, Color col2)
        {
            float rmean = ((float)(col1.R + col2.R)) / 2;
            float r = col1.R - col2.R;
            float g = col1.G - col2.G;
            float b = col1.B - col2.B;
            return 2 * r * r + 4 * g * g + 3 * b * b + (rmean * (r * r - b * b) / 256);
        }

        public static byte[] IndexesToData(int[] indexes, int bit, int pixelWidth)
        {
            int height = indexes.Length / pixelWidth;
            int stride = (bit * pixelWidth + 7) / 8;

            byte[] returned = new byte[height * stride];

            int remain = 8;
            int ind = 0;

            for (int i = 0; i < indexes.Length; i++)
            {
                int data = indexes[i] & Mask[bit];

                if (remain == bit)
                {
                    returned[ind] |= (byte)data;
                    ind++;
                    remain = 8;
                }
                else if (remain > bit)
                {
                    returned[ind] |= (byte)(data << (remain - bit));
                    remain -= bit;
                }
                else if (remain < bit)
                {
                    returned[ind] |= (byte)(data >> (bit - remain));
                    ind++;
                    remain = 8 - (bit - remain);
                    returned[ind] = (byte)((data & Mask[bit - remain]) << remain);
                }


                if ((i + 1) % pixelWidth == 0)
                {
                    if (remain != 8)
                    {
                        ind++;
                        remain = 8;
                    }
                }
            }

            return returned;
        }

        public static Color[] GetGrayPalette(int count)
        {
            if (count < 1 | count > 8)
                throw new ArgumentOutOfRangeException("count", count, "count must be between 1 and 8");

            int colorCount = (int)Math.Pow(2, count);
            int step = (int)Math.Pow(2, 8 - count);

            Color[] palette = new Color[colorCount];

            for (int i = 0, k = 0; i < 256; i += step, k++)
                palette[k] = Color.FromArgb(i, i, i);

            return palette;
        }
    }
}