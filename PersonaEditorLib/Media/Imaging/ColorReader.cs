using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PersonaEditorLib.Media.Imaging
{
    class ColorReader : IDisposable
    {
        public Stream BaseStream => reader == null ? null : reader.BaseStream;
        BinaryReader reader = null;
        bool dispose = false;
        bool leaveOpen = false;
        bool IsLittleEndian = true;

        public ColorReader(Stream stream, bool IsLittleEndian, bool leaveOpen = true)
        {
            reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen);
            this.leaveOpen = leaveOpen;
            this.IsLittleEndian = IsLittleEndian;
        }

        public ColorReader(byte[] data, bool IsLittleEndian)
        {
            reader = new BinaryReader(new MemoryStream(data), Encoding.ASCII, false);
            this.IsLittleEndian = IsLittleEndian;
        }

        public void Dispose()
        {
            if (!dispose)
            {
                reader.Dispose();
                dispose = true;
            }
        }

        public Color ReadBgra32()
        {
            if (!dispose)
            {
                byte[] temp = reader.ReadBytes(4);
                if (IsLittleEndian)
                    return Color.FromArgb(temp[3], temp[2], temp[1], temp[0]);
                else
                    return Color.FromArgb(temp[0], temp[1], temp[2], temp[3]);
            }
            else
                throw new Exception("ColorReader: reader close");
        }

        public Color ReadRgba32()
        {
            if (!dispose)
            {
                byte[] temp = reader.ReadBytes(4);
                if (IsLittleEndian)
                    return Color.FromArgb(temp[3] , temp[0], temp[1], temp[2]);
                else
                    return Color.FromArgb(temp[2], temp[1], temp[0], temp[3]);
            }
            else
                throw new Exception("ColorReader: reader close");
        }
    }
}