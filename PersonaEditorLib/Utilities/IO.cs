using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.Utilities
{

    public static class IO
    {
        public static class Files
        {
            public static List<FileInfo> GetSubFiles(DirectoryInfo rootdir)
            {
                List<FileInfo> returned = new List<FileInfo>();

                foreach (var dir in rootdir.GetDirectories())
                    returned.AddRange(GetSubFiles(dir));

                foreach (var file in rootdir.GetFiles())
                    returned.Add(file);

                return returned;
            }
        }

        public static BinaryReader OpenReadFile(string path, bool IsLittleEndian)
        {
            return OpenReadFile(File.OpenRead(path), IsLittleEndian);
        }

        public static BinaryReader OpenReadFile(Stream stream, bool IsLittleEndian, bool leaveOpen = true)
        {
            BinaryReader returned;

            if (IsLittleEndian)
                returned = new BinaryReader(stream, Encoding.ASCII, true);
            else
                returned = new BinaryReaderBE(stream, Encoding.ASCII, true);

            return returned;
        }

        public static BinaryWriter OpenWriteFile(string path, bool IsLittleEndian)
        {
            return OpenWriteFile(File.Create(path), IsLittleEndian);
        }

        public static BinaryWriter OpenWriteFile(Stream stream, bool IsLittleEndian)
        {
            BinaryWriter returned;

            if (IsLittleEndian)
                returned = new BinaryWriter(stream);
            else
                returned = new BinaryWriterBE(stream);

            return returned;
        }

    }
}
