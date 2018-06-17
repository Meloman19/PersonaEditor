using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PersonaEditorLib.Mathematics.Approximation;
using PersonaEditorLib.Mathematics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections;

namespace PersonaEditorLib.Media.Imaging
{
    public static class ImageCompress
    {
        // byte 4bit = (byte)((double)8bit * 15d / 255d + 0.5d);
        public static byte[] Table8bitTo4bit = new byte[]
        {
            0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,1 ,1 ,1 ,1 ,1 ,1 ,1 ,
            1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 ,2 ,2 ,2 ,2 ,2 ,2 ,
            2 ,2 ,2 ,2 ,2 ,2 ,2 ,2 ,2 ,2 ,2 ,3 ,3 ,3 ,3 ,3 ,
            3 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,4 ,4 ,4 ,4 ,
            4 ,4 ,4 ,4 ,4 ,4 ,4 ,4 ,4 ,4 ,4 ,4 ,4 ,5 ,5 ,5 ,
            5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,6 ,6 ,
            6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,6 ,7 ,
            7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,
            8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,8 ,
            8 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,
            9 ,9 ,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
            10,10,10,11,11,11,11,11,11,11,11,11,11,11,11,11,
            11,11,11,11,12,12,12,12,12,12,12,12,12,12,12,12,
            12,12,12,12,12,13,13,13,13,13,13,13,13,13,13,13,
            13,13,13,13,13,13,14,14,14,14,14,14,14,14,14,14,
            14,14,14,14,14,14,14,15,15,15,15,15,15,15,15,15
        };

        // byte 5bit = (byte)((double)8bit * 31d / 255d + 0.5d);
        public static byte[] Table8bitTo5bit = new byte[]
        {
            0 ,0 ,0 ,0 ,0 ,1 ,1 ,1 ,1 ,1 ,1 ,1 ,1 ,2 ,2 ,2 ,
            2 ,2 ,2 ,2 ,2 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,3 ,4 ,4 ,4 ,
            4 ,4 ,4 ,4 ,4 ,4 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,5 ,6 ,6 ,
            6 ,6 ,6 ,6 ,6 ,6 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,7 ,8 ,8 ,
            8 ,8 ,8 ,8 ,8 ,8 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,9 ,10,
            10,10,10,10,10,10,10,11,11,11,11,11,11,11,11,12,
            12,12,12,12,12,12,12,13,13,13,13,13,13,13,13,13,
            14,14,14,14,14,14,14,14,15,15,15,15,15,15,15,15,
            16,16,16,16,16,16,16,16,17,17,17,17,17,17,17,17,
            18,18,18,18,18,18,18,18,18,19,19,19,19,19,19,19,
            19,20,20,20,20,20,20,20,20,21,21,21,21,21,21,21,
            21,22,22,22,22,22,22,22,22,22,23,23,23,23,23,23,
            23,23,24,24,24,24,24,24,24,24,25,25,25,25,25,25,
            25,25,26,26,26,26,26,26,26,26,27,27,27,27,27,27,
            27,27,27,28,28,28,28,28,28,28,28,29,29,29,29,29,
            29,29,29,30,30,30,30,30,30,30,30,31,31,31,31,31
        };

        // byte 6bit = (byte)((double)8bit * 63d / 255d + 0.5d);
        public static byte[] Table8bitYo6bit = new byte[]
        {
            0 ,0 ,0 ,1 ,1 ,1 ,1 ,2 ,2 ,2 ,2 ,3 ,3 ,3 ,3 ,4 ,
            4 ,4 ,4 ,5 ,5 ,5 ,5 ,6 ,6 ,6 ,6 ,7 ,7 ,7 ,7 ,8 ,
            8 ,8 ,8 ,9 ,9 ,9 ,9 ,10,10,10,10,11,11,11,11,12,
            12,12,12,13,13,13,13,14,14,14,14,15,15,15,15,16,
            16,16,16,17,17,17,17,18,18,18,18,19,19,19,19,20,
            20,20,20,21,21,21,21,21,22,22,22,22,23,23,23,23,
            24,24,24,24,25,25,25,25,26,26,26,26,27,27,27,27,
            28,28,28,28,29,29,29,29,30,30,30,30,31,31,31,31,
            32,32,32,32,33,33,33,33,34,34,34,34,35,35,35,35,
            36,36,36,36,37,37,37,37,38,38,38,38,39,39,39,39,
            40,40,40,40,41,41,41,41,42,42,42,42,42,43,43,43,
            43,44,44,44,44,45,45,45,45,46,46,46,46,47,47,47,
            47,48,48,48,48,49,49,49,49,50,50,50,50,51,51,51,
            51,52,52,52,52,53,53,53,53,54,54,54,54,55,55,55,
            55,56,56,56,56,57,57,57,57,58,58,58,58,59,59,59,
            59,60,60,60,60,61,61,61,61,62,62,62,62,63,63,63
        };

        public static bool DDSCompress(int width, int height, byte[] data, PixelBaseFormat fourCC, byte alphaThreshold, out byte[] newData)
        {
            int Width = (int)Math.Ceiling((float)width / 4);
            int Heigth = (int)Math.Ceiling((float)height / 4);

            int step = fourCC == PixelBaseFormat.DXT1 ? 8 : 16;

            byte[,,] pixels = new byte[height, width, 4];
            Buffer.BlockCopy(data, 0, pixels, 0, height * width * 4);

            byte[] compressedData = new byte[Width * Heigth * step];

            for (int y = 0, index = 0; y < height; y += 4)
                for (int x = 0; x < width; x += 4, index += step)
                    if (fourCC == PixelBaseFormat.DXT1)
                        EncodeBC1(pixels, x, y, compressedData, index, false);
                    else if (fourCC == PixelBaseFormat.DXT3)
                        EncodeBC2(pixels, x, y, compressedData, index);
                    else if (fourCC == PixelBaseFormat.DXT5)
                        EncodeBC3(pixels, x, y, compressedData, index);
                    else
                    {
                        newData = null;
                        return false;
                    }

            newData = compressedData;
            return true;
        }

        public static void Calc()
        {
            double[,] data =
            {
                { 0, 0, 0 },
                { 1, 1, 1 },
                { 2, 2, 2 },
                { 3, 3, 3 }
            };
            //double[,] data =
            //{
            //    { 1.794638, 15.15426, 5.10998918E-1 },
            //    { 3.220726, 229.6516, 105.6583692 },
            //    { 5.780040, 3.480201e+3, 1.77699E3 }
            //};


            if (LinearApproximation3D.Start(data, out double[] start, out double[] direction))
            {
                Console.WriteLine($"Start: x={start[0]}, x={start[1]}, x={start[2]}");

                Console.WriteLine($"Start: x={direction[0]}, x={direction[1]}, x={direction[2]}");
            }
            else
            {
                Console.WriteLine("Error");
                return;
            }
        }

        public static void EncodeBC1(byte[,,] pixels, int x, int y, byte[] data, int dataIndex, bool alphaIgnore)
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

        public static void EncodeBC2(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
        {
            EncodeBC1(pixels, x, y, data, dataIndex + 8, true);

            int pixHeight = Math.Min(pixels.GetLength(0) - y, 4);
            int pixWidth = Math.Min(pixels.GetLength(1) - x, 4);

            byte[] alpha = new byte[16];

            for (int i = 0; i < pixHeight; i++)
                for (int k = 0; k < pixWidth; k++)
                    alpha[i * 4 + k] = Table8bitTo4bit[pixels[y + i, x + k, 3]];

            for (int i = 0; i < 8; i++)
                data[dataIndex + i] = Convert.ToByte(alpha[i * 2] + (alpha[i * 2 + 1] << 4));
        }

        public static void EncodeBC3(byte[,,] pixels, int x, int y, byte[] data, int dataIndex)
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
            byte R5 = Table8bitTo5bit[R];
            byte G6 = Table8bitYo6bit[G];
            byte B5 = Table8bitTo5bit[B];

            return Convert.ToUInt16((R5 << 11) + (G6 << 5) + B5);
        }

        private static byte[] BGR32ToRGB565Array(byte B, byte G, byte R)
        {
            byte R5 = Table8bitTo5bit[R];
            byte G6 = Table8bitYo6bit[G];
            byte B5 = Table8bitTo5bit[B];

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