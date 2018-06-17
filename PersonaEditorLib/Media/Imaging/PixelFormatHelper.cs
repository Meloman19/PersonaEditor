using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PersonaEditorLib.Media.Imaging
{
    public static class PixelFormatHelper
    {
        #region Dictionary and List
        static Dictionary<PixelFormatPS2Enum, PixelBaseFormat> PS2ToBase = new Dictionary<PixelFormatPS2Enum, PixelBaseFormat>()
        {
            { PixelFormatPS2Enum.PSMT4,   PixelBaseFormat.Indexed4PS2  },
            { PixelFormatPS2Enum.PSMT8,   PixelBaseFormat.Indexed8  },
            { PixelFormatPS2Enum.PSMTC32, PixelBaseFormat.Rgba32PS2 }
        };

        static Dictionary<PixelBaseFormat, PixelFormatPS2Enum> BaseToPS2 = new Dictionary<PixelBaseFormat, PixelFormatPS2Enum>()
        {
            { PixelBaseFormat.Indexed4PS2, PixelFormatPS2Enum.PSMT4   },
            { PixelBaseFormat.Indexed8,    PixelFormatPS2Enum.PSMT8   },
            { PixelBaseFormat.Rgba32,      PixelFormatPS2Enum.PSMTC32 }
        };

        static Dictionary<PixelBaseFormat, PixelFormat> BaseToSystem = new Dictionary<PixelBaseFormat, PixelFormat>()
        {
            { PixelBaseFormat.Indexed4, PixelFormats.Indexed4 },
            { PixelBaseFormat.Indexed8, PixelFormats.Indexed8 },
            { PixelBaseFormat.Bgra32,   PixelFormats.Bgra32   }
        };

        static Dictionary<PixelFormat, PixelBaseFormat> SystemToBase = new Dictionary<PixelFormat, PixelBaseFormat>()
        {
            { PixelFormats.Indexed4, PixelBaseFormat.Indexed4 },
            { PixelFormats.Indexed8, PixelBaseFormat.Indexed8 },
            { PixelFormats.Bgra32,   PixelBaseFormat.Bgra32   }
        };

        static Dictionary<PixelFormatDDSAtlus, PixelBaseFormat> DDSAtlusToBase = new Dictionary<PixelFormatDDSAtlus, PixelBaseFormat>()
        {
            { PixelFormatDDSAtlus.DXT1,     PixelBaseFormat.DXT1     },
            { PixelFormatDDSAtlus.DXT3,     PixelBaseFormat.DXT3     },
            { PixelFormatDDSAtlus.DXT5,     PixelBaseFormat.DXT5     }
        };

        static Dictionary<PixelBaseFormat, int> BitsPerPixelConvert = new Dictionary<PixelBaseFormat, int>()
        {
            { PixelBaseFormat.Indexed4,    4  },
            { PixelBaseFormat.Indexed4PS2, 4  },
            { PixelBaseFormat.Indexed8,    8  },
            { PixelBaseFormat.Bgra32,      32 },
            { PixelBaseFormat.Rgba32,      32 },
            { PixelBaseFormat.Rgba32PS2,   32 },
            { PixelBaseFormat.Argb32,      32 }
        };

        static Dictionary<PixelBaseFormat, PixelFormat> CompatibilityList = new Dictionary<PixelBaseFormat, PixelFormat>()
        {
            { PixelBaseFormat.Indexed4,    PixelFormats.Indexed4 },
            { PixelBaseFormat.Indexed4PS2, PixelFormats.Indexed4 },
            { PixelBaseFormat.Indexed8,    PixelFormats.Indexed8 },
            { PixelBaseFormat.Argb32,      PixelFormats.Bgra32   }
        };

        static List<PixelBaseFormat> IndexedList = new List<PixelBaseFormat>()
        {
            PixelBaseFormat.Indexed4,
            PixelBaseFormat.Indexed4PS2,
            PixelBaseFormat.Indexed8
        };

        static List<PixelBaseFormat> CompressedList = new List<PixelBaseFormat>()
        {
            PixelBaseFormat.DXT1,
            PixelBaseFormat.DXT3,
            PixelBaseFormat.DXT5
        };
        #endregion Dictionary and List

        public static PixelBaseFormat ConvertFromPS2(PixelFormatPS2Enum pixelFormat)
        {
            if (PS2ToBase.ContainsKey(pixelFormat))
                return PS2ToBase[pixelFormat];
            else
                throw new System.Exception("FormatConvert: (ConvertFromPS2) Unknown PS2PixelFormat\npixelFormat=" + pixelFormat.ToString());
        }

        public static PixelFormatPS2Enum ConvertToPS2(PixelBaseFormat pixelBaseFormat)
        {
            if (PS2ToBase.ContainsValue(pixelBaseFormat))
                return PS2ToBase.First(x => x.Value == pixelBaseFormat).Key;
            else
                throw new System.Exception("FormatConvert: (ConvertToPS2) Unknown PS2PixelFormat\npixelBaseFormat=" + pixelBaseFormat.ToString());
        }

        public static PixelBaseFormat ConvertFromSystem(PixelFormat pixelFormat)
        {
            if (SystemToBase.ContainsKey(pixelFormat))
                return SystemToBase[pixelFormat];
            else
                return PixelBaseFormat.Unknown;
        }

        public static PixelFormat ConvertToSystem(PixelBaseFormat pixelBaseFormat)
        {
            if (BaseToSystem.ContainsKey(pixelBaseFormat))
                return BaseToSystem[pixelBaseFormat];
            else
                return PixelFormats.Default;
        }

        public static PixelBaseFormat ConvertFromDDS(FileStructure.Graphic.PixelFormatFlags pixelFormatFlags, FileStructure.Graphic.DDSFourCC fourCC)
        {
            if (pixelFormatFlags == FileStructure.Graphic.PixelFormatFlags.DDPF_FOURCC)
                switch (fourCC)
                {
                    case FileStructure.Graphic.DDSFourCC.DXT1:
                        return PixelBaseFormat.DXT1;
                    case FileStructure.Graphic.DDSFourCC.DXT3:
                        return PixelBaseFormat.DXT3;
                    case FileStructure.Graphic.DDSFourCC.DXT5:
                        return PixelBaseFormat.DXT5;
                    default:
                        return PixelBaseFormat.Unknown;
                }
            if (pixelFormatFlags == FileStructure.Graphic.PixelFormatFlags.DDPF_RGBA)
                return PixelBaseFormat.Bgra32;
            return PixelBaseFormat.Unknown;
        }

        public static PixelBaseFormat ConvertFromDDSAtlus(PixelFormatDDSAtlus nativePixelFormat)
        {
            if (DDSAtlusToBase.ContainsKey(nativePixelFormat))
                return DDSAtlusToBase[nativePixelFormat];
            else
                return PixelBaseFormat.Unknown;
        }

        public static PixelFormat CompatibilityFormat(PixelBaseFormat pixelBaseFormat)
        {
            if (CompatibilityList.ContainsKey(pixelBaseFormat))
                return CompatibilityList[pixelBaseFormat];
            else
                return PixelFormats.Default;
        }

        public static int BitsPerPixel(this PixelBaseFormat pixelBaseFormat)
        {
            if (BitsPerPixelConvert.ContainsKey(pixelBaseFormat))
                return BitsPerPixelConvert[pixelBaseFormat];
            else
                return 0;
        }

        public static bool IsIndexed(this PixelBaseFormat pixelBaseFormat) => IndexedList.Contains(pixelBaseFormat);

        public static bool IsCompressed(this PixelBaseFormat pixelBaseFormat) => CompressedList.Contains(pixelBaseFormat);

    }
}