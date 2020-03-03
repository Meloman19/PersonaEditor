using System;
using System.Collections.Generic;
using System.Text;

namespace AuxiliaryLibraries.Media
{
    public class PixelFormats
    {
        private static Dictionary<PixelFormatEnum, PixelFormat> privateDic = new Dictionary<PixelFormatEnum, PixelFormat>()
        {
            { PixelFormatEnum.Indexed4,        new PixelFormat(PixelFormatEnum.Indexed4)        },
            { PixelFormatEnum.Indexed4Reverse, new PixelFormat(PixelFormatEnum.Indexed4Reverse) },
            { PixelFormatEnum.Indexed8,        new PixelFormat(PixelFormatEnum.Indexed8)        },
            { PixelFormatEnum.Bgra32,          new PixelFormat(PixelFormatEnum.Bgra32)          },
            { PixelFormatEnum.Rgba32,          new PixelFormat(PixelFormatEnum.Rgba32)          },
            { PixelFormatEnum.Rgba32PS2,       new PixelFormat(PixelFormatEnum.Rgba32PS2)       },
            { PixelFormatEnum.Argb32,          new PixelFormat(PixelFormatEnum.Argb32)          },
            { PixelFormatEnum.Undefined,       new PixelFormat(PixelFormatEnum.Undefined)       }
        };

        public static PixelFormat Indexed4 => privateDic[PixelFormatEnum.Indexed4];
        public static PixelFormat Indexed4Reverse => privateDic[PixelFormatEnum.Indexed4Reverse];
        public static PixelFormat Indexed8 => privateDic[PixelFormatEnum.Indexed8];
        public static PixelFormat Bgra32 => privateDic[PixelFormatEnum.Bgra32];
        public static PixelFormat Rgba32 => privateDic[PixelFormatEnum.Rgba32];
        public static PixelFormat Rgba32PS2 => privateDic[PixelFormatEnum.Rgba32PS2];
        public static PixelFormat Argb32 => privateDic[PixelFormatEnum.Argb32];
        public static PixelFormat Undefined => privateDic[PixelFormatEnum.Undefined];
    }
}