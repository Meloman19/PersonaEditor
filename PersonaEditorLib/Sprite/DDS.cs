using System.Collections.Generic;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Media.Formats.DDS;

namespace PersonaEditorLib.Sprite
{
    public class DDS : IGameData, IImage
    {
        DDSBase DDSBase = null;
        Bitmap bitmap = null;

        public static byte[] MagicNumber { get; } = new byte[] { 0x44, 0x44, 0x53, 0x20 };

        public int Width => DDSBase.Header.Width;
        public int Height => DDSBase.Header.Height;

        public DDS(byte[] data)
        {
            DDSBase = new DDSBase(data);
        }

        //public DDS(StreamPart streamFile)
        //{
        //    Read(streamFile);
        //}

        //public DDS(BitmapSource bitmapSource, DDSFourCC fourCC)
        //{
        //    dataList.Add(new ImageBase(bitmapSource));

        //    Header = new DDSHeader()
        //    {
        //        HeaderSize = 124,
        //        HeaderFlags = HeaderFlags.DDSD_STANDART,
        //        Height = bitmapSource.PixelHeight,
        //        Width = bitmapSource.PixelWidth,
        //        PitchOrLinearSize = dataList[0].LengthData,
        //        PixelFormat = new DDSHeaderPixelFormat()
        //        {
        //            PixelHeaderSize = 32,
        //            PixelFlags = PixelFormatFlags.DDPF_FOURCC,
        //            FourCC = fourCC
        //        },
        //        CapsFlags = HeaderCaps.DDSCAPS_TEXTURE
        //    };
        //}

        #region IGameFile

        public FormatEnum Type => FormatEnum.DDS;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize() => DDSBase.Size();

        public byte[] GetData() => DDSBase.Get();

        #endregion

        #region IImage

        public Bitmap GetBitmap()
        {
            if (bitmap == null)
                bitmap = DDSBase.GetBitmap();

            return bitmap;
        }

        public void SetBitmap(Bitmap bitmap)
        {
            DDSBase.SetBitmap(bitmap);
            this.bitmap = null;
        }

        #endregion IImage
    }
}