using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    public class DDSBase
    {
        static byte[] MagicNumber { get; } = new byte[] { 0x44, 0x44, 0x53, 0x20 };

        public DDSHeader Header;
        public DDSHeaderDXT10? HeaderDXT10;
        public List<byte[]> dataList = new List<byte[]>();

        public DDSBase(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, MS.Length, 0));
        }

        private void Read(StreamPart streamFile)
        {
            streamFile.Stream.Position = streamFile.Position;

            byte[] magicNumberArray = new byte[4];
            streamFile.Stream.Read(magicNumberArray, 0, 4);

            if (magicNumberArray.SequenceEqual(MagicNumber))
                using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
                {
                    Header = reader.ReadStruct<DDSHeader>();

                    if (Header.PixelFormat.FourCC == DDSFourCC.DX10)
                        HeaderDXT10 = reader.ReadStruct<DDSHeaderDXT10>();

                    if (!IsSupportedFormat(Header, HeaderDXT10))
                        throw new Exception("DDS: not supported format");

                    int temp = 0;

                    temp += ReadTexture(reader);
                }
            else
                throw new Exception("DDS: wrong Magic Number");
        }

        private static bool IsSupportedFormat(DDSHeader header, DDSHeaderDXT10? headerDXT10)
        {
            if (!IsSupportedPixelFlags(header.PixelFormat.PixelFlags))
                return false;

            if (header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_FOURCC &&
                header.PixelFormat.FourCC == DDSFourCC.DX10)
            {
                if (!IsSupportedDX10(headerDXT10.Value))
                    return false;
            }

            return true;
        }

        private static bool IsSupportedPixelFlags(PixelFormatFlags pixelFormatFlags)
        {
            switch (pixelFormatFlags)
            {
                case PixelFormatFlags.DDPF_FOURCC:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsSupportedDX10(DDSHeaderDXT10 headerDXT10)
        {
            if (headerDXT10.ArraySize != 1)
                return false;

            if (headerDXT10.D3D10ResourceDimension != D3D10ResourceDimension.D3D10_RESOURCE_DIMENSION_TEXTURE2D)
                return false;

            switch (headerDXT10.DXGIFormat)
            {
                case DXGIFormat.DXGI_FORMAT_BC7_UNORM:
                    return true;
                default:
                    return false;
            }
        }

        private int ReadTexture(BinaryReader reader)
        {
            int temp = 0;

            int BitPerBlock = 0;
            if (Header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_FOURCC)
                BitPerBlock = Header.PixelFormat.FourCC == DDSFourCC.DXT1 ? 8 : 16;
            else if (Header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_RGBA)
                BitPerBlock = Header.PixelFormat.RGBBitCount;
            else
                return 0;

            int width = Header.Width;
            int height = Header.Height;

            int size = 0;
            if (Header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_FOURCC)
                size = Convert.ToInt32(Math.Ceiling((double)width / 4) *
                    Math.Ceiling((double)height / 4) *
                    BitPerBlock);
            else if (Header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_RGBA)
                size = width * height * BitPerBlock / 8;

            if (Header.MipMapCount > 0)
                for (int i = 0; i < Header.MipMapCount; i++)
                {
                    temp += size;

                    dataList.Add(reader.ReadBytes(size));

                    width = width / 2 == 0 ? 1 : width / 2;
                    height = height / 2 == 0 ? 1 : height / 2;

                    if (Header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_FOURCC)
                        size = Convert.ToInt32(Math.Ceiling((double)width / 4) *
                            Math.Ceiling((double)height / 4) *
                            BitPerBlock);
                    else if (Header.PixelFormat.PixelFlags == PixelFormatFlags.DDPF_RGBA)
                        size = width * height * BitPerBlock / 8;
                }
            else
                dataList.Add(reader.ReadBytes(size));

            return temp;
        }

        public int Size() => Header.HeaderSize + 4 + dataList.Sum(x => x.Length);

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                MS.Write(MagicNumber, 0, 4);
                writer.WriteStruct(Header);
                if (HeaderDXT10.HasValue)
                    writer.WriteStruct(HeaderDXT10.Value);
                dataList.ForEach(x => writer.Write(x));
                returned = MS.ToArray();
            }

            return returned;
        }

        public PixelMap GetBitmap()
        {
            Pixel[] pixels;
            switch (Header.PixelFormat.PixelFlags)
            {
                case PixelFormatFlags.DDPF_FOURCC:
                    var newData = DDSDecompressor.DDSDecompress(Header.Width, Header.Height, dataList[0], Header.PixelFormat.FourCC, HeaderDXT10?.DXGIFormat);
                    pixels = DecodingHelper.FromBgra32(newData);
                    break;
                case PixelFormatFlags.DDPF_RGBA:
                    pixels = DecodingHelper.FromRgba32(dataList[0]);
                    break;
                default:
                    throw new Exception();
            }

            return new PixelMap(Header.Width, Header.Height, pixels);
        }

        public void SetBitmap(PixelMap bitmap)
        {
            switch (Header.PixelFormat.PixelFlags)
            {
                case PixelFormatFlags.DDPF_FOURCC:
                    {
                        if (Header.PixelFormat.FourCC == DDSFourCC.DX10)
                        {
                            var pixelFormat = Header.PixelFormat;
                            pixelFormat.FourCC = DDSFourCC.DXT5;
                            Header.PixelFormat = pixelFormat;
                            HeaderDXT10 = null;
                        }

                        var newData = DDSCompressor.DDSCompress(bitmap, Header.PixelFormat.FourCC);
                        dataList[0] = newData;
                        Header.PitchOrLinearSize = newData.Length;

                        if (dataList.Count > 1)
                        {
                            LanczosScaling lanczos = new LanczosScaling();
                            var temp = bitmap;
                            for (int i = 1; i < dataList.Count; i++)
                            {
                                temp = lanczos.imageScale(temp, 0.5f, 0.5f);
                                newData = DDSCompressor.DDSCompress(temp, Header.PixelFormat.FourCC);
                                dataList[i] = newData;
                            }
                        }
                    }
                    break;
                case PixelFormatFlags.DDPF_RGBA:
                    {
                        var newData = EncodingHelper.ToRgba32(bitmap.Pixels);
                        dataList[0] = newData;
                        Header.PitchOrLinearSize = newData.Length;

                        if (dataList.Count > 1)
                        {
                            LanczosScaling lanczos = new LanczosScaling();
                            var temp = bitmap;
                            for (int i = 1; i < dataList.Count; i++)
                            {
                                temp = lanczos.imageScale(temp, 0.5f, 0.5f);
                                newData = EncodingHelper.ToRgba32(temp.Pixels);
                                dataList[i] = newData;
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception();
            }

            Header.Width = bitmap.Width;
            Header.Height = bitmap.Height;
        }
    }
}