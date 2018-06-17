using PersonaEditorLib.Media.Imaging;
using System.Runtime.InteropServices;

namespace PersonaEditorLib.FileStructure.Graphic
{
    struct TMXHeader
    {
        public int ID;
        public int FileSize;
        public uint MagicNumber;
        public int Padding;
        public byte PaletteCount;
        public PixelFormatPS2Enum PaletteFormat;
        public ushort Width;
        public ushort Height;
        public PixelFormatPS2Enum PixelFormat;
        public byte MipMapCount;
        public byte MipMapK;
        public byte MipMapL;
        public ushort WrapMode;
        public uint TextureID;
        public uint ClutID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public byte[] Comment;
    }
}