using System;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    [Flags]
    public enum HeaderCaps : uint
    {
        DDSCAPS_NONE = 0x0,
        DDSCAPS_ALPHA = 0x2,
        DDSCAPS_COMPLEX = 0x8,
        DDSCAPS_MIPMAP = 0x400000,
        DDSCAPS_TEXTURE = 0x1000
    }
}