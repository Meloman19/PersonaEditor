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
        static void Main(string[] args)
        {
            string command = "";
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
                if (command == "exit") { return; }
                if ((command == "decom") & (File.Exists(@"FONT0.FNT") == false))
                {
                    Console.WriteLine("Missing 'FONT0.FNT'");
                    Console.ReadKey();
                    return;
                }
                if (command == "com")
                {
                    if (File.Exists(@"FONT0.FNT") == false)
                    {
                        Console.WriteLine("Missing 'FONT0.FNT'");
                        Console.ReadKey();
                        return;
                    }
                    if (File.Exists(@"FONT0.bmp") == false)
                    {
                        Console.WriteLine("Missing 'FONT0.BMP' file");
                        Console.ReadKey();
                        return;
                    }

                }
            }
            Console.WriteLine("");

            Addons Add = new Addons();
            FileStream FONT = new FileStream(@"FONT0.FNT", FileMode.Open, FileAccess.ReadWrite);
            Add.GetReady(ref FONT);

            if (command == "decom")
            {
                try
                {
                    FONT.Position = Add.CompressedFontBlock_Pos;
                    int temp = 0;
                    bool boolean = true;

                    //StreamWriter Writer = new StreamWriter(File.Create(@"GLYPH POSITION READ"), Encoding.Default);

                    //int a = 0;
                    //int b = 0;
                    do
                    {
                        if (FONT.Position == FONT.Length)
                        {
                            boolean = false;
                        }
                        else
                        {
                            int s4 = Add.Read2ByteFile(ref FONT);
                            //a++;
                            for (int i = 0; i < 16; i++)
                            {
                                temp = Add.Dictionary[temp, s4 % 2];
                                s4 = s4 >> 1;

                                if (Add.Dictionary[temp, 0] == 0)
                                {
                                    //if (b % 512 == 0)
                                    //{
                                    //    Writer.WriteLine((a - 1) * 2);
                                    //    Writer.Flush();
                                    //}
                                    Add.FontDec.WriteByte((byte)(Add.Dictionary[temp, 1]));
                                    //b++;
                                    temp = 0;
                                }
                            }
                        }
                    } while (boolean);

                    //FileStream FontDecFile = new FileStream(@"FONT0 DECOMPRESS", FileMode.Create);
                    //Add.FontDec.Position = 0;
                    //Add.FontDec.CopyTo(FontDecFile);

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

            if (command == "com")
            {
                try
                {
                    MemoryStream FontDecRev = Add.FontDecRev();

                    //FileStream newFile = new FileStream(@"FONT DEC",FileMode.Create);
                    //FontDecRev.Position = 0;
                    //for (int i = 0; i < FontDecRev.Length; i++)
                    //{
                    //    newFile.WriteByte((byte)FontDecRev.ReadByte());
                    //}

                    //FONT.Position = 0;
                    //MemoryStream FontDecRev = new MemoryStream();
                    FileStream FONT_COMPRESS_FILE = new FileStream(@"FONT0 NEW.FNT", FileMode.Create);
                    MemoryStream FONT_COMPRESS = new MemoryStream();

                    int DictPart = Add.FindDictPart(Add.Dictionary);

                    bool boolean = true;
                    //try
                    //{
                    //    FileStream FontDec = new FileStream(@"FONT0 DECOMPRESS", FileMode.Open, FileAccess.Read);
                    //    FontDec.Position = FontDec.Length - 1;
                    //    while (boolean)
                    //    {
                    //        FontDecRev.WriteByte((byte)FontDec.ReadByte());
                    //        FontDec.Position = FontDec.Position - 2;
                    //        if (FontDec.Position == 0)
                    //        {
                    //            FontDecRev.WriteByte((byte)FontDec.ReadByte());
                    //            boolean = false;
                    //        }
                    //    }
                    //    FontDecRev.Position = 0;
                    //}
                    //catch (Exception e)
                    //{
                    //    Console.WriteLine(e);
                    //    Console.ReadKey();
                    //    return;
                    //}

                    FontDecRev.Position = 0;
                    boolean = true;

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
                        FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(FONT.ReadByte()));
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
                            string str = Convert.ToString(Convert.ToInt32(Reader.ReadLine()), 16);
                            str = str.PadLeft(8, '0');
                            string str1 = Convert.ToString(str[6]) + Convert.ToString(str[7]);
                            string str2 = Convert.ToString(str[4]) + Convert.ToString(str[5]);
                            string str3 = Convert.ToString(str[2]) + Convert.ToString(str[3]);
                            string str4 = Convert.ToString(str[0]) + Convert.ToString(str[1]);
                            FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str1, 16));
                            FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str2, 16));
                            FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str3, 16));
                            FONT_COMPRESS_FILE.WriteByte(Convert.ToByte(str4, 16));
                        }
                    }


                    File.Delete(@"GLYPH POSITION NEW");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadKey();
                    return;
                }

            }

            Console.WriteLine("Success");
            Console.ReadKey();
            return;
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

            //FONT.Position = Dictionary_Pos + Dictionary_Size;
            //StreamWriter Writer = new StreamWriter(File.Create(@"GLYPH POSITION"), Encoding.Default);
            //for (int i = 0; i < GlyphPositionTable_Size / 4; i++)
            //{
            //    int a = Read4ByteFile(ref FONT);
            //    Writer.WriteLine(a);
            //    Writer.Flush();
            //}

            CompressedFontBlock_Pos = Dictionary_Pos + Dictionary_Size + GlyphPositionTable_Size;

        }

        public int Read2ByteFile(ref FileStream NewStream)
        {
            byte[] val = new byte[2];
            val[0] = (byte)NewStream.ReadByte();
            val[1] = (byte)NewStream.ReadByte();
            return (int)BitConverter.ToUInt16(val, 0);

            //NewStream.Position = NewStream.Position - 2;
            //string str = Convert.ToString(NewStream.ReadByte(), 16);
            //str = Convert.ToString(NewStream.ReadByte(), 16) + str.PadLeft(2, '0');
            //return Convert.ToInt32(str, 16);
        }

        public int Read4ByteFile(ref FileStream NewStream)
        {
            byte[] val = new byte[4];
            val[0] = (byte)NewStream.ReadByte();
            val[1] = (byte)NewStream.ReadByte();
            val[2] = (byte)NewStream.ReadByte();
            val[3] = (byte)NewStream.ReadByte();
            return (int)BitConverter.ToInt32(val, 0);

            //string str = Convert.ToString(NewStream.ReadByte(), 16);
            //str = Convert.ToString(NewStream.ReadByte(), 16) + str.PadLeft(2, '0');
            //str = Convert.ToString(NewStream.ReadByte(), 16) + str.PadLeft(4, '0');
            //str = Convert.ToString(NewStream.ReadByte(), 16) + str.PadLeft(6, '0');
            //return Convert.ToInt32(str, 16);
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

        public void Write2ByteFile(ref FileStream NewStream, int Byte2)
        {
            string str = Convert.ToString(Byte2, 16);
            str = str.PadLeft(4, '0');
            string str2 = Convert.ToString(str[0]) + Convert.ToString(str[1]);
            str = Convert.ToString(str[2]) + Convert.ToString(str[3]);
            NewStream.WriteByte(Convert.ToByte(str, 16));
            NewStream.WriteByte(Convert.ToByte(str2, 16));
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
                            //int a = data[BMPpos];
                            a = (a >> 4) + (a - (a >> 4 << 4) << 4);
                            FontDec.WriteByte((byte)a);
                        }
                    }
                }
            }
            return FontDec;
        }

        public void FindSize(int Count, ref int x, ref int y)
        {
            int abs = Count;
            for (int i = 1; i < Count; i++)
            {
                if ((Count / i == Math.Round((double)Count / i)) & (Math.Abs((decimal)i - Count / i)) < abs)
                {
                    abs = (int)Math.Abs((decimal)(i - Count / i));
                    x = i;
                    y = (int)Count / i;
                }
            }
        }

        public void Save2BMP(ref FileStream FONT)
        {
            int x = 0;
            int y = 0;
            FindSize(TotalNumberOfGlyphs, ref x, ref y);
            Bitmap FileBMP = new Bitmap(x * 32, y * 32, PixelFormat.Format4bppIndexed);
            BMPCopyPalette(ref FileBMP, ref FONT);

            byte[] data = Memory2BMP(ref FontDec, x, y);

            BitmapData bmpData = FileBMP.LockBits(new Rectangle(0, 0, FileBMP.Width, FileBMP.Height), ImageLockMode.ReadWrite, FileBMP.PixelFormat);
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
            FileBMP.UnlockBits(bmpData);

            FileBMP.Save(@"FONT0.BMP", ImageFormat.Bmp);
        }

        public MemoryStream FontDecRev()
        {
            Bitmap FileBMP = (Bitmap)Image.FromFile(@"FONT0.bmp", true);
            BitmapData bmpData = FileBMP.LockBits(new Rectangle(0, 0, FileBMP.Width, FileBMP.Height), ImageLockMode.ReadWrite, FileBMP.PixelFormat);
            byte[] data = new byte[bmpData.Stride * bmpData.Height];
            Marshal.Copy(bmpData.Scan0, data, 0, data.Length);
            FileBMP.UnlockBits(bmpData);

            int x = 0;
            int y = 0;
            FindSize(TotalNumberOfGlyphs, ref x, ref y);

            return BMP2Memory(data, x, y);
        }
    }
}