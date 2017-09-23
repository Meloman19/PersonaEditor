using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PersonaEditorLib.FileTypes;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using PersonaEditorLib.FileStructure;
using System.Text.RegularExpressions;

namespace PersonaEditorLib.Extension
{
    public static class ListExtentsion
    {
        public static MSG1.MyStringElement[] GetMyByteArray(this string String, CharList FontMap)
        {
            List<MSG1.MyStringElement> MyByteArrayList = new List<MSG1.MyStringElement>();

            int Index = 0;

            string[] split = Regex.Split(String, "(\r\n|\r|\n)");

            foreach (var a in split)
            {
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                {
                    MyByteArrayList.Add(new MSG1.MyStringElement(Index, MSG1.MyStringElement.arrayType.System, new byte[] { 0x0A }));
                    Index++;
                }
                else
                {
                    string[] splitstr = Regex.Split(a, @"({[^}]+})");

                    foreach (var b in splitstr)
                    {
                        if (Regex.IsMatch(b, @"{.+}"))
                        {
                            MyByteArrayList.Add(new MSG1.MyStringElement(Index, MSG1.MyStringElement.arrayType.System, b.Substring(1, b.Length - 2).GetSystemByte()));
                            Index++;
                        }
                        else
                        {
                            string[] splitsubstr = Regex.Split(b, @"(<[^>]+>)");
                            List<byte> ListByte = new List<byte>();

                            foreach (var c in splitsubstr)
                            {
                                if (Regex.IsMatch(c, @"<.+>"))
                                {
                                    ListByte.AddChar(c.Substring(1, c.Length - 2), FontMap.List);
                                }
                                else
                                {
                                    
                                    ListByte.AddRange(FontMap.GetEncodeBytes(c));
                                }
                            }

                            MyByteArrayList.Add(new MSG1.MyStringElement(Index, MSG1.MyStringElement.arrayType.Text, ListByte.ToArray()));
                        }
                    }
                }
            }

            return MyByteArrayList.ToArray();
        }

        public static MSG1.MyStringElement[] parseString(this byte[] B)
        {
            List<MSG1.MyStringElement> returned = new List<MSG1.MyStringElement>();

            if (B != null)
            {
                MSG1.MyStringElement.arrayType type = MSG1.MyStringElement.arrayType.Text;
                List<byte> temp = new List<byte>();

                int Index = 0;

                for (int i = 0; i < B.Length; i++)
                {
                    if (0x20 <= B[i] & B[i] < 0x80)
                    {
                        temp.Add(B[i]);
                    }
                    else if (0x80 <= B[i] & B[i] < 0xF0)
                    {
                        temp.Add(B[i]);
                        i = i + 1;
                        temp.Add(B[i]);
                    }
                    else
                    {
                        if (0x00 <= B[i] & B[i] < 0x20)
                        {
                            if (temp.Count != 0)
                            {
                                returned.Add(new MSG1.MyStringElement(Index, type, temp.ToArray()));
                                Index++;
                                temp.Clear();
                            }

                            type = MSG1.MyStringElement.arrayType.System;
                            temp.Add(B[i]);

                            returned.Add(new MSG1.MyStringElement(Index, type, temp.ToArray()));
                            Index++;
                            type = MSG1.MyStringElement.arrayType.Text;
                            temp.Clear();
                        }
                        else
                        {
                            if (temp.Count != 0)
                            {
                                returned.Add(new MSG1.MyStringElement(Index, type, temp.ToArray()));
                                Index++;
                                type = MSG1.MyStringElement.arrayType.Text;
                                temp.Clear();
                            }


                            type = MSG1.MyStringElement.arrayType.System;
                            temp.Add(B[i]);
                            int count = (B[i] - 0xF0) * 2 - 1;
                            for (int k = 0; k < count; k++)
                            {
                                i++;
                                temp.Add(B[i]);
                            }

                            returned.Add(new MSG1.MyStringElement(Index, type, temp.ToArray()));
                            Index++;
                            type = MSG1.MyStringElement.arrayType.Text;
                            temp.Clear();
                        }
                    }
                }

                if (temp.Count != 0)
                {
                    returned.Add(new MSG1.MyStringElement(Index, type, temp.ToArray()));
                    temp.Clear();
                }
            }

            return returned.ToArray();
        }

        public static void GetMyByteArray(this IList<MSG1.MyStringElement> MyByteArrayList, string String, CharList FontMap)
        {
            MyByteArrayList.Clear();
            foreach (var a in String.GetMyByteArray(FontMap))
            {
                MyByteArrayList.Add(a);
            }
        }

        public static void AddChar(this List<byte> ByteList, char Char, IList<CharList.FnMpData> FontMap)
        {
            ByteList.AddChar(Char.ToString(), FontMap);
        }

        public static void AddChar(this List<byte> ByteList, string Char, IList<CharList.FnMpData> FontMap)
        {
            if (Char != "")
            {
                CharList.FnMpData fnmp = FontMap.FirstOrDefault(x => x.Char == Char);

                if (fnmp != null)
                {
                    if (fnmp.Index < 0x80)
                    {
                        ByteList.Add((byte)fnmp.Index);
                    }
                    else
                    {
                        byte byte2 = Convert.ToByte((fnmp.Index - 0x20) % 0x80);
                        byte byte1 = Convert.ToByte(((fnmp.Index - 0x20 - byte2) / 0x80) + 0x81);

                        ByteList.Add(byte1);
                        ByteList.Add(byte2);
                    }
                }
            }
        }

        public static byte[] getAllBytes(this MSG1.MyStringElement[] listMyByteArray)
        {
            List<byte> returned = new List<byte>();
            foreach (var a in listMyByteArray)
            {
                returned.AddRange(a.Bytes);
            }
            return returned.ToArray();
        }

        public static string GetString(this MSG1.MyStringElement[] ByteCollection, CharList CharList, bool ClearText)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetText(CharList);


            return returned;
        }

        public static string GetString(this MSG1.MyStringElement[] ByteCollection)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetSystem();

            return returned;
        }
    }

    public static class FileStreamExtension
    {
        public static MemoryStream ReadMemoryStream(this FileStream FileStream, int Size)
        {
            byte[] buffer = new byte[Size];
            FileStream.Read(buffer, 0, Size);
            return new MemoryStream(buffer);
        }

        public static void WriteMemoryStream(this FileStream FileStream, MemoryStream MemoryStream)
        {
            MemoryStream.Position = 0;
            byte[] buffer = new byte[MemoryStream.Length];
            MemoryStream.Read(buffer, 0, (int)MemoryStream.Length);
            FileStream.Write(buffer, 0, buffer.Length);
        }

        public static void SaveToFile(this Stream stream, string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Create))
            {
                long temp = stream.Position;
                stream.Position = 0;
                stream.CopyTo(FS);
                stream.Position = temp;
            }
        }
    }

    public static class Utilities
    {
        public static void ReadShift(this List<IndexedByte> list)
        {
            list.Add(new IndexedByte { Index = 81, Shift = 2 });
            list.Add(new IndexedByte { Index = 103, Shift = 2 });
            list.Add(new IndexedByte { Index = 106, Shift = 2 });
            list.Add(new IndexedByte { Index = 112, Shift = 2 });
            list.Add(new IndexedByte { Index = 113, Shift = 2 });
            list.Add(new IndexedByte { Index = 121, Shift = 2 });
        }

        public static bool CheckEntrance(this byte[] B, byte[] Bytes, int StartIndex)
        {
            if (Bytes.Length != 0)
            {
                if (StartIndex < B.Length)
                {
                    if (B[StartIndex] == Bytes[0])
                        return B.CheckEntrance(Bytes.Skip(1).ToArray(), StartIndex + 1);
                    else return false;
                }
                else return false;
            }
            else return true;
        }

        public static List<byte[]> SplitSourceBytes(byte[] B)
        {
            List<byte[]> returned = new List<byte[]>();

            byte[] LineSplit = B.Take((B[0] - 0xF0) * 2).ToArray();

            List<byte> String = new List<byte>();
            for (int i = 0; i < B.Length; i++)
            {
                if (B.CheckEntrance(LineSplit, i))
                {
                    if (String.Count != 0)
                    {
                        returned.Add(String.ToArray());
                        String.Clear();
                    }
                }

                String.Add(B[i]);
            }

            if (String.Count != 0)
            {
                returned.Add(String.ToArray());
                String.Clear();
            }

            return returned;
        }
    }

    public static class StreamExtension
    {
        public static string ReadString(this BinaryReader BR, int length)
        {
            byte[] buffer = BR.ReadBytes(length);
            return System.Text.Encoding.ASCII.GetString(buffer.Where(x => x != 0).ToArray());
        }

        public static void WriteString(this BinaryWriter BW, string String, int Length)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(String);
                BW.Write(buffer);
                for (int i = 0; i < Length - String.Length; i++)
                {
                    BW.Write((byte)0);
                }
            }
            catch (Exception e)
            {
                Logging.Write("PersonaEditorLib", e);
            }
        }

        public static int[] ReadInt32Array(this BinaryReader BR, int count)
        {
            int[] returned = new int[count];

            for (int i = 0; i < count; i++)
                returned[i] = BR.ReadInt32();

            return returned;
        }

        public static int[][] ReadInt32ArrayArray(this BinaryReader BR, int count, int stride)
        {
            int[][] returned = new int[count][];

            for (int i = 0; i < count; i++)
                returned[i] = BR.ReadInt32Array(stride);

            return returned;
        }

        public static void WriteInt32Array(this BinaryWriter BW, int[] array)
        {
            for (int i = 0; i < array.Length; i++)
                BW.Write(array[i]);
        }

        public static bool CheckEntrance(this Stream stream, byte[] bytes)
        {
            if (bytes.Length != 0)
            {
                if (stream.CanRead)
                {
                    if (stream.ReadByte() == bytes[0])
                    {
                        return stream.CheckEntrance(bytes.Skip(1).ToArray());
                    }
                    else
                    {
                        return false;
                    }
                }
                else { return false; }
            }
            else { return true; }
        }

    }

    public static class StringExtension
    {
        public static byte[] GetSystemByte(this string String)
        {
            List<byte> ListByte = new List<byte>();
            string[] temp = String.Split(' ');
            foreach (var a in temp)
            {
                try
                {
                    ListByte.Add(Convert.ToByte(a, 16));
                }
                catch
                {
                    ListByte.Clear();
                    ListByte.AddRange(System.Text.Encoding.ASCII.GetBytes(String));
                    return ListByte.ToArray();
                }
            }
            return ListByte.ToArray();
        }

        public static string SplitByWidth(this string String, CharList FontMap, int width)
        {
            string returned = String.Join(" ", Regex.Split(String, @"\\n"));

            MSG1.MyStringElement[] temp = returned.GetMyByteArray(FontMap);

            List<int> widthlist = new List<int>();

            foreach (var a in temp)
            {
                if (a.Type == MSG1.MyStringElement.arrayType.Text)
                    for (int i = 0; i < a.Bytes.Length; i++)
                    {
                        CharList.FnMpData fnmp = null;
                        if (a.Bytes[i] == 0x20)
                        {
                            widthlist.Add(9);
                            continue;
                        }
                        else if (0x20 < a.Bytes[i] & a.Bytes[i] < 0x80)
                            fnmp = FontMap.List.FirstOrDefault(x => x.Index == a.Bytes[i]);
                        else if (0x80 <= a.Bytes[i] & a.Bytes[i] < 0xF0)
                        {
                            int newindex = (a.Bytes[i] - 0x81) * 0x80 + a.Bytes[i + 1] + 0x20;
                            i++;
                            fnmp = FontMap.List.FirstOrDefault(x => x.Index == newindex);
                        }

                        if (fnmp != null)
                        {
                            if (fnmp.Cut.Right - fnmp.Cut.Left > 0)
                                widthlist.Add(fnmp.Cut.Right - fnmp.Cut.Left - 1);
                            else
                                widthlist.Add(fnmp.Cut.Right - fnmp.Cut.Left);

                            if (fnmp.Char.Length > 1)
                            {
                                widthlist.AddRange(new int[2] { 0, 0 });
                                for (int k = 1; k < fnmp.Char.Length; k++)
                                    widthlist.Add(0);
                            }
                        }
                        else
                            widthlist.Add(0);
                    }
                else if (a.Type == MSG1.MyStringElement.arrayType.System)
                    widthlist.AddRange(new int[a.GetSystem().Length]);
            }

            int index = 0;
            int widthsum = 0;
            while (index < widthlist.Count)
            {
                if (widthsum + widthlist[index] <= width)
                {
                    widthsum += widthlist[index];
                    index++;
                }
                else
                {
                    bool te = true;
                    while (index != 0 & te)
                    {
                        if (widthlist[index - 1] != 0 & returned[index - 1] == ' ')
                        {
                            returned = returned.Insert(index, "\n");
                            widthlist.Insert(index, 0);
                            te = false;
                        }
                        index--;
                    }
                    widthsum = 0;
                }
            }

            return returned;
        }

        public static string BytesToString(byte[] bytes, CharList Font)
        {
            string returned = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                if (0x20 <= bytes[i] & bytes[i] < 0x80)
                {
                    CharList.FnMpData fnmp = Font.List.FirstOrDefault(x => x.Index == bytes[i]);

                    if (fnmp == null)
                    {
                        returned = returned + "<NCHAR>";
                    }
                    else
                    {
                        if (fnmp.Char != "")
                        {
                            returned = returned + fnmp.Char;
                        }
                        else
                        {
                            returned = returned + "<CHAR>";
                        }
                    }
                }
                else if (0x80 <= bytes[i] & bytes[i] < 0xF0)
                {
                    int link = (bytes[i] - 0x81) * 0x80 + bytes[i + 1] + 0x20;

                    i++;

                    CharList.FnMpData fnmp = Font.List.FirstOrDefault(x => x.Index == link);

                    if (fnmp == null)
                    {
                        returned = returned + "<NCHAR>";
                    }
                    else
                    {
                        if (fnmp.Char != "")
                        {
                            returned = returned + fnmp.Char;
                        }
                        else
                        {
                            returned = returned + "<CHAR>";
                        }
                    }
                }
            }

            return returned;
        }
    }

    public static class Imaging
    {
        public static void SaveBMP(BitmapSource image, string path)
        {
            BmpBitmapEncoder BMPencoder = new BmpBitmapEncoder();

            BMPencoder.Frames.Add(BitmapFrame.Create(image));
            BMPencoder.Save(new FileStream(path, FileMode.Create));
        }

        public static BitmapSource OpenImage(string path)
        {
            BmpBitmapDecoder BMPdecoder = new BmpBitmapDecoder(new FileStream(path, FileMode.Open), BitmapCreateOptions.None, BitmapCacheOption.Default);

            return BMPdecoder.Frames[0];
        }
    }
}