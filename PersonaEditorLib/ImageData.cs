using PersonaEditorLib;
using PersonaEditorLib.Text;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PersonaEditorLib
{
    public struct ImageData
    {
        //public struct Rect
        //{
        //    public Rect(int x, int y, int width, int height)
        //    {
        //        X = x;
        //        Y = y;
        //        Width = width;
        //        Height = height;
        //    }

        //    public int X { get; private set; }
        //    public int Y { get; private set; }
        //    public int Width { get; private set; }
        //    public int Height { get; private set; }
        //}

        private byte[][] Pixels;
        public byte[] Data
        {
            get { return GetData(Pixels, PixelFormat, PixelHeight, Stride); }
        }
        public PixelFormat PixelFormat { get; private set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public int Stride { get; private set; }
        public bool IsEmpty
        {
            get
            {
                if (Pixels == null | PixelWidth == 0) return true;
                else return false;
            }
        }

        public Bitmap GetImageSource(Color[] Pallete)
        {
            if (IsEmpty)
                return null;
            else
                return new Bitmap(PixelWidth, PixelHeight, PixelFormat, Data, Pallete);
        }

        public ImageData(byte[] data, PixelFormat pixelformat, int pixelwidth, int pixelheight)
        {
            PixelFormat = pixelformat;
            PixelWidth = pixelwidth;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
            Pixels = GetPixels(data, pixelformat, pixelwidth, pixelheight);
        }

        public ImageData(byte[][] pixels, PixelFormat pixelformat, int pixelwidth, int pixelheight)
        {
            Pixels = pixels;
            PixelFormat = pixelformat;
            PixelWidth = pixelwidth;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
        }

        public ImageData(PixelFormat pixelformat, int pixelwidth, int pixelheight)
        {
            PixelFormat = pixelformat;
            PixelWidth = pixelwidth;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
            Pixels = GetPixels(new byte[Stride * pixelheight], pixelformat, pixelwidth, pixelheight);
        }

        static byte[][] GetPixels(byte[] Data, PixelFormat PixelFormat, int PixelWidth, int PixelHeight)
        {
            if (PixelFormat.BitsPerPixel == 4)
            {
                byte[][] returned = new byte[PixelHeight][];
                int index = 0;
                for (int i = 0; i < PixelHeight; i++)
                {
                    returned[i] = new byte[PixelWidth];
                    for (int k = 0; k < PixelWidth; k += 2)
                    {
                        returned[i][k] = Convert.ToByte(Data[index] >> 4);
                        if (k + 1 < PixelWidth)
                            returned[i][k + 1] = (Convert.ToByte(Data[index] - (Data[index] >> 4 << 4)));
                        index++;
                    }
                }
                return returned;
            }
            else if (PixelFormat.BitsPerPixel == 8)
            {
                byte[][] returned = new byte[PixelHeight][];
                int index = 0;
                for (int i = 0; i < PixelHeight; i++)
                {
                    returned[i] = new byte[PixelWidth];
                    for (int k = 0; k < PixelWidth; k++)
                    {
                        returned[i][k] = Data[index];
                        index++;
                    }
                }

                return returned;
            }
            else
            {
                //Logging.Write("PersonaEditorLib", "ImageData: Unknown PixelFormat!");
                return null;
            }
        }

        static byte[] GetData(byte[][] Pixels, PixelFormat PixelFormat, int PixelHeight, int Stride)
        {
            byte[] returned = new byte[PixelHeight * Stride];

            if (Pixels != null)
            {
                if (PixelFormat.BitsPerPixel == 4)
                {
                    int index = 0;
                    for (int i = 0; i < Pixels.Length; i++)
                    {
                        for (int k = 0; k < Pixels[i].Length; k++)
                        {
                            if (k + 1 < Pixels[i].Length)
                            {
                                returned[index] = Convert.ToByte((Pixels[i][k] << 4) + Pixels[i][k + 1]);
                                index++;
                            }
                            else
                            {
                                returned[index] = Convert.ToByte(Pixels[i][k] << 4);
                                index++;
                            }
                            k++;
                        }
                    }
                }
                else if (PixelFormat.BitsPerPixel == 8)
                {
                    int index = 0;
                    for (int i = 0; i < Pixels.Length; i++)
                    {
                        for (int k = 0; k < Pixels[i].Length; k++)
                        {
                            returned[index] = Pixels[i][k];
                            index++;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }

            return returned;

        }

        public static ImageData DrawText(IEnumerable<TextBaseElement> text, PersonaFont personaFont, Dictionary<int, byte> Shift, int LineSpacing)
        {
            if (text != null && personaFont != null)
            {
                ImageData returned = new ImageData();
                ImageData line = new ImageData();
                foreach (var a in text)
                {
                    if (a.IsText)
                    {
                        for (int i = 0; i < a.Data.Length; i++)
                        {
                            int index = 0;

                            if (0x20 <= a.Data[i] & a.Data[i] < 0x80)
                                index = a.Data[i];
                            else if (0x80 <= a.Data[i] & a.Data[i] < 0xF0)
                            {
                                index = (a.Data[i] - 0x81) * 0x80 + a.Data[i + 1] + 0x20;
                                i++;
                            }

                            var Glyph = personaFont.GetGlyph(index);

                            if (Glyph.Item1 != null)
                            {
                                byte shift;
                                bool shiftb = Shift.TryGetValue(index, out shift);
                                ImageData glyph = new ImageData(Glyph.Item1, personaFont.PixelFormat, personaFont.Width, personaFont.Height);
                                byte Left = Glyph.Item2.Left;
                                byte Right = Glyph.Item2.Right;
                                glyph = shiftb == false ? Crop(glyph, new Rectangle(Left, 0, Right - Left, glyph.PixelHeight))
                                    : ImageData.Shift(Crop(glyph, new Rectangle(Left, 0, Right - Left, glyph.PixelHeight)), shift);
                                line = MergeLeftRight(line, glyph, 1);
                            }
                        }
                    }
                    else
                    {
                        if (ArrayTool.ByteArrayCompareWithSimplest(a.Data, new byte[] { 0x0A }))
                        {
                            if (returned.IsEmpty)
                            {
                                if (line.IsEmpty)
                                {
                                    returned = new ImageData(PixelFormats.Indexed4, 1, 32);
                                }
                                else
                                {
                                    returned = line;
                                    line = new ImageData();
                                }
                            }
                            else
                            {
                                if (line.IsEmpty)
                                {
                                    returned = MergeUpDown(returned, new ImageData(PixelFormats.Indexed4, 1, 32), LineSpacing);
                                }
                                else
                                {
                                    returned = MergeUpDown(returned, line, LineSpacing);
                                    line = new ImageData();
                                }
                            }
                        }
                    }
                }
                returned = ImageData.MergeUpDown(returned, line, LineSpacing);
                return returned;
            }
            return new ImageData();
        }

        public static ImageData MergeLeftRight(ImageData left, ImageData right, int horizontalshift)
        {
            if (left.Pixels == null)
            {
                return right;
            }
            else if (right.Pixels == null)
            {
                return left;
            }

            byte[][] buffer;
            if (left.Pixels[0].Length == 0)
                buffer = right.Pixels;
            else if (right.Pixels[0].Length == 0)
                buffer = left.Pixels;
            else
                buffer = GetMergePixelsLR(left.Pixels, right.Pixels, horizontalshift);

            return new ImageData(buffer, left.PixelFormat, buffer[0].Length, buffer.Length);
        }

        public static ImageData MergeUpDown(ImageData up, ImageData down, int h)
        {
            if (up.Pixels == null)
            {
                return down;
            }
            else if (down.Pixels == null)
            {
                return up;
            }
            else if (up.Pixels == null & down.Pixels == null)
            {
                return new ImageData();
            }

            byte[][] buffer = GetMergePixelsUD(up.Pixels, down.Pixels, h);
            return new ImageData(buffer, up.PixelFormat, buffer[0].Length, buffer.Length);
        }

        public static ImageData Crop(ImageData image, Rectangle rect)
        {
            if (image.Pixels == null)
                return new ImageData();

            byte[][] buffer = GetCropPixels(image.Pixels, rect);
            return new ImageData(buffer, image.PixelFormat, rect.Width, rect.Height);
        }

        public static ImageData Shift(ImageData image, int shift)
        {
            if (!image.IsEmpty)
            {
                byte[][] buffer = MovePixels(image.Pixels, shift);
                return new ImageData(buffer, image.PixelFormat, buffer[0].Length, buffer.Length);
            }
            else return image;
        }

        static byte[] GetDataS(byte[][] buffer, PixelFormat pixelformat)
        {
            byte[] returned = new byte[buffer.Length * buffer[0].Length];
            if (pixelformat.BitsPerPixel == 4)
            {
                int index = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    for (int k = 0; k < buffer[i].Length; k++)
                    {
                        if (k + 1 < buffer[i].Length)
                        {
                            returned[index] = Convert.ToByte((buffer[i][k] << 4) + buffer[i][k + 1]);
                            index++;
                        }
                        else
                        {
                            returned[index] = Convert.ToByte(buffer[i][k] << 4);
                            index++;
                        }
                        k++;
                    }
                }
            }
            else
            {
                return null;
            }
            return returned;
        }

        static byte[][] MovePixels(byte[][] buffer, int y)
        {
            byte[][] returned = new byte[buffer.Length][];
            for (int i = 0; i < returned.Length; i++)
                returned[i] = new byte[buffer[i].Length];

            for (int i = y; i < returned.Length; i++)
            {
                for (int k = 0; k < returned[i].Length; k++)
                {
                    returned[i][k] = buffer[i - y][k];
                }
            }

            return returned;
        }

        static byte[][] GetMergePixelsUD(byte[][] up, byte[][] down, int h)
        {
            byte[][] returned = new byte[up.Length + down.Length - h][];
            int max = Math.Max(up[0].Length, down[0].Length);
            for (int i = 0; i < returned.Length; i++)
                returned[i] = new byte[max];

            for (int i = 0; i < up.Length; i++)
            {
                for (int k = 0; k < up[i].Length; k++)
                {
                    returned[i][k] = up[i][k];
                }
            }

            for (int i = 0; i < down.Length; i++)
            {
                for (int k = 0; k < down[i].Length; k++)
                {
                    if (returned[i + up.Length - h][k] == 0)
                    {
                        returned[i + up.Length - h][k] = down[i][k];
                    }
                }
            }

            return returned;
        }

        static byte[][] GetMergePixelsLR(byte[][] left, byte[][] right, int shift)
        {
            if (left.Length != right.Length)
            {
                //Logging.Write("PersonaEditorLib", "ImageData: Image doesn't merge");
                return left;
            }

            byte[][] returned = new byte[left.Length][];
            for (int i = 0; i < returned.Length; i++)
            {
                returned[i] = new byte[left[0].Length + right[0].Length - shift];
                int index = 0;
                for (int k = 0; k < left[i].Length; k++)
                {
                    returned[i][index] = left[i][k];
                    index++;
                }
                index -= shift;
                for (int k = 0; k < right[i].Length; k++)
                {
                    returned[i][index] = right[i][k];
                    index++;
                }
            }
            return returned;
        }

        static byte[][] GetCropPixels(byte[][] buffer, Rectangle rect)
        {
            byte[][] returned = new byte[rect.Height][];
            for (int i = 0; i < returned.Length; i++)
            {
                returned[i] = new byte[rect.Width];
                for (int k = 0; k < returned[i].Length; k++)
                {
                    returned[i][k] = buffer[i + rect.Y][k + rect.X];
                }
            }
            return returned;
        }
    }
}