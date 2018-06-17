using PersonaEditorLib.Extension;
using PersonaEditorLib.FileStructure.Graphic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Media.Imaging
{
    class ImageDecompress
    {
        // byte 8bit = (byte)((double)4bit * 255.0d / 15.0d + 0.5d);
        public static byte[] Table4bitTo8bit = new byte[]
        {
            0  ,17 ,34 ,51 ,68 ,85 ,102,119,
            136,153,170,187,204,221,238,255,
        };

        // byte 8bit = (byte)((double)5bit * 255.0d / 31.0d + 0.5d);
        public static byte[] Table5bitTo8bit = new byte[]
        {
            0  ,8  ,16 ,25 ,33 ,41 ,49 ,58 ,
            66 ,74 ,82 ,90 ,99 ,107,115,123,
            132,140,148,156,165,173,181,189,
            197,206,214,222,230,239,247,255
        };

        // byte 8bit = (byte)((double)6bit * 255.0d / 63.0d + 0.5d);
        public static byte[] Table6bitTo8bit = new byte[]
        {
            0  ,4  ,8  ,12 ,16 ,20 ,24 ,28 ,
            32 ,36 ,40 ,45 ,49 ,53 ,57 ,61 ,
            65 ,69 ,73 ,77 ,81 ,85 ,89 ,93 ,
            97 ,101,105,109,113,117,121,125,
            130,134,138,142,146,150,154,158,
            162,166,170,174,178,182,186,190,
            194,198,202,206,210,215,219,223,
            227,231,235,239,243,247,251,255
        };

        public static bool DDSDecompress(int width, int height, byte[] data, PixelBaseFormat fourCC, out byte[] newData, out PixelBaseFormat newFormat)
        {
            int Width = (int)Math.Ceiling((float)width / 4);
            int Heigth = (int)Math.Ceiling((float)height / 4);

            int step = fourCC == PixelBaseFormat.DXT1 ? 8 : 16;

            byte[,,] pixel = new byte[height, width, 4];
            byte[] uncompressed_data = new byte[width * height * 4];

            for (int i = 0, index = 0; i < Heigth; i++)
                for (int k = 0; k < Width; k++, index += step)
                    if (fourCC == PixelBaseFormat.DXT1)
                        DDS_DXT1_GetPixels(pixel, k * 4, i * 4, data, index);
                    else if (fourCC == PixelBaseFormat.DXT3)
                        DDS_DXT3_GetPixels(pixel, k * 4, i * 4, data, index);
                    else if (fourCC == PixelBaseFormat.DXT5)
                        DDS_DXT5_GetPixels(pixel, k * 4, i * 4, data, index);
                    else
                    {
                        newData = null;
                        newFormat = PixelBaseFormat.Unknown;
                        return false;
                    }

            Buffer.BlockCopy(pixel, 0, uncompressed_data, 0, uncompressed_data.Length);

            newData = uncompressed_data;
            newFormat = PixelBaseFormat.Bgra32;
            return true;
        }

        public static void DDS_DXT1_GetPixels(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            byte[,] palette = new byte[4, 4];

            ushort color0 = BitConverter.ToUInt16(data, dataIndex);
            ushort color1 = BitConverter.ToUInt16(data, dataIndex + 2);
            // int color0 = data[dataIndex] + data[dataIndex + 1] * 256;
            // int color1 = data[dataIndex + 2] + data[dataIndex + 3] * 256;

            RGB565ToBGRA32(palette, 0, data, dataIndex);
            RGB565ToBGRA32(palette, 1, data, dataIndex + 2);

            if (color0 > color1)
            {
                palette[2, 0] = Convert.ToByte((palette[0, 0] * 2 + palette[1, 0]) / 3);
                palette[2, 1] = Convert.ToByte((palette[0, 1] * 2 + palette[1, 1]) / 3);
                palette[2, 2] = Convert.ToByte((palette[0, 2] * 2 + palette[1, 2]) / 3);
                palette[2, 3] = 0xFF;

                palette[3, 0] = Convert.ToByte((palette[0, 0] + palette[1, 0] * 2) / 3);
                palette[3, 1] = Convert.ToByte((palette[0, 1] + palette[1, 1] * 2) / 3);
                palette[3, 2] = Convert.ToByte((palette[0, 2] + palette[1, 2] * 2) / 3);
                palette[3, 3] = 0xFF;
            }
            else
            {
                palette[2, 0] = Convert.ToByte((palette[0, 0] + palette[1, 0]) / 2);
                palette[2, 1] = Convert.ToByte((palette[0, 1] + palette[1, 1]) / 2);
                palette[2, 2] = Convert.ToByte((palette[0, 2] + palette[1, 2]) / 2);
                palette[2, 3] = 0xFF;

                //palette[3, 3] = 0xFF;
            }

            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);

            int[,] pix = new int[4, 4];

            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 4; k++)
                {
                    int pixel = (data[i + dataIndex + 4] >> (k * 2)) & 3;
                    pix[i, k] = pixel;
                }

            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                {
                    pixels[y + i, x + k, 0] = palette[pix[i, k], 0];
                    pixels[y + i, x + k, 1] = palette[pix[i, k], 1];
                    pixels[y + i, x + k, 2] = palette[pix[i, k], 2];
                    pixels[y + i, x + k, 3] = palette[pix[i, k], 3];
                }
        }

        public static void DDS_DXT3_GetPixels(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            DDS_DXT1_GetPixels(pixels, x, y, data, dataIndex + 8);

            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);

            byte[] alpha = new byte[16];

            for (int i = 0; i < 8; i++)
            {
                alpha[i * 2] = Table4bitTo8bit[data[dataIndex + i] & 0xF];
                alpha[i * 2 + 1] = Table4bitTo8bit[(data[dataIndex + i] & 0xF0) >> 4];
            }

            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                    pixels[y + i, x + k, 3] = alpha[i * 4 + k];
        }

        public static void DDS_DXT5_GetPixels(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            DDS_DXT1_GetPixels(pixels, x, y, data, dataIndex + 8);

            byte[] alphaPalette = new byte[8];
            alphaPalette[0] = data[dataIndex];
            alphaPalette[1] = data[dataIndex + 1];

            if (alphaPalette[0] > alphaPalette[1])
                for (int i = 1; i < 7; i++)
                    alphaPalette[i + 1] = Convert.ToByte(((7 - i) * alphaPalette[0] + i * alphaPalette[1]) / 7);
            else
            {
                for (int i = 1; i < 5; i++)
                    alphaPalette[i + 1] = Convert.ToByte(((5 - i) * alphaPalette[0] + i * alphaPalette[1]) / 5);
                alphaPalette[6] = 0;
                alphaPalette[7] = 0xFF;
            }

            ulong dat = data[dataIndex + 2];
            for (int i = 3; i < 8; i++)
                dat += (ulong)data[dataIndex + i] << ((i - 2) * 8);

            int[,] pix = new int[4, 4];
            for (int i = 0, ind = 0; i < 4; i++)
                for (int k = 0; k < 4; k++, ind++)
                    pix[i, k] = (int)(dat >> (ind * 3)) & 7;

            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);
            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                    pixels[y + i, x + k, 3] = alphaPalette[pix[i, k]];
        }

        public static void RGB565ToBGRA32(byte[,] palette, int paletteIndex, byte[] data, int dataIndex)
        {
            palette[paletteIndex, 0] = Table5bitTo8bit[(data[dataIndex] & 31)];
            palette[paletteIndex, 1] = Table6bitTo8bit[(data[dataIndex] >> 5) | ((data[dataIndex + 1] & 7) << 3)];
            palette[paletteIndex, 2] = Table5bitTo8bit[(data[dataIndex + 1] >> 3)];
            palette[paletteIndex, 3] = 0xFF;
        }
    }
}