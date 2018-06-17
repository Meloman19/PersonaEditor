using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.Media.Imaging
{
    public enum PixelBaseFormat
    {
        Unknown = 0,
        Indexed4 = 4,
        Indexed4PS2 = 5,
        Indexed8 = 8,
        Bgra32 = 10,
        Rgba32,
        Rgba32PS2,
        Argb32,
        DXT1,
        DXT3,
        DXT5
    }
}