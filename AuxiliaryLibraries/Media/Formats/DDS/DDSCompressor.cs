using AuxiliaryLibraries.Mathematic;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    public static class DDSCompressor
    {
        public static bool DDSCompress(int width, int height, byte[] data, DDSFourCC fourCC, out byte[] newData)
        {
            newData = null;
            if (fourCC == DDSFourCC.NONE)
                return false;

            int Width = (int)Math.Ceiling((float)width / 4);
            int Heigth = (int)Math.Ceiling((float)height / 4);

            int step = fourCC == DDSFourCC.DXT1 ? 8 : 16;

            byte[,,] pixels = new byte[height, width, 4];
            Buffer.BlockCopy(data, 0, pixels, 0, height * width * 4);

            byte[] compressedData = new byte[Width * Heigth * step];

            for (int y = 0, index = 0; y < height; y += 4)
                for (int x = 0; x < width; x += 4, index += step)
                    if (fourCC == DDSFourCC.DXT1)
                        EncodeBC1(pixels, x, y, compressedData, index, false);
                    else if (fourCC == DDSFourCC.DXT3)
                        EncodeBC2(pixels, x, y, compressedData, index);
                    else if (fourCC == DDSFourCC.DXT5)
                        EncodeBC3(pixels, x, y, compressedData, index);

            newData = compressedData;
            return true;
        }

        public static bool DDSCompress(Bitmap bitmap, DDSFourCC fourCC, out byte[] newData)
        {
            Bitmap temp = null;
            if (bitmap.PixelFormat == PixelFormats.Bgra32)
                temp = bitmap;
            else
                temp = bitmap.ConvertTo(PixelFormats.Bgra32, null);

            bool returned = DDSCompress(temp.Width, temp.Height, temp.InternalGetData(), fourCC, out byte[] returneddata);
            newData = returneddata;

            return returned;
        }

        private static void EncodeBC1(byte[,,] pixels, int x, int y, byte[] data, int dataIndex, bool alphaIgnore)
        {
            int minHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int minWidth = Math.Min(pixels.GetLength(1) - x, 4);

            byte[,][] texel = new byte[minHeight, minWidth][];
            TexelCopy(pixels, x, y, texel);

            // Count transparent pixel considering alphaThreshold
            List<ushort> differingСolor = new List<ushort>();
            int alphaColors = 0;
            for (int i = 0; i < minHeight; i++)
                for (int k = 0; k < minWidth; k++)
                    if (texel[i, k][3] == 0)
                        alphaColors++;
                    else
                    {
                        ushort temp = BGR32ToRGB565(texel[i, k][0], texel[i, k][1], texel[i, k][2]);
                        if (!differingСolor.Contains(temp))
                            differingСolor.Add(temp);
                    }

            // If all pixels is transparent return transparent data
            if (alphaColors == minHeight * minWidth)
                Buffer.BlockCopy(new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0, data, dataIndex, 8);
            // If pixels have no more than 2 distinct colors (exclude transparents)
            else if (differingСolor.Count <= 2)
            {
                ushort ColorA = differingСolor[0];
                ushort ColorB = differingСolor.Count == 2 ? differingСolor[1] : (ushort)0;

                if ((alphaColors > 0 & ColorA > ColorB) | ColorB > ColorA)
                {
                    var temp = ColorB;
                    ColorB = ColorA;
                    ColorA = temp;
                }

                Buffer.BlockCopy(BitConverter.GetBytes(ColorA), 0, data, dataIndex, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(ColorB), 0, data, dataIndex + 2, 2);

                int[,] bitPixel = new int[4, 4];

                for (int i = 0; i < minHeight; i++)
                    for (int k = 0; k < minWidth; k++)
                        if (texel[i, k][3] == 0)
                            bitPixel[i, k] = 3;
                        else if (BGR32ToRGB565(texel[i, k][0], texel[i, k][1], texel[i, k][2]) == ColorA)
                            bitPixel[i, k] = 0;
                        else if (BGR32ToRGB565(texel[i, k][0], texel[i, k][1], texel[i, k][2]) == ColorB)
                            bitPixel[i, k] = 1;
                        else
                            bitPixel[i, k] = 2;

                IndexTo2Bit(bitPixel, data, dataIndex + 4);
            }
            // Other cases
            else
            {
                // RGB565 palette
                byte[][] palette;

                double[,] ColorMatrix565 = TexelTo565Data(texel, alphaColors);

                LinearApproximation3D.Start(ColorMatrix565, out double[] start, out double[] direction);
                var colors = FindOptimalColors(start, direction, ColorMatrix565);

                ushort ColorA = RGB565ArrayToRGB565(colors.Item1);
                ushort ColorB = RGB565ArrayToRGB565(colors.Item2);

                if (alphaColors > 0 && !alphaIgnore)
                {
                    palette = new byte[3][];
                    if (ColorA > ColorB)
                    {
                        palette[0] = colors.Item2;
                        palette[1] = colors.Item1;
                    }
                    else
                    {
                        palette[0] = colors.Item1;
                        palette[1] = colors.Item2;
                    }

                    palette[2] = new byte[3];
                    palette[2][0] = Convert.ToByte((palette[0][0] + palette[1][0]) / 2);
                    palette[2][1] = Convert.ToByte((palette[0][1] + palette[1][1]) / 2);
                    palette[2][2] = Convert.ToByte((palette[0][2] + palette[1][2]) / 2);
                }
                else
                {
                    palette = new byte[4][];
                    if (ColorA > ColorB)
                    {
                        palette[0] = colors.Item1;
                        palette[1] = colors.Item2;
                    }
                    else
                    {
                        palette[0] = colors.Item2;
                        palette[1] = colors.Item1;
                    }

                    palette[2] = new byte[3];
                    palette[2][0] = Convert.ToByte((palette[0][0] * 2 + palette[1][0]) / 3);
                    palette[2][1] = Convert.ToByte((palette[0][1] * 2 + palette[1][1]) / 3);
                    palette[2][2] = Convert.ToByte((palette[0][2] * 2 + palette[1][2]) / 3);
                    palette[3] = new byte[3];
                    palette[3][0] = Convert.ToByte((palette[0][0] + 2 * palette[1][0]) / 3);
                    palette[3][1] = Convert.ToByte((palette[0][1] + 2 * palette[1][1]) / 3);
                    palette[3][2] = Convert.ToByte((palette[0][2] + 2 * palette[1][2]) / 3);
                }

                Buffer.BlockCopy(BitConverter.GetBytes(RGB565ArrayToRGB565(palette[0])), 0, data, dataIndex, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(RGB565ArrayToRGB565(palette[1])), 0, data, dataIndex + 2, 2);

                int[,] bitPixel = CreateIndexies(palette, texel, alphaIgnore);
                IndexTo2Bit(bitPixel, data, dataIndex + 4);
            }
        }

        private static void EncodeBC2(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            EncodeBC1(pixels, x, y, data, dataIndex + 8, true);

            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);

            byte[] alpha = new byte[16];

            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                    alpha[i * 4 + k] = BitHelper.Table8bitTo4bit[pixels[y + i, x + k, 3]];

            for (int i = 0; i < 8; i++)
                data[dataIndex + i] = Convert.ToByte(alpha[i * 2] + (alpha[i * 2 + 1] << 4));
        }

        private static void EncodeBC3(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            EncodeBC1(pixels, x, y, data, dataIndex + 8, true);
            EncodeBC3Alpha(pixels, x, y, data, dataIndex);
        }

        private static void EncodeBC3Alpha(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);


            byte max = 0;
            byte submax = 0;
            byte min = 255;
            byte submin = 255;

            byte[] alpha = new byte[16];


            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                {
                    byte cur = pixels[y + i, x + k, 3];
                    alpha[i * 4 + k] = cur;

                    if (cur < min)
                    {
                        submin = min;
                        min = cur;
                    }
                    if (cur < submin && cur > min)
                        submin = cur;

                    if (cur > max)
                    {
                        submax = max;
                        max = cur;
                    }
                    if (cur > submax && max > cur)
                        submax = cur;
                }

            // If only one alpha
            if (min == max)
                Buffer.BlockCopy(new byte[] { min, max, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, data, dataIndex, 8);
            // If only two alpha
            else if (submin == max || submax == min)
            {
                byte[] alphaPalette = new byte[] { min, max };
                Buffer.BlockCopy(alphaPalette, 0, data, dataIndex, 2);
                Buffer.BlockCopy(CreateBC3AlphaData(alphaPalette, alpha), 0, data, dataIndex + 2, 6);
            }
            else
            {
                if (submin > submax)
                {

                }
                byte[] alphaPalette = new byte[8];
                if (max == 255 | min == 0)
                {


                    alphaPalette[0] = min == 0 ? submin : min;
                    alphaPalette[1] = max == 255 ? submax : max;
                    for (int i = 1; i < 5; i++)
                        alphaPalette[i + 1] = Convert.ToByte(((5 - i) * alphaPalette[0] + i * alphaPalette[1]) / 5);
                    alphaPalette[6] = 0;
                    alphaPalette[7] = 0xFF;
                }
                else
                {
                    alphaPalette[0] = max;
                    alphaPalette[1] = min;
                    for (int i = 1; i < 7; i++)
                        alphaPalette[i + 1] = Convert.ToByte(((7 - i) * alphaPalette[0] + i * alphaPalette[1]) / 7);
                }

                Buffer.BlockCopy(alphaPalette, 0, data, dataIndex, 2);
                Buffer.BlockCopy(CreateBC3AlphaData(alphaPalette, alpha), 0, data, dataIndex + 2, 6);
            }
        }

        private static byte[] CreateBC3AlphaData(byte[] palette, byte[] alhpadata)
        {
            ulong returned = 0;

            for (int i = 0; i < alhpadata.Length; i++)
            {
                int index = 0;
                byte dist = 255;
                for (int k = 0; k < palette.Length; k++)
                {
                    byte distance = (byte)Math.Abs(alhpadata[i] - palette[k]);
                    if (distance < dist)
                    {
                        dist = distance;
                        index = k;
                    }
                }

                returned += (ulong)index << (3 * i);
            }

            return BitConverter.GetBytes(returned);
        }

        private static int[,] CreateIndexies(byte[][] palette, byte[,][] texel, bool alphaIgnore)
        {
            int[,] bitPixel = new int[4, 4];

            for (int i = 0; i < texel.GetLength(0); i++)
                for (int k = 0; k < texel.GetLength(1); k++)
                    if (!alphaIgnore && texel[i, k][3] == 0)
                        bitPixel[i, k] = 3;
                    else
                    {
                        int index = 0;
                        double dist = double.MaxValue;
                        for (int q = 0; q < palette.Length; q++)
                        {
                            double distance = GeometryOperation.Distance(BGR32ToRGB565Array(texel[i, k][0], texel[i, k][1], texel[i, k][2]), palette[q]);
                            if (distance < dist)
                            {
                                dist = distance;
                                index = q;
                            }
                        }

                        bitPixel[i, k] = index;
                    }

            return bitPixel;
        }

        private static ushort BGR32ToRGB565(byte B, byte G, byte R)
        {
            byte R5 = BitHelper.Table8bitTo5bit[R];
            byte G6 = BitHelper.Table8bitYo6bit[G];
            byte B5 = BitHelper.Table8bitTo5bit[B];

            return Convert.ToUInt16((R5 << 11) + (G6 << 5) + B5);
        }

        private static byte[] BGR32ToRGB565Array(byte B, byte G, byte R)
        {
            byte R5 = BitHelper.Table8bitTo5bit[R];
            byte G6 = BitHelper.Table8bitYo6bit[G];
            byte B5 = BitHelper.Table8bitTo5bit[B];

            return new byte[] { R5, G6, B5 };
        }

        private static ushort RGB565ArrayToRGB565(byte[] array)
        {
            return Convert.ToUInt16((array[0] << 11) + (array[1] << 5) + array[2]);
        }

        private static Tuple<byte[], byte[]> FindOptimalColors(double[] start, double[] direction, double[,] colorMatrix)
        {
            List<double[]> proj = new List<double[]>();
            for (int i = 0; i < colorMatrix.GetLength(0); i++)
                proj.Add(GeometryOperation.Projection(start, direction, colorMatrix, i));

            double[] ColorA = proj[0];
            double[] ColorB = proj[1];
            double DistanceAB = GeometryOperation.Distance(ColorA, ColorB);
            for (int i = 2; i < proj.Count; i++)
            {
                var DistanceAC = GeometryOperation.Distance(ColorA, proj[i]);
                var DistanceBC = GeometryOperation.Distance(ColorB, proj[i]);

                if (DistanceAB < DistanceAC | DistanceAB < DistanceBC)
                {
                    if (DistanceAC > DistanceBC)
                    {
                        DistanceAB = DistanceAC;
                        ColorB = proj[i];
                    }
                    else
                    {
                        DistanceAB = DistanceBC;
                        ColorA = proj[i];
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (ColorA[i] < 0)
                    ColorA[i] = 0;
                if (ColorB[i] < 0)
                    ColorB[i] = 0;
            }

            if (ColorA[0] > 31)
                ColorA[0] = 31;
            if (ColorB[0] > 31)
                ColorB[0] = 31;

            if (ColorA[1] > 63)
                ColorA[1] = 63;
            if (ColorB[1] > 63)
                ColorB[1] = 63;

            if (ColorA[2] > 31)
                ColorA[2] = 31;
            if (ColorB[2] > 31)
                ColorB[2] = 31;

            byte[] colorA = new byte[] { Convert.ToByte(ColorA[0]), Convert.ToByte(ColorA[1]), Convert.ToByte(ColorA[2]) };
            byte[] colorB = new byte[] { Convert.ToByte(ColorB[0]), Convert.ToByte(ColorB[1]), Convert.ToByte(ColorB[2]) };

            return new Tuple<byte[], byte[]>(colorA, colorB);
        }

        private static double[,] TexelTo565Data(byte[,][] texel, int alphaColors)
        {
            int y = texel.GetLength(0);
            int x = texel.GetLength(1);
            double[,] returned = new double[x * y - alphaColors, 3];

            for (int i = 0, index = 0; i < y; i++)
                for (int k = 0; k < x; k++)
                    if (texel[i, k][3] != 0)
                    {
                        var ar = BGR32ToRGB565Array(texel[i, k][0], texel[i, k][1], texel[i, k][2]);
                        returned[index, 0] = ar[0];
                        returned[index, 1] = ar[1];
                        returned[index, 2] = ar[2];
                        index++;
                    }

            return returned;
        }

        private static void TexelCopy(byte[,,] src, int x, int y, byte[,][] dst)
        {
            for (int i = 0; i < dst.GetLength(0); i++)
                for (int k = 0; k < dst.GetLength(1); k++)
                {
                    dst[i, k] = new byte[]
                    {
                        src[y + i, x + k, 0],
                        src[y + i, x + k, 1],
                        src[y + i, x + k, 2],
                        src[y + i, x + k, 3]
                    };
                }
        }

        private static void IndexTo2Bit(int[,] indexes, byte[] data, int offset)
        {
            ulong bits = 0;

            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 4; k++)
                    bits += ((ulong)indexes[i, k] & 3) << (2 * (i * 4 + k));

            Buffer.BlockCopy(BitConverter.GetBytes(bits), 0, data, offset, 4);
        }
    }
}