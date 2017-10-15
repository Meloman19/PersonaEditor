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
        public TMXPalette Palette;
        public byte[] DataB;
        public FileTypes.ImageData Data;

        public TMX(Stream stream, bool IsLittleEndian)
        {
            try
            {
                BinaryReader reader;

                if (IsLittleEndian)
                    reader = new BinaryReader(stream);
                else
                    reader = new BinaryReaderBE(stream);

                Header = new TMXHeader(reader);
                Palette = new TMXPalette(reader, Header.PixelFormat);

                PixelFormat format;
                if (TMXPalette.GetIndexedColorCount(Header.PixelFormat) == 256)
                {
                    format = PixelFormats.Indexed8;
                    DataB = reader.ReadBytes(Header.Width * Header.Height);

                }
                else
                {
                    format = PixelFormats.Indexed4;
                    DataB = Utilities.DataReverse(reader.ReadBytes(Convert.ToInt32((double)Header.Width * Header.Height / 2)));
                }

                Data = new FileTypes.ImageData(DataB, format, Header.Width, Header.Height);



            }
            catch (Exception e)
            {
                Logging.Write("L", e);
            }
        }
    }
}