using PersonaEditorLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PersonaEditorLib.Utilities
{
    public static class ImageHelper
    {
        public const uint AlphaMask = 0x00FFFFFF;

        public static void DDS_DXT1_GetPixels(uint[,] pixels, int x, int y, byte[] data)
        {
            if (data.Length != 8)
                throw new ArgumentException("DXT1 wrong data", "data");

            uint[] Pallete = new uint[4];

            int color0 = data[0] + data[1] * 256;
            int color1 = data[2] + data[3] * 256;

            byte[] ar_color0 = TwoByte2RGBA(color0);
            byte[] ar_color1 = TwoByte2RGBA(color1);

            Pallete[0] = BitConverter.ToUInt32(ar_color0, 0);
            Pallete[1] = BitConverter.ToUInt32(ar_color1, 0);

            if (color0 > color1)
            {
                byte[] ar_color3 = new byte[]
                {
                    Convert.ToByte((2 * ar_color0[0] + ar_color1[0]) / 3),
                    Convert.ToByte((2 * ar_color0[1] + ar_color1[1]) / 3),
                    Convert.ToByte((2 * ar_color0[2] + ar_color1[2]) / 3),
                    255
                };
                byte[] ar_color4 = new byte[]
                {
                    Convert.ToByte((ar_color0[0] + 2 * ar_color1[0]) / 3),
                    Convert.ToByte((ar_color0[1] + 2 * ar_color1[1]) / 3),
                    Convert.ToByte((ar_color0[2] + 2 * ar_color1[2]) / 3),
                    255
                };

                Pallete[2] = BitConverter.ToUInt32(ar_color3, 0);
                Pallete[3] = BitConverter.ToUInt32(ar_color4, 0);
            }
            else
            {
                byte[] ar_color3 = new byte[]
                  {
                    Convert.ToByte((ar_color0[0] + ar_color1[0]) / 2),
                    Convert.ToByte((ar_color0[1] + ar_color1[1]) / 2),
                    Convert.ToByte((ar_color0[2] + ar_color1[2]) / 2),
                    255
                  };
                Pallete[2] = BitConverter.ToUInt32(ar_color3, 0);
                Pallete[3] = 0xFF000000;
            }

            for (int i = 4; i < data.Length; i++)
                for (int k = 0; k < 4; k++)
                {
                    int pixel = (data[i] >> (k * 2)) & 3;
                    pixels[y + i - 4, x + k] = Pallete[pixel];
                }
        }

        public static void DDS_DXT3_GetPixels(uint[,] pixels, int x, int y, byte[] data)
        {
            if (data.Length != 16)
                throw new ArgumentException("DXT5 wrong data", "data");

            DDS_DXT1_GetPixels(pixels, x, y, data.SubArray(8, 8));

            uint[] alpha = new uint[16];
            for (int i = 0; i < 8; i++)
            {
                alpha[i * 2] = data[i] & (uint)0X0F;
                alpha[i * 2] |= alpha[i * 2] << 4;
                alpha[i * 2 + 1] = (data[i] & (uint)0XF0);
                alpha[i * 2 + 1] |= alpha[i * 2 + 1] >> 4;
            }

            int index = 0;
            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 4; k++, index++)
                    pixels[y + i, x + k] = (pixels[y + i, x + k] & AlphaMask) | alpha[index] << 24;
        }

        public static void DDS_DXT5_GetPixels(uint[,] pixels, int x, int y, byte[] data)
        {
            if (data.Length != 16)
                throw new ArgumentException("DXT5 wrong data", "data");

            DDS_DXT1_GetPixels(pixels, x, y, data.SubArray(8, 8));

            uint[] alpha = new uint[8];

            alpha[0] = data[0];
            alpha[1] = data[1];

            if (alpha[0] > alpha[1])
                for (int i = 1; i < 7; i++)
                    alpha[i + 1] = (uint)((7 - i) * alpha[0] + i * alpha[1]) / 7;
            else
            {
                for (int i = 1; i < 5; i++)
                    alpha[i + 1] = (uint)((5 - i) * alpha[0] + i * alpha[1]) / 5;
                alpha[6] = 0;
                alpha[7] = 255;
            }

            long dat = data[2];
            for (int i = 3; i < 8; i++)
                dat += (long)data[i] << ((i - 2) * 8);

            int ind = 0;

            for (int i = 4; i < 8; i++)
                for (int k = 0; k < 4; k++, ind++)
                {
                    int index = (int)(dat >> (ind * 3)) & 7;

                    pixels[y + i - 4, x + k] = (pixels[y + i - 4, x + k] & AlphaMask) | alpha[index] << 24;
                }
        }

        public static void DDS_DXT1_GetPixels(Color[,] pixels, int x, int y, byte[] data)
        {
            if (data.Length != 8)
                throw new ArgumentException("DXT1 wrong data", "data");

            Color[] Pallete = new Color[4];

            int color0 = data[0] + data[1] * 256;
            int color1 = data[2] + data[3] * 256;

            Pallete[0] = TwoByte2Color(color0);
            Pallete[1] = TwoByte2Color(color1);

            if (color0 > color1)
            {
                Pallete[2] = Color.FromArgb(255,
                    Convert.ToByte((2 * Pallete[0].R + Pallete[1].R) / 3),
                    Convert.ToByte((2 * Pallete[0].G + Pallete[1].G) / 3),
                    Convert.ToByte((2 * Pallete[0].B + Pallete[1].B) / 3));

                Pallete[3] = Color.FromArgb(255,
                    Convert.ToByte((Pallete[0].R + 2 * Pallete[1].R) / 3),
                    Convert.ToByte((Pallete[0].G + 2 * Pallete[1].G) / 3),
                    Convert.ToByte((Pallete[0].B + 2 * Pallete[1].B) / 3));
            }
            else
            {
                Pallete[2] = Color.FromArgb(255,
                       Convert.ToByte((Pallete[0].R + Pallete[1].R) / 2),
                       Convert.ToByte((Pallete[0].G + Pallete[1].G) / 2),
                       Convert.ToByte((Pallete[0].B + Pallete[1].B) / 2));

                Pallete[3] = Colors.Transparent;
            }

            for (int i = 4; i < data.Length; i++)
                for (int k = 0; k < 4; k++)
                {
                    int pixel = (data[i] >> (k * 2)) & 3;
                    pixels[y + i - 4, x + k] = Pallete[pixel];
                }
        }

        public static uint[,] DDS_DXT5_GetPixels(byte[] data)
        {
            if (data.Length != 16)
                throw new ArgumentException("DXT5 wrong data", "data");

            uint[,] returned = new uint[4, 4];
            uint[] Pallete = new uint[4];



            return returned;
        }

        public static Color TwoByte2Color(int color)
        {
            int red_mask = 0xF800;
            int green_mask = 0x7E0;
            int blue_mask = 0x1F;

            byte red = Convert.ToByte((color & red_mask) >> 8);
            byte green = Convert.ToByte((color & green_mask) >> 3);
            byte blue = Convert.ToByte((color & blue_mask) << 3);

            return Color.FromArgb(255, red, green, blue);
        }

        public static byte[] TwoByte2RGBA(int color)
        {
            int red_mask = 0xF800;
            int green_mask = 0x7E0;
            int blue_mask = 0x1F;

            byte red = Convert.ToByte((color & red_mask) >> 8);
            byte green = Convert.ToByte((color & green_mask) >> 3);
            byte blue = Convert.ToByte((color & blue_mask) << 3);

            return new byte[] { blue, green, red, 255 };
        }
    }
}