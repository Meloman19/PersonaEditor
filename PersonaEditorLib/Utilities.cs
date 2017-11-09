using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static BinaryReader OpenReadFile(Stream stream, bool IsLittleEndian)
        {
            BinaryReader returned;

            if (IsLittleEndian)
                returned = new BinaryReader(stream);
            else
                returned = new BinaryReaderBE(stream);

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

    public static class String
    {
        public static byte[] SplitString (string str, char del)
        {
            string[] temp = str.Split(del);
            return Enumerable.Range(0, temp.Length).Select(x => Convert.ToByte(temp[x], 16)).ToArray();
        }
    }

    public static class Utilities
    {
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

        public static byte[] DataReverse(byte[] Data)
        {
            byte[] returned = new byte[Data.Length];
            for (int i = 0; i < Data.Length; i++)
                returned[i] = ReverseByte(Data[i]);
            return returned;
        }

        public static byte ReverseByte(byte toReverse)
        {
            int temp = (toReverse >> 4) + ((toReverse - (toReverse >> 4 << 4)) << 4);
            return Convert.ToByte(temp);
        }

        public static List<Color> ReadPalette(BinaryReader reader, int Count)
        {
            List<Color> Colors = new List<Color>();
            for (int i = 0; i < Count; i++)
                Colors.Add(new Color()
                {
                    R = reader.ReadByte(),
                    G = reader.ReadByte(),
                    B = reader.ReadByte(),
                    A = reader.ReadByte()
                });
            return Colors;
        }

        public static string[] GetAllFiles(DirectoryInfo rootdirinfo, List<string> root)
        {
            root.Add(rootdirinfo.Name);

            List<string> returned = new List<string>();

            foreach (var dir in rootdirinfo.GetDirectories())
                returned.AddRange(GetAllFiles(dir, root.ToList()));

            foreach (var file in rootdirinfo.GetFiles())
                returned.Add(Path.Combine(Path.Combine(root.Skip(1).ToArray()), file.Name));

            return returned.ToArray();


        }

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
    }
}