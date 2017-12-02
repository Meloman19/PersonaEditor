using PersonaEditorLib.Interfaces;
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
    public class TMX : BindingObject, IFile, IPersonaFile, IPreview, IImage
    {
        TMXHeader Header;
        TMXPalette Palette;
        byte[] Data;

        string _Name = "";
        bool NameIn = false;

        public TMX(Stream stream, long position, bool IsLittleEndian)
        {
            stream.Position = position;

            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            Open(reader);
        }

        public TMX(string name, byte[] data, bool IsLittleEndian)
        {
            _Name = name;
            if (_Name != "")
                NameIn = true;

            using (MemoryStream MS = new MemoryStream(data))
            {
                BinaryReader reader = Utilities.IO.OpenReadFile(MS, IsLittleEndian);

                Open(reader);
            }
        }

        private void Open(BinaryReader reader)
        {
            Header = new TMXHeader(reader);
            Palette = new TMXPalette(reader, Header.PixelFormat);

            int Length = (Header.Width * Header.Height * Palette.Format.BitsPerPixel) / 8;
            Data = reader.ReadBytes(Length);

            Notify("Image");
        }

        public TMX(string path, bool IsLittleEndian) : this(File.OpenRead(path), 0, true) { }

        private BitmapSource GetBitmapSource()
        {
            byte[] data = Palette.Format == PixelFormats.Indexed4 ? Utilities.Utilities.DataReverse(Data) : Data;
            return BitmapSource.Create(Header.Width, Header.Height, 96, 96, Palette.Format, Palette.Pallete, data, (Palette.Format.BitsPerPixel * Header.Width + 7) / 8);
        }

        private BitmapSource image = null;
        public BitmapSource Image
        {
            get
            {
                if (image == null)
                    image = GetBitmapSource();

                return image;
            }
            set { }
        }

        public string TMXname
        {
            get { return Encoding.ASCII.GetString(Header.UserComment.Where(x => x != 0).ToArray()); }
        }

        private object GetControl()
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = Image;
            return image;
        }

        private object _control = null;
        public object Control
        {
            get
            {
                if (_control == null)
                    _control = GetControl();

                return _control;
            }
        }

        public void Set(byte[] data, bool IsLittleEndian)
        {
            using (MemoryStream MS = new MemoryStream(data))
            {
                BinaryReader reader = Utilities.IO.OpenReadFile(MS, IsLittleEndian);

                Open(reader);
            }
        }

        #region IPersonaFile

        public string Name
        {
            get
            {
                if (NameIn)
                    return _Name;
                else
                    return Encoding.ASCII.GetString(Header.UserComment.Where(x => x != 0).ToArray()) + ".tmx";
            }
        }

        public FileType Type => FileType.TMX;

        public List<object> GetSubFiles()
        {
            return new List<object>();
        }

        public bool Replace(object newdata)
        {
            if (newdata is IPersonaFile pfile)
                if (pfile.Type == Type)
                    if (newdata is TMX tmx)
                    {
                        Header = tmx.Header;
                        Palette = tmx.Palette;
                        Data = tmx.Data;
                        Notify("Image");
                        return true;
                    }

            return false;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.Export);
                returned.Add(ContextMenuItems.Import);

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

        #endregion IPersonaFile

        #region IFile

        public bool IsLittleEndian { get; set; } = true;

        public int Size
        {
            get
            {
                return Header.Size + Palette.Size + Data.Length;
            }
        }
        
        public byte[] Get(bool IsLittleEndian)
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
    }
}