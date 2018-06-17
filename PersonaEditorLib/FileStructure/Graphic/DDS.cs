using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;
using PersonaEditorLib.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PersonaEditorLib.Media.Imaging;
using System.Runtime.InteropServices;

namespace PersonaEditorLib.FileStructure.Graphic
{
    public enum DDSFourCC
    {
        NONE = 0,
        DXT1 = 0x31545844,
        DXT3 = 0x33545844,
        DXT5 = 0x35545844
    }

    [FlagsAttribute]
    public enum PixelFormatFlags : uint
    {
        DDPF_RGBA = 0x41,         // RGBA use (DDPF_ALPHAPIXELS | DDPF_RGB)
        DDPF_ALPHAPIXELS = 0x1,   // Alpha Channel use
        DDPF_ALPHA = 0x2,
        DDPF_FOURCC = 0x4,        // FOURCC use
        DDPF_RGB = 0x40,          // RGB use
        DDPF_YUV = 0x200,
        DDPF_LUMINANCE = 0x20000
    }

    [FlagsAttribute]
    public enum HeaderFlags : uint
    {
        DDSD_NONE = 0x0,
        DDSD_CAPS = 0x1,
        DDSD_HEIGHT = 0x2,
        DDSD_WIDTH = 0x4,
        DDSD_PITCH = 0x8,
        DDSD_PIXELFORMAT = 0x1000,
        DDSD_MIPMAPCOUNT = 0x20000,
        DDSD_LINEARSIZE = 0x80000,
        DDSD_DEPTH = 0x800000
    }

    [FlagsAttribute]
    public enum HeaderCaps : uint
    {
        DDSCAPS_NONE = 0x0,
        DDSCAPS_ALPHA = 0x2,
        DDSCAPS_COMPLEX = 0x8,
        DDSCAPS_MIPMAP = 0x400000,
        DDSCAPS_TEXTURE = 0x1000
    }

    [FlagsAttribute]
    public enum HeaderCaps2 : uint
    {
        DDSCAPS2_NONE = 0x0,
        DDSCAPS2_CUBEMAP = 0x200,
        DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
        DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
        DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
        DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
        DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
        DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
        DDSCAPS2_VOLUME = 0x200000
    }

    struct DDSHeaderPixelFormat
    {
        public uint PixelHeaderSize;
        public PixelFormatFlags PixelFlags;
        public DDSFourCC FourCC;
        public int RGBBitCount;
        public uint RBitMask;
        public uint GBitMask;
        public uint BBitMask;
        public uint ABitMask;
    };

    struct DDSHeader
    {
        public int HeaderSize;
        public HeaderFlags HeaderFlags;
        public int Height;
        public int Width;
        public int PitchOrLinearSize;
        public int Depth;
        public int MipMapCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public int[] Reserved;
        public DDSHeaderPixelFormat PixelFormat;
        public HeaderCaps CapsFlags;
        public HeaderCaps2 Caps2Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] Reserved2;
    }

    public class DDS : IPersonaFile, IImage
    {
        public static byte[] MagicNumber { get; } = new byte[] { 0x44, 0x44, 0x53, 0x20 };

        private DDSHeader Header;
        private List<ImageBase> dataList = new List<ImageBase>();
        private BitmapSource bitmapSource = null;

        public int Width => Header.Width;
        public int Height => Header.Height;

        public DDS(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, MS.Length, 0));
        }

        public DDS(StreamPart streamFile)
        {
            Read(streamFile);
        }

        private void Read(StreamPart streamFile)
        {
            streamFile.Stream.Position = streamFile.Position;

            byte[] magicNumberArray = new byte[4];
            streamFile.Stream.Read(magicNumberArray, 0, 4);

            if (magicNumberArray.SequenceEqual(MagicNumber))
                using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
                {
                    Header = UtilitiesTool.fromBytes<DDSHeader>(reader.ReadBytes(124));

                    int temp = 0;

                    temp += ReadTexture(reader);
                }
            else
                throw new Exception("DDS: wrong Magic Number");
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

                    dataList.Add(new ImageBase(width, height, PixelFormatHelper.ConvertFromDDS(Header.PixelFormat.PixelFlags, Header.PixelFormat.FourCC), reader.ReadBytes(size)));

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
                dataList.Add(new ImageBase(width, height, PixelFormatHelper.ConvertFromDDS(Header.PixelFormat.PixelFlags, Header.PixelFormat.FourCC), reader.ReadBytes(size)));

            return temp;
        }

        #region IPersonaFile

        public FileType Type => FileType.DDS;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public ReadOnlyObservableCollection<PropertyClass> GetProperties => null;

        #endregion IPersonaFile

        #region IFile

        public int Size() => Header.HeaderSize + 4 + dataList.Sum(x => x.LengthData);

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                MS.Write(MagicNumber, 0, 4);
                writer.Write(UtilitiesTool.getBytes(Header));
                dataList.ForEach(x => writer.Write(x.GetImageData()));

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

            Header.Width = bitmapSource.PixelWidth;
            Header.Height = bitmapSource.PixelHeight;

            var image = new ImageBaseConverter(bitmapSource);
            if (!image.TryConvert(PixelFormatHelper.ConvertFromDDS(Header.PixelFormat.PixelFlags, Header.PixelFormat.FourCC)))
            {

            }
            dataList[0] = image;
            Header.PitchOrLinearSize = image.LengthData;

            for (int i = 1; i < dataList.Count; i++)
            {
                var scale = Math.Pow(0.5, i);
                ScaleTransform scaleTransform = new ScaleTransform(scale, scale);
                TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapSource, scaleTransform);

                image = new ImageBaseConverter(transformedBitmap);
                if (!image.TryConvert(PixelFormatHelper.ConvertFromDDS(Header.PixelFormat.PixelFlags, Header.PixelFormat.FourCC)))
                {

                }

                dataList[i] = image;
            }
        }

        #endregion IImage
    }
}