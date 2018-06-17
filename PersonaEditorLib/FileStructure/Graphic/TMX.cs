using PersonaEditorLib.Interfaces;
using PersonaEditorLib.Media.Imaging;
using PersonaEditorLib.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.FileStructure.Graphic
{
    public class TMX : BindingObject, IPersonaFile, IImage
    {
        #region Constants
        public const int ID = 0x2;
        public const uint MagicNumber = 0x30584D54;
        #endregion Constants

        #region Private Fields
        BitmapSource bitmapSource = null;
        ImageBase ImageBase = null;

        private byte[] comment;
        ObservableCollection<PropertyClass> properties = new ObservableCollection<PropertyClass>();
        #endregion Private Fields

        #region Public Properties

        public bool IsLittleEndian { get; set; } = true;

        public uint TextureID { get; private set; }
        public uint ClutID { get; private set; }
        public string Comment => Encoding.ASCII.GetString(comment).TrimEnd('\0');
        public PixelBaseFormat ImageFormat { get; private set; }
        public PixelBaseFormat PaletteFormat { get; private set; }
        public IQuantization Quantizer { get; set; } = new Media.Imaging.Quantization.WuQuantizer();
        public int Width => ImageBase?.Width ?? bitmapSource.PixelWidth;
        public int Height => ImageBase?.Height ?? bitmapSource.PixelHeight;

        #endregion Public Properties

        public TMX(StreamPart streamPart)
        {
            GetProperties = new ReadOnlyObservableCollection<PropertyClass>(properties);
            Read(streamPart);
            SetPropTable();
        }

        public TMX(byte[] data)
        {
            GetProperties = new ReadOnlyObservableCollection<PropertyClass>(properties);
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, data.LongLength, 0));
            SetPropTable();
        }

        private void Read(StreamPart streamPart)
        {
            streamPart.Stream.Position = streamPart.Position;
            using (BinaryReader reader = IO.OpenReadFile(streamPart.Stream, IsLittleEndian))
            {
                int tempsize = 0;

                byte[] paletteData = null;
                byte[] imageData;

                var Header = ReadHeader(reader.ReadBytes(0x40));
                ImageFormat = PixelFormatHelper.ConvertFromPS2(Header.PixelFormat);
                PaletteFormat = PixelFormatHelper.ConvertFromPS2(Header.PaletteFormat);
                TextureID = Header.TextureID;
                ClutID = Header.ClutID;
                comment = Header.Comment;

                tempsize += 0x40;

                if (ImageFormat == PixelBaseFormat.Indexed8)
                {
                    paletteData = TMXHelper.TilePalette(reader.ReadBytes(256 * 4));
                    tempsize += 256 * 4;
                }
                else if (ImageFormat == PixelBaseFormat.Indexed4PS2)
                {
                    paletteData = reader.ReadBytes(16 * 4);
                    tempsize += 16 * 4;
                }

                int datasize = Header.Height * ImageHelper.GetStride(ImageFormat, Header.Width);
                imageData = reader.ReadBytes(datasize);

                tempsize += datasize;

                if (Header.FileSize != tempsize)
                    throw new Exception("TMX: filesize not equal");

                ImageBase = new ImageBase(Header.Width, Header.Height, ImageFormat, imageData, PaletteFormat, paletteData);
            }
        }

        private TMXHeader ReadHeader(byte[] data)
        {
            var Header = UtilitiesTool.fromBytes<TMXHeader>(data);
            if (Header.ID != ID) throw new Exception("TMX: (0x00) wrong ID");
            if (Header.MagicNumber != MagicNumber) throw new Exception("TMX: (0x08) wrong MagicNumber");
            if (Header.Padding != 0) throw new Exception("TMX: (0x0C) wrong padding");
            if (Header.PaletteCount != 1) throw new Exception("TMX: (0x10) number of palette not 1");
            if (Header.MipMapCount != 0) throw new Exception("TMX: (0x17) mipMapCount more than 0");
            if (Header.MipMapK != 0) throw new Exception("TMX: (0x18) mipMapK more than 0");
            if (Header.MipMapL != 0) throw new Exception("TMX: (0x19) mipMapL more than 0");
            if (Header.WrapMode != 0xFF00) throw new Exception("TMX: (0x1A) error (wrapMode?)");
            return Header;
        }

        private byte[] CreateHeader()
        {
            TMXHeader Header = new TMXHeader();

            Header.ID = ID;
            Header.FileSize = Size();
            Header.MagicNumber = MagicNumber;
            Header.PaletteCount = 1;
            Header.PaletteFormat = PixelFormatHelper.ConvertToPS2(PaletteFormat);
            Header.Width = (ushort)ImageBase.Width;
            Header.Height = (ushort)ImageBase.Height;
            Header.PixelFormat = PixelFormatHelper.ConvertToPS2(ImageFormat);
            Header.WrapMode = 0xFF00;
            Header.TextureID = TextureID;
            Header.ClutID = ClutID;
            Header.Comment = comment;

            return UtilitiesTool.getBytes(Header);
        }

        private void SetPropTable()
        {
            properties.Clear();
            properties.Add(new PropertyClass("Width", ImageBase.Width.ToString(), true));
            properties.Add(new PropertyClass("Height", ImageBase.Height.ToString(), true));
            properties.Add(new PropertyClass("Pixel Format", Enum.GetNames(typeof(PixelFormatPS2Enum)).ToArray(), 0));
        }

        #region IPersonaFile

        public FileType Type => FileType.TMX;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public ReadOnlyObservableCollection<PropertyClass> GetProperties { get; }

        #endregion IPersonaFile

        #region IFile

        public int Size()
        {
            int returned = 0;
            returned += 0x40;
            returned += ImageFormat.IsIndexed() ?
                (int)Math.Pow(2, PixelFormatHelper.BitsPerPixel(ImageFormat)) * PixelFormatHelper.BitsPerPixel(PaletteFormat) / 8 : 0;
            returned += ImageHelper.GetStride(ImageFormat, Width) * Height;
            return returned;
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IO.OpenWriteFile(MS, IsLittleEndian);

                writer.Write(CreateHeader());

                if (ImageFormat.IsIndexed())
                    if (PixelFormatHelper.BitsPerPixel(ImageFormat) == 8)
                        writer.Write(TMXHelper.TilePalette(ImageBase.GetPaletteData(PaletteFormat)));
                    else
                        writer.Write(ImageBase.GetPaletteData(PaletteFormat));

                writer.Write(ImageBase.GetImageData());


                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile

        #region IImage

        public BitmapSource GetImage()
        {
            if (bitmapSource == null)
                bitmapSource = ImageBase.GetBitmapSource();

            return bitmapSource;
        }

        public void SetImage(BitmapSource bitmapSource)
        {
            this.bitmapSource = null;
            var imageConverter = new ImageBaseConverter(bitmapSource)
            {
                Quantizer = this.Quantizer
            };
            if (imageConverter.TryConvert(ImageFormat))
            {
                ImageBase = imageConverter;
            }
        }

        #endregion IImage
    }
}