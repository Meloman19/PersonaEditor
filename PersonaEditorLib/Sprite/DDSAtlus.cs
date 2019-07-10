using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Media.Formats.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.Sprite
{
    public class DDSAtlus : IGameData, IImage
    {
        public DDSAtlusHeader Header { get; private set; }

        private List<byte[]> dataList = new List<byte[]>();

        byte[] LastBlock = new byte[0];
        Bitmap bitmap = null;

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

            using (BinaryReader reader = new BinaryReaderEndian(streamFile.Stream, Encoding.ASCII, true))
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
            int size = 0;

            var temp = DDSHelper.ConvertFromDDSAtlus(Header.PixelFormat);
            if (temp != DDSFourCC.NONE)
            {
                BitPerBlock = temp == AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC.DXT1 ? 8 : 16;
                size = Convert.ToInt32(Math.Ceiling((double)width / 4) *
                    Math.Ceiling((double)height / 4) *
                    BitPerBlock) *
                    Header.TileCount;
            }
            else
            {
                size = width * height * BitPerBlock / 8;
            }

            if (Header.Tile)
            {
                dataList.Add(reader.ReadBytes(size));
                returned += size;
            }
            else
            {
                for (int i = 0; i < Header.MipMapCount; i++)
                {
                    returned += size;
                    dataList.Add(reader.ReadBytes(size));

                    width = width / 2 == 0 ? 1 : width / 2;
                    height = height / 2 == 0 ? 1 : height / 2;

                    if (temp != AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC.NONE)
                        size = Convert.ToInt32(Math.Ceiling((double)width / 4) *
                            Math.Ceiling((double)height / 4) *
                            BitPerBlock);
                    else
                        size = width * height * BitPerBlock / 8;
                }
            }

            return returned;
        }

        #region IGameFormat

        public FormatEnum Type => FormatEnum.DDS;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize() => Header.Size + 4 + dataList.Sum(x => x.Length) + LastBlock.Length;

        public byte[] GetData()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                writer.Write(Header.Get());
                foreach (var tex in dataList)
                {
                }
                dataList.ForEach(x => writer.Write(x));
                writer.Write(LastBlock);

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion

        #region IImage

        public Bitmap GetBitmap()
        {
            if (bitmap == null)
            {
                var ddsPF = DDSHelper.ConvertFromDDSAtlus(Header.PixelFormat);

                if (ddsPF == DDSFourCC.NONE)
                {
                    var PF = DDSHelper.DDSAtlusToPixelFormat(Header.PixelFormat);
                    bitmap = new Bitmap(Header.Width, Header.Height * Header.TileCount, PF, dataList[0], null);
                }
                else
                {
                    DDSDecompressor.DDSDecompress(Header.Width, Header.Height * Header.TileCount, dataList[0], ddsPF, out byte[] newData);
                    bitmap = new Bitmap(Header.Width, Header.Height * Header.TileCount, PixelFormats.Bgra32, newData, null);
                }
            }

            return bitmap;
        }

        public void SetBitmap(Bitmap bitmap)
        {
            Header.Width = (ushort)bitmap.Width;
            Header.Height = (ushort)(bitmap.Height / Header.TileCount);

            var ddsPF = DDSHelper.ConvertFromDDSAtlus(Header.PixelFormat);

            if (ddsPF == DDSFourCC.NONE)
            {
                var PF = DDSHelper.DDSAtlusToPixelFormat(Header.PixelFormat);
                if (bitmap.PixelFormat != PF)
                    bitmap = bitmap.ConvertTo(PF, null);
                dataList[0] = bitmap.CopyData();

                if (dataList.Count > 1)
                {

                }
            }
            else
            {
                DDSCompressor.DDSCompress(bitmap, ddsPF, out byte[] newData);
                dataList[0] = newData;

                if (dataList.Count > 1)
                {
                    AuxiliaryLibraries.Media.Processing.Scale.Lanczos lanczos = new AuxiliaryLibraries.Media.Processing.Scale.Lanczos();
                    Bitmap temp = bitmap;
                    for (int i = 1; i < dataList.Count; i++)
                    {
                        temp = lanczos.imageScale(temp, 0.5f, 0.5f);
                        DDSCompressor.DDSCompress(temp.Width, temp.Height, temp.CopyData(), ddsPF, out newData);
                        dataList[i] = newData;
                    }
                }
            }

            Header.SizeTexture = dataList.Sum(x => x.Length);
            Header.SizeWOHeader = Header.SizeTexture + (LastBlock == null ? 0 : LastBlock.Length);

            this.bitmap = null;
        }

        #endregion IImage
    }
}