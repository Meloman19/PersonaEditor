using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using PersonaEditorLib.Extension;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Diagnostics;
using PersonaEditorLib.FileStructure.Text;
using System.Collections.ObjectModel;

namespace PersonaEditorLib
{
    public static class Logging
    {
        public static void Write(string name, string text)
        {
            string temp = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            File.AppendAllText(Path.Combine(temp, "Log.log"), DateTime.Now + ": " + text + "\r\n", Encoding.UTF8);
        }

        public static void Write(string name, Exception ex)
        {
            Write(name, "Error:" + ex.ToString());
        }
    }

    public class BitWriter
    {
        private List<byte> finale = new List<byte>();

        private BitArray currentByte = new BitArray(8);

        private int _index = 0;
        private int index
        {
            get { return _index; }
            set
            {
                _index = value;
                if (value == 8)
                {
                    _index = 0;
                    finale.Add(BitArray2Byte());
                }
            }
        }

        public void Write(bool bit)
        {
            currentByte[_index] = bit;
            index++;


        }

        public byte[] GetArray()
        {
            finale.Add(BitArray2Byte());
            return finale.ToArray();
        }

        private byte BitArray2Byte()
        {
            byte result = 0;
            for (byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += currentByte.Get(index) ? m : (byte)0;
            return result;
        }


    }

    public class BinaryReaderBE : BinaryReader
    {
        public BinaryReaderBE(Stream stream) : base(stream) { }

        public override short ReadInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override ushort ReadUInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override uint ReadUInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public override long ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public override ulong ReadUInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }
    }

    public class BinaryWriterBE : BinaryWriter
    {
        public BinaryWriterBE(Stream stream) : base(stream) { }

        public override void Write(short value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            short newvalue = BitConverter.ToInt16(data, 0);
            base.Write(newvalue);
        }

        public override void Write(ushort value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            ushort newvalue = BitConverter.ToUInt16(data, 0);
            base.Write(newvalue);
        }

        public override void Write(int value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            int newvalue = BitConverter.ToInt32(data, 0);
            base.Write(newvalue);
        }

        public override void Write(uint value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            uint newvalue = BitConverter.ToUInt32(data, 0);
            base.Write(newvalue);
        }

        public override void Write(long value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            long newvalue = BitConverter.ToInt64(data, 0);
            base.Write(newvalue);
        }

        public override void Write(ulong value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            ulong newvalue = BitConverter.ToUInt64(data, 0);
            base.Write(newvalue);
        }
    }

    public struct VerticalCut
    {
        public byte Left { get; private set; }
        public byte Right { get; private set; }

        public VerticalCut(byte[] bytes)
        {
            if (bytes != null)
                if (bytes.Length > 2)
                {
                    Left = bytes[0];
                    Right = bytes[1];
                    return;
                }

            Left = 0;
            Right = 0;
        }

        public VerticalCut(byte Left, byte Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public byte[] Get()
        {
            return new byte[2] { Left, Right };
        }
    }

    public struct ImageData
    {
        public struct Rect
        {
            public Rect(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public int X { get; private set; }
            public int Y { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
        }

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

        public ImageSource GetImageSource(BitmapPalette Pallete)
        {
            if (IsEmpty)
                return null;
            else
                return BitmapSource.Create(PixelWidth, PixelHeight, 96, 96, PixelFormat, Pallete, Data, Stride);
        }

        public BitmapSource GetBitmapSource(BitmapPalette Pallete)
        {
            if (IsEmpty)
                return null;
            else
                return BitmapSource.Create(PixelWidth, PixelHeight, 96, 96, PixelFormat, Pallete, Data, Stride);
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
                Logging.Write("PersonaEditorLib", "ImageData: Unknown PixelFormat!");
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

        public static ImageData DrawText(IList<TextBaseElement> text, CharList CharList, Dictionary<int, byte> Shift, int LineSpacing)
        {
            if (text != null)
            {
                ImageData returned = new ImageData();
                ImageData line = new ImageData();
                foreach (var a in text)
                {
                    if (a.Type == "System")
                    {
                        if (Util.ByteArrayCompareWithSimplest(a.Array, new byte[] { 0x0A }))
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
                    else
                    {
                        for (int i = 0; i < a.Array.Length; i++)
                        {
                            CharList.FnMpData fnmp;
                            if (0x20 <= a.Array[i] & a.Array[i] < 0x80)
                            {
                                fnmp = CharList.List.FirstOrDefault(x => x.Index == a.Array[i]);
                            }
                            else if (0x80 <= a.Array[i] & a.Array[i] < 0xF0)
                            {
                                int newindex = (a.Array[i] - 0x81) * 0x80 + a.Array[i + 1] + 0x20;

                                i++;
                                fnmp = CharList.List.FirstOrDefault(x => x.Index == newindex);
                            }
                            else
                            {
                                Console.WriteLine("ASD");
                                fnmp = null;
                            }

                            if (fnmp != null)
                            {
                                byte shift;
                                bool shiftb = Shift.TryGetValue(fnmp.Index, out shift);
                                ImageData glyph = new ImageData(fnmp.Image_data, CharList.PixelFormat, CharList.Width, CharList.Height);
                                byte Left = fnmp.Cut.Left;
                                byte Right = fnmp.Cut.Right;
                                glyph = shiftb == false ? ImageData.Crop(glyph, new ImageData.Rect(Left, 0, Right - Left, glyph.PixelHeight))
                                    : ImageData.Shift(ImageData.Crop(glyph, new ImageData.Rect(Left, 0, Right - Left, glyph.PixelHeight)), shift);
                                line = ImageData.MergeLeftRight(line, glyph, 1);
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

        public static ImageData Crop(ImageData image, Rect rect)
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
                Logging.Write("PersonaEditorLib", "ImageData: Image doesn't merge");
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

        static byte[][] GetCropPixels(byte[][] buffer, Rect rect)
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