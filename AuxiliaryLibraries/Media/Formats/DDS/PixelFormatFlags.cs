using System;

namespace AuxiliaryLibraries.Media.Formats.DDS
{
    [Flags]
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
}