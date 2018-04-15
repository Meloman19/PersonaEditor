using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.FileStructure.Graphic
{
    class DDSPixelFormat
    {
        public enum PixelFormatFlags
        {
            DDPF_ALPHAPIXELS = 0x1,
            DDPF_ALPHA = 0x2,
            DDPF_FOURCC = 0x4,
            DDPF_RGB = 0x40,
            DDPF_YUV = 0x200,
            DDPF_LUMINANCE = 0x20000
        }

        public enum PixelFormatFourCC
        {
            DXT1 = 0x31545844,
            DXT3 = 0x33545844,
            DXT5 = 0x35545844
        }

        public List<PixelFormatFlags> Flags { get; set; } = new List<PixelFormatFlags>();
        public PixelFormatFourCC FourCC { get; set; }
        public int RGBBitCount { get; set; }
        public int RBitMask { get; set; }
        public int GBitMask { get; set; }
        public int BBitMask { get; set; }
        public int ABitMask { get; set; }

        public DDSPixelFormat(BinaryReader reader)
        {
            if (reader.ReadInt32() != 0x20)
                throw new Exception("DDSPixelFormat: wrong header size");

            Utilities.Utilities.ReadFlags(Flags, reader.ReadInt32(), typeof(PixelFormatFlags));
            FourCC = ReadPixelFormatFourCC(reader.ReadInt32());
            RGBBitCount = reader.ReadInt32();
            RBitMask = reader.ReadInt32();
            GBitMask = reader.ReadInt32();
            BBitMask = reader.ReadInt32();
            ABitMask = reader.ReadInt32();
        }

        public byte[] Get()
        {
            byte[] returned = null;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                writer.Write(0x20);
                writer.Write(Utilities.Utilities.WriteFlags(Flags));
                writer.Write((int)FourCC);
                writer.Write(RGBBitCount);
                writer.Write(RBitMask);
                writer.Write(GBitMask);
                writer.Write(BBitMask);
                writer.Write(ABitMask);

                returned = MS.ToArray();
            }

            return returned;
        }

        private PixelFormatFourCC ReadPixelFormatFourCC(int pixelFormat)
        {
            foreach (var a in Enum.GetValues(typeof(PixelFormatFourCC)))
                if (pixelFormat == (int)a)
                    return (PixelFormatFourCC)a;

            throw new Exception("DDSPixelFormat: FourCC read error");
        }
    }

    class DDSHeader
    {
        public int Size { get; } = 124;

        public enum HeaderFlags
        {
            DDSD_CAPS = 0x1,
            DDSD_HEIGHT = 0x2,
            DDSD_WIDTH = 0x4,
            DDSD_PITCH = 0x8,
            DDSD_PIXELFORMAT = 0x1000,
            DDSD_MIPMAPCOUNT = 0x20000,
            DDSD_LINEARSIZE = 0x80000,
            DDSD_DEPTH = 0x800000
        }

        public enum HeaderCaps
        {
            DDSCAPS_COMPLEX = 0x8,
            DDSCAPS_MIPMAP = 0x400000,
            DDSCAPS_TEXTURE = 0x1000
        }

        public enum HeaderCaps2
        {
            DDSCAPS2_CUBEMAP = 0x200,
            DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
            DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
            DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
            DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
            DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
            DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
            DDSCAPS2_VOLUME = 0x200000
        }

        public List<HeaderFlags> Flags { get; } = new List<HeaderFlags>();
        public int Height { get; }
        public int Width { get; }
        public int PitchOrLinearSize { get; }
        public int Depth { get; }
        public int MipMapCount { get; }
        public int[] Reserved { get; }
        public DDSPixelFormat DDSPixelFormat { get; }
        public List<HeaderCaps> Caps { get; } = new List<HeaderCaps>();
        public List<HeaderCaps2> Caps2 { get; } = new List<HeaderCaps2>();
        public int[] Reserved2 { get; }

        public DDSHeader(BinaryReader reader)
        {
            if (reader.ReadInt32() != Size)
                throw new Exception("DDSHeader: wrong header size");

            Utilities.Utilities.ReadFlags(Flags, reader.ReadInt32(), typeof(HeaderFlags));
            Height = reader.ReadInt32();
            Width = reader.ReadInt32();
            PitchOrLinearSize = reader.ReadInt32();
            Depth = reader.ReadInt32();
            MipMapCount = reader.ReadInt32();
            Reserved = reader.ReadInt32Array(11);
            DDSPixelFormat = new DDSPixelFormat(reader);
            Utilities.Utilities.ReadFlags(Caps, reader.ReadInt32(), typeof(HeaderCaps));
            Utilities.Utilities.ReadFlags(Caps2, reader.ReadInt32(), typeof(HeaderCaps2));
            Reserved2 = reader.ReadInt32Array(3);
        }

        public byte[] Get()
        {
            byte[] returned = null;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                writer.Write(Size);
                writer.Write(Utilities.Utilities.WriteFlags(Flags));
                writer.Write(Height);
                writer.Write(Width);
                writer.Write(PitchOrLinearSize);
                writer.Write(Depth);
                writer.Write(MipMapCount);
                writer.WriteInt32Array(Reserved);
                writer.Write(DDSPixelFormat.Get());
                writer.Write(Utilities.Utilities.WriteFlags(Caps));
                writer.Write(Utilities.Utilities.WriteFlags(Caps2));
                writer.WriteInt32Array(Reserved2);

                returned = MS.ToArray();
            }

            return returned;
        }
    }

    public class DDS : IPersonaFile, IImage
    {
        const int MagicNumber = 0x20534444;

        DDSHeader Header;

        List<byte[]> data = new List<byte[]>();
        List<BitmapSource> image = new List<BitmapSource>();

        public DDS(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamFile(MS, MS.Length, 0));
        }

        public DDS(StreamFile streamFile)
        {
            Read(streamFile);
        }

        private void Read(StreamFile streamFile)
        {
            streamFile.Stream.Position = streamFile.Position;
            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
            {
                if (reader.ReadInt32() != MagicNumber)
                    throw new Exception("DDS: wrong Magic Number");

                Header = new DDSHeader(reader);

                int BytePerBlock = Header.DDSPixelFormat.FourCC == DDSPixelFormat.PixelFormatFourCC.DXT1 ? 8 : 16;

                int temp = 0;

                int width = Header.Width;
                int height = Header.Height;
                if (Header.MipMapCount > 0)
                    for (int i = 0; i < Header.MipMapCount; i++)
                    {
                        int size = (width / 4 == 0 ? 1 : width / 4) *
                            (height / 4 == 0 ? 1 : height / 4) * BytePerBlock;

                        temp += size;
                        data.Add(reader.ReadBytes(size));

                        width = width / 2 == 0 ? 1 : width / 2;
                        height = height / 2 == 0 ? 1 : height / 2;
                    }
                else
                    data.Add(reader.ReadBytes(Header.PitchOrLinearSize));
            }
        }

        List<ObjectFile> SubFiles = new List<ObjectFile>();

        void CreateImages()
        {
            image.Clear();
            int width = Header.Width;
            int height = Header.Height;
            foreach (var a in data)
            {
                image.Add(CreateBitmapSource(width, height, a));
                width = width / 2 == 0 ? 1 : width / 2;
                height = height / 2 == 0 ? 1 : height / 2;
            }
        }

        BitmapSource CreateBitmapSource(int width, int height, byte[] data)
        {
            int Width = width / 4;
            int Heigth = height / 4;

            uint[,] pixels = new uint[height, width];

            int step = Header.DDSPixelFormat.FourCC == DDSPixelFormat.PixelFormatFourCC.DXT1 ? 8 : 16;

            int index = 0;
            for (int i = 0; i < Heigth; i++)
                for (int k = 0; k < Width; k++, index += step)
                    if (Header.DDSPixelFormat.FourCC == DDSPixelFormat.PixelFormatFourCC.DXT1)
                        Utilities.ImageHelper.DDS_DXT1_GetPixels(pixels, k * 4, i * 4, data.SubArray(index, step));
                    else if (Header.DDSPixelFormat.FourCC == DDSPixelFormat.PixelFormatFourCC.DXT3)
                        Utilities.ImageHelper.DDS_DXT3_GetPixels(pixels, k * 4, i * 4, data.SubArray(index, step));
                    else
                        Utilities.ImageHelper.DDS_DXT5_GetPixels(pixels, k * 4, i * 4, data.SubArray(index, step));

            byte[] uncompressed_data = null;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                foreach (var a in pixels)
                    writer.Write(a);

                uncompressed_data = MS.ToArray();
            }

            PixelFormat pixelFormat = PixelFormats.Bgra32;
            int Stride = (pixelFormat.BitsPerPixel * width + 7) / 8;
            return BitmapSource.Create(width, height, 96, 96, pixelFormat, null, uncompressed_data, Stride);
        }

        #region IPersonaFile

        public FileType Type => FileType.DDS;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size
        {
            get
            {
                return Header.Size + data.Sum(x => x.Length);
            }
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                writer.Write(MagicNumber);
                writer.Write(Header.Get());
                data.ForEach(x => writer.Write(x));

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile

        #region IImage

        public BitmapSource GetImage()
        {
            if (image.Count == 0)
                CreateImages();

            return image[0];
        }

        public void SetImage(BitmapSource bitmapSource)
        {
        }

        #endregion IImage
    }
}