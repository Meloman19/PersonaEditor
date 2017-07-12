using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace PersonaFont
{
    public static class WidthTable
    {
        public static void WriteToFile(MemoryStream MemoryStream)
        {
            try
            {
                List<List<byte>> WidthTable = new List<List<byte>>();
                List<byte> WidthTableLine = new List<byte>();

                int k = 0;
                while (MemoryStream.Position < MemoryStream.Length)
                {
                    k++;
                    WidthTableLine.Add(Convert.ToByte(MemoryStream.ReadByte()));
                    WidthTableLine.Add(Convert.ToByte(MemoryStream.ReadByte()));

                    if (k >= 16)
                    {
                        k = 0;
                        WidthTable.Add(WidthTableLine);
                        WidthTableLine = new List<byte>();
                    }
                }
                if (k != 0)
                {
                    WidthTable.Add(WidthTableLine);
                }

                k = 1;
                int index = 1;

                XDocument xDoc = new XDocument();
                XElement WT = new XElement("WidthTable");
                xDoc.Add(WT);
                foreach (var line in WidthTable)
                {
                    XElement Line = new XElement("Line_" + k);
                    WT.Add(Line);
                    k++;
                    for (int i = 0; i < line.Count; i++)
                    {
                        XElement Glyph = new XElement("Glyph_" + index);
                        Line.Add(Glyph);
                        index++;
                        Glyph.Add(new XElement("LeftCut", line[i]));
                        Glyph.Add(new XElement("RightCut", line[i + 1]));
                        i++;
                    }
                    index = 1;
                }
                xDoc.Save("FONT0 WIDTH TABLE.XML");
                Static.LOG.W("Width Table was created.");
            }
            catch (Exception e)
            {
                Static.LOG.W("ERROR: " + e.ToString());
                return;
            }
        }

        public static MemoryStream WriteToFont()
        {
            int index = 0;
            MemoryStream MS = new MemoryStream();
            try
            {
                XDocument xDoc = XDocument.Load("FONT0 WIDTH TABLE.XML");
                XElement WT = xDoc.Element("WidthTable");
                foreach(var line in WT.Elements())
                {
                    foreach(var glyph in line.Elements())
                    {
                        MS.WriteByte(Convert.ToByte(glyph.Element("LeftCut").Value));
                        MS.WriteByte(Convert.ToByte(glyph.Element("RightCut").Value));
                        index++;
                    }
                }
            }
            catch (Exception e)
            {
                Static.LOG.W("ERROR: " + e.ToString());
                return MS;
            }

            Static.LOG.W("Width Table was writed. Get " + index + " glyphs");
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

            Static.LOG.W("Writed new Glyph Position Table");
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

            Static.LOG.W("Get data from FONT0.FNT");
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
            MemoryStream FontDec2;
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

                FontDec2 = new MemoryStream();
                FontDec.Position = (x * y - TotalNumberOfGlyphs) * 512;
                for (int i = 0; i < 512 * TotalNumberOfGlyphs; i++) { FontDec2.WriteByte((byte)FontDec.ReadByte()); }


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

                FontDec2 = new MemoryStream();
                FontDec.Position = (x * y - TotalNumberOfGlyphs) * 1024;
                for (int i = 0; i < 1024 * TotalNumberOfGlyphs; i++) { FontDec2.WriteByte((byte)FontDec.ReadByte()); }
            }

            Static.LOG.W("Get data from BMP");
            return FontDec2;
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
            Static.LOG.W("Get BMP data");
            BitmapData bmpData = FileBMP.LockBits(new Rectangle(0, 0, FileBMP.Width, FileBMP.Height), ImageLockMode.WriteOnly, FileBMP.PixelFormat);
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            FileBMP.UnlockBits(bmpData);

            FileBMP.Save(@"FONT0.BMP", ImageFormat.Bmp);
            Static.LOG.W("Created BMP");
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
