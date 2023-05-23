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
        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            byte[] buffer = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            return FromBytes<T>(buffer);
        }

        public static void WriteStruct<T>(this BinaryWriter writer, T structure) where T : struct
        {
            byte[] buffer = GetBytes<T>(structure);
            writer.Write(buffer);
        }

        public static T FromBytes<T>(byte[] buffer) where T : struct
        {
            T structure;
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return structure;
        }

        public static byte[] GetBytes<T>(T structure) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return buffer;
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