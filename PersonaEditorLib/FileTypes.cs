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

namespace PersonaEditorLib
{
    public static class Logging
    {
        public static void Write(string name, string text)
        {
            File.AppendAllText(name + ".log", DateTime.Now + ": " + text + "\r\n", Encoding.UTF8);
        }

        public static void Write(string name, Exception ex)
        {
            Write(name, "Error:" + ex.ToString());
        }
    }

    public static class Utils
    {
        public static byte ReverseByte(byte toReverse)
        {
            int temp = (toReverse >> 4) + ((toReverse - (toReverse >> 4 << 4)) << 4);
            return Convert.ToByte(temp);
        }

        public static void ReverseByteInList(IList<byte[]> list)
        {
            foreach (var a in list)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    a[i] = ReverseByte(a[i]);
                }
            }
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

    public delegate void ValueChangedEventHandler();

    public struct VerticalCut
    {
        public byte Left { get; set; }
        public byte Right { get; set; }

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

        public byte[] Get()
        {
            return new byte[2] { Left, Right };
        }
    }

    public struct IndexedByte
    {
        public int Index { get; set; }
        public byte Shift { get; set; }
    }

    public class CharList
    {
        public class FnMpImg
        {
            public int Index { get; set; } = 0;
            public string Char { get; set; } = "";
            public BitmapSource Image { get; set; }
        }

        public class FnMpData
        {
            public int Index { get; set; } = 0;
            public string Char { get; set; } = "";
            public byte[] Image_data { get; set; } = new byte[0];
            public VerticalCut Cut { get; set; }
        }

        public CharList(string FontMap, FileTypes.Text.FNTwork.FNT FNT)
        {
            try
            {
                ReadFNMP(FontMap);
                ReadFONT(FNT);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
            }
        }

        public event ValueChangedEventHandler CharListChanged;

        public void Update()
        {
            CharListChanged?.Invoke();
        }

        public bool Save(string filename)
        {
            try
            {
                WriteFNMP(filename);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
                return false;
            }
            return true;
        }

        private void ReadFONT(FileTypes.Text.FNTwork.FNT FNT)
        {
            Height = FNT.Header.Glyphs.Size1;
            Width = FNT.Header.Glyphs.Size2;
            Palette = FNT.Palette.Pallete;
            if (FNT.Header.Glyphs.BitsPerPixel == 4)
                PixelFormat = PixelFormats.Indexed4;
            else if (FNT.Header.Glyphs.BitsPerPixel == 8)
                PixelFormat = PixelFormats.Indexed8;
            else throw new Exception("ReadFONT Error: unknown PixelFormat");

            var DecList = FNT.Compressed.GetDecompressedData();

            for (int i = 0; i < DecList.Count; i++)
            {
                FnMpData fnmp = List.FirstOrDefault(x => x.Index == i + 32);
                if (fnmp == null)
                    List.Add(new FnMpData { Index = i + 32, Char = "", Cut = FNT.WidthTable.WidthTable[i], Image_data = DecList[i] });
                else
                {
                    fnmp.Cut = FNT.WidthTable.WidthTable[i];
                    fnmp.Image_data = DecList[i];
                }
            }
        }

        private void ReadFNMP(string filename)
        {
            StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open));

            while (sr.EndOfStream == false)
            {
                string str = sr.ReadLine();

                int Index = Convert.ToInt32(str.Substring(0, str.IndexOf('=')));
                string Char = str.Substring(str.IndexOf('=') + 1);
                if (Char.Length > 3)
                    Char = Char.Substring(0, 3);

                FnMpData fnmp = List.FirstOrDefault(x => x.Index == Index);
                if (fnmp == null)
                    List.Add(new FnMpData { Index = Index, Char = Char });
                else
                    fnmp.Char = Char;
            }

            sr.Close();
        }

        private void WriteFNMP(string filename)
        {
            StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create));

            foreach (var CL in List)
            {
                if (CL.Char != "")
                {
                    string str = Convert.ToString(CL.Index) + "=" + Convert.ToString(CL.Char);
                    sw.WriteLine(str);
                    sw.Flush();
                }
            }

            sw.Dispose();
        }

        public string GetChar(int index)
        {
            string returned = "";

            CharList.FnMpData fnmp = List.FirstOrDefault(x => x.Index == index);
            if (fnmp == null)
            {
                returned += "<NCHAR>";
            }
            else
            {
                if (fnmp.Char.Length == 0)
                {
                    returned += "<CHAR>";
                }
                else if (fnmp.Char.Length == 1)
                {
                    returned += fnmp.Char;
                }
                else
                {
                    returned += "<" + fnmp.Char + ">";
                }
            }

            return returned;
        }

        public byte[] GetEncodeBytes(string String)
        {
            List<byte> LB = new List<byte>();
            foreach (var C in String)
            {
                LB.AddChar(C, List);
            }
            return LB.ToArray();
        }

        public List<FnMpData> List { get; set; } = new List<FnMpData>();
        public BitmapPalette Palette { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelFormat PixelFormat { get; set; }
    }
}

namespace PersonaEditorLib.FileTypes
{
    public class ImageData
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

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public byte[][] Pixels { get; set; }
        public byte[] Data
        {
            get { return GetData(); }
        }
        public PixelFormat PixelFormat { get; private set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public int Stride { get; private set; }

        public ImageData(byte[] data, PixelFormat pixelformat, int pixelwidth, int pixelheight)
        {
            PixelFormat = pixelformat;
            PixelWidth = pixelwidth;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
            Pixels = GetPixels(data);
        }

        public ImageData(byte[][] pixels, PixelFormat pixelformat, int pixelwidth, int pixelheight)
        {
            Pixels = pixels;
            PixelFormat = pixelformat;
            PixelWidth = pixelwidth;
            PixelHeight = pixelheight;
            Stride = (pixelformat.BitsPerPixel * PixelWidth + 7) / 8;
        }

        private byte[][] GetPixels(byte[] Data)
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
            else
            {
                Logging.Write("PersonaEditorLib", "ImageData: Unknown PixelFormat!");
                return null;
            }
        }

        private byte[] GetData()
        {
            byte[] returned = new byte[Pixels.Length * Stride];
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
            else
            {
                return null;
            }
            return returned;
        }

        public static ImageData MergeLeftRight(ImageData left, ImageData right)
        {
            if (left == null)
            {
                return right;
            }
            else if (right == null)
            {
                return left;
            }

            byte[][] buffer = GetMergePixelsLR(left.Pixels, right.Pixels);
            return new ImageData(buffer, left.PixelFormat, buffer[0].Length, buffer.Length);
        }

        public static ImageData MergeUpDown(ImageData up, ImageData down, int h)
        {
            if (up == null)
            {
                return down;
            }
            else if (down == null)
            {
                return up;
            }
            else if (up == null & down == null)
            {
                return null;
            }

            byte[][] buffer = GetMergePixelsUD(up.Pixels, down.Pixels, h);
            return new ImageData(buffer, up.PixelFormat, buffer[0].Length, buffer.Length);
        }

        public static ImageData Crop(ImageData image, Rect rect)
        {
            byte[][] buffer = image.Pixels;
            if (buffer == null)
                return image;

            buffer = GetCropPixels(buffer, rect);
            return new ImageData(buffer, image.PixelFormat, rect.Width, rect.Height);
        }

        public static ImageData Shift(ImageData image, int shift)
        {
            byte[][] buffer = MovePixels(image.Pixels, shift);
            return new ImageData(buffer, image.PixelFormat, buffer[0].Length, buffer.Length);
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

        static byte[][] GetMergePixelsLR(byte[][] left, byte[][] right)
        {
            if (left.Length != right.Length)
            {
                Logging.Write("PersonaEditorLib", "ImageData: Image doesn't merge");
                return left;
            }

            byte[][] returned = new byte[left.Length][];
            for (int i = 0; i < returned.Length; i++)
            {
                returned[i] = new byte[left[0].Length + right[0].Length];
                int index = 0;
                for (int k = 0; k < left[i].Length; k++)
                {
                    returned[i][index] = left[i][k];
                    index++;
                }
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

        static byte[][] GetPixelsS(ImageData image)
        {
            if (image.PixelFormat.BitsPerPixel == 4)
            {
                byte[][] returned = new byte[image.PixelHeight][];
                int index = 0;
                for (int i = 0; i < image.PixelHeight; i++)
                {
                    returned[i] = new byte[image.PixelWidth];
                    for (int k = 0; k < image.PixelWidth; k += 2)
                    {
                        returned[i][k] = Convert.ToByte(image.Data[index] >> 4);
                        if (k + 1 < image.PixelWidth)
                            returned[i][k + 1] = (Convert.ToByte(image.Data[index] - (image.Data[index] >> 4 << 4)));
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
    }

    public class Text
    {
        public class FNTwork
        {
            public class FNT
            {
                public class FontHeader
                {
                    public class Glyph
                    {
                        public Glyph(BinaryReader reader)
                        {
                            Count = reader.ReadUInt16();
                            Size1 = reader.ReadUInt16();
                            Size2 = reader.ReadUInt16();
                            ByteSize = reader.ReadUInt16();
                        }

                        public ushort Count { get; set; }
                        public ushort Size1 { get; set; }
                        public ushort Size2 { get; set; }
                        public ushort ByteSize { get; set; }

                        public byte BitsPerPixel { get { return Convert.ToByte((double)(ByteSize * 8) / (Size1 * Size2)); } }
                        public int NumberOfColor { get { return Convert.ToInt32(Math.Pow(2, BitsPerPixel)); } }

                        public void Get(BinaryWriter writer)
                        {
                            writer.Write(Count);
                            writer.Write(Size1);
                            writer.Write(Size2);
                            writer.Write(ByteSize);
                        }
                    }

                    public FontHeader(BinaryReader reader)
                    {
                        HeaderSize = reader.ReadInt32();
                        FileSize = reader.ReadInt32();
                        UnknownH = reader.ReadBytes(6);
                        Glyphs = new Glyph(reader);
                        UnknownUShort = reader.ReadUInt16();
                        Last = reader.ReadBytes(8);
                    }

                    public int HeaderSize { get; set; }
                    public int FileSize { get; set; }
                    public byte[] UnknownH { get; set; }
                    public Glyph Glyphs { get; set; }
                    public ushort UnknownUShort { get; set; }
                    public byte[] Last { get; set; }

                    public void Get(BinaryWriter writer)
                    {
                        writer.Write(HeaderSize);
                        writer.Write(FileSize);
                        writer.Write(UnknownH);
                        Glyphs.Get(writer);
                        writer.Write(UnknownUShort);
                        writer.Write(Last);
                    }
                }

                public class FontPalette
                {
                    public FontPalette(BinaryReader reader, int NumberOfColor)
                    {
                        List<Color> Colors = new List<Color>();
                        for (int i = 0; i < NumberOfColor; i++)
                            Colors.Add(new Color()
                            {
                                R = reader.ReadByte(),
                                G = reader.ReadByte(),
                                B = reader.ReadByte(),
                                A = reader.ReadByte()
                            });
                        Pallete = new BitmapPalette(Colors);
                    }

                    public BitmapPalette Pallete { get; set; }

                    public int Size()
                    {
                        return Pallete.Colors.Count * 4;
                    }

                    public void Get(BinaryWriter writer)
                    {
                        foreach (var color in Pallete.Colors)
                        {
                            writer.Write(color.R);
                            writer.Write(color.G);
                            writer.Write(color.B);
                            writer.Write(color.A);
                        }
                    }
                }

                public class FontWidthTable
                {
                    public FontWidthTable(BinaryReader reader)
                    {
                        uint Size = reader.ReadUInt32();

                        for (int i = 0; i < Size / 2; i++)
                        {
                            VerticalCut cut = new VerticalCut();
                            cut.Left = reader.ReadByte();
                            cut.Right = reader.ReadByte();
                            WidthTable.Add(cut);
                        }
                    }

                    public List<VerticalCut> WidthTable = new List<VerticalCut>();

                    public int Size()
                    {
                        return WidthTable.Count * 4 + 4;
                    }

                    public void Get(BinaryWriter writer)
                    {
                        writer.Write(WidthTable.Count * 4);
                        foreach (var glyph in WidthTable)
                            writer.Write(glyph.Get());
                    }
                }

                public class FontUnknown
                {
                    public FontUnknown(BinaryReader reader)
                    {
                        uint Size = reader.ReadUInt32();
                        for (int i = 0; i < Size; i++)
                            Unknown.Add(reader.ReadByte());
                    }

                    public List<byte> Unknown { get; set; } = new List<byte>();

                    public int Size()
                    {
                        return Unknown.Count + 4;
                    }

                    public void Get(BinaryWriter writer)
                    {
                        writer.Write(Unknown.Count);
                        foreach (var b in Unknown)
                            writer.Write(b);
                    }
                }

                public class FontReserved
                {
                    public FontReserved(BinaryReader reader, uint size)
                    {
                        for (int i = 0; i < size; i++)
                            Reserved.Add(reader.ReadInt32());
                    }

                    public List<int> Reserved = new List<int>();

                    public int Size()
                    {
                        return Reserved.Count * 4;
                    }

                    public void Get(BinaryWriter writer)
                    {
                        foreach (var b in Reserved)
                            writer.Write(b);
                    }
                }

                public class FontCompressed
                {
                    private class CompressedHeader
                    {
                        public CompressedHeader(BinaryReader reader)
                        {
                            HeaderSize = reader.ReadInt32();
                            DictionarySize = reader.ReadInt32();
                            CompressedBlockSize = reader.ReadInt32();
                            Unknown = reader.ReadInt32();
                            BytesPerGlyph = reader.ReadUInt16();
                            UnknownU = reader.ReadUInt16();
                            GlyphTableCount = reader.ReadInt32();
                            GlyphPosTableSize = reader.ReadInt32();
                            UncompressedFontSize = reader.ReadInt32();
                        }

                        public int HeaderSize { get; set; }
                        public int DictionarySize { get; set; }
                        public int CompressedBlockSize { get; set; }
                        public int Unknown { get; set; }
                        public ushort BytesPerGlyph { get; set; }
                        public ushort UnknownU { get; set; }
                        public int GlyphTableCount { get; set; }
                        public int GlyphPosTableSize { get; set; }
                        public int UncompressedFontSize { get; set; }

                        public void Get(BinaryWriter writer)
                        {
                            writer.Write(HeaderSize);
                            writer.Write(DictionarySize);
                            writer.Write(CompressedBlockSize);
                            writer.Write(Unknown);
                            writer.Write(BytesPerGlyph);
                            writer.Write(UnknownU);
                            writer.Write(GlyphTableCount);
                            writer.Write(GlyphPosTableSize);
                            writer.Write(UncompressedFontSize);
                        }
                    }

                    private class CompressedDictionary
                    {
                        public CompressedDictionary(BinaryReader reader, int size)
                        {
                            List<ushort[]> temp = new List<ushort[]>();
                            for (int i = 0; i < size / 6; i++)
                            {
                                ushort[] added = new ushort[3];
                                added[0] = reader.ReadUInt16();
                                added[1] = reader.ReadUInt16();
                                added[2] = reader.ReadUInt16();
                                temp.Add(added);
                            }
                            Dictionary = temp.ToArray();
                        }

                        public ushort[][] Dictionary { get; set; }

                        public void Get(BinaryWriter writer)
                        {
                            foreach (var b in Dictionary)
                                foreach (var a in b)
                                    writer.Write(a);
                        }
                    }

                    private class CompressedGlyphTable
                    {
                        public CompressedGlyphTable(BinaryReader reader, int count)
                        {
                            for (int i = 0; i < count; i++)
                                Table.Add(reader.ReadInt32());
                        }

                        public List<int> Table = new List<int>();

                        public void Get(BinaryWriter writer)
                        {
                            foreach (var a in Table)
                                writer.Write(a);
                        }
                    }

                    public FontCompressed(BinaryReader reader)
                    {
                        Header = new CompressedHeader(reader);
                        Dictionary = new CompressedDictionary(reader, Header.DictionarySize);
                        GlyphTable = new CompressedGlyphTable(reader, Header.GlyphTableCount);
                        CompressedData = reader.ReadBytes(Header.CompressedBlockSize);
                    }

                    private CompressedHeader Header { get; set; }
                    private CompressedDictionary Dictionary { get; set; }
                    private CompressedGlyphTable GlyphTable { get; set; }
                    private byte[] CompressedData { get; set; }

                    public List<byte[]> GetDecompressedData()
                    {
                        List<byte[]> returned = new List<byte[]>();
                        List<byte> Decompress = new List<byte>();


                        int temp = 0;

                        for (int i = 0; i < CompressedData.Length; i++)
                        {
                            int Byte = CompressedData[i];
                            for (int k = 0; k < 8; k++)
                            {
                                temp = Dictionary.Dictionary[temp][Byte % 2 + 1];
                                Byte = Byte >> 1;

                                if (Dictionary.Dictionary[temp][1] == 0)
                                {
                                    if (Decompress.Count == Header.BytesPerGlyph)
                                    {
                                        returned.Add(Decompress.ToArray());
                                        Decompress = new List<byte>();
                                    }
                                    Decompress.Add((byte)(Dictionary.Dictionary[temp][2]));
                                    temp = 0;
                                }
                            }
                        }

                        Logging.Write("PersonaEditor", "Get decompressed font");


                        return returned;
                    }

                    public void CompressData(List<byte[]> list)
                    {
                        BitWriter BitW = new BitWriter();

                        int DictPart = FindDictPart();

                        List<bool> returned = new List<bool>();

                        for (int i1 = list.Count - 1; i1 >= 0; i1--)
                            for (int i2 = list[i1].Length - 1; i2 >= 0; i2--)
                            {
                                int s4 = list[i1][i2];
                                int i = 1;

                                while (Dictionary.Dictionary[i][2] != s4)
                                {
                                    i++;
                                    if (Dictionary.Dictionary[i - 1][2] == 0)
                                    {
                                        if ((s4 >> 4) > ((s4 << 4) >> 4))
                                        {
                                            s4 = s4 - (1 << 4);
                                        }
                                        else
                                        {
                                            s4 = s4 - 1;
                                        }
                                        i = 1;
                                    }
                                }
                                int v0 = i;
                                while (v0 != 0)
                                    v0 = FindDictIndex(v0, DictPart, returned);
                            }

                        for (int i = returned.Count - 1; i >= 0; i--)
                            BitW.Write(returned[i]);


                        CompressedData = BitW.GetArray();
                        Header.CompressedBlockSize = Convert.ToInt32(CompressedData.Length);
                        WriteGlyphPosition();
                    }

                    private int FindDictIndex(int v0, int DictPart, List<bool> list)
                    {
                        if (Dictionary.Dictionary[0][1] == v0)
                        {
                            list.Add(false);
                            return 0;
                        }
                        else if (Dictionary.Dictionary[0][2] == v0)
                        {
                            list.Add(true);
                            return 0;
                        }

                        for (int i = DictPart + 1; i < Dictionary.Dictionary.Length; i++)
                        {
                            if (Dictionary.Dictionary[i][1] == v0)
                            {
                                list.Add(false);
                                return i;
                            }
                            else if (Dictionary.Dictionary[i][2] == v0)
                            {
                                list.Add(true);
                                return i;
                            }
                        }
                        return -1;
                    }

                    private int FindDictPart()
                    {
                        for (int i = 1; i < Dictionary.Dictionary.Length; i++)
                        {
                            if (Dictionary.Dictionary[i][2] == 0)
                            {
                                return i;
                            }
                        }
                        return -1;
                    }

                    private void WriteGlyphPosition()
                    {
                        List<int> GlyphNewPosition = new List<int>();

                        int temp = 0;

                        int a = 0;
                        int b = 0;

                        foreach (var by in CompressedData)
                        {
                            int s4 = by;
                            a++;
                            for (int i = 0; i < 8; i++)
                            {
                                temp = Dictionary.Dictionary[temp][s4 % 2 + 1];
                                s4 = s4 >> 1;

                                if (Dictionary.Dictionary[temp][1] == 0)
                                {
                                    if (b % Header.BytesPerGlyph == 0)
                                    {
                                        GlyphNewPosition.Add(((a - 1) << 3) + i);
                                    }
                                    b++;
                                    temp = 0;
                                }
                            }
                        }

                        Header.Unknown = GlyphNewPosition[GlyphNewPosition.Count - 1];
                        GlyphTable.Table = GlyphNewPosition;

                        Logging.Write("PersonaEditorLib", "Writed new Glyph Position Table");
                    }

                    public int Size()
                    {
                        return Header.HeaderSize + Header.DictionarySize + Header.GlyphTableCount * 4 + Header.CompressedBlockSize;
                    }

                    public void Get(BinaryWriter writer)
                    {
                        Header.Get(writer);
                        Dictionary.Get(writer);
                        GlyphTable.Get(writer);
                        writer.Write(CompressedData);
                    }
                }

                public FontHeader Header { get; set; }
                public FontPalette Palette { get; set; }
                public FontWidthTable WidthTable { get; private set; }
                public FontUnknown Unknown { get; set; }
                public FontReserved Reserved { get; set; }
                public FontCompressed Compressed { get; private set; }

                public FNT(Stream stream, long position)
                {
                    stream.Position = position;
                    BinaryReader reader = new BinaryReader(stream);

                    Header = new FontHeader(reader);
                    Palette = new FontPalette(reader, Header.Glyphs.NumberOfColor);
                    WidthTable = new FontWidthTable(reader);
                    Unknown = new FontUnknown(reader);
                    Reserved = new FontReserved(reader, Header.Glyphs.Count);
                    Compressed = new FontCompressed(reader);

                    Logging.Write("PersonaEditor", "Get data from FONT0.FNT");
                }

                public int Size()
                {
                    return Header.HeaderSize + Palette.Size() + WidthTable.Size() + Unknown.Size() + Reserved.Size() + Compressed.Size();
                }

                public void Get(BinaryWriter writer)
                {
                    Header.Get(writer);
                    Palette.Get(writer);
                    WidthTable.Get(writer);
                    Unknown.Get(writer);
                    Reserved.Get(writer);
                    Compressed.Get(writer);
                }
            }

            public FNTwork(FNT FNT)
            {
                this.FNT0 = FNT;
            }

            public FNTwork(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read), 0)
            {

            }

            public FNTwork(Stream stream, long position)
            {
                FNT0 = new FNT(stream, position);
                WidthTable = new FNTWorkWidthTable(FNT0.WidthTable);
            }

            private FNT FNT0;

            public class FNTWorkWidthTable
            {
                private FNT.FontWidthTable local;

                public FNTWorkWidthTable(FNT.FontWidthTable WidthTable)
                {
                    local = WidthTable;
                }

                public void WriteToFile(string FileName)
                {
                    XDocument xDoc = new XDocument();
                    XElement WT = new XElement("WidthTable");
                    xDoc.Add(WT);

                    XElement Line = null;
                    int k = 0;
                    for (int i = 0; i < local.WidthTable.Count; i++)
                    {
                        if (i % 16 == 0)
                        {
                            k++;
                            Line = new XElement("Line_" + k);
                            WT.Add(Line);
                        }
                        XElement Glyph = new XElement("Glyph_" + ((i % 16) + 1));
                        Line.Add(Glyph);
                        Glyph.Add(new XElement("LeftCut", local.WidthTable[i].Left));
                        Glyph.Add(new XElement("RightCut", local.WidthTable[i].Right));
                    }

                    xDoc.Save(FileName);
                    Logging.Write("PersonaEditorLib", "Width Table was created.");

                }

                public void ReadFromFile(string FileName)
                {
                    XDocument xDoc = XDocument.Load(FileName);
                    XElement WT = xDoc.Element("WidthTable");

                    int index = 0;
                    foreach (var line in WT.Elements())
                    {
                        int lineindex = Convert.ToInt32(line.Name.LocalName.Split('_')[1]);
                        foreach (var glyph in line.Elements())
                        {
                            int glyphindex = Convert.ToInt32(glyph.Name.LocalName.Split('_')[1]);
                            index = (lineindex - 1) * 16 + (glyphindex - 1);
                            local.WidthTable[index] = new VerticalCut() { Left = Convert.ToByte(glyph.Element("LeftCut").Value), Right = Convert.ToByte(glyph.Element("RightCut").Value) };
                        }
                    }

                    Logging.Write("PersonaEditorLib", "Width Table was writed. Get " + index + " glyphs");
                }
            }

            public FNTWorkWidthTable WidthTable { get; private set; }

            public BitmapSource GetFontImage()
            {
                List<byte[]> data = FNT0.Compressed.GetDecompressedData();

                PixelFormat currentPF;
                if (FNT0.Header.Glyphs.BitsPerPixel == 4)
                {
                    currentPF = PixelFormats.Indexed4;
                    Utils.ReverseByteInList(data);
                }
                else if (FNT0.Header.Glyphs.BitsPerPixel == 8)
                    currentPF = PixelFormats.Indexed8;
                else return null;

                ImageData BMP = null;
                ImageData Line = null;

                int glyphindex = 0;
                foreach (var a in data)
                {
                    Line = ImageData.MergeLeftRight(Line, new ImageData(a, currentPF, FNT0.Header.Glyphs.Size1, FNT0.Header.Glyphs.Size2));
                    glyphindex++;
                    if (glyphindex % 16 == 0)
                    {
                        BMP = ImageData.MergeUpDown(BMP, Line, 0);
                        Line = null;
                    }
                }
                if (Line != null)
                    BMP = ImageData.MergeUpDown(BMP, Line, 0);

                return BitmapSource.Create(BMP.PixelWidth, BMP.PixelHeight, 96, 96, BMP.PixelFormat, FNT0.Palette.Pallete, BMP.Data, BMP.Stride);
            }

            public void SetFontImage(BitmapSource image)
            {
                int stride = (image.Format.BitsPerPixel * image.PixelWidth + 7) / 8;
                byte[] data = new byte[image.PixelHeight * stride];
                image.CopyPixels(data, stride, 0);

                ImageData BMP = new ImageData(data, image.Format, image.PixelWidth, image.PixelHeight);
                List<byte[]> BMPdata = new List<byte[]>();

                int row = 0;
                int column = 0;

                for (int i = 0; i < FNT0.Header.Glyphs.Count; i++)
                {
                    BMPdata.Add(ImageData.Crop(BMP, new ImageData.Rect(column * FNT0.Header.Glyphs.Size1,
                        row * FNT0.Header.Glyphs.Size2, FNT0.Header.Glyphs.Size1, FNT0.Header.Glyphs.Size2)).Data);
                    column++;
                    if (column == 16)
                    {
                        row++;
                        column = 0;
                    }
                }

                if (FNT0.Header.Glyphs.BitsPerPixel == 4)
                    Utils.ReverseByteInList(BMPdata);

                FNT0.Compressed.CompressData(BMPdata);
            }

            public MemoryStream GetFont()
            {
                MemoryStream returned = new MemoryStream();
                BinaryWriter BW = new BinaryWriter(returned);

                FNT0.Header.FileSize = 1 + FNT0.Header.HeaderSize + FNT0.Palette.Size() + FNT0.Reserved.Size() + FNT0.Compressed.Size();

                FNT0.Get(BW);

                returned.Position = 0;
                return returned;
            }
        }

        public class PMDwork
        {
            public class PMD1
            {
                bool IsLittleEndian { get; set; } = true;

                public class PMDHeader
                {
                    public int EmptyHead { get; set; }
                    public int Size { get; private set; }
                    public long Name { get; private set; }
                    public int TableLineCount { get; private set; }
                    private int[] Unknown { get; set; }

                    public PMDHeader(BinaryReader reader)
                    {
                        EmptyHead = reader.ReadInt32();
                        Size = reader.ReadInt32();
                        Name = reader.ReadInt64();
                        TableLineCount = reader.ReadInt32();
                        Unknown = new int[3] { reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32() };
                    }

                    public MemoryStream GetMS(bool IsLittleEndian)
                    {
                        BinaryWriter BW;

                        if (IsLittleEndian)
                            BW = new BinaryWriter(new MemoryStream());
                        else
                            BW = new BinaryWriterBE(new MemoryStream());

                        BW.Write(EmptyHead);
                        BW.Write(Size);
                        BW.Write(Name);
                        BW.Write(TableLineCount);
                        BW.WriteInt32Array(Unknown);

                        byte[] buffer = new byte[BW.BaseStream.Length];
                        BW.BaseStream.Position = 0;
                        BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                        return new MemoryStream(buffer);
                    }

                    public void Shift(int shift)
                    {
                        Size += shift;
                    }
                }

                class PMDTable
                {
                    public class Element
                    {
                        public int Index { get; set; } = -1;
                        public int Size { get; set; } = 0;
                        public int Count { get; set; } = 0;
                        public int Position { get; set; } = 0;

                        public Element(int index, int size, int count, int position)
                        {
                            Index = index;
                            Size = size;
                            Count = count;
                            Position = position;
                        }
                    }

                    public List<Element> PM1Table { get; private set; } = new List<Element>();

                    public PMDTable(int[][] array)
                    {
                        for (int i = 0; i < array.Length; i++)
                            PM1Table.Add(new Element(array[i][0], array[i][1], array[i][2], array[i][3]));
                    }

                    public MemoryStream GetMS(bool IsLittleEndian)
                    {
                        BinaryWriter BW;

                        if (IsLittleEndian)
                            BW = new BinaryWriter(new MemoryStream());
                        else
                            BW = new BinaryWriterBE(new MemoryStream());

                        foreach (var line in PM1Table)
                        {
                            BW.Write(line.Index);
                            BW.Write(line.Size);
                            BW.Write(line.Count);
                            BW.Write(line.Position);
                        }

                        byte[] buffer = new byte[BW.BaseStream.Length];
                        BW.BaseStream.Position = 0;
                        BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                        return new MemoryStream(buffer);
                    }

                    public void Shift(int shift)
                    {
                        PM1Table.FindAll(x => x.Index > 0x6).ForEach(a => a.Position += shift);
                    }
                }

                enum Map
                {
                    FileList = 0x1,
                    T3HeadList = 0x2,
                    RMDHeadList = 0x3,
                    MSG = 0x6,
                    EPLHeadList = 0x7,
                    EPL = 0x8,
                    RMD = 0x9
                }

                PMDHeader Header;
                PMDTable Table;

                class PM1element
                {
                    public Map Type { get; set; }
                    public List<MemoryStream> StreamList { get; set; }
                }

                List<PM1element> PM1List = new List<PM1element>();

                public PMD1(Stream stream, long position, bool IsLittleEndian)
                {
                    this.IsLittleEndian = IsLittleEndian;

                    BinaryReader BR;

                    if (IsLittleEndian)
                        BR = new BinaryReader(stream);
                    else
                        BR = new BinaryReaderBE(stream);

                    stream.Position = position;
                    Header = new PMDHeader(BR);
                    Table = new PMDTable(BR.ReadInt32ArrayArray(Header.TableLineCount, 4));


                    foreach (var element in Table.PM1Table)
                    {
                        if (element.Size * element.Count > 0)
                        {
                            PM1element temp = new PM1element()
                            {
                                Type = (Map)element.Index,
                                StreamList = GetListMS(BR, element)
                            };
                            PM1List.Add(temp);
                        }
                    }
                }

                private List<MemoryStream> GetListMS(BinaryReader BR, PMDTable.Element line)
                {
                    List<MemoryStream> returned = new List<MemoryStream>();
                    BR.BaseStream.Position = line.Position;

                    for (int i = 0; i < line.Count; i++)
                        returned.Add(new MemoryStream(BR.ReadBytes(line.Size)));

                    return returned;
                }

                public void SaveNew(string FileName)
                {
                    using (FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        Header.GetMS(IsLittleEndian).CopyTo(FS);
                        Table.GetMS(IsLittleEndian).CopyTo(FS);
                        foreach (var a in PM1List)
                        {
                            foreach (var b in a.StreamList)
                            {
                                b.Position = 0;
                                b.CopyTo(FS);
                            }
                        }
                    }
                }

                public void SetNewMSG1(MemoryStream NewMSG)
                {
                    int SizeShift = ChangeMSG(NewMSG);
                    ShiftEPLHeader(SizeShift);
                    ShiftRMDHeader(SizeShift);
                    Table.Shift(SizeShift);
                    Header.Shift(SizeShift);
                }

                int ChangeMSG(MemoryStream NewMSG)
                {
                    PM1element temp = PM1List.Find(x => x.Type == Map.MSG);
                    if (temp != null)
                    {
                        if (temp.StreamList.Count == 1)
                        {

                            MemoryStream newMSG = new MemoryStream();

                            NewMSG.CopyTo(newMSG);
                            while (newMSG.Length % 16 != 0)
                            {
                                newMSG.WriteByte(0);
                            }

                            int SizeShift = Convert.ToInt32(newMSG.Length - temp.StreamList.First().Length);
                            temp.StreamList.Clear();
                            temp.StreamList.Add(newMSG);

                            Table.PM1Table.Find(x => x.Index == 0x6).Size = (int)newMSG.Length;

                            return SizeShift;
                        }
                        else if (temp.StreamList.Count > 1)
                        {
                            Logging.Write("PersonaEditorLib", "Exception: 2 or more MSG");
                            return -1;
                        }
                        else
                        {
                            Logging.Write("PersonaEditorLib", "Exception: 0 MSG");
                            return -1;
                        }
                    }
                    else
                    {
                        Logging.Write("PersonaEditorLib", "File does not contain MSG");
                        return -1;
                    }
                }

                private void ShiftEPLHeader(int shift)
                {
                    PM1element temp = PM1List.Find(x => x.Type == Map.EPLHeadList);
                    if (temp != null)
                    {
                        if (temp.StreamList.Count >= 1)
                        {
                            foreach (var EPL in temp.StreamList)
                            {
                                BinaryReader BR;

                                if (IsLittleEndian)
                                    BR = new BinaryReader(EPL);
                                else
                                    BR = new BinaryReaderBE(EPL);

                                BR.BaseStream.Position = 4;
                                int Size = BR.ReadInt32();
                                Size += shift;
                                BR.BaseStream.Position = 4;

                                BinaryWriter BW;

                                if (IsLittleEndian)
                                    BW = new BinaryWriter(EPL);
                                else
                                    BW = new BinaryWriterBE(EPL);

                                BW.Write(Size);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Exception: 0 EPL Header");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("File does not contain EPL Header");
                        return;
                    }
                }

                private void ShiftRMDHeader(int shift)
                {
                    PM1element temp = PM1List.Find(x => x.Type == Map.RMDHeadList);
                    if (temp != null)
                    {
                        if (temp.StreamList.Count >= 1)
                        {
                            foreach (var RMD in temp.StreamList)
                            {
                                BinaryReader BR;

                                if (IsLittleEndian)
                                    BR = new BinaryReader(RMD);
                                else
                                    BR = new BinaryReaderBE(RMD);

                                BR.BaseStream.Position = 0x10;
                                int Size = BR.ReadInt32();
                                Size += shift;
                                BR.BaseStream.Position = 0x10;

                                BinaryWriter BW;

                                if (IsLittleEndian)
                                    BW = new BinaryWriter(RMD);
                                else
                                    BW = new BinaryWriterBE(RMD);

                                BW.Write(Size);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Exception: 0 EPL Header");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("File does not contain EPL Header");
                        return;
                    }
                }

                public byte[] GetMSG()
                {
                    return PM1List.Find(x => x.Type == Map.MSG).StreamList.First().ToArray();
                }
            }

            private PMD1 PM1;

            public PMDwork(PMD1 PM1)
            {
                this.PM1 = PM1;
            }

            public PMDwork(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read), 0)
            {

            }

            public PMDwork(Stream stream, long position)
            {
                PM1 = new PMD1(stream, position, true);
            }

            public MemoryStream GetMSG()
            {
                return new MemoryStream(PM1.GetMSG());
            }
        }

        public class MSG1work
        {

        }
    }
}

namespace PersonaEditorLib.FileStructure
{
    public class FLW0
    {
        bool IsLittleEndian { get; set; } = true;

        class Header
        {
            private int Head { get; set; }
            public int Size { get; private set; }
            public long Name { get; private set; }
            public int TableLineCount { get; private set; }
            private int[] Unknown { get; set; }

            public Header(BinaryReader BR)
            {
                Head = BR.ReadInt32();
                Size = BR.ReadInt32();
                Name = BR.ReadInt64();
                TableLineCount = BR.ReadInt32();
                Unknown = new int[3] { BR.ReadInt32(), BR.ReadInt32(), BR.ReadInt32() };
            }

            public MemoryStream GetMS(bool IsLittleEndian)
            {
                BinaryWriter BW;

                if (IsLittleEndian)
                    BW = new BinaryWriter(new MemoryStream());
                else
                    BW = new BinaryWriterBE(new MemoryStream());

                BW.Write(Head);
                BW.Write(Size);
                BW.Write(Name);
                BW.Write(TableLineCount);
                BW.WriteInt32Array(Unknown);

                byte[] buffer = new byte[BW.BaseStream.Length];
                BW.BaseStream.Position = 0;
                BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                return new MemoryStream(buffer);
            }

            public void Shift(int shift)
            {
                Size += shift;
            }
        }

        class Table
        {
            public class Element
            {
                public int Index { get; set; } = -1;
                public int Size { get; set; } = 0;
                public int Count { get; set; } = 0;
                public int Position { get; set; } = 0;

                public Element(int index, int size, int count, int position)
                {
                    Index = index;
                    Size = size;
                    Count = count;
                    Position = position;
                }
            }

            public List<Element> FLW0Table { get; private set; } = new List<Element>();

            public Table(int[][] array)
            {
                for (int i = 0; i < 3; i++)
                    FLW0Table.Add(new Element(array[i][0], array[i][1], array[i][2], array[i][3]));
                for (int i = 3; i < array.Length; i++)
                    FLW0Table.Add(new Element(array[i][0], array[i][2], array[i][1], array[i][3]));
            }

            public MemoryStream GetMS(bool IsLittleEndian)
            {
                BinaryWriter BW;

                if (IsLittleEndian)
                    BW = new BinaryWriter(new MemoryStream());
                else
                    BW = new BinaryWriterBE(new MemoryStream());

                for (int i = 0; i < 3; i++)
                {
                    BW.Write(FLW0Table[i].Index);
                    BW.Write(FLW0Table[i].Size);
                    BW.Write(FLW0Table[i].Count);
                    BW.Write(FLW0Table[i].Position);
                }
                for (int i = 3; i < FLW0Table.Count; i++)
                {
                    BW.Write(FLW0Table[i].Index);
                    BW.Write(FLW0Table[i].Count);
                    BW.Write(FLW0Table[i].Size);
                    BW.Write(FLW0Table[i].Position);
                }

                byte[] buffer = new byte[BW.BaseStream.Length];
                BW.BaseStream.Position = 0;
                BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                return new MemoryStream(buffer);
            }

            public void Shift(int shift)
            {
                FLW0Table.FindAll(x => x.Index > 0x3).ForEach(a => a.Position += shift);
            }
        }

        Header _Header;
        Table _Table;

        enum Map
        {
            E0 = 0x0,
            E1 = 0x1,
            E2 = 0x2,
            MSG = 0x3,
            E4 = 0x4
        }

        class ElementFLW0
        {
            public Map Type { get; set; }
            public List<MemoryStream> StreamList { get; set; }
        }

        List<ElementFLW0> FLW0List = new List<ElementFLW0>();

        public FLW0(string SourceFile, bool IsLittleEndian)
        {
            this.IsLittleEndian = IsLittleEndian;
            BinaryReader BR;

            if (IsLittleEndian)
                BR = new BinaryReader(new FileStream(SourceFile, FileMode.Open, FileAccess.Read));
            else
                BR = new BinaryReaderBE(new FileStream(SourceFile, FileMode.Open, FileAccess.Read));

            _Header = new Header(BR);

            _Table = new Table(BR.ReadInt32ArrayArray(_Header.TableLineCount, 4));

            foreach (var element in _Table.FLW0Table)
            {
                if (element.Size * element.Count > 0)
                {
                    ElementFLW0 temp = new ElementFLW0()
                    {
                        Type = (Map)element.Index,
                        StreamList = GetListMS(BR, element)
                    };
                    FLW0List.Add(temp);
                }
            }
        }

        public void SaveNew(string FileName)
        {
            using (FileStream FS = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite))
            {
                _Header.GetMS(IsLittleEndian).CopyTo(FS);
                _Table.GetMS(IsLittleEndian).CopyTo(FS);
                foreach (var a in FLW0List)
                {
                    foreach (var b in a.StreamList)
                    {
                        b.Position = 0;
                        b.CopyTo(FS);
                    }
                }
            }
        }

        private List<MemoryStream> GetListMS(BinaryReader BR, Table.Element line)
        {
            List<MemoryStream> returned = new List<MemoryStream>();
            BR.BaseStream.Position = line.Position;

            for (int i = 0; i < line.Count; i++)
                returned.Add(new MemoryStream(BR.ReadBytes(line.Size)));

            return returned;
        }

        public void SetNewMSG1(MemoryStream NewMSG)
        {
            int SizeShift = ChangeMSG(NewMSG);
            _Table.Shift(SizeShift);
            _Header.Shift(SizeShift);
        }

        private int ChangeMSG(MemoryStream NewMSG)
        {
            ElementFLW0 temp = FLW0List.Find(x => x.Type == Map.MSG);
            if (temp != null)
            {
                if (temp.StreamList.Count == 1)
                {

                    MemoryStream newMSG = new MemoryStream();

                    NewMSG.CopyTo(newMSG);

                    int SizeShift = Convert.ToInt32(newMSG.Length - temp.StreamList.First().Length);
                    temp.StreamList.Clear();
                    temp.StreamList.Add(newMSG);

                    _Table.FLW0Table.Find(x => x.Index == 0x3).Size = (int)newMSG.Length;

                    return SizeShift;
                }
                else if (temp.StreamList.Count > 1)
                {
                    Logging.Write("PersonaEditorLib", "Exception: 2 or more MSG");
                    return -1;
                }
                else
                {
                    Logging.Write("PersonaEditorLib", "Exception: 0 MSG");
                    return -1;
                }
            }
            else
            {
                Logging.Write("PersonaEditorLib", "File does not contain MSG");
                return -1;
            }
        }

        public byte[] GetMSG
        {
            get { return FLW0List.Find(x => x.Type == Map.MSG).StreamList.First().ToArray(); }
        }
    }

    public class MSG1
    {
        public static MemoryStream GetMSG1FromFile(string FileName, bool IsLittleEndian)
        {
            byte[] buffer = new byte[4];
            using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                FS.Position = 8;
                FS.Read(buffer, 0, 4);
            }
            string FileType = System.Text.Encoding.Default.GetString(buffer);

            if (FileType == "PMD1")
            {
                FileTypes.Text.PMDwork.PMD1 PMD1 = new FileTypes.Text.PMDwork.PMD1(new FileStream(FileName, FileMode.Open), 0, IsLittleEndian);
                return new MemoryStream(PMD1.GetMSG());
            }
            else if (FileType == "FLW0")
            {
                FLW0 FLW0 = new FLW0(FileName, IsLittleEndian);
                return new MemoryStream(FLW0.GetMSG);
            }
            else if (FileType == "MSG1" | FileType == "1GSM")
            {
                MemoryStream returned = null;
                using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                {
                    returned = new MemoryStream();
                    FS.CopyTo(returned);
                }
                return returned;
            }
            else
                return null;
        }

        public MSG1(bool IsLittleEndian, CharList OldChar, CharList NewChar)
        {
            this.OldChar = OldChar;
            this.NewChar = NewChar;
            this.IsLittleEndian = IsLittleEndian;
        }

        public int SaveAsTextOption { get; set; } = 0;


        public delegate void StringChangedEventHandler(string String);
        public delegate void ArrayChangedEventHandler(byte[] array);
        public delegate void ListChangedEventHandler(List<MyStringElement> list);
        public delegate void ElementArrayEventHandler(MyStringElement[] array);

        public struct MyStringElement
        {
            public enum arrayType
            {
                Empty,
                System,
                Text
            }

            public string GetText(CharList CharList)
            {
                if (Type == arrayType.System)
                {
                    if (Bytes[0] == 0x0A)
                    {
                        return "\n";
                    }
                    else
                    {
                        return GetSystem();
                    }
                }
                else
                {
                    string returned = "";
                    for (int i = 0; i < Bytes.Length; i++)
                    {
                        if (0x20 <= Bytes[i] & Bytes[i] < 0x80)
                        {
                            returned += CharList.GetChar(Bytes[i]);
                        }
                        else if (0x80 <= Bytes[i] & Bytes[i] < 0xF0)
                        {
                            int newindex = (Bytes[i] - 0x81) * 0x80 + Bytes[i + 1] + 0x20;

                            i++;
                            returned += CharList.GetChar(newindex);
                        }
                        else
                        {
                            Console.WriteLine("ASD");
                        }
                    }
                    return returned;
                }
            }

            public string GetSystem()
            {
                string returned = "";

                if (Bytes.Length > 0)
                {
                    returned += "{" + Convert.ToString(Bytes[0], 16).PadLeft(2, '0').ToUpper();
                    for (int i = 1; i < Bytes.Length; i++)
                    {
                        returned += " " + Convert.ToString(Bytes[i], 16).PadLeft(2, '0').ToUpper();
                    }
                    returned += "}";
                }

                return returned;
            }

            public MyStringElement(int Index, arrayType Type, byte[] Bytes)
            {
                this.Index = Index;
                this.Type = Type;
                this.Bytes = Bytes;
            }

            public int Index { get; private set; }
            public arrayType Type { get; private set; }
            public byte[] Bytes { get; private set; }

        }

        public class Names : INotifyPropertyChanged
        {
            public event StringChangedEventHandler OldNameChanged;
            public event StringChangedEventHandler NewNameChanged;
            public event ArrayChangedEventHandler OldNameArrayChanged;
            public event ArrayChangedEventHandler NewNameArrayChanged;

            #region INotifyPropertyChanged implementation
            public event PropertyChangedEventHandler PropertyChanged;

            protected void Notify(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            #endregion INotifyPropertyChanged implementation

            public CharList OldCharList { get; set; }
            public CharList NewCharList { get; set; }

            public Names(CharList Old, CharList New, int Index, byte[] OldNameBytes, string NewName)
            {
                OldCharList = Old;
                NewCharList = New;
                this.Index = Index;
                this.OldNameBytes = OldNameBytes;
                this.NewName = NewName;

                OldCharList.CharListChanged += OLD_CharListChanged;
                NewCharList.CharListChanged += NEW_CharListChanged;
            }

            private void NEW_CharListChanged()
            {
                NewNameBytes = NewName.GetMyByteArray(NewCharList).getAllBytes();
            }

            private void OLD_CharListChanged()
            {
                OldName = OldNameBytes.parseString().GetString(OldCharList, false);
            }

            public int Index { get; set; } = 0;

            byte[] _OldNameBytes;
            byte[] _NewNameBytes;
            string _NewName = "";
            string _OldName = "";

            public byte[] OldNameBytes
            {
                get { return _OldNameBytes; }
                set
                {
                    _OldNameBytes = value;
                    OldName = _OldNameBytes.parseString().GetString(OldCharList, false);
                    OldNameArrayChanged?.Invoke(_OldNameBytes);
                    Notify("OldNameBytes");
                }
            }
            public string OldName
            {
                get { return _OldName; }
                set
                {
                    if (value != _OldName)
                    {
                        _OldName = value;
                        OldNameChanged?.Invoke(_OldName);
                        Notify("OldName");
                    }
                }
            }
            public byte[] NewNameBytes
            {
                get { return _NewNameBytes; }
                set
                {
                    _NewNameBytes = value;
                    NewNameArrayChanged?.Invoke(_NewNameBytes);
                    Notify("NewNameBytes");
                }
            }
            public string NewName
            {
                get
                {
                    return _NewName;
                }
                set
                {
                    if (value != _NewName)
                    {
                        _NewName = value;
                        NewNameBytes = _NewName.GetMyByteArray(NewCharList).getAllBytes();
                        NewNameChanged?.Invoke(_NewName);
                        Notify("NewName");
                    }
                }
            }
        }

        public class MSGs
        {
            public class MSGstr
            {
                public class MSGstrElement : INotifyPropertyChanged
                {
                    #region INotifyPropertyChanged implementation
                    public event PropertyChangedEventHandler PropertyChanged;

                    protected void Notify(string propertyName)
                    {
                        if (this.PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                        }
                    }
                    #endregion INotifyPropertyChanged implementation       

                    public enum Mode
                    {
                        ArrayToText,
                        ArrayToTextSYS,
                        TextToArray
                    }

                    public event StringChangedEventHandler TextChanged;
                    public event ElementArrayEventHandler ElementArrayChanged;

                    private Mode CurrentMode { get; set; }
                    public CharList CurrentCharList { get; set; }

                    private MyStringElement[] _ElementArray;
                    public MyStringElement[] ElementArray
                    {
                        get { return _ElementArray; }
                        set
                        {
                            _ElementArray = value;
                            if (CurrentMode == Mode.ArrayToText)
                                Text = _ElementArray.GetString(CurrentCharList, false);
                            else if (CurrentMode == Mode.ArrayToTextSYS)
                                Text = _ElementArray.GetString();
                            ElementArrayChanged?.Invoke(_ElementArray);
                        }
                    }

                    private string _Text = "";
                    public string Text
                    {
                        get { return _Text; }
                        set
                        {
                            if (value != _Text)
                            {
                                _Text = value;
                                TextChanged?.Invoke(Text);
                                if (CurrentMode == Mode.TextToArray)
                                    ElementArray = Text.GetMyByteArray(CurrentCharList);
                            }
                        }
                    }

                    public MSGstrElement(CharList CharList, Mode Mode)
                    {
                        CurrentMode = Mode;

                        if (CurrentMode != Mode.ArrayToTextSYS)
                        {
                            CurrentCharList = CharList;
                            CurrentCharList.CharListChanged += CurrentCharList_CharListChanged;
                        }
                    }

                    private void CurrentCharList_CharListChanged()
                    {
                        if (CurrentMode == Mode.ArrayToText)
                            Text = _ElementArray.GetString(CurrentCharList, false);
                        else if (CurrentMode == Mode.TextToArray)
                            ElementArray = Text.GetMyByteArray(CurrentCharList);
                    }
                }

                public MSGstr(CharList Old, CharList New)
                {
                    Prefix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
                    OldString = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToText);
                    NewString = new MSGstrElement(New, MSGstrElement.Mode.TextToArray);
                    Postfix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
                }

                public int Index { get; set; } = 0;
                public MSGstrElement Prefix { get; set; }
                public MSGstrElement OldString { get; set; }
                public MSGstrElement NewString { get; set; }
                public MSGstrElement Postfix { get; set; }
            }

            public enum MsgType
            {
                MSG = 0,
                SEL = 1
            }

            public MSGs(int Index, string Name, MsgType Type, int CharacterIndex, byte[] MsgBytes, CharList Old, CharList New)
            {
                this.Index = Index;
                this.Type = Type;
                this.Name = Name;
                this.CharacterIndex = CharacterIndex;
                this.MsgBytes = MsgBytes;
            }

            public int Index { get; set; }
            public MsgType Type { get; set; }
            public string Name { get; set; }
            public int CharacterIndex { get; set; }
            public byte[] MsgBytes { get; set; }
            public List<MSGstr> Strings { get; set; } = new List<MSGstr>();
        }

        public bool IsLittleEndian { get; private set; }
        public CharList OldChar { get; private set; }
        public CharList NewChar { get; private set; }

        public BindingList<MSGs> msg { get; set; } = new BindingList<MSGs>();
        public BindingList<Names> name { get; set; } = new BindingList<Names>();

        public bool Load(string FileName, bool IsLittleEndian)
        {
            Stopwatch SW = new Stopwatch();
            SW.Start();
            this.IsLittleEndian = IsLittleEndian;

            if (File.Exists(FileName))
            {
                try
                {
                    MemoryStream MemoryStreamMSG1 = GetMSG1FromFile(FileName, IsLittleEndian);
                    ParseMSG1(MemoryStreamMSG1);
                }
                catch (Exception e)
                {
                    Logging.Write("PersonaEditorLib", e);
                    return false;
                }
            }
            else
            {
                Logging.Write("PersonaEditorLib", "The file does not exist");
                return false;
            }

            SW.Stop();
            Console.WriteLine(SW.Elapsed.TotalSeconds);
            return true;
        }

        public MemoryStream GetNewMSG()
        {
            return GetNewMSG1.Get(msg, name, IsLittleEndian);
        }

        static class GetNewMSG1
        {
            public static MemoryStream Get(IList<MSGs> msg, IList<Names> name, bool IsLittleEndian)
            {
                byte[] buffer;

                BinaryWriter BW;

                if (IsLittleEndian)
                    BW = new BinaryWriter(new MemoryStream());
                else
                    BW = new BinaryWriterBE(new MemoryStream());

                List<List<int>> MSG_pos = new List<List<int>>();
                List<int> NAME_pos = new List<int>();
                List<int> LastBlock = new List<int>();

                buffer = new byte[4] { 7, 0, 0, 0 };
                BW.Write(buffer);
                BW.Write((int)0x0);

                buffer = System.Text.Encoding.ASCII.GetBytes("MSG1");
                if (!IsLittleEndian)
                    Array.Reverse(buffer);

                BW.Write(buffer);

                BW.Write((int)0x0);
                BW.Write((int)0x0);
                BW.Write((int)0x0);
                BW.Write(msg.Count);
                BW.Write((ushort)0);
                BW.Write((ushort)0x2);

                foreach (var MSG in msg)
                {
                    if (MSG.Type == MSGs.MsgType.MSG) { BW.Write((int)0x0); }
                    else if (MSG.Type == MSGs.MsgType.SEL) { BW.Write((int)0x1); }
                    else
                    {
                        Logging.Write("PersonaEditorLib", "Error: Unknown MSG Type");
                        return null;
                    }

                    LastBlock.Add((int)BW.BaseStream.Position);
                    BW.Write((int)0x0);
                }

                LastBlock.Add((int)BW.BaseStream.Position);
                BW.Write((int)0x0);
                BW.Write(name.Count);
                BW.Write((int)0x0);
                BW.Write((int)0x0);

                foreach (var MSG in msg)
                {
                    List<int> MSG_o = new List<int>();
                    MSG_o.Add((int)BW.BaseStream.Position);

                    BW.WriteString(MSG.Name, 24);

                    if (MSG.Type == MSGs.MsgType.MSG)
                    {
                        BW.Write((ushort)MSG.Strings.Count);

                        if (MSG.CharacterIndex == -1) { BW.Write((ushort)0xFFFF); }
                        else { BW.Write((ushort)MSG.CharacterIndex); }
                    }
                    else if (MSG.Type == MSGs.MsgType.SEL)
                    {
                        BW.Write((ushort)0);
                        BW.Write((ushort)MSG.Strings.Count);
                        BW.Write((int)0x0);
                    }

                    int Size = 0;

                    foreach (var String in MSG.Strings)
                    {
                        LastBlock.Add((int)BW.BaseStream.Position);
                        BW.Write((int)0x0);
                        foreach (var Str in String.Prefix.ElementArray)
                        {
                            Size += Str.Bytes.Length;
                        }
                        foreach (var Str in String.NewString.ElementArray)
                        {
                            Size += Str.Bytes.Length;
                        }
                        foreach (var Str in String.Postfix.ElementArray)
                        {
                            Size += Str.Bytes.Length;
                        }
                    }
                    MSG_o.Add(Size);

                    BW.Write((int)0x0);

                    foreach (var String in MSG.Strings)
                    {
                        List<byte> NewString = new List<byte>();
                        foreach (var prefix in String.Prefix.ElementArray)
                        {
                            NewString.AddRange(prefix.Bytes);
                        }
                        foreach (var str in String.NewString.ElementArray)
                        {
                            NewString.AddRange(str.Bytes);
                        }
                        foreach (var postfix in String.Postfix.ElementArray)
                        {
                            NewString.AddRange(postfix.Bytes);
                        }

                        MSG_o.Add((int)BW.BaseStream.Position);
                        BW.Write(NewString.ToArray());
                    }

                    while (BW.BaseStream.Length % 4 != 0)
                    {
                        BW.Write((byte)0);
                    }

                    MSG_pos.Add(MSG_o);
                }

                long Name_Block_pos = BW.BaseStream.Length;
                BW.BaseStream.Position = 0x20;
                for (int i = 0; i < msg.Count; i++)
                {
                    BW.BaseStream.Position += 4;
                    BW.Write((int)MSG_pos[i][0] - 0x20);
                }
                BW.Write((int)Name_Block_pos - 0x20);
                for (int i = 0; i < msg.Count; i++)
                {
                    BW.BaseStream.Position = MSG_pos[i][0];

                    if (msg[i].Type == MSGs.MsgType.MSG)
                    {
                        BW.BaseStream.Position += 28;
                    }
                    else if (msg[i].Type == MSGs.MsgType.SEL)
                    {
                        BW.BaseStream.Position += 32;
                    }

                    for (int k = 0; k < msg[i].Strings.Count; k++)
                    {
                        BW.Write((int)MSG_pos[i][k + 2] - 0x20);
                    }
                    BW.Write((int)MSG_pos[i][1]);
                }


                BW.BaseStream.Position = Name_Block_pos;
                for (int i = 0; i < name.Count; i++)
                {
                    LastBlock.Add((int)BW.BaseStream.Position);
                    BW.Write((int)0);
                }

                foreach (var NAME in name)
                {
                    NAME_pos.Add((int)BW.BaseStream.Position);
                    if (NAME.NewNameBytes.Length == 0)
                        BW.Write(NAME.OldNameBytes);
                    else
                        BW.Write(NAME.NewNameBytes);

                    BW.Write((byte)0);
                }
                BW.BaseStream.Position = Name_Block_pos;
                for (int i = 0; i < name.Count; i++)
                {
                    BW.Write((int)NAME_pos[i] - 0x20);
                }
                BW.BaseStream.Position = BW.BaseStream.Length;
                while (BW.BaseStream.Length % 4 != 0)
                {
                    BW.Write((byte)0);
                }

                int LastBlockPos = (int)BW.BaseStream.Position;
                byte[] LastBlockBytes = getLastBlock(LastBlock);
                BW.Write(LastBlockBytes);

                BW.BaseStream.Position = 0x10;
                BW.Write((int)LastBlockPos);
                BW.Write((int)LastBlockBytes.Length);

                BW.BaseStream.Position = 0x4;
                BW.Write((int)BW.BaseStream.Length);

                BW.BaseStream.Position = 0;

                buffer = new byte[BW.BaseStream.Length];
                BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                return new MemoryStream(buffer);
            }

            static byte[] getLastBlock(List<int> Addresses)
            {
                int sum = 0;
                List<byte> returned = new List<byte>();

                for (int i = 0; i < Addresses.Count; i++)
                {
                    int reloc = Addresses[i] - sum - 0x20;
                    int amount = getSeq(ref Addresses, i);
                    Encode(reloc, ref returned, ref sum);
                    if (amount > 1)
                    {
                        reloc = 7;
                        reloc |= ((amount - 2) / 2) << 4;
                        if (amount % 2 == 1)
                        {
                            reloc |= 8;
                        }
                        returned.Add((byte)reloc);
                        i += amount;
                        sum += amount * 4;
                    }
                }

                return returned.ToArray();
            }

            static int getSeq(ref List<int> Addresses, int index)
            {
                if (index < Addresses.Count - 1)
                {
                    if (Addresses[index + 1] - Addresses[index] == 4)
                    {
                        return getSeq(ref Addresses, index + 1) + 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            }

            static void Encode(int reloc, ref List<byte> LastBlock, ref int sum)
            {
                if (reloc % 2 == 0)
                {
                    int temp = reloc >> 1;
                    if (temp <= 0xFF)
                    {
                        LastBlock.Add((byte)temp);
                    }
                    else
                    {
                        byte item = (byte)((reloc & 0xff) + 1);
                        byte num2 = (byte)((reloc & 0xff00) >> 8);
                        LastBlock.Add(item);
                        LastBlock.Add(num2);
                    }

                }
                else
                {
                    byte item = (byte)((reloc & 0xff) + 1);
                    byte num2 = (byte)((reloc & 0xff00) >> 8);
                    LastBlock.Add(item);
                    LastBlock.Add(num2);
                }
                sum += reloc;
            }
        }

        public bool ImportTXT(string MSGName, int StringIndex, string Text)
        {
            var temp = msg.FirstOrDefault(x => x.Name == MSGName);
            if (temp != null)
            {
                var temp2 = temp.Strings.FirstOrDefault(x => x.Index == StringIndex);
                if (temp2 != null)
                {
                    temp2.NewString.Text = Text;
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool ImportTXT(string MSGName, int StringIndex, string Text, string NewName)
        {
            if (ImportTXT(MSGName, StringIndex, Text))
            {
                var temp = msg.FirstOrDefault(x => x.Name == MSGName);
                if (temp != null)
                {
                    var temp2 = name.FirstOrDefault(x => x.Index == temp.CharacterIndex);
                    if (temp2 != null)
                    {
                        temp2.NewName = NewName;
                    }
                }
                return true;
            }
            else return false;
        }

        void ParseMSG1(MemoryStream MemoryStreamMSG1)
        {
            BinaryReader BR;

            if (IsLittleEndian)
                BR = new BinaryReader(MemoryStreamMSG1);
            else
                BR = new BinaryReaderBE(MemoryStreamMSG1);

            BR.BaseStream.Position = 0;
            try
            {
                name.Clear();
                msg.Clear();

                byte[] buffer;

                int MSG_PointBlock_Pos = 0x20;
                BR.BaseStream.Position = 24;
                int MSG_count = BR.ReadInt32();
                BR.BaseStream.Position = MSG_PointBlock_Pos;
                List<int[]> MSG_Position = new List<int[]>();

                for (int i = 0; i < MSG_count; i++)
                {
                    int[] temp = new int[2];
                    temp[0] = BR.ReadInt32();
                    temp[1] = BR.ReadInt32();
                    MSG_Position.Add(temp);
                }

                int Name_Block_Position = BR.ReadInt32();
                int Name_Count = BR.ReadInt32();
                BR.BaseStream.Position = Name_Block_Position + MSG_PointBlock_Pos;
                List<long> Name_Position = new List<long>();
                for (int i = 0; i < Name_Count; i++)
                    Name_Position.Add(BR.ReadInt32());

                int Index = 0;
                foreach (var a in Name_Position)
                {
                    BR.BaseStream.Position = a + MSG_PointBlock_Pos;
                    byte Byte = BR.ReadByte();
                    List<byte> Bytes = new List<byte>();
                    while (Byte != 0)
                    {
                        Bytes.Add(Byte);
                        Byte = BR.ReadByte();
                    }
                    name.Add(new Names(OldChar, NewChar, Index, Bytes.ToArray(), ""));
                    Index++;
                }

                Index = 0;

                foreach (var pos in MSG_Position)
                {
                    BR.BaseStream.Position = MSG_PointBlock_Pos + pos[1];
                    buffer = BR.ReadBytes(24);
                    string MSG_Name = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
                    if (string.IsNullOrEmpty(MSG_Name))
                        MSG_Name = "<EMPTY>";

                    byte[] MSG_bytes;
                    MSGs.MsgType Type;
                    int Character_Index = 0xFFFF;

                    if (pos[0] == 0)
                    {
                        Type = MSGs.MsgType.MSG;
                        int count = BR.ReadUInt16();
                        Character_Index = BR.ReadUInt16();
                        BR.BaseStream.Position = BR.BaseStream.Position + 4 * count;

                        int size = BR.ReadInt32();

                        MSG_bytes = BR.ReadBytes(size);
                    }
                    else if (pos[0] == 1)
                    {
                        Type = MSGs.MsgType.SEL;
                        BR.BaseStream.Position += 2;
                        int count = BR.ReadUInt16();
                        BR.BaseStream.Position += 4 * count + 4;

                        int size = BR.ReadInt32();

                        MSG_bytes = BR.ReadBytes(size);
                    }
                    else
                    {
                        Logging.Write("PersonaEditorLib", "Error: Unknown message type!");
                        return;
                    }

                    MSGs MSG = new MSGs(Index, MSG_Name, Type, Character_Index, MSG_bytes, OldChar, NewChar);

                    ParseString(MSG.Strings, MSG.MsgBytes, OldChar, NewChar);

                    msg.Add(MSG);

                    Index++;
                }
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", "Error: Parse MSG1 error!");
                Logging.Write("PersonaEditorLib", e);
                name.Clear();
                msg.Clear();
            }
        }

        private void ParseString(IList<MSGs.MSGstr> StringsList, byte[] SourceBytes, CharList Old, CharList New)
        {
            StringsList.Clear();

            int Index = 0;
            foreach (var Bytes in Utilities.SplitSourceBytes(SourceBytes))
            {
                MSG1.MyStringElement[] temp = Bytes.parseString();
                List<MSG1.MyStringElement> Prefix = new List<MSG1.MyStringElement>();
                List<MSG1.MyStringElement> Postfix = new List<MSG1.MyStringElement>();
                List<MSG1.MyStringElement> Strings = new List<MSG1.MyStringElement>();

                int tempdown = 0;
                int temptop = temp.Length;

                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Type == MSG1.MyStringElement.arrayType.System)
                        Prefix.Add(temp[i]);
                    else
                    {
                        tempdown = i;
                        i = temp.Length;
                    }
                }

                for (int i = temp.Length - 1; i >= tempdown; i--)
                {
                    if (temp[i].Type == MSG1.MyStringElement.arrayType.System)
                        Postfix.Add(temp[i]);
                    else
                    {
                        temptop = i;
                        i = 0;
                    }
                }

                Postfix.Reverse();

                for (int i = tempdown; i <= temptop; i++)
                    Strings.Add(temp[i]);

                MSG1.MSGs.MSGstr NewString = new MSG1.MSGs.MSGstr(Old, New);
                NewString.Index = Index;
                NewString.Prefix.ElementArray = Prefix.ToArray();
                NewString.OldString.ElementArray = Strings.ToArray();
                NewString.Postfix.ElementArray = Postfix.ToArray();

                StringsList.Add(NewString);
                Index++;
            }
        }

        //public void SaveAsText(string FileName, string Index, int Option)
        //{
        //    if (Option == 1)
        //    {
        //        SaveAsTextOp1(FileName, Index);
        //    }
        //    else if (Option == 2)
        //    {
        //        SaveAsTextOp2(FileName, Index);
        //    }
        //    else
        //    {
        //        Logging.Write("SaveAsText Option invalid");
        //    }
        //}

        //void SaveAsTextOp1(string FileName, string Index)
        //{
        //    Directory.CreateDirectory("Export Text");

        //    string FileNameWE = Path.GetFileName(FileName);
        //    FileStream FS = new FileStream(@"Export Text\\NAMES.TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //    FS.Position = FS.Length;
        //    using (StreamWriter SW = new StreamWriter(FS))
        //        foreach (var NAME in name)
        //        {
        //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
        //        }


        //    string DirectoryName = new DirectoryInfo(Path.GetDirectoryName(FileName)).Name;
        //    FS = new FileStream("Export Text\\" + DirectoryName.ToUpper() + ".TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //    FS.Position = FS.Length;
        //    using (StreamWriter SW = new StreamWriter(FS))
        //    {
        //        List<name> Name = name.ToList();
        //        foreach (var MSG in msg)
        //        {
        //            foreach (var STR in MSG.Strings)
        //            {
        //                SW.Write(FileNameWE + "\t");
        //                SW.Write(Index + "\t");
        //                SW.Write(MSG.Name + "\t");
        //                SW.Write(STR.Index + "\t");
        //                if (Name.Exists(x => x.Index == MSG.Character_Index))
        //                {
        //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
        //                    SW.Write(Name_i.OldName);
        //                }
        //                else if (MSG.Type == "SEL")
        //                {
        //                    SW.Write("<SELECT>");
        //                }
        //                else { SW.Write("<NO_NAME>"); }

        //                SW.Write("\t");
        //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
        //                SW.Write(split[0]);
        //                for (int i = 1; i < split.Length; i++)
        //                {
        //                    SW.Write(" " + split[i]);
        //                }

        //                SW.WriteLine();
        //            }
        //        }
        //    }
        //}

        //void SaveAsTextOp2(string FileName, string Index)
        //{

        //    string newFileName = Index == "" ? Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".TXT"
        //        : Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "-" + Index + ".TXT";

        //    using (StreamWriter SW = new StreamWriter(new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
        //    {
        //        foreach (var NAME in name)
        //        {
        //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
        //        }
        //        SW.WriteLine();

        //        List<name> Name = name.ToList();
        //        foreach (var MSG in msg)
        //        {
        //            foreach (var STR in MSG.Strings)
        //            {
        //                SW.Write(MSG.Name + "\t");
        //                SW.Write(STR.Index + "\t");
        //                if (Name.Exists(x => x.Index == MSG.Character_Index))
        //                {
        //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
        //                    SW.Write(Name_i.OldName);
        //                }
        //                else if (MSG.Type == "SEL")
        //                {
        //                    SW.Write("<SELECT>");
        //                }
        //                else { SW.Write("<NO_NAME>"); }

        //                SW.Write("\t");
        //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
        //                SW.Write(split[0]);
        //                for (int i = 1; i < split.Length; i++)
        //                {
        //                    SW.Write(" " + split[i]);
        //                }

        //                SW.WriteLine();
        //            }
        //        }
        //    }
        //}
    }
}