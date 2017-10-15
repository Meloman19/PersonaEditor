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
using static PersonaEditorLib.FileTypes.Text;
using System.ComponentModel;

namespace PersonaEditorLib.Extension
{
    public static class ListExtentsion
    {
        public static List<PTP.MSG.MSGstr.MSGstrElement> GetPTPMsgStrEl(this string String, CharList FontMap)
        {
            List<PTP.MSG.MSGstr.MSGstrElement> MyByteArrayList = new List<PTP.MSG.MSGstr.MSGstrElement>();

            int Index = 0;

            string[] split = Regex.Split(String, "(\r\n|\r|\n)");

            foreach (var a in split)
            {
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                {
                    MyByteArrayList.Add(new PTP.MSG.MSGstr.MSGstrElement("System", new byte[] { 0x0A }));
                    Index++;
                }
                else
                {
                    string[] splitstr = Regex.Split(a, @"({[^}]+})");

                    foreach (var b in splitstr)
                    {
                        if (Regex.IsMatch(b, @"{.+}"))
                        {
                            MyByteArrayList.Add(new PTP.MSG.MSGstr.MSGstrElement("System", b.Substring(1, b.Length - 2).GetSystemByte()));
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
                                    ListByte.AddChar(c.Substring(1, c.Length - 2), FontMap);
                                }
                                else
                                {

                                    ListByte.AddRange(FontMap.Encode(c));
                                }
                            }

                            MyByteArrayList.Add(new PTP.MSG.MSGstr.MSGstrElement("Text", ListByte.ToArray()));
                        }
                    }
                }
            }

            return MyByteArrayList;
        }

        public static List<PTP.MSG.MSGstr.MSGstrElement> GetPTPMsgStrEl(this ByteArray B)
        {
            List<PTP.MSG.MSGstr.MSGstrElement> returned = new List<PTP.MSG.MSGstr.MSGstrElement>();

            string type = "Text";
            List<byte> temp = new List<byte>();

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
                            returned.Add(new PTP.MSG.MSGstr.MSGstrElement(type, temp.ToArray()));
                            temp.Clear();
                        }

                        type = "System";
                        temp.Add(B[i]);

                        returned.Add(new PTP.MSG.MSGstr.MSGstrElement(type, temp.ToArray()));
                        type = "Text";
                        temp.Clear();
                    }
                    else
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(new PTP.MSG.MSGstr.MSGstrElement(type, temp.ToArray()));
                            type = "Text";
                            temp.Clear();
                        }


                        type = "System";
                        temp.Add(B[i]);
                        int count = (B[i] - 0xF0) * 2 - 1;
                        for (int k = 0; k < count; k++)
                        {
                            i++;
                            temp.Add(B[i]);
                        }

                        returned.Add(new PTP.MSG.MSGstr.MSGstrElement(type, temp.ToArray()));
                        type = "Text";
                        temp.Clear();
                    }
                }
            }

            if (temp.Count != 0)
            {
                returned.Add(new PTP.MSG.MSGstr.MSGstrElement(type, temp.ToArray()));
                temp.Clear();
            }


            return returned;
        }

        public static void ParseStrings(this IList<PTP.MSG.MSGstr> Strings, ByteArray SourceBytes)
        {
            Strings.Clear();

            int Index = 0;
            foreach (var Bytes in Util.SplitSourceBytes(SourceBytes))
            {
                PTP.MSG.MSGstr MSG = new PTP.MSG.MSGstr(Index, "");

                List<PTP.MSG.MSGstr.MSGstrElement> temp = Bytes.GetPTPMsgStrEl();

                int tempdown = 0;
                int temptop = temp.Count;

                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i].Type == "System")
                        MSG.Prefix.Add(temp[i]);
                    else
                    {
                        tempdown = i;
                        i = temp.Count;
                    }
                }

                for (int i = temp.Count - 1; i >= tempdown; i--)
                {
                    if (temp[i].Type == "System")
                        MSG.Postfix.Add(temp[i]);
                    else
                    {
                        temptop = i;
                        i = 0;
                    }
                }

                MSG.Postfix.Reverse();

                for (int i = tempdown; i <= temptop; i++)
                    MSG.OldString.Add(temp[i]);

                Strings.Add(MSG);
                Index++;
            }
        }

        public static void AddChar(this IList<byte> ByteList, char Char, CharList FontMap)
        {
            ByteList.AddChar(Char.ToString(), FontMap);
        }

        public static void AddChar(this IList<byte> ByteList, string Char, CharList FontMap)
        {
            if (Char != "")
            {
                CharList.FnMpData fnmp = FontMap.List.FirstOrDefault(x => x.Char == Char);

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

        public static string GetString(this IList<PTP.MSG.MSGstr.MSGstrElement> ByteCollection, CharList CharList, bool ClearText)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetText(CharList);

            return returned;
        }

        public static string GetString(this IList<PTP.MSG.MSGstr.MSGstrElement> ByteCollection)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetSystem();

            return returned;
        }

        public static ByteArray GetByteArray(this IList<PTP.MSG.MSGstr.MSGstrElement> ByteCollection)
        {
            List<byte> temp = new List<byte>();
            foreach (var a in ByteCollection)
                temp.AddRange(a.Array.ToArray());
            return new ByteArray(temp.ToArray());
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

        public static void WriteMemoryStream(this Stream Stream, MemoryStream MemoryStream)
        {
            MemoryStream.Position = 0;
            byte[] buffer = new byte[MemoryStream.Length];
            MemoryStream.Read(buffer, 0, (int)MemoryStream.Length);
            Stream.Write(buffer, 0, buffer.Length);
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

    public static class Util
    {
        public static BitmapPalette CreatePallete(Color color, PixelFormat pixelformat)
        {
            int colorcount = 0;
            byte step = 0;
            if (pixelformat == PixelFormats.Indexed4)
            {
                colorcount = 16;
                step = 0x10;
            }
            else if (pixelformat == PixelFormats.Indexed8)
            {
                colorcount = 256;
                step = 1;
            }


            List<Color> ColorBMP = new List<Color>();
            ColorBMP.Add(new Color { A = 0, R = 0, G = 0, B = 0 });
            for (int i = 1; i < colorcount; i++)
            {
                ColorBMP.Add(new Color
                {
                    A = ByteTruncate(i * step),
                    R = color.R,
                    G = color.G,
                    B = color.B
                });
            }
            return new BitmapPalette(ColorBMP);
        }

        public static byte ByteTruncate(int value)
        {
            if (value < 0) { return 0; }
            else if (value > 255) { return 255; }
            else { return (byte)value; }
        }

        public static void ReadShift(this Dictionary<int, byte> list)
        {
            list.Add(81, 2);
            list.Add(103, 2);
            list.Add(106, 2);
            list.Add(112, 2);
            list.Add(113, 2);
            list.Add(121, 2);
        }

        public static bool CheckEntrance(this ByteArray B, byte[] Bytes, int StartIndex)
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

        public static List<ByteArray> SplitSourceBytes(this ByteArray B)
        {
            List<ByteArray> returned = new List<ByteArray>();

            byte[] LineSplit = B.ToArray().Take((B[0] - 0xF0) * 2).ToArray();

            List<byte> String = new List<byte>();
            for (int i = 0; i < B.Length; i++)
            {
                if (B.CheckEntrance(LineSplit, i))
                {
                    if (String.Count != 0)
                    {
                        returned.Add(new ByteArray(String.ToArray()));
                        String.Clear();
                    }
                }

                String.Add(B[i]);
            }

            if (String.Count != 0)
            {
                returned.Add(new ByteArray(String.ToArray()));
                String.Clear();
            }

            return returned;
        }

        public static string GetNewPath(string source, string end)
        {
            string fullpath = Path.GetFullPath(source);
            return Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + end;
        }

        public static bool ByteArrayCompareWithSimplest(byte[] BytesLeft, byte[] BytesRight)
        {
            if (BytesLeft.Length != BytesRight.Length)
                return false;

            var length = BytesLeft.Length;

            for (int i = 0; i < length; i++)
            {
                if (BytesLeft[i] != BytesRight[i])
                    return false;
            }

            return true;
        }

        public static bool ByteArrayCompareWithSimplest(ByteArray BytesLeft, byte[] BytesRight)
        {
            if (BytesLeft.Length != BytesRight.Length)
                return false;

            var length = BytesLeft.Length;

            for (int i = 0; i < length; i++)
            {
                if (BytesLeft[i] != BytesRight[i])
                    return false;
            }

            return true;
        }

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

        public static byte[] GetBytes(this string str, int size)
        {
            List<byte> returned = new List<byte>();
            returned.AddRange(str.Select(x => Convert.ToByte(x)).ToArray());
            for (int i = returned.Count; i < size; i++)
                returned.Add(0);

            return returned.ToArray();
        }

        public static byte[] GetBytes(this string str)
        {
            return str.GetBytes(str.Length);
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

        public static void Write(this BinaryWriter writer, Stream stream)
        {
            long position = stream.Position;
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            writer.Write(buffer);
            stream.Position = position;
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

            List<PTP.MSG.MSGstr.MSGstrElement> temp = returned.GetPTPMsgStrEl(FontMap);
            List<int> widthlist = new List<int>();

            foreach (var a in temp)
            {
                if (a.Type == "Text")
                    for (int i = 0; i < a.Array.Length; i++)
                    {
                        CharList.FnMpData fnmp = null;
                        if (a.Array[i] == 0x20)
                        {
                            widthlist.Add(9);
                            continue;
                        }
                        else if (0x20 < a.Array[i] & a.Array[i] < 0x80) fnmp = FontMap.List.FirstOrDefault(x => x.Index == a.Array[i]);
                        else if (0x80 <= a.Array[i] & a.Array[i] < 0xF0)
                        {
                            int newindex = (a.Array[i] - 0x81) * 0x80 + a.Array[i + 1] + 0x20;
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
                else if (a.Type == "System")
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

        public static void SavePNG(BitmapSource image, string path)
        {
            PngBitmapEncoder PNGencoder = new PngBitmapEncoder();

            PNGencoder.Frames.Add(BitmapFrame.Create(image));
            PNGencoder.Save(new FileStream(path, FileMode.Create));
        }

        public static BitmapSource OpenImage(string path)
        {
            BmpBitmapDecoder BMPdecoder = new BmpBitmapDecoder(new FileStream(path, FileMode.Open), BitmapCreateOptions.None, BitmapCacheOption.Default);

            return BMPdecoder.Frames[0];
        }
    }
}