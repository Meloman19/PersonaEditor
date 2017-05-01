using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PersonaFont
{
    class Program
    {
        private static bool check_command(ref string command)
        {
            while ((command != "decom") & (command != "com"))
            {
                Console.Clear();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine("Persona's font decompressor/compressor by Meloman19");
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine("--Decompress [decom], Compress [com], Exit [exit]--");
                Console.WriteLine("---------------------------------------------------");
                Console.Write("Command: ");
                command = Console.ReadLine();
                if (command == "exit") { return false; }
                if ((command == "decom") & (File.Exists(@"FONT0.FNT") == false))
                {
                    Console.WriteLine("Missing 'FONT0.FNT'");
                    Console.ReadKey();
                    return false;
                }
                if (command == "com")
                {
                    if (File.Exists(@"FONT0.FNT") == false)
                    {
                        Console.WriteLine("Missing 'FONT0.FNT'");
                        Console.ReadKey();
                        return false;
                    }
                    if (File.Exists(@"FONT0 CUT.TXT") == false)
                    {
                        Console.WriteLine("Missing 'FONT0 CUT.TXT'");
                        Console.ReadKey();
                        return false;
                    }
                    if (File.Exists(@"FONT0.bmp") == false)
                    {
                        Console.WriteLine("Missing 'FONT0.BMP'");
                        Console.ReadKey();
                        return false;
                    }
                }
                Console.WriteLine("");

            }
            return true;
        }

        private static void decom()
        {
            try
            {
                Addons Add = new Addons();
                FileStream FONT = new FileStream(@"FONT0.FNT", FileMode.Open, FileAccess.Read);
                Add.GetReady(ref FONT);

                FONT.Position = Add.GlyphCutTable_Pos;
                try
                {
                    FileStream CutTable = new FileStream(@"FONT0 CUT.TXT", FileMode.Create, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(CutTable);
                    int k = 0;
                    for (int i = 0; i < Add.TotalNumberOfGlyphs; i++)
                    {
                        k++;
                        sw.Write('(');
                        sw.Write(FONT.ReadByte().ToString("00"));
                        sw.Write(',');
                        sw.Write(FONT.ReadByte().ToString("00"));
                        sw.Write(')');
                        if (k >= 16)
                        {
                            k = 0;
                            sw.WriteLine();
                            sw.Flush();
                        }
                    }
                    sw.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadKey();
                    return;
                }

                FONT.Position = Add.CompressedFontBlock_Pos;
                int temp = 0;
                bool boolean = true;

                do
                {
                    if (FONT.Position == FONT.Length)
                    {
                        boolean = false;
                    }
                    else
                    {
                        int s4 = Add.Read2ByteFile(ref FONT);
                        for (int i = 0; i < 16; i++)
                        {
                            temp = Add.Dictionary[temp, s4 % 2];
                            s4 = s4 >> 1;

                            if (Add.Dictionary[temp, 0] == 0)
                            {
                                Add.FontDec.WriteByte((byte)(Add.Dictionary[temp, 1]));
                                temp = 0;
                            }
                        }
                    }
                } while (boolean);

                FONT.Position = Add.MainHeaderSize;
                Add.Save2BMP(ref FONT);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                return;
            }
        }

        private static void com()
        {
            Addons Add = new Addons();
            FileStream FONT = new FileStream(@"FONT0.FNT", FileMode.Open, FileAccess.Read);
            Add.GetReady(ref FONT);

            try
            {
                MemoryStream FontDecRev = Add.FontDecRev();
                FileStream FONT_COMPRESS_FILE = new FileStream(@"FONT0 NEW.FNT", FileMode.Create);
                MemoryStream FONT_COMPRESS = new MemoryStream();

                int DictPart = Add.FindDictPart(Add.Dictionary);

                bool boolean = true;
                FontDecRev.Position = 0;

                while (boolean)
                {
                    if (FontDecRev.Position == FontDecRev.Length) { boolean = false; }
                    else
                    {
                        int s4 = FontDecRev.ReadByte();
                        int i = 1;

                        while (Add.Dictionary[i, 1] != s4)
                        {
                            i++;
                            if (Add.Dictionary[i - 1, 1] == 0)
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
                        {
                            v0 = Add.FindDictIndex(Add.Dictionary, v0, DictPart, ref FONT_COMPRESS);
                        }
                    }
                }

                FONT.Position = 0;
                while (FONT.Position < Add.CompressedFontBlock_Pos)
                {
                    FONT_COMPRESS_FILE.WriteByte((byte)FONT.ReadByte());
                }

                int GlyphSize = 0;
                do
                {
                    int i = 0;
                    string str = "";
                    while ((i < 8) & (FONT_COMPRESS.Position != 0))
                    {
                        FONT_COMPRESS.Position--;
                        str = Convert.ToString(FONT_COMPRESS.ReadByte()) + str;
                        FONT_COMPRESS.Position--;
                        i++;
                    }
                    str = str.PadLeft(8, '0');
                    FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str, 2));
                    GlyphSize++;
                } while (FONT_COMPRESS.Position != 0);
                if (GlyphSize % 2 == 1)
                {
                    FONT_COMPRESS_FILE.WriteByte(0);
                    GlyphSize++;
                }

                FONT_COMPRESS_FILE.Position = Add.DictionaryHeader_Pos + 8;
                Add.Write2ByteFile(ref FONT_COMPRESS_FILE, GlyphSize);

                FONT_COMPRESS_FILE.Position = Add.CompressedFontBlock_Pos;
                int temp = 0;
                boolean = true;

                using (StreamWriter Writer = new StreamWriter(File.Create(@"GLYPH POSITION NEW"), Encoding.Default))
                {
                    int a = 0;
                    int b = 0;
                    do
                    {
                        if (FONT_COMPRESS_FILE.Position == FONT_COMPRESS_FILE.Length)
                        {
                            boolean = false;
                        }
                        else
                        {
                            int s4 = Add.Read2ByteFile(ref FONT_COMPRESS_FILE);
                            a++;
                            for (int i = 0; i < 16; i++)
                            {
                                temp = Add.Dictionary[temp, s4 % 2];
                                s4 = s4 >> 1;

                                if (Add.Dictionary[temp, 0] == 0)
                                {
                                    if (b % 512 == 0)
                                    {
                                        Writer.WriteLine((((a - 1) * 2) << 3) + i);
                                        Writer.Flush();
                                    }
                                    b++;
                                    temp = 0;
                                }
                            }
                        }
                    } while (boolean);
                }

                using (StreamReader Reader = new StreamReader(File.OpenRead(@"GLYPH POSITION NEW")))
                {
                    FONT_COMPRESS_FILE.Position = Add.Dictionary_Pos + Add.Dictionary_Size;
                    for (int i = 0; i < Add.GlyphPositionTable_Size / 4; i++)
                    {
                        Add.Write4ByteFile(ref FONT_COMPRESS_FILE, Convert.ToInt32(Reader.ReadLine()));
                    }
                }

                FONT_COMPRESS_FILE.Position = FONT_COMPRESS_FILE.Position - 4;
                int temp2 = Add.Read4ByteFile(ref FONT_COMPRESS_FILE);
                FONT_COMPRESS_FILE.Position = Add.DictionaryHeader_Pos + 12;
                Add.Write4ByteFile(ref FONT_COMPRESS_FILE, temp2);

                FONT_COMPRESS_FILE.Position = 0x4;
                Add.Write4ByteFile(ref FONT_COMPRESS_FILE, ((int)FONT_COMPRESS_FILE.Length - (Add.TotalNumberOfGlyphs * 2 + 7)));

                File.Delete(@"GLYPH POSITION NEW");

                FONT_COMPRESS_FILE.Position = Add.GlyphCutTable_Pos;

                using(FileStream Cut_Table = new FileStream(@"FONT0 CUT.TXT", FileMode.Open))
                {
                   using (StreamReader sr = new StreamReader(Cut_Table))
                    {
                        while (sr.EndOfStream == false)
                        {
                            string str = sr.ReadLine();
                            while (str.Length != 0)
                            {
                                str = str.Remove(0, 1);
                                FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str.Remove(2)));
                                str = str.Remove(0, 3);
                                FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str.Remove(2)));
                                str = str.Remove(0, 3);
                            }
                        }
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

        static void Main(string[] args)
        {
            string command = "";

            if (check_command(ref command) == true)
            {
                if (command == "decom") { decom(); }
                else { com(); }

                Console.WriteLine("Success");
                Console.ReadKey();
                return;
            }
            else { return; }
        }

    }

    class Addons
    {
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

        public void GetReady(ref FileStream FONT)
        {
            MainHeaderSize = Read4ByteFile(ref FONT);
            FONT.Position = 0xE;
            TotalNumberOfGlyphs = Read2ByteFile(ref FONT);
            GlyphCutTable_Pos = MainHeaderSize + 64 + 4;
            FONT.Position = GlyphCutTable_Pos - 4;
            GlyphCutTable_Size = Read4ByteFile(ref FONT);


            DictionaryHeader_Pos = GlyphCutTable_Pos + GlyphCutTable_Size + TotalNumberOfGlyphs * 4 + 4;

            FONT.Position = DictionaryHeader_Pos;
            DictionaryHeader_Size = Read4ByteFile(ref FONT);
            Dictionary_Size = Read4ByteFile(ref FONT);
            CompressedFontBlock_Size = Read4ByteFile(ref FONT);
            Dictionary_Pos = DictionaryHeader_Pos + DictionaryHeader_Size;

            FONT.Position = DictionaryHeader_Pos + 24;
            GlyphPositionTable_Size = Read4ByteFile(ref FONT);

            FONT.Position = Dictionary_Pos;
            Dictionary = ReadDict(ref FONT);

            CompressedFontBlock_Pos = Dictionary_Pos + Dictionary_Size + GlyphPositionTable_Size;

        }

        public int Read2ByteFile(ref FileStream NewStream)
        {
            byte[] val = new byte[2];
            val[0] = (byte)NewStream.ReadByte();
            val[1] = (byte)NewStream.ReadByte();
            return BitConverter.ToUInt16(val, 0);
        }

        public void Write2ByteFile(ref FileStream NewStream, int Byte2)
        {
            string str = Convert.ToString(Byte2, 16);
            str = str.PadLeft(4, '0');
            string str2 = Convert.ToString(str[0]) + Convert.ToString(str[1]);
            str = Convert.ToString(str[2]) + Convert.ToString(str[3]);
            NewStream.WriteByte(Convert.ToByte(str, 16));
            NewStream.WriteByte(Convert.ToByte(str2, 16));
        }

        public int Read4ByteFile(ref FileStream NewStream)
        {
            byte[] val = new byte[4];
            val[0] = (byte)NewStream.ReadByte();
            val[1] = (byte)NewStream.ReadByte();
            val[2] = (byte)NewStream.ReadByte();
            val[3] = (byte)NewStream.ReadByte();
            return (int)BitConverter.ToInt32(val, 0);
        }

        public void Write4ByteFile(ref FileStream NewStream, int Byte4)
        {
            string str = Convert.ToString(Byte4, 16);
            str = str.PadLeft(8, '0');
            string str1 = Convert.ToString(str[0]) + Convert.ToString(str[1]);
            string str2 = Convert.ToString(str[2]) + Convert.ToString(str[3]);
            string str3 = Convert.ToString(str[4]) + Convert.ToString(str[5]);
            string str4 = Convert.ToString(str[6]) + Convert.ToString(str[7]);
            NewStream.WriteByte(Convert.ToByte(str4, 16));
            NewStream.WriteByte(Convert.ToByte(str3, 16));
            NewStream.WriteByte(Convert.ToByte(str2, 16));
            NewStream.WriteByte(Convert.ToByte(str1, 16));
        }

        public int[,] ReadDict(ref FileStream NewStream)
        {
            int[,] Dict = new int[Dictionary_Size / 6, 2];
            for (int i = 0; i < Dictionary_Size / 6; i++)
            {
                NewStream.Position = NewStream.Position + 2;
                Dict[i, 0] = Read2ByteFile(ref NewStream);
                Dict[i, 1] = Read2ByteFile(ref NewStream);
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

        public int FindDictIndex(int[,] Dict, int v0, int DictPart, ref MemoryStream FONT_COMPRESS)
        {
            if (Dict[0, 0] == v0)
            {
                FONT_COMPRESS.WriteByte(0);
                return 0;
            }
            else if (Dict[0, 1] == v0)
            {
                FONT_COMPRESS.WriteByte(1);
                return 0;
            }

            for (int i = DictPart + 1; i < Dict.Length / 2; i++)
            {
                if (Dict[i, 0] == v0)
                {
                    FONT_COMPRESS.WriteByte(0);
                    return i;
                }
                else if (Dict[i, 1] == v0)
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
            byte[] BMP = new byte[512 * x * y];
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
                            int a = (byte)FontDec.ReadByte();
                            a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                            BMP[BMPpos] = (byte)a;
                        }
                    }
                }
            }

            return BMP;
        }

        private MemoryStream BMP2Memory(byte[] data, int x, int y)
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

        public void Save2BMP(ref FileStream FONT)
        {
            int x = 16;
            int y = (int)Math.Ceiling((decimal)TotalNumberOfGlyphs / 16);
            Bitmap FileBMP = new Bitmap(x * 32, y * 32, PixelFormat.Format4bppIndexed);
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