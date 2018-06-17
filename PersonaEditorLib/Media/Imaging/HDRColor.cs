using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.Media.Imaging
{
    public struct HDRColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public HDRColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static HDRColor operator +(HDRColor a, HDRColor b)
        {
            return new HDRColor(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
        }

        public static HDRColor operator -(HDRColor a, HDRColor b)
        {
            return new HDRColor(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
        }

        public static HDRColor operator *(HDRColor a, float b)
        {
            return new HDRColor(a.R * b, a.G * b, a.B * b, a.B * b);
        }

        public static HDRColor operator /(HDRColor a, float b)
        {
            return new HDRColor(a.R / b, a.G / b, a.B / b, a.A / b);
        }


    }
}
