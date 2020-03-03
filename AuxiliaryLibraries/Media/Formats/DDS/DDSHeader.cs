using System.Runtime.InteropServices;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    public struct DDSHeader
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
}