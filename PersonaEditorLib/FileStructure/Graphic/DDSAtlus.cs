using PersonaEditorLib.Interfaces;
using PersonaEditorLib.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.Graphic
{
    public class DDSAtlusHeader
    {
        public int Size { get; } = 124;

        public int HeaderFlags { get; } // Header flags?
        public int SizeWOHeader { get; set; } // Size texture without header
        public int Unknown0x08 { get; } // Version?
        public int Unknown0x0C { get; } // Padding?

        public int HeaderSize { get; }
        public int SizeTexture { get; set; }
        public PixelFormatDDSAtlus PixelFormat { get; }
        public byte MipMapCount { get; }
        public byte Unknown0x1A { get; }
        public byte TileByte { get; private set; }
        public uint UnknownFlags { get; }

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public uint Unknown0x24 { get; }
        public int[] Reserved { get; }

        public int TileCount { get; } = 1;
        public bool Tile { get; private set; }

        private void CheckHeader()
        {
            if (Unknown0x08 != 0x1) throw new Exception("DDSHeaderV2: exception 0x08");
            if (Unknown0x0C != 0x0) throw new Exception("DDSHeaderV2: exception 0x0C");
            if (HeaderSize != 0x80) throw new Exception("DDSHeaderV2: exception 0x10");
            if (Unknown0x1A != 0x02) throw new Exception("DDSHeaderV2: exception 0x1A");
            if (Unknown0x24 != 0x10000) throw new Exception("DDSHeaderV2: exception 0x24");
            if (Reserved.Contains<int>(0, new ReverseStructComparer<int>()))
                throw new Exception("DDSHeaderV2: exception 0x28 array");
        }

        public DDSAtlusHeader(BinaryReader reader)
        {
            // 0x00-0x10
            HeaderFlags = reader.ReadInt32();
            SizeWOHeader = reader.ReadInt32();
            Unknown0x08 = reader.ReadInt32();
            Unknown0x0C = reader.ReadInt32();

            // 0x10-0x20
            HeaderSize = reader.ReadInt32();
            SizeTexture = reader.ReadInt32();
            PixelFormat = (PixelFormatDDSAtlus)reader.ReadByte();
            MipMapCount = reader.ReadByte();
            Unknown0x1A = reader.ReadByte();
            TileByte = reader.ReadByte();
            UnknownFlags = reader.ReadUInt32();

            // 0x20-0x80
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Unknown0x24 = reader.ReadUInt32();
            Reserved = reader.ReadInt32Array(22);

            CheckHeader();

            GetTile(TileByte);

            if (Tile)
            {
                int BytePerBlock = PixelFormat == PixelFormatDDSAtlus.DXT1 ? 8 : 16;
                int size = Width * Height * BytePerBlock / 16;
                TileCount = SizeTexture % size == 0 ? SizeTexture / size : throw new Exception("DDSAtlus: tile read error");
            }

        }

        public byte[] Get()
        {
            byte[] returned = null;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriterBE(MS))
            {
                writer.Write(HeaderFlags);
                writer.Write(SizeWOHeader);
                writer.Write(Unknown0x08);
                writer.Write(Unknown0x0C);

                writer.Write(HeaderSize);
                writer.Write(SizeWOHeader);
                writer.Write((byte)PixelFormat);
                writer.Write(MipMapCount);
                writer.Write(Unknown0x1A);
                writer.Write(TileByte);
                writer.Write(UnknownFlags);

                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Unknown0x24);
                writer.WriteInt32Array(Reserved);

                returned = MS.ToArray();
            }

            return returned;
        }

        private void GetTile(byte tile)
        {
            if (tile == 0)
                Tile = false;
            else if (tile == 1)
                Tile = true;
            else
                throw new Exception("DDSHeaderV2: exception 0x1B");
        }
    }

    class DDSAtlus : IPersonaFile, IImage
    {
        public DDSAtlusHeader Header { get; private set; }

        private List<ImageBase> dataList = new List<ImageBase>();
        private BitmapSource bitmapSource = null;

        byte[] LastBlock = new byte[0];

        public DDSAtlus(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, MS.Length, 0));
        }

        public DDSAtlus(StreamPart streamFile)
        {
            Read(streamFile);
        }

        private void Read(StreamPart streamFile)
        {
            streamFile.Stream.Position = streamFile.Position;

            using (BinaryReader reader = new BinaryReaderBE(streamFile.Stream, Encoding.ASCII, true))
            {
                Header = new DDSAtlusHeader(reader);

                int temp = 0;

                temp += ReadTexture(reader);

                if (temp != Header.SizeTexture)
                {
                    throw new Exception("DDSAtlus: wrong texture size");
                }

                LastBlock = reader.ReadBytes(Header.SizeWOHeader - Header.SizeTexture);
            }
        }

        private int ReadTexture(BinaryReader reader)
        {
            int returned = 0;

            int width = Header.Width;
            int height = Header.Height;
            int BitPerBlock = 0;

            var pixelFormat = PixelFormatHelper.ConvertFromDDSAtlus(Header.PixelFormat);
            if (pixelFormat == PixelBaseFormat.Unknown)
                throw new Exception("DDSAtlus: unknown PixelFormat");
            if (pixelFormat.IsCompressed())
                BitPerBlock = pixelFormat == PixelBaseFormat.DXT1 ? 8 : 16;
            else
                BitPerBlock = pixelFormat.BitsPerPixel();

            int size = 0;
            if (pixelFormat.IsCompressed())
                size = Convert.ToInt32(Math.Ceiling((double)width / 4) *
                    Math.Ceiling((double)height / 4) *
                    BitPerBlock) *
                    Header.TileCount;
            else
                size = width * height * BitPerBlock / 8;

            if(Header.SizeWOHeader != Header.SizeTexture)
            {

            }

            if (Header.Tile)
            {
                dataList.Add(new ImageBase(width, height * Header.TileCount, pixelFormat, reader.ReadBytes(size)));
                returned += size;
            }
            else
            {
                for (int i = 0; i < Header.MipMapCount; i++)
                {
                    returned += size;
                    dataList.Add(new ImageBase(width, height, pixelFormat, reader.ReadBytes(size), BitmapPalettes.Gray256));

                    width = width / 2 == 0 ? 1 : width / 2;
                    height = height / 2 == 0 ? 1 : height / 2;

                    if (pixelFormat.IsCompressed())
                        size = Convert.ToInt32(Math.Ceiling((double)width / 4) *
                            Math.Ceiling((double)height / 4) *
                            BitPerBlock);
                    else
                        size = width * height * BitPerBlock / 8;
                }
            }

            return returned;
        }

        #region IPersonaFile

        public FileType Type => FileType.DDS;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public ReadOnlyObservableCollection<PropertyClass> GetProperties => null;

        #endregion IPersonaFile

        #region IFile

        public int Size() => Header.Size + 4 + dataList.Sum(x => x.LengthData) + LastBlock.Length;

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                writer.Write(Header.Get());
                dataList.ForEach(x => writer.Write(x.GetImageData()));
                writer.Write(LastBlock);

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile

        #region IImage

        public BitmapSource GetImage()
        {
            if (bitmapSource == null)
                bitmapSource = dataList[0].GetBitmapSource();

            return bitmapSource;
        }

        public void SetImage(BitmapSource bitmapSource)
        {
            this.bitmapSource = null;

            Header.Width = (ushort)bitmapSource.PixelWidth;
            Header.Height = (ushort)(bitmapSource.PixelHeight / Header.TileCount);

            var image = new ImageBaseConverter(bitmapSource);
            if (!image.TryConvert(PixelFormatHelper.ConvertFromDDSAtlus(Header.PixelFormat)))
            {

            }
            dataList[0] = image;

            for (int i = 1; i < dataList.Count; i++)
            {
                var scale = Math.Pow(0.5, i);
                ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapSource, scaleTransform);

                image = new ImageBaseConverter(transformedBitmap);
                if (!image.TryConvert(PixelFormatHelper.ConvertFromDDSAtlus(Header.PixelFormat)))
                {

                }

                dataList[i] = image;
            }

            Header.SizeWOHeader = dataList.Sum(x => x.LengthData);
            Header.SizeTexture = Header.SizeWOHeader;
        }

        #endregion IImage
    }
}