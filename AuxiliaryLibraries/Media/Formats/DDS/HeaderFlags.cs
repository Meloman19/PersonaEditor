using System;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    [Flags]
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
        DDSD_DEPTH = 0x800000,
        DDSD_STANDART = 0x81007 // Classic DDS (DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT | DDSD_LINEARSIZE)
    }
}