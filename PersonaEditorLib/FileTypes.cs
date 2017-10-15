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
    public struct ByteArray
    {
        byte[] Array { get; set; }

        public ByteArray(byte[] array)
        {
            if (array == null)
                Array = new byte[0];
            else
            {
                Array = new byte[array.Length];
                for (int i = 0; i < array.Length; i++)
                    Array[i] = array[i];
            }
        }

        public ByteArray(string array)
        {
            Array = Enumerable.Range(0, array.Split('-').Length).Select(x => Convert.ToByte(array.Split('-')[x], 16)).ToArray();
        }

        public byte[] ToArray()
        {
            byte[] returned = new byte[Array.Length];
            for (int i = 0; i < Array.Length; i++)
                returned[i] = Array[i];
            return returned;
        }

        public override string ToString()
        {
            return BitConverter.ToString(Array);
        }

        public int Length
        {
            get { return Array.Length; }
        }

        public byte this[int i]
        {
            get { return Array[i]; }
            set { Array[i] = value; }
        }
    }

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

    public class CharList : INotifyPropertyChanged
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

        public class FnMpImg : INotifyPropertyChanged
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

            private string _Char = "";

            public int Index { get; set; } = 0;

            public string Char
            {
                get { return _Char; }
                set
                {
                    if (_Char != value)
                    {
                        _Char = value;
                        Notify("Char");
                    }
                }
            }
            public BitmapSource Image { get; set; }
        }

        public class FnMpData
        {
            public int Index { get; set; } = 0;
            public string Char { get; set; } = "";
            public byte[] Image_data { get; set; } = new byte[0];
            public VerticalCut Cut { get; set; }
        }

        private void Init()
        {
        }

        public CharList()
        {
            Init();
        }

        public CharList(string FontMap, FileStructure.FNT.FNT FNT)
        {
            Init();
            OpenFont(FNT);
            OpenFontMap(FontMap);
        }

        public void OpenFont(FileStructure.FNT.FNT FNT)
        {
            try
            {
                ReadFONT(FNT);
                var a = List.Find(x => x.Index == 32);
                if (a != null) a.Cut = new VerticalCut(10, 20);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
            }
            Update();
        }

        public void OpenFont(string FontName)
        {
            OpenFont(new FileStructure.FNT.FNT(File.OpenRead(FontName), 0));
        }

        public void OpenFontMap(string FontMap)
        {
            try
            {
                ReadFNMP(FontMap);
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
            }
            Update();
        }

        public void Update()
        {
            Notify("Update");
        }

        public bool SaveFontMap(string filename)
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

        private void ReadFONT(FileStructure.FNT.FNT FNT)
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
            if (FNT.Header.Glyphs.BitsPerPixel == 4)
                Util.ReverseByteInList(DecList);

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
            using (StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
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
            }
        }

        private void WriteFNMP(string filename)
        {
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                foreach (var CL in List)
                {
                    if (CL.Char != "")
                    {
                        string str = Convert.ToString(CL.Index) + "=" + Convert.ToString(CL.Char);
                        sw.WriteLine(str);
                        sw.Flush();
                    }
                }
            }
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

        public byte[] Encode(string String)
        {
            List<byte> LB = new List<byte>();
            foreach (var C in String)
            {
                LB.AddChar(C, this);
            }
            return LB.ToArray();
        }

        public List<FnMpData> List { get; set; } = new List<FnMpData>();
        public BitmapPalette Palette { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelFormat PixelFormat { get; set; }

        public string Tag { get; set; } = "";
    }
}

namespace PersonaEditorLib.FileTypes
{
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

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
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
                if (Pixels == null) return true;
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

        public static ImageData DrawText(IList<FileStructure.PTP.MSG.MSGstr.MSGstrElement> text, CharList CharList, Dictionary<int, byte> Shift)
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
                                    returned = MergeUpDown(returned, new ImageData(PixelFormats.Indexed4, 1, 32), 7);
                                }
                                else
                                {
                                    returned = MergeUpDown(returned, line, 7);
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
                                glyph = shiftb == false ? ImageData.Crop(glyph, new ImageData.Rect(fnmp.Cut.Left, 0, fnmp.Cut.Right - fnmp.Cut.Left - 1, glyph.PixelHeight))
                                    : ImageData.Shift(ImageData.Crop(glyph, new ImageData.Rect(fnmp.Cut.Left, 0, fnmp.Cut.Right - fnmp.Cut.Left - 1, glyph.PixelHeight)), shift);
                                line = ImageData.MergeLeftRight(line, glyph);
                            }
                        }
                    }
                }
                returned = ImageData.MergeUpDown(returned, line, 7);
                return returned;
            }
            return new ImageData();
        }

        public static ImageData MergeLeftRight(ImageData left, ImageData right)
        {
            if (left.Pixels == null)
            {
                return right;
            }
            else if (right.Pixels == null)
            {
                return left;
            }

            byte[][] buffer = GetMergePixelsLR(left.Pixels, right.Pixels);
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
    }

    public class Text
    {
        public class BMDfactory
        {
            private FileStructure.BMD BMD;

            public BMDfactory(FileStructure.BMD BMD)
            {
                this.BMD = BMD;
            }

            public BMDfactory(string filename)
            {
                BMD = new FileStructure.BMD();
                BMD.Load(File.OpenRead(filename), filename, true);
            }

            public void SavePTP(string filename)
            {
                FileStructure.PTP PTP = new FileStructure.PTP(new CharList(), new CharList());
                PTP.Open(BMD);
                PTPfactory PTPfac = new PTPfactory(PTP);
                PTP.SaveProject().Save(filename);
            }

            public MemoryStream Get(bool IsLittleEnding)
            {
                return null;
                // return BMD.GetNewMSG(IsLittleEnding);
            }
        }

        public class PTPfactory
        {
            public FileStructure.PTP PTP;

            public PTPfactory(string filename, string oldfont, string newfont, string oldmap, string newmap)
            {
                PTP = new FileStructure.PTP(new CharList(oldmap, new FileStructure.FNT.FNT(new FileStream(oldfont, FileMode.Open, FileAccess.Read), 0)),
                   new CharList(newmap, new FileStructure.FNT.FNT(new FileStream(newfont, FileMode.Open, FileAccess.Read), 0)));
                PTP.Open(XDocument.Load(filename));
            }

            public PTPfactory(string filename)
            {
                PTP = new FileStructure.PTP(new CharList(), new CharList());
                PTP.Open(new XDocument(filename));
            }

            public PTPfactory(FileStructure.PTP PTP)
            {
                this.PTP = PTP;
            }

            public bool OpenProject(string path)
            {
                return PTP.Open(new XDocument(path));
            }

            public bool Save(string path)
            {
                try
                {
                    PTP.SaveProject().Save(path);
                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }
        }
    }
}

namespace PersonaEditorLib.FileStructure
{
    public delegate void StringChangedEventHandler(string text);
    public delegate void ByteArrayChangedEventHandler(ByteArray array);

    public class BMD
    {
        public bool Load(Stream stream, string filename, bool IsLittleEndian)
        {
            this.IsLittleEndian = IsLittleEndian;

            stream.Position = 0;
            BinaryReader BR;

            if (IsLittleEndian)
                BR = new BinaryReader(stream);
            else
                BR = new BinaryReaderBE(stream);

            ParseMSG1(BR);

            OpenFileName = filename;
            return true;
        }

        public bool Load(PTP PTP)
        {
            CharList CharList = PTP.NewCharList;
            name.Clear();
            msg.Clear();
            foreach (var a in PTP.names)
                name.Add(new Names(a.Index, a.NewName.GetPTPMsgStrEl(CharList).GetByteArray().ToArray()));

            foreach (var a in PTP.msg)
            {
                int Index = a.Index;
                string Name = a.Name;
                MSGs.MsgType Type = a.Type == "MSG" ? MSGs.MsgType.MSG : MSGs.MsgType.SEL;
                int CharacterIndex = a.CharacterIndex;

                List<byte> Msg = new List<byte>();
                foreach (var b in a.Strings)
                {
                    foreach (var c in b.Prefix)
                        Msg.AddRange(c.Array.ToArray());

                    Msg.AddRange(b.NewString.GetPTPMsgStrEl(CharList).GetByteArray().ToArray());

                    foreach (var c in b.Postfix)
                        Msg.AddRange(c.Array.ToArray());
                }
                ByteArray MsgBytes = new ByteArray(Msg.ToArray());

                msg.Add(new MSGs(Index, Name, Type, CharacterIndex, MsgBytes.ToArray()));
            }

            return true;
        }

        public MemoryStream Get(bool IsLittleEndian)
        {
            MemoryStream returned = new MemoryStream();
            BinaryWriter writer;

            if (IsLittleEndian)
                writer = new BinaryWriter(returned);
            else
                writer = new BinaryWriterBE(returned);

            GetNewMSG1.Get(msg, name, writer);
            return returned;
        }

        public string OpenFileName = "";

        private void ParseMSG1(BinaryReader BR)
        {
            try
            {
                name.Clear();
                msg.Clear();
                byte[] buffer;

                int MSG_PointBlock_Pos = 0x20;
                BR.BaseStream.Position = 24;
                int MSG_count = BR.ReadInt32();
                BR.BaseStream.Position = MSG_PointBlock_Pos;
                List<int[]> MSGPosition = new List<int[]>();

                for (int i = 0; i < MSG_count; i++)
                {
                    int[] temp = new int[2];
                    temp[0] = BR.ReadInt32();
                    temp[1] = BR.ReadInt32();
                    MSGPosition.Add(temp);
                }

                int Name_Block_Position = BR.ReadInt32();
                int Name_Count = BR.ReadInt32();
                BR.BaseStream.Position = Name_Block_Position + MSG_PointBlock_Pos;

                List<long> NamePosition = new List<long>();
                for (int i = 0; i < Name_Count; i++)
                    NamePosition.Add(BR.ReadInt32());

                for (int i = 0; i < NamePosition.Count; i++)
                {
                    BR.BaseStream.Position = NamePosition[i] + MSG_PointBlock_Pos;
                    byte Byte = BR.ReadByte();
                    List<byte> Bytes = new List<byte>();
                    while (Byte != 0)
                    {
                        Bytes.Add(Byte);
                        Byte = BR.ReadByte();
                    }
                    name.Add(new Names(i, Bytes.ToArray()));
                }

                for (int i = 0; i < MSGPosition.Count; i++)
                {
                    BR.BaseStream.Position = MSG_PointBlock_Pos + MSGPosition[i][1];
                    buffer = BR.ReadBytes(24);
                    string MSG_Name = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
                    if (string.IsNullOrEmpty(MSG_Name))
                        MSG_Name = "<EMPTY>";

                    byte[] MSG_bytes;
                    MSGs.MsgType Type;
                    int CharacterIndex = 0xFFFF;

                    if (MSGPosition[i][0] == 0)
                    {
                        Type = MSGs.MsgType.MSG;
                        int count = BR.ReadUInt16();
                        CharacterIndex = BR.ReadUInt16();
                        BR.BaseStream.Position = BR.BaseStream.Position + 4 * count;

                        int size = BR.ReadInt32();

                        MSG_bytes = BR.ReadBytes(size);
                    }
                    else if (MSGPosition[i][0] == 1)
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

                    MSGs MSG = new MSGs(i, MSG_Name, Type, CharacterIndex, MSG_bytes);

                    msg.Add(MSG);
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

        public class Names
        {
            public Names(int Index, byte[] NameBytes)
            {
                this.Index = Index;
                this.NameBytes = NameBytes;
            }

            public int Index { get; set; }
            public byte[] NameBytes { get; set; }
        }

        public class MSGs
        {
            public enum MsgType
            {
                MSG = 0,
                SEL = 1
            }

            public MSGs(int index, string name, MsgType type, int characterIndex, byte[] msgBytes)
            {
                Index = index;
                Type = type;
                Name = name;
                CharacterIndex = characterIndex;
                MsgBytes = new ByteArray(msgBytes);
            }

            public int Index { get; set; }
            public MsgType Type { get; set; }
            public string Name { get; set; }
            public int CharacterIndex { get; set; }
            public ByteArray MsgBytes { get; set; }
        }

        public bool IsLittleEndian { get; private set; }
        public CharList CharList { get; private set; }

        public List<MSGs> msg { get; set; } = new List<MSGs>();
        public List<Names> name { get; set; } = new List<Names>();

        static class GetNewMSG1
        {
            public static void Get(IList<MSGs> msg, IList<Names> name, BinaryWriter BW)
            {

                byte[] buffer;

                List<List<int>> MSG_pos = new List<List<int>>();
                List<int> NAME_pos = new List<int>();
                List<int> LastBlock = new List<int>();

                buffer = new byte[4] { 7, 0, 0, 0 };
                BW.Write(buffer);
                BW.Write((int)0x0);

                buffer = System.Text.Encoding.ASCII.GetBytes("MSG1");

                BW.Write(BitConverter.ToInt32(buffer, 0));

                BW.Write((int)0x0);
                BW.Write((int)0x0);
                BW.Write((int)0x0);
                BW.Write(msg.Count);
                BW.Write((ushort)0);
                BW.Write((ushort)0x2);

                foreach (var MSG in msg)
                {
                    if (MSG.Type == MSGs.MsgType.MSG)
                        BW.Write((int)0x0);
                    else if (MSG.Type == MSGs.MsgType.SEL)
                        BW.Write((int)0x1);
                    else
                        return;


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
                    var split = MSG.MsgBytes.SplitSourceBytes();

                    // List<PTP.MSG.MSGstr> MSGStrings = new List<PTP.MSG.MSGstr>();
                    // MSGStrings.ParseString(MSG.MsgBytes);

                    List<int> MSG_o = new List<int>();
                    MSG_o.Add((int)BW.BaseStream.Position);

                    BW.WriteString(MSG.Name, 24);

                    if (MSG.Type == MSGs.MsgType.MSG)
                    {
                        BW.Write((ushort)split.Count);

                        if (MSG.CharacterIndex == -1) { BW.Write((ushort)0xFFFF); }
                        else { BW.Write((ushort)MSG.CharacterIndex); }
                    }
                    else if (MSG.Type == MSGs.MsgType.SEL)
                    {
                        BW.Write((ushort)0);
                        BW.Write((ushort)split.Count);
                        BW.Write((int)0x0);
                    }

                    int Size = 0;

                    foreach (var String in split)
                    {
                        LastBlock.Add((int)BW.BaseStream.Position);
                        BW.Write((int)0x0);
                        Size += String.Length;
                    }
                    MSG_o.Add(Size);

                    BW.Write((int)0x0);

                    foreach (var String in split)
                    {
                        List<byte> NewString = new List<byte>();
                        NewString.AddRange(String.ToArray());

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

                    var split = msg[i].MsgBytes.SplitSourceBytes();

                    for (int k = 0; k < split.Count; k++)
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
                    if (NAME.NameBytes.Length == 0)
                        BW.Write(new byte[] { 0x20 });
                    else
                        BW.Write(NAME.NameBytes);

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

                // buffer = new byte[BW.BaseStream.Length];
                // BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

                // return new MemoryStream(buffer);
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
                        return getSeq(ref Addresses, index + 1) + 1;
                    else
                        return 0;
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
    }

    //public class MSG1
    //{
    //    public static MemoryStream GetMSG1FromFile(string FileName, bool IsLittleEndian)
    //    {
    //        byte[] buffer = new byte[4];
    //        using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
    //        {
    //            FS.Position = 8;
    //            FS.Read(buffer, 0, 4);
    //        }
    //        string FileType = System.Text.Encoding.Default.GetString(buffer);

    //        if (FileType == "MSG1" | FileType == "1GSM")
    //        {
    //            MemoryStream returned = null;
    //            using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
    //            {
    //                returned = new MemoryStream();
    //                FS.CopyTo(returned);
    //            }
    //            return returned;
    //        }
    //        else
    //            return null;
    //    }

    //    public MSG1(bool IsLittleEndian, CharList OldChar, CharList NewChar)
    //    {
    //        this.OldChar = OldChar;
    //        this.NewChar = NewChar;
    //        this.IsLittleEndian = IsLittleEndian;
    //    }

    //    public int SaveAsTextOption { get; set; } = 0;

    //    public delegate void StringChangedEventHandler(string String);
    //    public delegate void ArrayChangedEventHandler(byte[] array);
    //    public delegate void ListChangedEventHandler(List<MyStringElement> list);
    //    public delegate void ElementArrayEventHandler(MyStringElement[] array);

    //    public struct MyStringElement
    //    {
    //        public enum arrayType
    //        {
    //            Empty,
    //            System,
    //            Text
    //        }

    //        public string GetText(IList<FnMpData> CharList)
    //        {
    //            if (Type == arrayType.System)
    //            {
    //                if (Bytes[0] == 0x0A)
    //                {
    //                    return "\n";
    //                }
    //                else
    //                {
    //                    return GetSystem();
    //                }
    //            }
    //            else
    //            {
    //                string returned = "";
    //                for (int i = 0; i < Bytes.Length; i++)
    //                {
    //                    if (0x20 <= Bytes[i] & Bytes[i] < 0x80)
    //                    {
    //                        returned += CharList.GetChar(Bytes[i]);
    //                    }
    //                    else if (0x80 <= Bytes[i] & Bytes[i] < 0xF0)
    //                    {
    //                        int newindex = (Bytes[i] - 0x81) * 0x80 + Bytes[i + 1] + 0x20;

    //                        i++;
    //                        returned += CharList.GetChar(newindex);
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine("ASD");
    //                    }
    //                }
    //                return returned;
    //            }
    //        }

    //        public string GetSystem()
    //        {
    //            string returned = "";

    //            if (Bytes.Length > 0)
    //            {
    //                returned += "{" + Convert.ToString(Bytes[0], 16).PadLeft(2, '0').ToUpper();
    //                for (int i = 1; i < Bytes.Length; i++)
    //                {
    //                    returned += " " + Convert.ToString(Bytes[i], 16).PadLeft(2, '0').ToUpper();
    //                }
    //                returned += "}";
    //            }

    //            return returned;
    //        }

    //        public MyStringElement(int Index, arrayType Type, byte[] Bytes)
    //        {
    //            this.Index = Index;
    //            this.Type = Type;
    //            this.Bytes = Bytes;
    //        }

    //        public int Index { get; private set; }
    //        public arrayType Type { get; private set; }
    //        public byte[] Bytes { get; private set; }

    //    }

    //    public class Names : INotifyPropertyChanged
    //    {
    //        public event StringChangedEventHandler OldNameChanged;
    //        public event StringChangedEventHandler NewNameChanged;
    //        public event ArrayChangedEventHandler OldNameArrayChanged;
    //        public event ArrayChangedEventHandler NewNameArrayChanged;

    //        #region INotifyPropertyChanged implementation
    //        public event PropertyChangedEventHandler PropertyChanged;

    //        protected void Notify(string propertyName)
    //        {
    //            if (this.PropertyChanged != null)
    //            {
    //                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //            }
    //        }
    //        #endregion INotifyPropertyChanged implementation

    //        public CharList OldCharList { get; set; }
    //        public CharList NewCharList { get; set; }

    //        public Names(CharList Old, CharList New, int Index, byte[] OldNameBytes, string NewName)
    //        {
    //            OldCharList = Old;
    //            NewCharList = New;
    //            this.Index = Index;
    //            this.OldNameBytes = OldNameBytes;
    //            this.NewName = NewName;

    //            OldCharList.CharListChanged += OLD_CharListChanged;
    //            NewCharList.CharListChanged += NEW_CharListChanged;
    //        }

    //        private void NEW_CharListChanged()
    //        {
    //            NewNameBytes = NewName.GetMyByteArray(NewCharList.List).getAllBytes();
    //        }

    //        private void OLD_CharListChanged()
    //        {
    //            OldName = OldNameBytes.parseString().GetString(OldCharList.List, false);
    //        }

    //        public int Index { get; set; } = 0;

    //        byte[] _OldNameBytes;
    //        byte[] _NewNameBytes;
    //        string _NewName = "";
    //        string _OldName = "";

    //        public byte[] OldNameBytes
    //        {
    //            get { return _OldNameBytes; }
    //            set
    //            {
    //                _OldNameBytes = value;
    //                OldName = _OldNameBytes.parseString().GetString(OldCharList.List, false);
    //                OldNameArrayChanged?.Invoke(_OldNameBytes);
    //                Notify("OldNameBytes");
    //            }
    //        }
    //        public string OldName
    //        {
    //            get { return _OldName; }
    //            set
    //            {
    //                if (value != _OldName)
    //                {
    //                    _OldName = value;
    //                    OldNameChanged?.Invoke(_OldName);
    //                    Notify("OldName");
    //                }
    //            }
    //        }
    //        public byte[] NewNameBytes
    //        {
    //            get { return _NewNameBytes; }
    //            set
    //            {
    //                _NewNameBytes = value;
    //                NewNameArrayChanged?.Invoke(_NewNameBytes);
    //                Notify("NewNameBytes");
    //            }
    //        }
    //        public string NewName
    //        {
    //            get
    //            {
    //                return _NewName;
    //            }
    //            set
    //            {
    //                if (value != _NewName)
    //                {
    //                    _NewName = value;
    //                    NewNameBytes = _NewName.GetMyByteArray(NewCharList.List).getAllBytes();
    //                    NewNameChanged?.Invoke(_NewName);
    //                    Notify("NewName");
    //                }
    //            }
    //        }
    //    }

    //    public class MSGs
    //    {
    //        public class MSGstr
    //        {
    //            public class MSGstrElement : INotifyPropertyChanged
    //            {
    //                #region INotifyPropertyChanged implementation
    //                public event PropertyChangedEventHandler PropertyChanged;

    //                protected void Notify(string propertyName)
    //                {
    //                    if (this.PropertyChanged != null)
    //                    {
    //                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //                    }
    //                }
    //                #endregion INotifyPropertyChanged implementation       

    //                public enum Mode
    //                {
    //                    ArrayToText,
    //                    ArrayToTextSYS,
    //                    TextToArray
    //                }

    //                public event StringChangedEventHandler TextChanged;
    //                public event ElementArrayEventHandler ElementArrayChanged;

    //                private Mode CurrentMode { get; set; }
    //                public CharList CurrentCharList { get; set; }

    //                private MyStringElement[] _ElementArray;
    //                public MyStringElement[] ElementArray
    //                {
    //                    get { return _ElementArray; }
    //                    set
    //                    {
    //                        _ElementArray = value;
    //                        if (CurrentMode == Mode.ArrayToText)
    //                            Text = _ElementArray.GetString(CurrentCharList.List, false);
    //                        else if (CurrentMode == Mode.ArrayToTextSYS)
    //                            Text = _ElementArray.GetString();
    //                        ElementArrayChanged?.Invoke(_ElementArray);
    //                    }
    //                }

    //                private string _Text = "";
    //                public string Text
    //                {
    //                    get { return _Text; }
    //                    set
    //                    {
    //                        if (value != _Text)
    //                        {
    //                            _Text = value;
    //                            TextChanged?.Invoke(Text);
    //                            if (CurrentMode == Mode.TextToArray)
    //                                ElementArray = Text.GetMyByteArray(CurrentCharList.List);
    //                        }
    //                    }
    //                }

    //                public MSGstrElement(CharList CharList, Mode Mode)
    //                {
    //                    CurrentMode = Mode;

    //                    if (CurrentMode != Mode.ArrayToTextSYS)
    //                    {
    //                        CurrentCharList = CharList;
    //                        CurrentCharList.CharListChanged += CurrentCharList_CharListChanged;
    //                    }
    //                }

    //                private void CurrentCharList_CharListChanged()
    //                {
    //                    if (CurrentMode == Mode.ArrayToText)
    //                        Text = _ElementArray.GetString(CurrentCharList.List, false);
    //                    else if (CurrentMode == Mode.TextToArray)
    //                        ElementArray = Text.GetMyByteArray(CurrentCharList.List);
    //                }
    //            }

    //            public MSGstr(CharList Old, CharList New)
    //            {
    //                Prefix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
    //                OldString = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToText);
    //                NewString = new MSGstrElement(New, MSGstrElement.Mode.TextToArray);
    //                Postfix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
    //            }

    //            public int Index { get; set; } = 0;
    //            public MSGstrElement Prefix { get; set; }
    //            public MSGstrElement OldString { get; set; }
    //            public MSGstrElement NewString { get; set; }
    //            public MSGstrElement Postfix { get; set; }
    //        }

    //        public enum MsgType
    //        {
    //            MSG = 0,
    //            SEL = 1
    //        }

    //        public MSGs(int Index, string Name, MsgType Type, int CharacterIndex, byte[] MsgBytes, CharList Old, CharList New)
    //        {
    //            this.Index = Index;
    //            this.Type = Type;
    //            this.Name = Name;
    //            this.CharacterIndex = CharacterIndex;
    //            this.MsgBytes = MsgBytes;
    //        }

    //        public int Index { get; set; }
    //        public MsgType Type { get; set; }
    //        public string Name { get; set; }
    //        public int CharacterIndex { get; set; }
    //        public byte[] MsgBytes { get; set; }
    //        public List<MSGstr> Strings { get; set; } = new List<MSGstr>();
    //    }

    //    public bool IsLittleEndian { get; private set; }
    //    public CharList OldChar { get; private set; }
    //    public CharList NewChar { get; private set; }

    //    public BindingList<MSGs> msg { get; set; } = new BindingList<MSGs>();
    //    public BindingList<Names> name { get; set; } = new BindingList<Names>();

    //    public bool Load(string FileName, bool IsLittleEndian)
    //    {
    //        Stopwatch SW = new Stopwatch();
    //        SW.Start();
    //        this.IsLittleEndian = IsLittleEndian;

    //        if (File.Exists(FileName))
    //        {
    //            try
    //            {
    //                MemoryStream MemoryStreamMSG1 = GetMSG1FromFile(FileName, IsLittleEndian);
    //                ParseMSG1(MemoryStreamMSG1);
    //            }
    //            catch (Exception e)
    //            {
    //                Logging.Write(e.ToString());
    //                return false;
    //            }
    //        }
    //        else
    //        {
    //            Logging.Write("The file does not exist");
    //            return false;
    //        }

    //        SW.Stop();
    //        Console.WriteLine(SW.Elapsed.TotalSeconds);
    //        return true;
    //    }

    //    public MemoryStream GetNewMSG()
    //    {
    //        return GetNewMSG1.Get(msg, name, IsLittleEndian);
    //    }

    //    static class GetNewMSG1
    //    {
    //        public static MemoryStream Get(IList<MSGs> msg, IList<Names> name, bool IsLittleEndian)
    //        {
    //            byte[] buffer;

    //            BinaryWriter BW;

    //            if (IsLittleEndian)
    //                BW = new BinaryWriter(new MemoryStream());
    //            else
    //                BW = new BinaryWriterBE(new MemoryStream());

    //            List<List<int>> MSG_pos = new List<List<int>>();
    //            List<int> NAME_pos = new List<int>();
    //            List<int> LastBlock = new List<int>();

    //            buffer = new byte[4] { 7, 0, 0, 0 };
    //            BW.Write(buffer);
    //            BW.Write((int)0x0);

    //            buffer = System.Text.Encoding.ASCII.GetBytes("MSG1");
    //            if (!IsLittleEndian)
    //                Array.Reverse(buffer);

    //            BW.Write(buffer);

    //            BW.Write((int)0x0);
    //            BW.Write((int)0x0);
    //            BW.Write((int)0x0);
    //            BW.Write(msg.Count);
    //            BW.Write((ushort)0);
    //            BW.Write((ushort)0x2);

    //            foreach (var MSG in msg)
    //            {
    //                if (MSG.Type == MSGs.MsgType.MSG) { BW.Write((int)0x0); }
    //                else if (MSG.Type == MSGs.MsgType.SEL) { BW.Write((int)0x1); }
    //                else
    //                {
    //                    Logging.Write("Error: Unknown MSG Type");
    //                    return null;
    //                }

    //                LastBlock.Add((int)BW.BaseStream.Position);
    //                BW.Write((int)0x0);
    //            }

    //            LastBlock.Add((int)BW.BaseStream.Position);
    //            BW.Write((int)0x0);
    //            BW.Write(name.Count);
    //            BW.Write((int)0x0);
    //            BW.Write((int)0x0);

    //            foreach (var MSG in msg)
    //            {
    //                List<int> MSG_o = new List<int>();
    //                MSG_o.Add((int)BW.BaseStream.Position);

    //                BW.WriteString(MSG.Name, 24);

    //                if (MSG.Type == MSGs.MsgType.MSG)
    //                {
    //                    BW.Write((ushort)MSG.Strings.Count);

    //                    if (MSG.CharacterIndex == -1) { BW.Write((ushort)0xFFFF); }
    //                    else { BW.Write((ushort)MSG.CharacterIndex); }
    //                }
    //                else if (MSG.Type == MSGs.MsgType.SEL)
    //                {
    //                    BW.Write((ushort)0);
    //                    BW.Write((ushort)MSG.Strings.Count);
    //                    BW.Write((int)0x0);
    //                }

    //                int Size = 0;

    //                foreach (var String in MSG.Strings)
    //                {
    //                    LastBlock.Add((int)BW.BaseStream.Position);
    //                    BW.Write((int)0x0);
    //                    foreach (var Str in String.Prefix.ElementArray)
    //                    {
    //                        Size += Str.Bytes.Length;
    //                    }
    //                    foreach (var Str in String.NewString.ElementArray)
    //                    {
    //                        Size += Str.Bytes.Length;
    //                    }
    //                    foreach (var Str in String.Postfix.ElementArray)
    //                    {
    //                        Size += Str.Bytes.Length;
    //                    }
    //                }
    //                MSG_o.Add(Size);

    //                BW.Write((int)0x0);

    //                foreach (var String in MSG.Strings)
    //                {
    //                    List<byte> NewString = new List<byte>();
    //                    foreach (var prefix in String.Prefix.ElementArray)
    //                    {
    //                        NewString.AddRange(prefix.Bytes);
    //                    }
    //                    foreach (var str in String.NewString.ElementArray)
    //                    {
    //                        NewString.AddRange(str.Bytes);
    //                    }
    //                    foreach (var postfix in String.Postfix.ElementArray)
    //                    {
    //                        NewString.AddRange(postfix.Bytes);
    //                    }

    //                    MSG_o.Add((int)BW.BaseStream.Position);
    //                    BW.Write(NewString.ToArray());
    //                }

    //                while (BW.BaseStream.Length % 4 != 0)
    //                {
    //                    BW.Write((byte)0);
    //                }

    //                MSG_pos.Add(MSG_o);
    //            }

    //            long Name_Block_pos = BW.BaseStream.Length;
    //            BW.BaseStream.Position = 0x20;
    //            for (int i = 0; i < msg.Count; i++)
    //            {
    //                BW.BaseStream.Position += 4;
    //                BW.Write((int)MSG_pos[i][0] - 0x20);
    //            }
    //            BW.Write((int)Name_Block_pos - 0x20);
    //            for (int i = 0; i < msg.Count; i++)
    //            {
    //                BW.BaseStream.Position = MSG_pos[i][0];

    //                if (msg[i].Type == MSGs.MsgType.MSG)
    //                {
    //                    BW.BaseStream.Position += 28;
    //                }
    //                else if (msg[i].Type == MSGs.MsgType.SEL)
    //                {
    //                    BW.BaseStream.Position += 32;
    //                }

    //                for (int k = 0; k < msg[i].Strings.Count; k++)
    //                {
    //                    BW.Write((int)MSG_pos[i][k + 2] - 0x20);
    //                }
    //                BW.Write((int)MSG_pos[i][1]);
    //            }


    //            BW.BaseStream.Position = Name_Block_pos;
    //            for (int i = 0; i < name.Count; i++)
    //            {
    //                LastBlock.Add((int)BW.BaseStream.Position);
    //                BW.Write((int)0);
    //            }

    //            foreach (var NAME in name)
    //            {
    //                NAME_pos.Add((int)BW.BaseStream.Position);
    //                if (NAME.NewNameBytes.Length == 0)
    //                    BW.Write(NAME.OldNameBytes);
    //                else
    //                    BW.Write(NAME.NewNameBytes);

    //                BW.Write((byte)0);
    //            }
    //            BW.BaseStream.Position = Name_Block_pos;
    //            for (int i = 0; i < name.Count; i++)
    //            {
    //                BW.Write((int)NAME_pos[i] - 0x20);
    //            }
    //            BW.BaseStream.Position = BW.BaseStream.Length;
    //            while (BW.BaseStream.Length % 4 != 0)
    //            {
    //                BW.Write((byte)0);
    //            }

    //            int LastBlockPos = (int)BW.BaseStream.Position;
    //            byte[] LastBlockBytes = getLastBlock(LastBlock);
    //            BW.Write(LastBlockBytes);

    //            BW.BaseStream.Position = 0x10;
    //            BW.Write((int)LastBlockPos);
    //            BW.Write((int)LastBlockBytes.Length);

    //            BW.BaseStream.Position = 0x4;
    //            BW.Write((int)BW.BaseStream.Length);

    //            BW.BaseStream.Position = 0;

    //            buffer = new byte[BW.BaseStream.Length];
    //            BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

    //            return new MemoryStream(buffer);
    //        }

    //        static byte[] getLastBlock(List<int> Addresses)
    //        {
    //            int sum = 0;
    //            List<byte> returned = new List<byte>();

    //            for (int i = 0; i < Addresses.Count; i++)
    //            {
    //                int reloc = Addresses[i] - sum - 0x20;
    //                int amount = getSeq(ref Addresses, i);
    //                Encode(reloc, ref returned, ref sum);
    //                if (amount > 1)
    //                {
    //                    reloc = 7;
    //                    reloc |= ((amount - 2) / 2) << 4;
    //                    if (amount % 2 == 1)
    //                    {
    //                        reloc |= 8;
    //                    }
    //                    returned.Add((byte)reloc);
    //                    i += amount;
    //                    sum += amount * 4;
    //                }
    //            }

    //            return returned.ToArray();
    //        }

    //        static int getSeq(ref List<int> Addresses, int index)
    //        {
    //            if (index < Addresses.Count - 1)
    //            {
    //                if (Addresses[index + 1] - Addresses[index] == 4)
    //                {
    //                    return getSeq(ref Addresses, index + 1) + 1;
    //                }
    //                else
    //                {
    //                    return 0;
    //                }
    //            }
    //            return 0;
    //        }

    //        static void Encode(int reloc, ref List<byte> LastBlock, ref int sum)
    //        {
    //            if (reloc % 2 == 0)
    //            {
    //                int temp = reloc >> 1;
    //                if (temp <= 0xFF)
    //                {
    //                    LastBlock.Add((byte)temp);
    //                }
    //                else
    //                {
    //                    byte item = (byte)((reloc & 0xff) + 1);
    //                    byte num2 = (byte)((reloc & 0xff00) >> 8);
    //                    LastBlock.Add(item);
    //                    LastBlock.Add(num2);
    //                }

    //            }
    //            else
    //            {
    //                byte item = (byte)((reloc & 0xff) + 1);
    //                byte num2 = (byte)((reloc & 0xff00) >> 8);
    //                LastBlock.Add(item);
    //                LastBlock.Add(num2);
    //            }
    //            sum += reloc;
    //        }

    //    }

    //    public bool ImportTXT(string MSGName, int StringIndex, string Text)
    //    {
    //        var temp = msg.FirstOrDefault(x => x.Name == MSGName);
    //        if (temp != null)
    //        {
    //            var temp2 = temp.Strings.FirstOrDefault(x => x.Index == StringIndex);
    //            if (temp2 != null)
    //            {
    //                temp2.NewString.Text = Text;
    //                return true;
    //            }
    //            return false;
    //        }
    //        return false;
    //    }

    //    public bool ImportTXT(string MSGName, int StringIndex, string Text, string NewName)
    //    {
    //        if (ImportTXT(MSGName, StringIndex, Text))
    //        {
    //            var temp = msg.FirstOrDefault(x => x.Name == MSGName);
    //            if (temp != null)
    //            {
    //                var temp2 = name.FirstOrDefault(x => x.Index == temp.CharacterIndex);
    //                if (temp2 != null)
    //                {
    //                    temp2.NewName = NewName;
    //                }
    //            }
    //            return true;
    //        }
    //        else return false;
    //    }

    //    void ParseMSG1(MemoryStream MemoryStreamMSG1)
    //    {
    //        BinaryReader BR;

    //        if (IsLittleEndian)
    //            BR = new BinaryReader(MemoryStreamMSG1);
    //        else
    //            BR = new BinaryReaderBE(MemoryStreamMSG1);

    //        BR.BaseStream.Position = 0;
    //        try
    //        {
    //            name.Clear();
    //            msg.Clear();

    //            byte[] buffer;

    //            int MSG_PointBlock_Pos = 0x20;
    //            BR.BaseStream.Position = 24;
    //            int MSG_count = BR.ReadInt32();
    //            BR.BaseStream.Position = MSG_PointBlock_Pos;
    //            List<int[]> MSG_Position = new List<int[]>();

    //            for (int i = 0; i < MSG_count; i++)
    //            {
    //                int[] temp = new int[2];
    //                temp[0] = BR.ReadInt32();
    //                temp[1] = BR.ReadInt32();
    //                MSG_Position.Add(temp);
    //            }

    //            int Name_Block_Position = BR.ReadInt32();
    //            int Name_Count = BR.ReadInt32();
    //            BR.BaseStream.Position = Name_Block_Position + MSG_PointBlock_Pos;
    //            List<long> Name_Position = new List<long>();
    //            for (int i = 0; i < Name_Count; i++)
    //            {
    //                Name_Position.Add(BR.ReadInt32());
    //            }


    //            int Index = 0;
    //            foreach (var a in Name_Position)
    //            {
    //                BR.BaseStream.Position = a + MSG_PointBlock_Pos;
    //                byte Byte = BR.ReadByte();
    //                List<byte> Bytes = new List<byte>();
    //                while (Byte != 0)
    //                {
    //                    Bytes.Add(Byte);
    //                    Byte = BR.ReadByte();
    //                }
    //                name.Add(new Names(OldChar, NewChar, Index, Bytes.ToArray(), ""));
    //                Index++;
    //            }

    //            Index = 0;

    //            foreach (var pos in MSG_Position)
    //            {
    //                BR.BaseStream.Position = MSG_PointBlock_Pos + pos[1];
    //                buffer = BR.ReadBytes(24);
    //                string MSG_Name = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
    //                if (string.IsNullOrEmpty(MSG_Name))
    //                {
    //                    MSG_Name = "<EMPTY>";
    //                }

    //                byte[] MSG_bytes;
    //                MSGs.MsgType Type;
    //                int Character_Index = 0xFFFF;

    //                if (pos[0] == 0)
    //                {
    //                    Type = MSGs.MsgType.MSG;
    //                    int count = BR.ReadUInt16();
    //                    Character_Index = BR.ReadUInt16();
    //                    BR.BaseStream.Position = BR.BaseStream.Position + 4 * count;

    //                    int size = BR.ReadInt32();

    //                    MSG_bytes = BR.ReadBytes(size);
    //                }
    //                else if (pos[0] == 1)
    //                {
    //                    Type = MSGs.MsgType.SEL;
    //                    BR.BaseStream.Position += 2;
    //                    int count = BR.ReadUInt16();
    //                    BR.BaseStream.Position += 4 * count + 4;

    //                    int size = BR.ReadInt32();

    //                    MSG_bytes = BR.ReadBytes(size);
    //                }
    //                else
    //                {
    //                    Logging.Write("Error: Unknown message type!");
    //                    return;
    //                }

    //                MSGs MSG = new MSGs(Index, MSG_Name, Type, Character_Index, MSG_bytes, OldChar, NewChar);

    //                ParseString(MSG.Strings, MSG.MsgBytes, OldChar, NewChar);

    //                msg.Add(MSG);

    //                Index++;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Logging.Write("Error: Parse MSG1 error!");
    //            Logging.Write(e.ToString());
    //            name.Clear();
    //            msg.Clear();
    //        }
    //    }

    //    private void ParseString(IList<MSGs.MSGstr> StringsList, byte[] SourceBytes, CharList Old, CharList New)
    //    {
    //        StringsList.Clear();

    //        int Index = 0;
    //        foreach (var Bytes in Utilities.SplitSourceBytes(SourceBytes))
    //        {
    //            MyStringElement[] temp = Bytes.parseString();
    //            List<MyStringElement> Prefix = new List<MyStringElement>();
    //            List<MyStringElement> Postfix = new List<MyStringElement>();
    //            List<MyStringElement> Strings = new List<MyStringElement>();

    //            int tempdown = 0;
    //            int temptop = temp.Length;

    //            for (int i = 0; i < temp.Length; i++)
    //            {
    //                if (temp[i].Type == MyStringElement.arrayType.System)
    //                    Prefix.Add(temp[i]);
    //                else
    //                {
    //                    tempdown = i;
    //                    i = temp.Length;
    //                }
    //            }

    //            for (int i = temp.Length - 1; i >= tempdown; i--)
    //            {
    //                if (temp[i].Type == MyStringElement.arrayType.System)
    //                    Postfix.Add(temp[i]);
    //                else
    //                {
    //                    temptop = i;
    //                    i = 0;
    //                }
    //            }

    //            Postfix.Reverse();

    //            for (int i = tempdown; i <= temptop; i++)
    //                Strings.Add(temp[i]);

    //            MSGs.MSGstr NewString = new MSGs.MSGstr(Old, New);
    //            NewString.Index = Index;
    //            NewString.Prefix.ElementArray = Prefix.ToArray();
    //            NewString.OldString.ElementArray = Strings.ToArray();
    //            NewString.Postfix.ElementArray = Postfix.ToArray();

    //            StringsList.Add(NewString);
    //            Index++;
    //        }
    //    }


    //    //public void SaveAsText(string FileName, string Index, int Option)
    //    //{
    //    //    if (Option == 1)
    //    //    {
    //    //        SaveAsTextOp1(FileName, Index);
    //    //    }
    //    //    else if (Option == 2)
    //    //    {
    //    //        SaveAsTextOp2(FileName, Index);
    //    //    }
    //    //    else
    //    //    {
    //    //        Logging.Write("SaveAsText Option invalid");
    //    //    }
    //    //}

    //    //void SaveAsTextOp1(string FileName, string Index)
    //    //{
    //    //    Directory.CreateDirectory("Export Text");

    //    //    string FileNameWE = Path.GetFileName(FileName);
    //    //    FileStream FS = new FileStream(@"Export Text\\NAMES.TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
    //    //    FS.Position = FS.Length;
    //    //    using (StreamWriter SW = new StreamWriter(FS))
    //    //        foreach (var NAME in name)
    //    //        {
    //    //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
    //    //        }


    //    //    string DirectoryName = new DirectoryInfo(Path.GetDirectoryName(FileName)).Name;
    //    //    FS = new FileStream("Export Text\\" + DirectoryName.ToUpper() + ".TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
    //    //    FS.Position = FS.Length;
    //    //    using (StreamWriter SW = new StreamWriter(FS))
    //    //    {
    //    //        List<name> Name = name.ToList();
    //    //        foreach (var MSG in msg)
    //    //        {
    //    //            foreach (var STR in MSG.Strings)
    //    //            {
    //    //                SW.Write(FileNameWE + "\t");
    //    //                SW.Write(Index + "\t");
    //    //                SW.Write(MSG.Name + "\t");
    //    //                SW.Write(STR.Index + "\t");
    //    //                if (Name.Exists(x => x.Index == MSG.Character_Index))
    //    //                {
    //    //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
    //    //                    SW.Write(Name_i.OldName);
    //    //                }
    //    //                else if (MSG.Type == "SEL")
    //    //                {
    //    //                    SW.Write("<SELECT>");
    //    //                }
    //    //                else { SW.Write("<NO_NAME>"); }

    //    //                SW.Write("\t");
    //    //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
    //    //                SW.Write(split[0]);
    //    //                for (int i = 1; i < split.Length; i++)
    //    //                {
    //    //                    SW.Write(" " + split[i]);
    //    //                }

    //    //                SW.WriteLine();
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //void SaveAsTextOp2(string FileName, string Index)
    //    //{

    //    //    string newFileName = Index == "" ? Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".TXT"
    //    //        : Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "-" + Index + ".TXT";

    //    //    using (StreamWriter SW = new StreamWriter(new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
    //    //    {
    //    //        foreach (var NAME in name)
    //    //        {
    //    //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
    //    //        }
    //    //        SW.WriteLine();

    //    //        List<name> Name = name.ToList();
    //    //        foreach (var MSG in msg)
    //    //        {
    //    //            foreach (var STR in MSG.Strings)
    //    //            {
    //    //                SW.Write(MSG.Name + "\t");
    //    //                SW.Write(STR.Index + "\t");
    //    //                if (Name.Exists(x => x.Index == MSG.Character_Index))
    //    //                {
    //    //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
    //    //                    SW.Write(Name_i.OldName);
    //    //                }
    //    //                else if (MSG.Type == "SEL")
    //    //                {
    //    //                    SW.Write("<SELECT>");
    //    //                }
    //    //                else { SW.Write("<NO_NAME>"); }

    //    //                SW.Write("\t");
    //    //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
    //    //                SW.Write(split[0]);
    //    //                for (int i = 1; i < split.Length; i++)
    //    //                {
    //    //                    SW.Write(" " + split[i]);
    //    //                }

    //    //                SW.WriteLine();
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //}


    //public class Project
    //{
    //    PersonaFileTypes.MSG1 MSG1;

    //    public string SourceFileName = "";
    //    public long SelectPosition = -1;

    //    public Project(PersonaFileTypes.MSG1 MSG1)
    //    {
    //        this.MSG1 = MSG1;
    //    }

    //    public Project(PersonaFileTypes.MSG1 MSG1, string SourceFileName, long SelectPosition)
    //    {
    //        this.MSG1 = MSG1;
    //        this.SourceFileName = SourceFileName;
    //        this.SelectPosition = SelectPosition;
    //    }

    //    public void SaveProject(string path)
    //    {
    //        try
    //        {
    //            XDocument xDoc = new XDocument();
    //            XElement Document = new XElement("MSG1");
    //            Document.Add(new XAttribute("SourceFileName", SourceFileName));
    //            Document.Add(new XAttribute("Position", SelectPosition));
    //            xDoc.Add(Document);
    //            XElement CharName = new XElement("CharacterNames");
    //            Document.Add(CharName);

    //            foreach (var NAME in MSG1.name)
    //            {
    //                XElement Name = new XElement("Name");
    //                Name.Add(new XAttribute("Index", NAME.Index));
    //                Name.Add(new XElement("OldNameSource", BitConverter.ToString(NAME.OldNameBytes)));
    //                Name.Add(new XElement("NewName", NAME.NewName));
    //                CharName.Add(Name);
    //            }

    //            XElement MES = new XElement("MSG");
    //            Document.Add(MES);

    //            foreach (var MSG in MSG1.msg)
    //            {
    //                XElement Msg = new XElement("Message");
    //                Msg.Add(new XAttribute("Index", MSG.Index));
    //                Msg.Add(new XElement("Type", MSG.Type));
    //                Msg.Add(new XElement("Name", MSG.Name));
    //                Msg.Add(new XElement("CharacterNameIndex", MSG.CharacterIndex));
    //                Msg.Add(new XElement("SourceBytes", BitConverter.ToString(MSG.MsgBytes)));

    //                XElement Strings = new XElement("MessageStrings");
    //                Msg.Add(Strings);
    //                foreach (var STR in MSG.Strings)
    //                {
    //                    XElement String = new XElement("String");
    //                    String.Add(new XAttribute("Index", STR.Index));
    //                    Strings.Add(String);

    //                    foreach (var A in STR.Prefix.ElementArray)
    //                    {
    //                        XElement PrefixBytes = new XElement("PrefixBytes", BitConverter.ToString(A.Bytes));
    //                        PrefixBytes.Add(new XAttribute("Index", Array.IndexOf(STR.Prefix.ElementArray, A)));
    //                        PrefixBytes.Add(new XAttribute("Type", A.Type));
    //                        String.Add(PrefixBytes);
    //                    }

    //                    foreach (var A in STR.OldString.ElementArray)
    //                    {
    //                        XElement OldStringBytes = new XElement("OldStringBytes", BitConverter.ToString(A.Bytes));
    //                        OldStringBytes.Add(new XAttribute("Index", Array.IndexOf(STR.OldString.ElementArray, A)));
    //                        OldStringBytes.Add(new XAttribute("Type", A.Type));
    //                        String.Add(OldStringBytes);
    //                    }

    //                    String.Add(new XElement("NewString", STR.NewString.Text));

    //                    foreach (var A in STR.Postfix.ElementArray)
    //                    {
    //                        XElement PostfixBytes = new XElement("PostfixBytes", BitConverter.ToString(A.Bytes));
    //                        PostfixBytes.Add(new XAttribute("Index", Array.IndexOf(STR.Postfix.ElementArray, A)));
    //                        PostfixBytes.Add(new XAttribute("Type", A.Type));
    //                        String.Add(PostfixBytes);
    //                    }
    //                }

    //                MES.Add(Msg);
    //            }

    //            xDoc.Save(path);
    //        }
    //        catch (Exception e)
    //        {
    //            Logging.Write(e.ToString());
    //        }
    //    }

    //    public bool OpenProject(string path)
    //    {
    //        MSG1.msg.Clear();
    //        MSG1.name.Clear();

    //        try
    //        {
    //            XDocument xDoc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
    //            XElement MSG1Doc = xDoc.Element("MSG1");
    //            XAttribute temp = MSG1Doc.Attribute("Position");
    //            SelectPosition = temp != null ? Convert.ToInt32(temp.Value) : -1;

    //            temp = MSG1Doc.Attribute("SourceFileName");
    //            SourceFileName = temp != null ? temp.Value : "";

    //            foreach (var NAME in MSG1Doc.Element("CharacterNames").Elements())
    //            {
    //                int Index = Convert.ToInt32(NAME.Attribute("Index").Value);

    //                byte[] OldNameSource = new byte[0];
    //                string OldNameSource_str = NAME.Element("OldNameSource").Value;
    //                if (OldNameSource_str != "")
    //                    OldNameSource = Enumerable.Range(0, OldNameSource_str.Split('-').Length).Select(x => Convert.ToByte(OldNameSource_str.Split('-')[x], 16)).ToArray();

    //                string NewName = NAME.Element("NewName").Value;

    //                MSG1.name.Add(new PersonaFileTypes.MSG1.Names(MSG1.OldChar, MSG1.NewChar, Index, OldNameSource, NewName));
    //            }

    //            foreach (var Message in MSG1Doc.Element("MSG").Elements())
    //            {
    //                int Index = Convert.ToInt32(Message.Attribute("Index").Value);
    //                string Type = Message.Element("Type").Value;
    //                string Name = Message.Element("Name").Value;
    //                int CharacterNameIndex = Convert.ToInt32(Message.Element("CharacterNameIndex").Value);

    //                byte[] SourceBytes = new byte[0];
    //                string SourceBytes_str = Message.Element("SourceBytes").Value;
    //                if (SourceBytes_str != "")
    //                {
    //                    SourceBytes = Enumerable.Range(0, SourceBytes_str.Split('-').Length).Select(x => Convert.ToByte(SourceBytes_str.Split('-')[x], 16)).ToArray();
    //                }

    //                PersonaFileTypes.MSG1.MSGs MSG = new PersonaFileTypes.MSG1.MSGs(Index, Name, (PersonaFileTypes.MSG1.MSGs.MsgType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MSGs.MsgType), Type),
    //                    CharacterNameIndex, SourceBytes, MSG1.OldChar, MSG1.NewChar);

    //                foreach (var Strings in Message.Element("MessageStrings").Elements())
    //                {
    //                    PersonaFileTypes.MSG1.MSGs.MSGstr String = new PersonaFileTypes.MSG1.MSGs.MSGstr(MSG1.OldChar, MSG1.NewChar);

    //                    String.Index = Convert.ToInt32(Strings.Attribute("Index").Value);

    //                    List<PersonaFileTypes.MSG1.MyStringElement> TempList = new List<PersonaFileTypes.MSG1.MyStringElement>();
    //                    foreach (var PrefixByte in Strings.Elements("PrefixBytes"))
    //                    {
    //                        int PrefixIndex = Convert.ToInt32(PrefixByte.Attribute("Index").Value);
    //                        PersonaFileTypes.MSG1.MyStringElement.arrayType PrefixType = (PersonaFileTypes.MSG1.MyStringElement.arrayType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MyStringElement.arrayType), PrefixByte.Attribute("Type").Value);

    //                        byte[] PrefixBytes = new byte[0];
    //                        string PrefixBytes_str = PrefixByte.Value;
    //                        if (PrefixBytes_str != "")
    //                        {
    //                            PrefixBytes = Enumerable.Range(0, PrefixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PrefixBytes_str.Split('-')[x], 16)).ToArray();
    //                        }
    //                        TempList.Add(new PersonaFileTypes.MSG1.MyStringElement(PrefixIndex, PrefixType, PrefixBytes));
    //                    }
    //                    String.Prefix.ElementArray = TempList.ToArray();

    //                    TempList = new List<PersonaFileTypes.MSG1.MyStringElement>();
    //                    foreach (var Old in Strings.Elements("OldStringBytes"))
    //                    {
    //                        int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
    //                        PersonaFileTypes.MSG1.MyStringElement.arrayType OldType = (PersonaFileTypes.MSG1.MyStringElement.arrayType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MyStringElement.arrayType), Old.Attribute("Type").Value);

    //                        byte[] OldBytes = new byte[0];
    //                        string OldBytes_str = Old.Value;
    //                        if (OldBytes_str != "")
    //                        {
    //                            OldBytes = Enumerable.Range(0, OldBytes_str.Split('-').Length).Select(x => Convert.ToByte(OldBytes_str.Split('-')[x], 16)).ToArray();
    //                        }

    //                        TempList.Add(new PersonaFileTypes.MSG1.MyStringElement(OldIndex, OldType, OldBytes));
    //                    }
    //                    String.OldString.ElementArray = TempList.ToArray();

    //                    String.NewString.Text = Strings.Element("NewString").Value;

    //                    TempList = new List<PersonaFileTypes.MSG1.MyStringElement>();
    //                    foreach (var PostfixByte in Strings.Elements("PostfixBytes"))
    //                    {
    //                        int PostfixIndex = Convert.ToInt32(PostfixByte.Attribute("Index").Value);
    //                        PersonaFileTypes.MSG1.MyStringElement.arrayType PostfixType = (PersonaFileTypes.MSG1.MyStringElement.arrayType)Enum.Parse(typeof(PersonaFileTypes.MSG1.MyStringElement.arrayType), PostfixByte.Attribute("Type").Value);

    //                        byte[] PostfixBytes = new byte[0];
    //                        string PostfixBytes_str = PostfixByte.Value;
    //                        if (PostfixBytes_str != "")
    //                        {
    //                            PostfixBytes = Enumerable.Range(0, PostfixBytes_str.Split('-').Length).Select(x => Convert.ToByte(PostfixBytes_str.Split('-')[x], 16)).ToArray();
    //                        }

    //                        TempList.Add(new PersonaFileTypes.MSG1.MyStringElement(PostfixIndex, PostfixType, PostfixBytes));
    //                    }
    //                    String.Postfix.ElementArray = TempList.ToArray();

    //                    MSG.Strings.Add(String);
    //                }

    //                MSG1.msg.Add(MSG);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Logging.Write(e.ToString());
    //            return false;
    //        }
    //        return true;
    //    }

    //    private class LineMap
    //    {
    //        public LineMap(string map)
    //        {
    //            string[] temp = Regex.Split(map, " ");
    //            FileName = Array.IndexOf(temp, "%FN");
    //            MSGname = Array.IndexOf(temp, "%MSG");
    //            StringIndex = Array.IndexOf(temp, "%MSGIND");
    //            NewText = Array.IndexOf(temp, "%NEWSTR");
    //            OldName = Array.IndexOf(temp, "%OLDNM");
    //            NewName = Array.IndexOf(temp, "%NEWNM");
    //        }

    //        public int FileName { get; set; }
    //        public int MSGname { get; set; }
    //        public int StringIndex { get; set; }
    //        public int NewText { get; set; }

    //        public int OldName { get; set; }
    //        public int NewName { get; set; }
    //    }

    //    public bool ImportTXT(string txtfile, string map, bool auto, int width)
    //    {
    //        int Width = (int)Math.Round((double)width / 0.9375);
    //        LineMap MAP = new LineMap(map);

    //        if (MAP.FileName >= 0 & MAP.MSGname >= 0 & MAP.StringIndex >= 0 & MAP.NewText >= 0)
    //        {
    //            StreamReader SR = new StreamReader(new FileStream(txtfile, FileMode.Open, FileAccess.Read));
    //            while (SR.EndOfStream == false)
    //            {
    //                string line = SR.ReadLine();
    //                string[] linespl = Regex.Split(line, "\t");

    //                if (Path.GetFileNameWithoutExtension(SourceFileName) == Path.GetFileNameWithoutExtension(linespl[MAP.FileName]))
    //                {
    //                    string NewText = linespl[MAP.NewText];
    //                    if (auto)
    //                        NewText = NewText.SplitByWidth(MSG1.NewChar.List, Width);
    //                    else
    //                        NewText = NewText.Replace("\\n", "\n");

    //                    if (MAP.NewName >= 0)
    //                        MSG1.ImportTXT(linespl[MAP.MSGname], Convert.ToInt32(linespl[MAP.StringIndex]), NewText, linespl[MAP.NewName]);
    //                    else
    //                        MSG1.ImportTXT(linespl[MAP.MSGname], Convert.ToInt32(linespl[MAP.StringIndex]), NewText);

    //                }
    //            }
    //        }
    //        else if (MAP.OldName >= 0 & MAP.NewName >= 0)
    //        {

    //        }

    //        return false;
    //    }

    //    public bool ExportNewFile()
    //    {
    //        return true;
    //    }
    //}


    //public class BMDbase
    //{
    //    public bool Load(string FileName, bool IsLittleEndian)
    //    {
    //        this.IsLittleEndian = IsLittleEndian;
    //        MemoryStream MemoryStreamMSG1;
    //        using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read))
    //        {
    //            MemoryStreamMSG1 = new MemoryStream();
    //            FS.CopyTo(MemoryStreamMSG1);
    //        }
    //        ParseMSG1(MemoryStreamMSG1);

    //        return true;
    //    }

    //    public BMDbase(bool IsLittleEndian, CharList OldChar, CharList NewChar)
    //    {
    //        this.OldChar = OldChar;
    //        this.NewChar = NewChar;
    //        this.IsLittleEndian = IsLittleEndian;
    //    }

    //    public int SaveAsTextOption { get; set; } = 0;

    //    public delegate void StringChangedEventHandler(string String);
    //    public delegate void ArrayChangedEventHandler(byte[] array);
    //    public delegate void ListChangedEventHandler(List<MyStringElement> list);
    //    public delegate void ElementArrayEventHandler(MyStringElement[] array);

    //    public struct MyStringElement
    //    {
    //        public enum arrayType
    //        {
    //            Empty,
    //            System,
    //            Text
    //        }

    //        public string GetText(CharList CharList)
    //        {
    //            if (Type == arrayType.System)
    //            {
    //                if (Bytes[0] == 0x0A)
    //                {
    //                    return "\n";
    //                }
    //                else
    //                {
    //                    return GetSystem();
    //                }
    //            }
    //            else
    //            {
    //                string returned = "";
    //                for (int i = 0; i < Bytes.Length; i++)
    //                {
    //                    if (0x20 <= Bytes[i] & Bytes[i] < 0x80)
    //                    {
    //                        returned += CharList.GetChar(Bytes[i]);
    //                    }
    //                    else if (0x80 <= Bytes[i] & Bytes[i] < 0xF0)
    //                    {
    //                        int newindex = (Bytes[i] - 0x81) * 0x80 + Bytes[i + 1] + 0x20;

    //                        i++;
    //                        returned += CharList.GetChar(newindex);
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine("ASD");
    //                    }
    //                }
    //                return returned;
    //            }
    //        }

    //        public string GetSystem()
    //        {
    //            string returned = "";

    //            if (Bytes.Length > 0)
    //            {
    //                returned += "{" + Convert.ToString(Bytes[0], 16).PadLeft(2, '0').ToUpper();
    //                for (int i = 1; i < Bytes.Length; i++)
    //                {
    //                    returned += " " + Convert.ToString(Bytes[i], 16).PadLeft(2, '0').ToUpper();
    //                }
    //                returned += "}";
    //            }

    //            return returned;
    //        }

    //        public MyStringElement(int Index, arrayType Type, byte[] Bytes)
    //        {
    //            this.Index = Index;
    //            this.Type = Type;
    //            this.Bytes = Bytes;
    //        }

    //        public int Index { get; private set; }
    //        public arrayType Type { get; private set; }
    //        public byte[] Bytes { get; private set; }

    //    }

    //    public class Names : INotifyPropertyChanged
    //    {
    //        public event StringChangedEventHandler OldNameChanged;
    //        public event StringChangedEventHandler NewNameChanged;
    //        public event ArrayChangedEventHandler OldNameArrayChanged;
    //        public event ArrayChangedEventHandler NewNameArrayChanged;

    //        #region INotifyPropertyChanged implementation
    //        public event PropertyChangedEventHandler PropertyChanged;

    //        protected void Notify(string propertyName)
    //        {
    //            if (this.PropertyChanged != null)
    //            {
    //                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //            }
    //        }
    //        #endregion INotifyPropertyChanged implementation

    //        public CharList OldCharList { get; set; }
    //        public CharList NewCharList { get; set; }

    //        public Names(CharList Old, CharList New, int Index, byte[] OldNameBytes, string NewName)
    //        {
    //            OldCharList = Old;
    //            NewCharList = New;
    //            this.Index = Index;
    //            this.OldNameBytes = OldNameBytes;
    //            this.NewName = NewName;

    //            OldCharList.PropertyChanged += OldCharList_PropertyChanged;
    //            NewCharList.PropertyChanged += NewCharList_PropertyChanged;
    //        }

    //        private void NewCharList_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //        {
    //            NewNameBytes = NewName.GetMyByteArray(NewCharList).getAllBytes();
    //        }

    //        private void OldCharList_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //        {
    //            OldName = OldNameBytes.parseString().GetString(OldCharList, false);
    //        }

    //        public int Index { get; set; } = 0;

    //        byte[] _OldNameBytes;
    //        byte[] _NewNameBytes;
    //        string _NewName = "";
    //        string _OldName = "";

    //        public byte[] OldNameBytes
    //        {
    //            get { return _OldNameBytes; }
    //            set
    //            {
    //                _OldNameBytes = value;
    //                OldName = _OldNameBytes.parseString().GetString(OldCharList, false);
    //                OldNameArrayChanged?.Invoke(_OldNameBytes);
    //                Notify("OldNameBytes");
    //            }
    //        }
    //        public string OldName
    //        {
    //            get { return _OldName; }
    //            set
    //            {
    //                if (value != _OldName)
    //                {
    //                    _OldName = value;
    //                    OldNameChanged?.Invoke(_OldName);
    //                    Notify("OldName");
    //                }
    //            }
    //        }
    //        public byte[] NewNameBytes
    //        {
    //            get { return _NewNameBytes; }
    //            set
    //            {
    //                _NewNameBytes = value;
    //                NewNameArrayChanged?.Invoke(_NewNameBytes);
    //                Notify("NewNameBytes");
    //            }
    //        }
    //        public string NewName
    //        {
    //            get
    //            {
    //                return _NewName;
    //            }
    //            set
    //            {
    //                if (value != _NewName)
    //                {
    //                    _NewName = value;
    //                    NewNameBytes = _NewName.GetMyByteArray(NewCharList).getAllBytes();
    //                    NewNameChanged?.Invoke(_NewName);
    //                    Notify("NewName");
    //                }
    //            }
    //        }
    //    }

    //    public class MSGs
    //    {
    //        public class MSGstr
    //        {
    //            public class MSGstrElement : INotifyPropertyChanged
    //            {
    //                #region INotifyPropertyChanged implementation
    //                public event PropertyChangedEventHandler PropertyChanged;

    //                protected void Notify(string propertyName)
    //                {
    //                    if (this.PropertyChanged != null)
    //                    {
    //                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //                    }
    //                }
    //                #endregion INotifyPropertyChanged implementation       

    //                public enum Mode
    //                {
    //                    ArrayToText,
    //                    ArrayToTextSYS,
    //                    TextToArray
    //                }

    //                public event StringChangedEventHandler TextChanged;
    //                public event ElementArrayEventHandler ElementArrayChanged;

    //                private Mode CurrentMode { get; set; }
    //                public CharList CurrentCharList { get; set; }

    //                private MyStringElement[] _ElementArray;
    //                public MyStringElement[] ElementArray
    //                {
    //                    get { return _ElementArray; }
    //                    set
    //                    {
    //                        _ElementArray = value;
    //                        if (CurrentMode == Mode.ArrayToText)
    //                            Text = _ElementArray.GetString(CurrentCharList, false);
    //                        else if (CurrentMode == Mode.ArrayToTextSYS)
    //                            Text = _ElementArray.GetString();
    //                        ElementArrayChanged?.Invoke(_ElementArray);
    //                    }
    //                }

    //                private string _Text = "";
    //                public string Text
    //                {
    //                    get { return _Text; }
    //                    set
    //                    {
    //                        if (value != _Text)
    //                        {
    //                            _Text = value;
    //                            TextChanged?.Invoke(Text);
    //                            if (CurrentMode == Mode.TextToArray)
    //                                ElementArray = Text.GetMyByteArray(CurrentCharList);
    //                        }
    //                    }
    //                }

    //                public MSGstrElement(CharList CharList, Mode Mode)
    //                {
    //                    CurrentMode = Mode;

    //                    if (CurrentMode != Mode.ArrayToTextSYS)
    //                    {
    //                        CurrentCharList = CharList;
    //                        CurrentCharList.PropertyChanged += CurrentCharList_PropertyChanged;
    //                    }
    //                }

    //                private void CurrentCharList_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //                {
    //                    if (CurrentMode == Mode.ArrayToText)
    //                        Text = _ElementArray.GetString(CurrentCharList, false);
    //                    else if (CurrentMode == Mode.TextToArray)
    //                        ElementArray = Text.GetMyByteArray(CurrentCharList);
    //                }
    //            }

    //            public MSGstr(CharList Old, CharList New)
    //            {
    //                Prefix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
    //                OldString = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToText);
    //                NewString = new MSGstrElement(New, MSGstrElement.Mode.TextToArray);
    //                Postfix = new MSGstrElement(Old, MSGstrElement.Mode.ArrayToTextSYS);
    //            }

    //            public int Index { get; set; } = 0;
    //            public MSGstrElement Prefix { get; set; }
    //            public MSGstrElement OldString { get; set; }
    //            public MSGstrElement NewString { get; set; }
    //            public MSGstrElement Postfix { get; set; }
    //        }

    //        public enum MsgType
    //        {
    //            MSG = 0,
    //            SEL = 1
    //        }

    //        public MSGs(int Index, string Name, MsgType Type, int CharacterIndex, byte[] MsgBytes, CharList Old, CharList New)
    //        {
    //            this.Index = Index;
    //            this.Type = Type;
    //            this.Name = Name;
    //            this.CharacterIndex = CharacterIndex;
    //            this.MsgBytes = MsgBytes;
    //        }

    //        public int Index { get; set; }
    //        public MsgType Type { get; set; }
    //        public string Name { get; set; }
    //        public int CharacterIndex { get; set; }
    //        public byte[] MsgBytes { get; set; }
    //        public List<MSGstr> Strings { get; set; } = new List<MSGstr>();
    //    }

    //    public bool IsLittleEndian { get; private set; }
    //    public CharList OldChar { get; private set; }
    //    public CharList NewChar { get; private set; }

    //    public BindingList<MSGs> msg { get; set; } = new BindingList<MSGs>();
    //    public BindingList<Names> name { get; set; } = new BindingList<Names>();

    //    public MemoryStream GetNewMSG(bool IsLittleEndian)
    //    {
    //        return GetNewMSG1.Get(msg, name, IsLittleEndian);
    //    }

    //    static class GetNewMSG1
    //    {
    //        public static MemoryStream Get(IList<MSGs> msg, IList<Names> name, bool IsLittleEndian)
    //        {
    //            byte[] buffer;

    //            BinaryWriter BW;

    //            if (IsLittleEndian)
    //                BW = new BinaryWriter(new MemoryStream());
    //            else
    //                BW = new BinaryWriterBE(new MemoryStream());

    //            List<List<int>> MSG_pos = new List<List<int>>();
    //            List<int> NAME_pos = new List<int>();
    //            List<int> LastBlock = new List<int>();

    //            buffer = new byte[4] { 7, 0, 0, 0 };
    //            BW.Write(buffer);
    //            BW.Write((int)0x0);

    //            buffer = System.Text.Encoding.ASCII.GetBytes("MSG1");
    //            if (!IsLittleEndian)
    //                Array.Reverse(buffer);

    //            BW.Write(buffer);

    //            BW.Write((int)0x0);
    //            BW.Write((int)0x0);
    //            BW.Write((int)0x0);
    //            BW.Write(msg.Count);
    //            BW.Write((ushort)0);
    //            BW.Write((ushort)0x2);

    //            foreach (var MSG in msg)
    //            {
    //                if (MSG.Type == MSGs.MsgType.MSG) { BW.Write((int)0x0); }
    //                else if (MSG.Type == MSGs.MsgType.SEL) { BW.Write((int)0x1); }
    //                else
    //                {
    //                    Logging.Write("PersonaEditorLib", "Error: Unknown MSG Type");
    //                    return null;
    //                }

    //                LastBlock.Add((int)BW.BaseStream.Position);
    //                BW.Write((int)0x0);
    //            }

    //            LastBlock.Add((int)BW.BaseStream.Position);
    //            BW.Write((int)0x0);
    //            BW.Write(name.Count);
    //            BW.Write((int)0x0);
    //            BW.Write((int)0x0);

    //            foreach (var MSG in msg)
    //            {
    //                List<int> MSG_o = new List<int>();
    //                MSG_o.Add((int)BW.BaseStream.Position);

    //                BW.WriteString(MSG.Name, 24);

    //                if (MSG.Type == MSGs.MsgType.MSG)
    //                {
    //                    BW.Write((ushort)MSG.Strings.Count);

    //                    if (MSG.CharacterIndex == -1) { BW.Write((ushort)0xFFFF); }
    //                    else { BW.Write((ushort)MSG.CharacterIndex); }
    //                }
    //                else if (MSG.Type == MSGs.MsgType.SEL)
    //                {
    //                    BW.Write((ushort)0);
    //                    BW.Write((ushort)MSG.Strings.Count);
    //                    BW.Write((int)0x0);
    //                }

    //                int Size = 0;

    //                foreach (var String in MSG.Strings)
    //                {
    //                    LastBlock.Add((int)BW.BaseStream.Position);
    //                    BW.Write((int)0x0);
    //                    foreach (var Str in String.Prefix.ElementArray)
    //                    {
    //                        Size += Str.Bytes.Length;
    //                    }
    //                    foreach (var Str in String.NewString.ElementArray)
    //                    {
    //                        Size += Str.Bytes.Length;
    //                    }
    //                    foreach (var Str in String.Postfix.ElementArray)
    //                    {
    //                        Size += Str.Bytes.Length;
    //                    }
    //                }
    //                MSG_o.Add(Size);

    //                BW.Write((int)0x0);

    //                foreach (var String in MSG.Strings)
    //                {
    //                    List<byte> NewString = new List<byte>();
    //                    foreach (var prefix in String.Prefix.ElementArray)
    //                    {
    //                        NewString.AddRange(prefix.Bytes);
    //                    }
    //                    foreach (var str in String.NewString.ElementArray)
    //                    {
    //                        NewString.AddRange(str.Bytes);
    //                    }
    //                    foreach (var postfix in String.Postfix.ElementArray)
    //                    {
    //                        NewString.AddRange(postfix.Bytes);
    //                    }

    //                    MSG_o.Add((int)BW.BaseStream.Position);
    //                    BW.Write(NewString.ToArray());
    //                }

    //                while (BW.BaseStream.Length % 4 != 0)
    //                {
    //                    BW.Write((byte)0);
    //                }

    //                MSG_pos.Add(MSG_o);
    //            }

    //            long Name_Block_pos = BW.BaseStream.Length;
    //            BW.BaseStream.Position = 0x20;
    //            for (int i = 0; i < msg.Count; i++)
    //            {
    //                BW.BaseStream.Position += 4;
    //                BW.Write((int)MSG_pos[i][0] - 0x20);
    //            }
    //            BW.Write((int)Name_Block_pos - 0x20);
    //            for (int i = 0; i < msg.Count; i++)
    //            {
    //                BW.BaseStream.Position = MSG_pos[i][0];

    //                if (msg[i].Type == MSGs.MsgType.MSG)
    //                {
    //                    BW.BaseStream.Position += 28;
    //                }
    //                else if (msg[i].Type == MSGs.MsgType.SEL)
    //                {
    //                    BW.BaseStream.Position += 32;
    //                }

    //                for (int k = 0; k < msg[i].Strings.Count; k++)
    //                {
    //                    BW.Write((int)MSG_pos[i][k + 2] - 0x20);
    //                }
    //                BW.Write((int)MSG_pos[i][1]);
    //            }


    //            BW.BaseStream.Position = Name_Block_pos;
    //            for (int i = 0; i < name.Count; i++)
    //            {
    //                LastBlock.Add((int)BW.BaseStream.Position);
    //                BW.Write((int)0);
    //            }

    //            foreach (var NAME in name)
    //            {
    //                NAME_pos.Add((int)BW.BaseStream.Position);
    //                if (NAME.NewNameBytes.Length == 0)
    //                    BW.Write(NAME.OldNameBytes);
    //                else
    //                    BW.Write(NAME.NewNameBytes);

    //                BW.Write((byte)0);
    //            }
    //            BW.BaseStream.Position = Name_Block_pos;
    //            for (int i = 0; i < name.Count; i++)
    //            {
    //                BW.Write((int)NAME_pos[i] - 0x20);
    //            }
    //            BW.BaseStream.Position = BW.BaseStream.Length;
    //            while (BW.BaseStream.Length % 4 != 0)
    //            {
    //                BW.Write((byte)0);
    //            }

    //            int LastBlockPos = (int)BW.BaseStream.Position;
    //            byte[] LastBlockBytes = getLastBlock(LastBlock);
    //            BW.Write(LastBlockBytes);

    //            BW.BaseStream.Position = 0x10;
    //            BW.Write((int)LastBlockPos);
    //            BW.Write((int)LastBlockBytes.Length);

    //            BW.BaseStream.Position = 0x4;
    //            BW.Write((int)BW.BaseStream.Length);

    //            BW.BaseStream.Position = 0;

    //            buffer = new byte[BW.BaseStream.Length];
    //            BW.BaseStream.Read(buffer, 0, (int)BW.BaseStream.Length);

    //            return new MemoryStream(buffer);
    //        }

    //        static byte[] getLastBlock(List<int> Addresses)
    //        {
    //            int sum = 0;
    //            List<byte> returned = new List<byte>();

    //            for (int i = 0; i < Addresses.Count; i++)
    //            {
    //                int reloc = Addresses[i] - sum - 0x20;
    //                int amount = getSeq(ref Addresses, i);
    //                Encode(reloc, ref returned, ref sum);
    //                if (amount > 1)
    //                {
    //                    reloc = 7;
    //                    reloc |= ((amount - 2) / 2) << 4;
    //                    if (amount % 2 == 1)
    //                    {
    //                        reloc |= 8;
    //                    }
    //                    returned.Add((byte)reloc);
    //                    i += amount;
    //                    sum += amount * 4;
    //                }
    //            }

    //            return returned.ToArray();
    //        }

    //        static int getSeq(ref List<int> Addresses, int index)
    //        {
    //            if (index < Addresses.Count - 1)
    //            {
    //                if (Addresses[index + 1] - Addresses[index] == 4)
    //                {
    //                    return getSeq(ref Addresses, index + 1) + 1;
    //                }
    //                else
    //                {
    //                    return 0;
    //                }
    //            }
    //            return 0;
    //        }

    //        static void Encode(int reloc, ref List<byte> LastBlock, ref int sum)
    //        {
    //            if (reloc % 2 == 0)
    //            {
    //                int temp = reloc >> 1;
    //                if (temp <= 0xFF)
    //                {
    //                    LastBlock.Add((byte)temp);
    //                }
    //                else
    //                {
    //                    byte item = (byte)((reloc & 0xff) + 1);
    //                    byte num2 = (byte)((reloc & 0xff00) >> 8);
    //                    LastBlock.Add(item);
    //                    LastBlock.Add(num2);
    //                }

    //            }
    //            else
    //            {
    //                byte item = (byte)((reloc & 0xff) + 1);
    //                byte num2 = (byte)((reloc & 0xff00) >> 8);
    //                LastBlock.Add(item);
    //                LastBlock.Add(num2);
    //            }
    //            sum += reloc;
    //        }
    //    }

    //    public bool ImportTXT(string MSGName, int StringIndex, string Text)
    //    {
    //        var temp = msg.FirstOrDefault(x => x.Name == MSGName);
    //        if (temp != null)
    //        {
    //            var temp2 = temp.Strings.FirstOrDefault(x => x.Index == StringIndex);
    //            if (temp2 != null)
    //            {
    //                temp2.NewString.Text = Text;
    //                return true;
    //            }
    //            return false;
    //        }
    //        return false;
    //    }

    //    public bool ImportTXT(string MSGName, int StringIndex, string Text, string NewName)
    //    {
    //        if (ImportTXT(MSGName, StringIndex, Text))
    //        {
    //            var temp = msg.FirstOrDefault(x => x.Name == MSGName);
    //            if (temp != null)
    //            {
    //                var temp2 = name.FirstOrDefault(x => x.Index == temp.CharacterIndex);
    //                if (temp2 != null)
    //                {
    //                    temp2.NewName = NewName;
    //                }
    //            }
    //            return true;
    //        }
    //        else return false;
    //    }

    //    private void ParseMSG1(MemoryStream MemoryStreamMSG1)
    //    {
    //        BinaryReader BR;

    //        if (IsLittleEndian)
    //            BR = new BinaryReader(MemoryStreamMSG1);
    //        else
    //            BR = new BinaryReaderBE(MemoryStreamMSG1);

    //        BR.BaseStream.Position = 0;
    //        try
    //        {
    //            name.Clear();
    //            msg.Clear();

    //            byte[] buffer;

    //            int MSG_PointBlock_Pos = 0x20;
    //            BR.BaseStream.Position = 24;
    //            int MSG_count = BR.ReadInt32();
    //            BR.BaseStream.Position = MSG_PointBlock_Pos;
    //            List<int[]> MSG_Position = new List<int[]>();

    //            for (int i = 0; i < MSG_count; i++)
    //            {
    //                int[] temp = new int[2];
    //                temp[0] = BR.ReadInt32();
    //                temp[1] = BR.ReadInt32();
    //                MSG_Position.Add(temp);
    //            }

    //            int Name_Block_Position = BR.ReadInt32();
    //            int Name_Count = BR.ReadInt32();
    //            BR.BaseStream.Position = Name_Block_Position + MSG_PointBlock_Pos;
    //            List<long> Name_Position = new List<long>();
    //            for (int i = 0; i < Name_Count; i++)
    //                Name_Position.Add(BR.ReadInt32());

    //            int Index = 0;
    //            foreach (var a in Name_Position)
    //            {
    //                BR.BaseStream.Position = a + MSG_PointBlock_Pos;
    //                byte Byte = BR.ReadByte();
    //                List<byte> Bytes = new List<byte>();
    //                while (Byte != 0)
    //                {
    //                    Bytes.Add(Byte);
    //                    Byte = BR.ReadByte();
    //                }
    //                name.Add(new Names(OldChar, NewChar, Index, Bytes.ToArray(), ""));
    //                Index++;
    //            }

    //            Index = 0;

    //            foreach (var pos in MSG_Position)
    //            {
    //                BR.BaseStream.Position = MSG_PointBlock_Pos + pos[1];
    //                buffer = BR.ReadBytes(24);
    //                string MSG_Name = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
    //                if (string.IsNullOrEmpty(MSG_Name))
    //                    MSG_Name = "<EMPTY>";

    //                byte[] MSG_bytes;
    //                MSGs.MsgType Type;
    //                int Character_Index = 0xFFFF;

    //                if (pos[0] == 0)
    //                {
    //                    Type = MSGs.MsgType.MSG;
    //                    int count = BR.ReadUInt16();
    //                    Character_Index = BR.ReadUInt16();
    //                    BR.BaseStream.Position = BR.BaseStream.Position + 4 * count;

    //                    int size = BR.ReadInt32();

    //                    MSG_bytes = BR.ReadBytes(size);
    //                }
    //                else if (pos[0] == 1)
    //                {
    //                    Type = MSGs.MsgType.SEL;
    //                    BR.BaseStream.Position += 2;
    //                    int count = BR.ReadUInt16();
    //                    BR.BaseStream.Position += 4 * count + 4;

    //                    int size = BR.ReadInt32();

    //                    MSG_bytes = BR.ReadBytes(size);
    //                }
    //                else
    //                {
    //                    Logging.Write("PersonaEditorLib", "Error: Unknown message type!");
    //                    return;
    //                }

    //                MSGs MSG = new MSGs(Index, MSG_Name, Type, Character_Index, MSG_bytes, OldChar, NewChar);

    //                ParseString(MSG.Strings, MSG.MsgBytes, OldChar, NewChar);

    //                msg.Add(MSG);

    //                Index++;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Logging.Write("PersonaEditorLib", "Error: Parse MSG1 error!");
    //            Logging.Write("PersonaEditorLib", e);
    //            name.Clear();
    //            msg.Clear();
    //        }
    //    }

    //    private void ParseString(IList<MSGs.MSGstr> StringsList, byte[] SourceBytes, CharList Old, CharList New)
    //    {
    //        StringsList.Clear();

    //        int Index = 0;
    //        foreach (var Bytes in Utilities.SplitSourceBytes(SourceBytes))
    //        {
    //            BMDbase.MyStringElement[] temp = Bytes.parseString();
    //            List<BMDbase.MyStringElement> Prefix = new List<BMDbase.MyStringElement>();
    //            List<BMDbase.MyStringElement> Postfix = new List<BMDbase.MyStringElement>();
    //            List<BMDbase.MyStringElement> Strings = new List<BMDbase.MyStringElement>();

    //            int tempdown = 0;
    //            int temptop = temp.Length;

    //            for (int i = 0; i < temp.Length; i++)
    //            {
    //                if (temp[i].Type == BMDbase.MyStringElement.arrayType.System)
    //                    Prefix.Add(temp[i]);
    //                else
    //                {
    //                    tempdown = i;
    //                    i = temp.Length;
    //                }
    //            }

    //            for (int i = temp.Length - 1; i >= tempdown; i--)
    //            {
    //                if (temp[i].Type == BMDbase.MyStringElement.arrayType.System)
    //                    Postfix.Add(temp[i]);
    //                else
    //                {
    //                    temptop = i;
    //                    i = 0;
    //                }
    //            }

    //            Postfix.Reverse();

    //            for (int i = tempdown; i <= temptop; i++)
    //                Strings.Add(temp[i]);

    //            BMDbase.MSGs.MSGstr NewString = new BMDbase.MSGs.MSGstr(Old, New);
    //            NewString.Index = Index;
    //            NewString.Prefix.ElementArray = Prefix.ToArray();
    //            NewString.OldString.ElementArray = Strings.ToArray();
    //            NewString.Postfix.ElementArray = Postfix.ToArray();

    //            StringsList.Add(NewString);
    //            Index++;
    //        }
    //    }

    //    //public void SaveAsText(string FileName, string Index, int Option)
    //    //{
    //    //    if (Option == 1)
    //    //    {
    //    //        SaveAsTextOp1(FileName, Index);
    //    //    }
    //    //    else if (Option == 2)
    //    //    {
    //    //        SaveAsTextOp2(FileName, Index);
    //    //    }
    //    //    else
    //    //    {
    //    //        Logging.Write("SaveAsText Option invalid");
    //    //    }
    //    //}

    //    //void SaveAsTextOp1(string FileName, string Index)
    //    //{
    //    //    Directory.CreateDirectory("Export Text");

    //    //    string FileNameWE = Path.GetFileName(FileName);
    //    //    FileStream FS = new FileStream(@"Export Text\\NAMES.TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
    //    //    FS.Position = FS.Length;
    //    //    using (StreamWriter SW = new StreamWriter(FS))
    //    //        foreach (var NAME in name)
    //    //        {
    //    //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
    //    //        }


    //    //    string DirectoryName = new DirectoryInfo(Path.GetDirectoryName(FileName)).Name;
    //    //    FS = new FileStream("Export Text\\" + DirectoryName.ToUpper() + ".TXT", FileMode.OpenOrCreate, FileAccess.ReadWrite);
    //    //    FS.Position = FS.Length;
    //    //    using (StreamWriter SW = new StreamWriter(FS))
    //    //    {
    //    //        List<name> Name = name.ToList();
    //    //        foreach (var MSG in msg)
    //    //        {
    //    //            foreach (var STR in MSG.Strings)
    //    //            {
    //    //                SW.Write(FileNameWE + "\t");
    //    //                SW.Write(Index + "\t");
    //    //                SW.Write(MSG.Name + "\t");
    //    //                SW.Write(STR.Index + "\t");
    //    //                if (Name.Exists(x => x.Index == MSG.Character_Index))
    //    //                {
    //    //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
    //    //                    SW.Write(Name_i.OldName);
    //    //                }
    //    //                else if (MSG.Type == "SEL")
    //    //                {
    //    //                    SW.Write("<SELECT>");
    //    //                }
    //    //                else { SW.Write("<NO_NAME>"); }

    //    //                SW.Write("\t");
    //    //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
    //    //                SW.Write(split[0]);
    //    //                for (int i = 1; i < split.Length; i++)
    //    //                {
    //    //                    SW.Write(" " + split[i]);
    //    //                }

    //    //                SW.WriteLine();
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //void SaveAsTextOp2(string FileName, string Index)
    //    //{

    //    //    string newFileName = Index == "" ? Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".TXT"
    //    //        : Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "-" + Index + ".TXT";

    //    //    using (StreamWriter SW = new StreamWriter(new FileStream(newFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)))
    //    //    {
    //    //        foreach (var NAME in name)
    //    //        {
    //    //            SW.WriteLine("Name № " + NAME.Index + ":\t" + NAME.OldName);
    //    //        }
    //    //        SW.WriteLine();

    //    //        List<name> Name = name.ToList();
    //    //        foreach (var MSG in msg)
    //    //        {
    //    //            foreach (var STR in MSG.Strings)
    //    //            {
    //    //                SW.Write(MSG.Name + "\t");
    //    //                SW.Write(STR.Index + "\t");
    //    //                if (Name.Exists(x => x.Index == MSG.Character_Index))
    //    //                {
    //    //                    name Name_i = Name.Find(x => x.Index == MSG.Character_Index);
    //    //                    SW.Write(Name_i.OldName);
    //    //                }
    //    //                else if (MSG.Type == "SEL")
    //    //                {
    //    //                    SW.Write("<SELECT>");
    //    //                }
    //    //                else { SW.Write("<NO_NAME>"); }

    //    //                SW.Write("\t");
    //    //                var split = Regex.Split(STR.Old_string, "\r\n|\r|\n");
    //    //                SW.Write(split[0]);
    //    //                for (int i = 1; i < split.Length; i++)
    //    //                {
    //    //                    SW.Write(" " + split[i]);
    //    //                }

    //    //                SW.WriteLine();
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //}



    public class PTP
    {
        public PTP(CharList OldCharList, CharList NewCharList)
        {
            this.OldCharList = OldCharList;
            this.NewCharList = NewCharList;
        }

        public CharList OldCharList { get; set; }
        public CharList NewCharList { get; set; }

        public string SourceFileName = "";
        public long SelectPosition = -1;

        public class Names
        {
            public event ByteArrayChangedEventHandler OldNameChanged;
            public event StringChangedEventHandler NewNameChanged;

            public Names(int Index, string OldName, string NewName)
            {
                this.Index = Index;
                this.NewName = NewName;
                this.OldName = new ByteArray(OldName);
            }

            public Names(int Index, byte[] OldName, string NewName)
            {
                this.Index = Index;
                this.NewName = NewName;
                this.OldName = new ByteArray(OldName);
            }

            private ByteArray _OldName;
            private string _NewName;

            public int Index { get; set; }
            public ByteArray OldName
            {
                get { return _OldName; }
                set
                {
                    _OldName = value;
                    OldNameChanged?.Invoke(_OldName);
                }
            }
            public string NewName
            {
                get { return _NewName; }
                set
                {
                    _NewName = value;
                    NewNameChanged?.Invoke(_NewName);
                }
            }
        }

        public class MSG
        {
            public class MSGstr : INotifyPropertyChanged
            {
                public event StringChangedEventHandler NewStringChanged;

                public struct MSGstrElement
                {
                    public MSGstrElement(string type, string array)
                    {
                        Type = type;
                        Array = new ByteArray(array);
                    }

                    public MSGstrElement(string type, byte[] array)
                    {
                        Type = type;
                        Array = new ByteArray(array);
                    }

                    public string GetText(CharList CharList)
                    {
                        if (Type == "System")
                            if (Array[0] == 0x0A)
                                return "\n";
                            else
                                return GetSystem();
                        else
                        {
                            string returned = "";
                            for (int i = 0; i < Array.Length; i++)
                            {
                                if (0x20 <= Array[i] & Array[i] < 0x80)
                                {
                                    returned += CharList.GetChar(Array[i]);
                                }
                                else if (0x80 <= Array[i] & Array[i] < 0xF0)
                                {
                                    int newindex = (Array[i] - 0x81) * 0x80 + Array[i + 1] + 0x20;

                                    i++;
                                    returned += CharList.GetChar(newindex);
                                }
                                else
                                {
                                    throw new Exception("GetText error");
                                }
                            }
                            return returned;
                        }
                    }

                    public string GetSystem()
                    {
                        string returned = "";

                        if (Array.Length > 0)
                        {
                            returned += "{" + Convert.ToString(Array[0], 16).PadLeft(2, '0').ToUpper();
                            for (int i = 1; i < Array.Length; i++)
                                returned += " " + Convert.ToString(Array[i], 16).PadLeft(2, '0').ToUpper();

                            returned += "}";
                        }

                        return returned;
                    }

                    public string Type { get; set; }
                    public ByteArray Array { get; set; }
                }

                public MSGstr(int index, string newstring)
                {
                    Index = index;
                    NewString = newstring;
                    OldString.ListChanged += OldString_ListChanged;
                }

                private void OldString_ListChanged(object sender, ListChangedEventArgs e)
                {
                    Notify("OldString");
                }

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

                private string _NewString = "";

                public int Index { get; set; }
                public int CharacterIndex { get; set; }
                public BindingList<MSGstrElement> Prefix { get; set; } = new BindingList<MSGstrElement>();
                public BindingList<MSGstrElement> OldString { get; set; } = new BindingList<MSGstrElement>();
                public BindingList<MSGstrElement> Postfix { get; set; } = new BindingList<MSGstrElement>();
                public string NewString
                {
                    get { return _NewString; }
                    set
                    {
                        if (_NewString != value)
                        {
                            _NewString = value;
                            NewStringChanged?.Invoke(_NewString);
                            Notify("NewString");
                        }
                    }
                }
            }

            public MSG(int index, string type, string name, int charindex, string array)
            {
                Index = index;
                Type = type;
                Name = name;
                CharacterIndex = charindex;
                MsgBytes = new ByteArray(array);
            }

            public MSG(int index, string type, string name, int charindex, ByteArray array)
            {
                Index = index;
                Type = type;
                Name = name;
                CharacterIndex = charindex;
                MsgBytes = array;
            }

            public int Index { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public int CharacterIndex { get; set; }
            public ByteArray MsgBytes { get; set; }

            public BindingList<MSGstr> Strings { get; set; } = new BindingList<MSGstr>();
        }

        public BindingList<Names> names { get; set; } = new BindingList<Names>();
        public BindingList<MSG> msg { get; set; } = new BindingList<MSG>();

        public bool Open(XDocument xDoc)
        {
            names.Clear();
            msg.Clear();
            try
            {
                XElement MSG1Doc = xDoc.Element("MSG1");
                XAttribute Position = MSG1Doc.Attribute("Position");
                SelectPosition = Position != null ? Convert.ToInt32(Position.Value) : -1;

                Position = MSG1Doc.Attribute("SourceFileName");
                SourceFileName = Position != null ? Position.Value : "";

                foreach (var NAME in MSG1Doc.Element("CharacterNames").Elements())
                {
                    int Index = Convert.ToInt32(NAME.Attribute("Index").Value);
                    string OldNameSource = NAME.Element("OldNameSource").Value;
                    string NewName = NAME.Element("NewName").Value;

                    names.Add(new Names(Index, OldNameSource, NewName));
                }

                foreach (var Message in MSG1Doc.Element("MSG").Elements())
                {
                    int Index = Convert.ToInt32(Message.Attribute("Index").Value);
                    string Type = Message.Element("Type").Value;
                    string Name = Message.Element("Name").Value;
                    int CharacterNameIndex = Convert.ToInt32(Message.Element("CharacterNameIndex").Value);

                    string SourceBytes_str = Message.Element("SourceBytes").Value;

                    MSG temp = new MSG(Index, Type, Name, CharacterNameIndex, SourceBytes_str);
                    msg.Add(temp);

                    foreach (var Strings in Message.Element("MessageStrings").Elements())
                    {
                        int StringIndex = Convert.ToInt32(Strings.Attribute("Index").Value);
                        string NewString = Strings.Element("NewString").Value;

                        MSG.MSGstr temp2 = new MSG.MSGstr(StringIndex, NewString) { CharacterIndex = CharacterNameIndex };
                        temp.Strings.Add(temp2);

                        foreach (var Prefix in Strings.Elements("PrefixBytes"))
                        {
                            int PrefixIndex = Convert.ToInt32(Prefix.Attribute("Index").Value);
                            string PrefixType = Prefix.Attribute("Type").Value;
                            string PrefixBytes = Prefix.Value;

                            temp2.Prefix.Add(new MSG.MSGstr.MSGstrElement(PrefixType, PrefixBytes));
                        }

                        foreach (var Old in Strings.Elements("OldStringBytes"))
                        {
                            int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
                            string OldType = Old.Attribute("Type").Value;
                            string OldBytes = Old.Value;

                            temp2.OldString.Add(new MSG.MSGstr.MSGstrElement(OldType, OldBytes));
                        }

                        foreach (var Postfix in Strings.Elements("PostfixBytes"))
                        {
                            int PostfixIndex = Convert.ToInt32(Postfix.Attribute("Index").Value);
                            string PostfixType = Postfix.Attribute("Type").Value;
                            string PostfixBytes = Postfix.Value;

                            temp2.Postfix.Add(new MSG.MSGstr.MSGstrElement(PostfixType, PostfixBytes));
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logging.Write("PTPfactory", e.ToString());
                return false;
            }
        }

        public bool Open(BMD BMD)
        {
            names.Clear();
            msg.Clear();
            try
            {
                SourceFileName = BMD.OpenFileName;

                foreach (var NAME in BMD.name)
                {
                    int Index = NAME.Index;
                    byte[] OldNameSource = NAME.NameBytes;
                    string NewName = "";

                    names.Add(new Names(Index, OldNameSource, NewName));
                }

                foreach (var Message in BMD.msg)
                {
                    int Index = Message.Index;
                    string Type = Message.Type.ToString();
                    string Name = Message.Name;
                    int CharacterNameIndex = Message.CharacterIndex;
                    ByteArray SourceBytes_str = Message.MsgBytes;

                    MSG temp = new MSG(Index, Type, Name, CharacterNameIndex, SourceBytes_str) { CharacterIndex = CharacterNameIndex };
                    temp.Strings.ParseStrings(SourceBytes_str);
                    msg.Add(temp);
                }

                return true;
            }
            catch (Exception e)
            {
                Logging.Write("PTPfactory", e.ToString());
                return false;
            }
        }

        private class LineMap
        {
            public LineMap(string map)
            {
                string[] temp = Regex.Split(map, " ");
                FileName = Array.IndexOf(temp, "%FN");
                MSGname = Array.IndexOf(temp, "%MSG");
                StringIndex = Array.IndexOf(temp, "%MSGIND");
                NewText = Array.IndexOf(temp, "%NEWSTR");
                OldName = Array.IndexOf(temp, "%OLDNM");
                NewName = Array.IndexOf(temp, "%NEWNM");
            }

            public int FileName { get; set; }
            public int MSGname { get; set; }
            public int StringIndex { get; set; }
            public int NewText { get; set; }

            public int OldName { get; set; }
            public int NewName { get; set; }
        }

        public bool ImportTXT(string txtfile, string map, bool auto, int width)
        {
            int Width = (int)Math.Round((double)width / 0.9375);
            LineMap MAP = new LineMap(map);

            if (MAP.FileName >= 0 & MAP.MSGname >= 0 & MAP.StringIndex >= 0 & MAP.NewText >= 0)
            {
                StreamReader SR = new StreamReader(new FileStream(txtfile, FileMode.Open, FileAccess.Read));
                while (SR.EndOfStream == false)
                {
                    string line = SR.ReadLine();
                    string[] linespl = Regex.Split(line, "\t");

                    if (Path.GetFileNameWithoutExtension(SourceFileName) == Path.GetFileNameWithoutExtension(linespl[MAP.FileName]))
                    {
                        string NewText = linespl[MAP.NewText];
                        if (auto)
                            NewText = NewText.SplitByWidth(NewCharList, Width);
                        else
                            NewText = NewText.Replace("\\n", "\n");

                        if (MAP.NewName >= 0)
                            ImportText(linespl[MAP.MSGname], Convert.ToInt32(linespl[MAP.StringIndex]), NewText, linespl[MAP.NewName]);
                        else
                            ImportText(linespl[MAP.MSGname], Convert.ToInt32(linespl[MAP.StringIndex]), NewText);

                    }
                }
            }
            else if (MAP.OldName >= 0 & MAP.NewName >= 0)
            {

            }

            return false;
        }

        public bool ImportText(string MSGName, int StringIndex, string Text)
        {
            var temp = msg.FirstOrDefault(x => x.Name == MSGName);
            if (temp != null)
            {
                var temp2 = temp.Strings.FirstOrDefault(x => x.Index == StringIndex);
                if (temp2 != null)
                {
                    temp2.NewString = Text;
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool ImportText(string MSGName, int StringIndex, string Text, string NewName)
        {
            if (ImportText(MSGName, StringIndex, Text))
            {
                var temp = msg.FirstOrDefault(x => x.Name == MSGName);
                if (temp != null)
                {
                    var temp2 = names.FirstOrDefault(x => x.Index == temp.CharacterIndex);
                    if (temp2 != null)
                    {
                        temp2.NewName = NewName;
                    }
                }
                return true;
            }
            else return false;
        }

        public XDocument SaveProject()
        {
            try
            {
                XDocument xDoc = new XDocument();
                XElement Document = new XElement("MSG1");
                Document.Add(new XAttribute("SourceFileName", SourceFileName));
                Document.Add(new XAttribute("Position", SelectPosition));
                xDoc.Add(Document);
                XElement CharName = new XElement("CharacterNames");
                Document.Add(CharName);

                foreach (var NAME in names)
                {
                    XElement Name = new XElement("Name");
                    Name.Add(new XAttribute("Index", NAME.Index));
                    Name.Add(new XElement("OldNameSource", NAME.OldName));
                    Name.Add(new XElement("NewName", NAME.NewName));
                    CharName.Add(Name);
                }

                XElement MES = new XElement("MSG");
                Document.Add(MES);

                foreach (var MSG in msg)
                {
                    XElement Msg = new XElement("Message");
                    Msg.Add(new XAttribute("Index", MSG.Index));
                    Msg.Add(new XElement("Type", MSG.Type));
                    Msg.Add(new XElement("Name", MSG.Name));
                    Msg.Add(new XElement("CharacterNameIndex", MSG.CharacterIndex));
                    Msg.Add(new XElement("SourceBytes", MSG.MsgBytes));

                    XElement Strings = new XElement("MessageStrings");
                    Msg.Add(Strings);
                    foreach (var STR in MSG.Strings)
                    {
                        XElement String = new XElement("String");
                        String.Add(new XAttribute("Index", STR.Index));
                        Strings.Add(String);

                        for (int i = 0; i < STR.Prefix.Count; i++)
                        {
                            XElement PrefixBytes = new XElement("PrefixBytes", STR.Prefix[i].Array);
                            PrefixBytes.Add(new XAttribute("Index", i));
                            PrefixBytes.Add(new XAttribute("Type", STR.Prefix[i].Type));
                            String.Add(PrefixBytes);
                        }

                        for (int i = 0; i < STR.OldString.Count; i++)
                        {
                            XElement OldStringBytes = new XElement("OldStringBytes", STR.OldString[i].Array);
                            OldStringBytes.Add(new XAttribute("Index", i));
                            OldStringBytes.Add(new XAttribute("Type", STR.OldString[i].Type));
                            String.Add(OldStringBytes);
                        }

                        String.Add(new XElement("NewString", STR.NewString));

                        for (int i = 0; i < STR.Postfix.Count; i++)
                        {
                            XElement PostfixBytes = new XElement("PostfixBytes", STR.Postfix[i].Array);
                            PostfixBytes.Add(new XAttribute("Index", i));
                            PostfixBytes.Add(new XAttribute("Type", STR.Postfix[i].Type));
                            String.Add(PostfixBytes);
                        }
                    }

                    MES.Add(Msg);
                }

                return xDoc;
            }
            catch (Exception e)
            {
                Logging.Write("PTPfactory", e.ToString());
                return null;
            }
        }
    }
}