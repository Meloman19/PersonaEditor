using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PersonaEditorLib.Sprite
{
    public class TMX : IGameData, IImage
    {
        #region Constants
        public const int ID = 0x2;
        public const uint MagicNumber = 0x30584D54;
        #endregion Constants

        #region Private Fields

        Bitmap bitmap;
        TMXHeader header;

        #endregion Private Fields

        #region Public Properties

        public bool IsLittleEndian { get; set; } = true;

        public uint TextureID => header.TextureID;
        public uint ClutID => header.ClutID;
        public string Comment => Encoding.ASCII.GetString(header.Comment).TrimEnd('\0');
        private PixelFormat ImageFormat { get; set; }
        private PixelFormat PaletteFormat { get; set; }
        public int Width => bitmap.Width;
        public int Height => bitmap.Height;

        #endregion Public Properties

        public TMX(StreamPart streamPart) : this()
        {
            Read(streamPart);
        }

        public TMX(byte[] data) : this()
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, data.LongLength, 0));
        }

        protected TMX()
        {
        }

        private void Read(StreamPart streamPart)
        {
            streamPart.Stream.Position = streamPart.Position;
            using (BinaryReader reader = IOTools.OpenReadFile(streamPart.Stream, IsLittleEndian))
            {
                int tempsize = 0;

                byte[] imageData;

                header = ReadHeader(reader.ReadBytes(0x40));

                ImageFormat = TMXHelper.PS2ToAux(header.PixelFormat);
                PaletteFormat = TMXHelper.PS2ToAux(header.PaletteFormat);

                tempsize += 0x40;
                System.Drawing.Color[] colors = null;
                if (header.PaletteCount == 1)
                {
                    tempsize += TMXHelper.ReadPalette(reader, header.PixelFormat, header.PaletteFormat, out colors);
                }

                int datasize = header.Height * ImageHelper.GetStride(ImageFormat, header.Width);
                imageData = reader.ReadBytes(datasize);

                tempsize += datasize;

                if (header.FileSize != tempsize)
                    throw new Exception("TMX: filesize not equal");

                bitmap = new Bitmap(header.Width, header.Height, ImageFormat, imageData, colors);
            }
        }

        private TMXHeader ReadHeader(byte[] data)
        {
            var Header = IOTools.FromBytes<TMXHeader>(data);
            if (Header.ID != ID) throw new Exception("TMX: (0x00) wrong ID");
            if (Header.MagicNumber != MagicNumber) throw new Exception("TMX: (0x08) wrong MagicNumber");
            if (Header.Padding != 0) throw new Exception("TMX: (0x0C) wrong padding");
            if (Header.PaletteCount > 1) throw new Exception("TMX: (0x10) number of palette more than 1");
            if (Header.MipMapCount != 0) throw new Exception("TMX: (0x17) mipMapCount more than 0");
            if (Header.MipMapK != 0) throw new Exception("TMX: (0x18) mipMapK more than 0");
            if (Header.MipMapL != 0) throw new Exception("TMX: (0x19) mipMapL more than 0");
            if (Header.WrapMode != 0xFF00) throw new Exception("TMX: (0x1A) error (wrapMode?)");
            return Header;
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.TMX;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            int returned = 0;
            returned += 0x40;
            returned += ImageFormat.IsIndexed ?
                Convert.ToInt32(Math.Pow(2, ImageFormat.BitsPerPixel)) * PaletteFormat.BitsPerPixel / 8
                : 0;
            returned += ImageHelper.GetStride(ImageFormat, Width) * Height;
            return returned;
        }

        public byte[] GetData()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

                writer.Write(IOTools.GetBytes(header));

                if (ImageFormat.IsIndexed)
                    TMXHelper.WritePalette(writer, header.PixelFormat, header.PaletteFormat, bitmap.CopyPalette());

                writer.Write(bitmap.CopyData());


                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion

        #region IImage

        public Bitmap GetBitmap()
        {
            return bitmap;
        }

        public void SetBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                return;

            if (bitmap.PixelFormat == this.bitmap.PixelFormat)
                this.bitmap = bitmap;
            else
                this.bitmap = bitmap.ConvertTo(this.bitmap.PixelFormat, null);

            header.Width = (ushort)bitmap.Width;
            header.Height = (ushort)bitmap.Height;
            header.FileSize = GetSize();
        }

        #endregion IImage
    }
}