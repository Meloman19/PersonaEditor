using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.FileStructure.TMX
{
    public class TMX
    {
        public TMXHeader Header;
        TMXPalette Palette;
        byte[] Data;

        public TMX(Stream stream, long position, bool IsLittleEndian)
        {
            stream.Position = position;

            BinaryReader reader;

            if (IsLittleEndian)
                reader = new BinaryReader(stream);
            else
                reader = new BinaryReaderBE(stream);

            Header = new TMXHeader(reader);
            Palette = new TMXPalette(reader, Header.PixelFormat);

            int Length = (Header.Width * Header.Height * Palette.Format.BitsPerPixel) / 8;
            Data = Palette.Format == PixelFormats.Indexed4 ? Utilities.Utilities.DataReverse(reader.ReadBytes(Length)) : reader.ReadBytes(Length);
        }

        public TMX(string path, bool IsLittleEndian) : this(File.OpenRead(path), 0, true) { }

        private BitmapSource GetBitmapSource()
        {
            return BitmapSource.Create(Header.Width, Header.Height, 96, 96, Palette.Format, Palette.Pallete, Data, (Palette.Format.BitsPerPixel * Header.Width + 7) / 8);
        }

        public BitmapSource Image
        {
            get { return GetBitmapSource(); }
        }
    }
}