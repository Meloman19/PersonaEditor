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
using System.ComponentModel;

namespace PersonaEditorLib.Extension
{
    public static class ListExtentsion
    {
        public static string GetString(this IList<FileStructure.PTP.PTP.MSG.MSGstr.MSGstrElement> ByteCollection, CharList CharList, bool LineSplit)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetText(CharList);

            return returned;
        }

        public static string GetString(this IList<FileStructure.PTP.PTP.MSG.MSGstr.MSGstrElement> ByteCollection)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetSystem();

            return returned;
        }

        public static ByteArray GetByteArray(this IList<FileStructure.PTP.PTP.MSG.MSGstr.MSGstrElement> ByteCollection)
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

        public static void Write(this BinaryWriter writer, Stream stream)
        {
            long position = stream.Position;
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            writer.Write(buffer);
            stream.Position = position;
        }

        public static bool CheckEntrance(this Stream B, byte[] Bytes)
        {
            if (Bytes.Length != 0)
            {
                if (B.Position < B.Length)
                    if (B.ReadByte() == Bytes[0])
                        return B.CheckEntrance(Bytes.Skip(1).ToArray());

                return false;
            }
            else return true;
        }
    }

    public static class StringExtension
    {
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
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream FS = new FileStream(path, FileMode.Create))
            {
                BmpBitmapEncoder BMPencoder = new BmpBitmapEncoder();

                BMPencoder.Frames.Add(BitmapFrame.Create(image));
                BMPencoder.Save(FS);
            }
        }

        public static void SavePNG(BitmapSource image, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream FS = new FileStream(path, FileMode.Create))
            {
                PngBitmapEncoder PNGencoder = new PngBitmapEncoder();

                PNGencoder.Frames.Add(BitmapFrame.Create(image));
                PNGencoder.Save(FS);
            }
        }

        public static BitmapSource OpenImage(string path)
        {
            BmpBitmapDecoder BMPdecoder = new BmpBitmapDecoder(new FileStream(path, FileMode.Open), BitmapCreateOptions.None, BitmapCacheOption.Default);

            return BMPdecoder.Frames[0];
        }
    }
}