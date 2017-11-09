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
        public static string GetString(this IList<FileStructure.PTP.TextBaseElement> ByteCollection, CharList CharList, bool LineSplit)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetText(CharList);

            return returned;
        }

        public static string GetString(this IList<FileStructure.PTP.TextBaseElement> ByteCollection)
        {
            string returned = "";

            foreach (var MSG in ByteCollection)
                returned += MSG.GetSystem();

            return returned;
        }

        public static byte[] GetByteArray(this IList<FileStructure.PTP.TextBaseElement> ByteCollection)
        {
            List<byte> temp = new List<byte>();
            foreach (var a in ByteCollection)
                temp.AddRange(a.Array.ToArray());
            return temp.ToArray();
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
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

    public static class Imaging
    {
        public static void SaveBMP(BitmapSource image, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

            using (FileStream FS = new FileStream(path, FileMode.Create))
            {
                BmpBitmapEncoder BMPencoder = new BmpBitmapEncoder();

                BMPencoder.Frames.Add(BitmapFrame.Create(image));
                BMPencoder.Save(FS);
            }
        }

        public static void SavePNG(BitmapSource image, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
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