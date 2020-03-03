namespace AuxiliaryLibraries.Media.Formats.DDS
{
    public struct DDSHeaderPixelFormat
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
}