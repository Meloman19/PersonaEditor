using AuxiliaryLibraries.Media;
using System.Collections.Generic;

namespace PersonaEditorLib.Sprite
{
    public static class DDSHelper
    {
        static Dictionary<DDSAtlusPixelFormat, AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC> DDSAtlusToDDS = new Dictionary<DDSAtlusPixelFormat, AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC>()
        {
            { DDSAtlusPixelFormat.DXT1, AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC.DXT1 },
            { DDSAtlusPixelFormat.DXT3, AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC.DXT3 },
            { DDSAtlusPixelFormat.DXT5, AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC.DXT5 }
        };

        static Dictionary<DDSAtlusPixelFormat, PixelFormat> DDSAtlusToAux = new Dictionary<DDSAtlusPixelFormat, PixelFormat>()
        {
            { DDSAtlusPixelFormat.Argb32, PixelFormats.Argb32 }
        };

        public static AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC ConvertFromDDSAtlus(DDSAtlusPixelFormat nativePixelFormat)
        {
            if (DDSAtlusToDDS.ContainsKey(nativePixelFormat))
                return DDSAtlusToDDS[nativePixelFormat];
            else
                return AuxiliaryLibraries.Media.Formats.DDS.DDSFourCC.NONE;
        }

        public static PixelFormat DDSAtlusToPixelFormat(DDSAtlusPixelFormat nativePixelFormat)
        {
            if (DDSAtlusToAux.ContainsKey(nativePixelFormat))
                return DDSAtlusToAux[nativePixelFormat];
            else
                return PixelFormats.Undefined;
        }
    }
}