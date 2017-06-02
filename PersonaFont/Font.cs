using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PersonaFont
{
    public static class WidthTable
    {
        public static void WriteToFile(MemoryStream MemoryStream)
        {
            try
            {
                FileStream CutTable = new FileStream(@"FONT0 CUT.TXT", FileMode.Create, FileAccess.ReadWrite);
                StreamWriter SW = new StreamWriter(CutTable);
                SW.AutoFlush = true;

                int k = 0;

                while (MemoryStream.Position < MemoryStream.Length)
                {
                    k++;
                    SW.Write('(');
                    SW.Write(MemoryStream.ReadByte().ToString("00"));
                    SW.Write(',');
                    SW.Write(MemoryStream.ReadByte().ToString("00"));
                    SW.Write(')');
                    if (k >= 16)
                    {
                        k = 0;
                        SW.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return;
            }
        }

        public static MemoryStream WriteToFont()
        {
            MemoryStream MS = new MemoryStream();

            using (FileStream Cut_Table = new FileStream(@"FONT0 CUT.TXT", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(Cut_Table))
                {
                    while (sr.EndOfStream == false)
                    {
                        string str = sr.ReadLine();
                        while (str.Length != 0)
                        {
                            str = str.Remove(0, 1);
                            MS.WriteByte(Convert.ToByte(str.Remove(2)));
                            str = str.Remove(0, 3);
                            MS.WriteByte(Convert.ToByte(str.Remove(2)));
                            str = str.Remove(0, 3);
                        }
                    }
                }
            }
            return MS;
        }
    }

    class Font
    {


        private int _NumberOfColor = 0;
        private int NumberOfColor
        {
            get { return _NumberOfColor; }
        }

        private ushort GlyphSize_1 = 0;
        private ushort GlyphSize_2 = 0;
        private ushort GlyphSizeInByte = 0;

        private byte BitPerPixel = 0;

        public int UnknownPos = 0;
        public int UnknownSize = 0;
        public int ReservedPos = 0;
        public int ReservedSize = 0;

        public int MainHeaderSize = 0;
        public int TotalNumberOfGlyphs = 0;
        public int GlyphCutTable_Pos = 0;
        public int GlyphCutTable_Size = 0;
        public int DictionaryHeader_Pos = 0;
        public int DictionaryHeader_Size = 0;
        public int Dictionary_Pos = 0;
        public int Dictionary_Size = 0;
        public int GlyphPositionTable_Size = 0;
        public int CompressedFontBlock_Size = 0;
        public int CompressedFontBlock_Pos = 0;
        public int[,] Dictionary;
        public MemoryStream FontDec = new MemoryStream();

        public void WriteGlyphPosition(FileStream Font)
        {
            Font.Position = CompressedFontBlock_Pos;
            double ko = (double)BitPerPixel / 8;
            int size = Convert.ToInt32(GlyphSize_1 * GlyphSize_1 * ko);

            List<int> GlyphNewPosition = new List<int>();

            int temp = 0;
            bool boolean = true;


            int a = 0;
            int b = 0;
            do
            {
                if (Font.Position == Font.Length)
                {
                    boolean = false;
                }
                else
                {
                    int s4 = Font.ReadUshort();
                    a++;
                    for (int i = 0; i < 16; i++)
                    {
                        temp = Dictionary[temp, s4 % 2];
                        s4 = s4 >> 1;

                        if (Dictionary[temp, 0] == 0)
                        {
                            if (b % size == 0)
                            {
                                GlyphNewPosition.Add((((a - 1) * 2) << 3) + i);
                            }
                            b++;
                            temp = 0;
                        }
                    }
                }
            } while (boolean);

            Font.Position = Dictionary_Pos + Dictionary_Size;
            for (int i = 0; i < GlyphPositionTable_Size / 4; i++)
            {
                Font.WriteInt(GlyphNewPosition[i]);
            }
        }

        public Font(FileStream FONT)
        {
            MainHeaderSize = FONT.ReadInt();
            FONT.Position = 0xE;
            TotalNumberOfGlyphs = FONT.ReadUshort();
            GlyphSize_1 = FONT.ReadUshort();
            GlyphSize_2 = FONT.ReadUshort();
            GlyphSizeInByte = FONT.ReadUshort();
            BitPerPixel = Convert.ToByte((double)(GlyphSizeInByte * 8) / (GlyphSize_1 * GlyphSize_2));

            _NumberOfColor = Convert.ToInt32(Math.Pow(2, BitPerPixel));

            GlyphCutTable_Pos = MainHeaderSize + NumberOfColor * 4 + 4;
            FONT.Position = GlyphCutTable_Pos - 4;
            GlyphCutTable_Size = FONT.ReadInt();

            UnknownPos = GlyphCutTable_Pos + GlyphCutTable_Size + 4;
            FONT.Position = UnknownPos - 4;
            UnknownSize = FONT.ReadInt();

            ReservedPos = UnknownPos + UnknownSize;
            ReservedSize = TotalNumberOfGlyphs * 4;

            DictionaryHeader_Pos = ReservedPos + ReservedSize;

            FONT.Position = DictionaryHeader_Pos;
            DictionaryHeader_Size = FONT.ReadInt();
            Dictionary_Size = FONT.ReadInt();
            CompressedFontBlock_Size = FONT.ReadInt();
            Dictionary_Pos = DictionaryHeader_Pos + DictionaryHeader_Size;

            FONT.Position = DictionaryHeader_Pos + 24;
            GlyphPositionTable_Size = FONT.ReadInt();

            FONT.Position = Dictionary_Pos;
            Dictionary = ReadDict(ref FONT);

            CompressedFontBlock_Pos = Dictionary_Pos + Dictionary_Size + GlyphPositionTable_Size;
        }

        public int[,] ReadDict(ref FileStream NewStream)
        {
            int[,] Dict = new int[Dictionary_Size / 6, 2];
            for (int i = 0; i < Dictionary_Size / 6; i++)
            {
                NewStream.Position = NewStream.Position + 2;
                Dict[i, 0] = NewStream.ReadUshort();
                Dict[i, 1] = NewStream.ReadUshort();
            }
            return Dict;
        }

        public int FindDictPart(int[,] Dict)
        {
            for (int i = 1; i < Dict.Length / 2; i++)
            {
                if (Dict[i, 1] == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindDictIndex(int v0, int DictPart, ref MemoryStream FONT_COMPRESS)
        {
            if (Dictionary[0, 0] == v0)
            {
                FONT_COMPRESS.WriteByte(0);
                return 0;
            }
            else if (Dictionary[0, 1] == v0)
            {
                FONT_COMPRESS.WriteByte(1);
                return 0;
            }

            for (int i = DictPart + 1; i < Dictionary.Length / 2; i++)
            {
                if (Dictionary[i, 0] == v0)
                {
                    FONT_COMPRESS.WriteByte(0);
                    return i;
                }
                else if (Dictionary[i, 1] == v0)
                {
                    FONT_COMPRESS.WriteByte(1);
                    return i;
                }
            }
            return -1;
        }

        private void BMPCopyPalette(ref Bitmap bmp, ref FileStream FONT)
        {
            ColorPalette ColorPaletteBMP = bmp.Palette;
            Color[] ColorBMP = ColorPaletteBMP.Entries;
            for (int i = 0; i < ColorBMP.Length; i++)
            {
                int r = FONT.ReadByte();
                int g = FONT.ReadByte();
                int b = FONT.ReadByte();
                int a = FONT.ReadByte();
                ColorBMP[i] = Color.FromArgb(a, r, g, b);
            }
            bmp.Palette = ColorPaletteBMP;
        }

        private byte[] Memory2BMP(ref MemoryStream FontDec, int x, int y)
        {
            double ko = (double)BitPerPixel / 8;
            int size = Convert.ToInt32(GlyphSize_1 * GlyphSize_1 * ko);
            int stride = Convert.ToInt32(GlyphSize_1 * ko);

            List<byte> BMP = new List<byte>();
            for (int i = 0; i < size * x * y; i = i + size * x)
            {
                for (int u = 0; u < size; u = u + stride)
                {
                    for (int k = 0; k < size * x; k = k + size)
                    {
                        FontDec.Position = i + k + u;
                        for (int t = 0; t < stride; t++)
                        {
                            int a = i + k + u >= FontDec.Length ? 0xFF : (byte)FontDec.ReadByte();

                            if (BitPerPixel == 4)
                            {
                                a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                            }
                            BMP.Add((byte)a);
                        }
                    }
                }
            }

            return BMP.ToArray();
        }

        private MemoryStream BMP2Memory(byte[] data, int x, int y)
        {
            if (NumberOfColor == 16)
            {
                MemoryStream FontDec = new MemoryStream();
                int BMPpos = -1;
                for (int i = 0; i < 512 * x * y; i = i + 512 * x)
                {
                    for (int u = 0; u < 512; u = u + 16)
                    {
                        for (int k = 0; k < 512 * x; k = k + 512)
                        {
                            FontDec.Position = i + k + u;
                            for (int t = 0; t < 16; t++)
                            {
                                BMPpos++;
                                int a = data[data.Length - 1 - BMPpos];
                                a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                                FontDec.WriteByte((byte)a);
                            }
                        }
                    }
                }

                MemoryStream FontDec2 = new MemoryStream();
                FontDec.Position = (x * y - TotalNumberOfGlyphs) * 512;
                for (int i = 0; i < 512 * TotalNumberOfGlyphs; i++) { FontDec2.WriteByte((byte)FontDec.ReadByte()); }
                return FontDec2;
            }
            else
            {
                MemoryStream FontDec = new MemoryStream();
                int BMPpos = -1;
                for (int i = 0; i < 1024 * x * y; i = i + 1024 * x)
                {
                    for (int u = 0; u < 1024; u = u + 32)
                    {
                        for (int k = 0; k < 1024 * x; k = k + 1024)
                        {
                            FontDec.Position = i + k + u;
                            for (int t = 0; t < 32; t++)
                            {
                                BMPpos++;
                                int a = data[data.Length - 1 - BMPpos];
                                //a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                                FontDec.WriteByte((byte)a);
                            }
                        }
                    }
                }

                MemoryStream FontDec2 = new MemoryStream();
                FontDec.Position = (x * y - TotalNumberOfGlyphs) * 1024;
                for (int i = 0; i < 1024 * TotalNumberOfGlyphs; i++) { FontDec2.WriteByte((byte)FontDec.ReadByte()); }
                return FontDec2;
            }

        }

        public void Save2BMP(ref FileStream FONT)
        {
            int x = 16;
            int y = (int)Math.Ceiling((decimal)TotalNumberOfGlyphs / 16);
            Bitmap FileBMP = new Bitmap(1, 1);
            if (NumberOfColor == 16)
            {
                FileBMP = new Bitmap(x * 32, y * 32, PixelFormat.Format4bppIndexed);
            }
            else if (NumberOfColor == 256)
            {
                FileBMP = new Bitmap(x * 32, y * 32, PixelFormat.Format8bppIndexed);
            }

            BMPCopyPalette(ref FileBMP, ref FONT);

            byte[] data = Memory2BMP(ref FontDec, x, y);

            BitmapData bmpData = FileBMP.LockBits(new Rectangle(0, 0, FileBMP.Width, FileBMP.Height), ImageLockMode.WriteOnly, FileBMP.PixelFormat);
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            FileBMP.UnlockBits(bmpData);

            FileBMP.Save(@"FONT0.BMP", ImageFormat.Bmp);
        }

        public MemoryStream FontDecRev()
        {
            Bitmap FileBMP = (Bitmap)Image.FromFile(@"FONT0.bmp", true);
            BitmapData bmpData = FileBMP.LockBits(new Rectangle(0, 0, FileBMP.Width, FileBMP.Height), ImageLockMode.ReadOnly, FileBMP.PixelFormat);
            byte[] data = new byte[bmpData.Stride * bmpData.Height];
            Marshal.Copy(bmpData.Scan0, data, 0, data.Length);
            FileBMP.UnlockBits(bmpData);

            int x = 16;
            int y = (int)Math.Ceiling((decimal)TotalNumberOfGlyphs / 16);

            return BMP2Memory(data, x, y);
        }
    }
}
