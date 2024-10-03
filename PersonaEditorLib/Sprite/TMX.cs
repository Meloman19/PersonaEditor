using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
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

        private PixelMap _pixelMap;
        private TMXHeader _header;

        #endregion Private Fields

        #region Public Properties

        public bool IsLittleEndian { get; set; } = true;

        public TMXHeader Header => _header;
        public byte[][] Pallete { get; set; }
        public byte[] ImageData { get; set; }
        public int CurrentPallete { get; set; }

        public string Comment => Encoding.ASCII.GetString(_header.Comment).TrimEnd('\0');

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

                _header = ReadHeader(reader);

                if (!IsSupported(_header))
                    throw new Exception("TMX: unsupported format");

                tempsize += Marshal.SizeOf<TMXHeader>();

                Pallete = new byte[_header.PaletteCount][];
                CurrentPallete = 0;

                for (int i = 0; i < _header.PaletteCount; i++)
                {
                    Pallete[i] = TMXHelper.ReadPallete(reader, _header.PixelFormat);
                    tempsize += Pallete[i].Length;
                }

                int datasize = _header.Height * TMXHelper.GetStride(_header.PixelFormat, _header.Width);
                ImageData = reader.ReadBytes(datasize);

                tempsize += datasize;

                if (_header.FileSize != tempsize)
                    throw new Exception("TMX: filesize not equal");
            }
        }

        private TMXHeader ReadHeader(BinaryReader reader)
        {
            var Header = reader.ReadStruct<TMXHeader>();
            if (Header.ID != ID) throw new Exception("TMX: (0x00) wrong ID");
            if (Header.MagicNumber != MagicNumber) throw new Exception("TMX: (0x08) wrong MagicNumber");
            if (Header.Padding != 0) throw new Exception("TMX: (0x0C) wrong padding");
            if (Header.MipMapCount != 0) throw new Exception("TMX: (0x17) mipMapCount more than 0");
            if (Header.MipMapK != 0) throw new Exception("TMX: (0x18) mipMapK more than 0");
            if (Header.MipMapL != 0) throw new Exception("TMX: (0x19) mipMapL more than 0");
            if (Header.WrapMode != 0xFF00) throw new Exception("TMX: (0x1A) error (wrapMode?)");
            return Header;
        }

        private static bool IsSupported(TMXHeader tmxHeader)
        {
            switch (tmxHeader.PixelFormat)
            {
                case TMXPixelFormatEnum.PSMT4:
                case TMXPixelFormatEnum.PSMT8:
                case TMXPixelFormatEnum.PSMTC32:
                    return true;
                default:
                    return false;
            }
        }

        #region IGameFile

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            int returned = 0;
            returned += 0x40;
            
            if (Pallete.Length != 0)
            {
                for (int i = 0; i < _header.PaletteCount; i++)
                    returned += Pallete[i].Length;
            }

            returned += ImageData.Length;
            return returned;
        }

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

                writer.WriteStruct(_header);
                if (Pallete.Length != 0)
                {
                    for (int i = 0; i < _header.PaletteCount; i++)
                    {
                        writer.Write(Pallete[i]);
                    }
                }
                writer.Write(ImageData);
                return MS.ToArray();
            }
        }

        #endregion

        #region IImage

        public PixelMap GetBitmap()
        {
            if (_pixelMap == null)
                _pixelMap = TMXDecoding.Decode(this);

            return _pixelMap;
        }

        public void SetBitmap(PixelMap bitmap)
        {
            _header.PaletteCount = 1;
            Pallete = new byte[1][];
            CurrentPallete = 0;
            TMXEncoding.Encode(this, bitmap);
            _header.Width = (ushort)bitmap.Width;
            _header.Height = (ushort)bitmap.Height;
            _header.FileSize = GetSize();
            _pixelMap = bitmap;
        }

        #endregion IImage
    }
}