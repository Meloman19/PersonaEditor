using AuxiliaryLibraries.IO;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AuxiliaryLibraries.Tools
{
    public static class IOTools
    {
        public static T FromBytes<T>(byte[] arr) where T : struct
        {
            T str = new T();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }

        public static byte[] GetBytes<T>(T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static BinaryReader OpenReadFile(Stream stream, bool IsLittleEndian, bool leaveOpen = true)
        {
            BinaryReader returned;

            if ((BitConverter.IsLittleEndian & IsLittleEndian) || !(BitConverter.IsLittleEndian | IsLittleEndian))
                returned = new BinaryReader(stream, Encoding.ASCII, leaveOpen);
            else
                returned = new BinaryReaderEndian(stream, Encoding.ASCII, leaveOpen);

            return returned;
        }

        public static BinaryReader OpenReadFile(byte[] data, bool IsLittleEndian)
        {
            BinaryReader returned;

            if ((BitConverter.IsLittleEndian & IsLittleEndian) || !(BitConverter.IsLittleEndian | IsLittleEndian))
                returned = new BinaryReader(new MemoryStream(data), Encoding.ASCII, false);
            else
                returned = new BinaryReaderEndian(new MemoryStream(data), Encoding.ASCII, false);

            return returned;
        }

        public static BinaryWriter OpenWriteFile(Stream stream, bool IsLittleEndian, bool leaveOpen = true)
        {
            BinaryWriter returned;

            if (BitConverter.IsLittleEndian & IsLittleEndian || !(BitConverter.IsLittleEndian | IsLittleEndian))
                returned = new BinaryWriter(stream, Encoding.ASCII, leaveOpen);
            else
                returned = new BinaryWriterEndian(stream, Encoding.ASCII, leaveOpen);

            return returned;
        }

        public static int Alignment(int Size, int Align)
        {
            return Alignment((long)Size, Align);
        }

        public static int Alignment(long Size, int Align)
        {
            int temp = (int)Size % Align;
            temp = Align - temp;
            return temp % Align;
        }

        public static string RelativePath(string srcPath, string relPath)
        {
            string[] srcDirs = srcPath.Split('\\').Where(x => x != "").ToArray();
            string[] relDirs = relPath.Split('\\').Where(x => x != "").ToArray();

            int startindex = 0;

            for (int i = 0; i < relDirs.Length; i++)
            {
                if (relDirs[i] != srcDirs[i])
                    throw new Exception("RelativePath");
                startindex = i;
            }


            string returned = "";

            for (startindex = startindex + 1; startindex < srcDirs.Length - 1; startindex++)
                returned += srcDirs[startindex] + "\\";

            returned += srcDirs[startindex];

            return returned;
        }
    }
}