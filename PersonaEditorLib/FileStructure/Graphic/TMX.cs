using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.FileStructure.Graphic
{
    public enum PS2PixelFormat
    {
        PSMTC32 = 0x00,
        PSMTC24 = 0x01,
        PSMTC16 = 0x02,
        PSMTC16S = 0x0A,
        PSMT8 = 0x13,
        PSMT4 = 0x14,
        PSMT8H = 0x1B,
        PSMT4HL = 0x24,
        PSMT4HH = 0x2C,
        PSMZ32 = 0x30,
        PSMZ24 = 0x31,
        PSMZ16 = 0x32,
        PSMZ16S = 0x3A
    }

    public class TMX : BindingObject, IPersonaFile, IImage
    {
        TMXHeader Header;
        TMXPalette Palette;
        byte[] Data;

        public TMX(Stream stream, long position)
        {
            stream.Position = position;

            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            Open(reader);
        }

        public TMX(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
            {
                BinaryReader reader = Utilities.IO.OpenReadFile(MS, IsLittleEndian);
                Open(reader);
            }
        }

        public TMX(string path, bool IsLittleEndian) : this(File.OpenRead(path), 0) { }

        private void Open(BinaryReader reader)
        {
            Header = new TMXHeader(reader);
            Palette = new TMXPalette(reader, Header.PixelFormat);

            int Length = (Header.Width * Header.Height * Palette.Format.BitsPerPixel) / 8;
            Data = reader.ReadBytes(Length);

            Notify("Image");
        }

        public string TMXname
        {
            get { return Encoding.ASCII.GetString(Header.UserComment.Where(x => x != 0).ToArray()); }
        }

        public void Set(byte[] data, bool IsLittleEndian)
        {
            using (MemoryStream MS = new MemoryStream(data))
            {
                BinaryReader reader = Utilities.IO.OpenReadFile(MS, IsLittleEndian);

                Open(reader);
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        public string Name => Encoding.ASCII.GetString(Header.UserComment.Where(x => x != 0).ToArray()) + ".tmx";

        #region IPersonaFile

        public FileType Type => FileType.TMX;

        public List<ObjectFile> GetSubFiles()
        {
            return new List<ObjectFile>();
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.SaveAs);
                returned.Add(ContextMenuItems.Replace);

                return returned;
            }
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Width", Header.Width);
                returned.Add("Height", Header.Height);
                returned.Add("Pixel Format", Palette.Format);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #region IFile

        public int Size
        {
            get
            {
                return Header.Size + Palette.Size + Data.Length;
            }
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                Header.Get(writer);
                Palette.Get(writer);
                writer.Write(Data);

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile

        #endregion IPersonaFile

        #region IImage

        public BitmapSource GetImage()
        {
            byte[] data = Palette.Format == PixelFormats.Indexed4 ? Utilities.Utilities.DataReverse(Data) : Data;
            return BitmapSource.Create(Header.Width, Header.Height, 96, 96, Palette.Format, Palette.Pallete, data, (Palette.Format.BitsPerPixel * Header.Width + 7) / 8);
        }

        public void SetImage(BitmapSource bitmapSource)
        {
        }

        #endregion IImage
    }
}