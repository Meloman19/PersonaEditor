using AuxiliaryLibraries.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AuxiliaryLibraries.Media.Quantization
{
    public class BruteForce
    {
        private int paletteLength;
        private Color[] palette;

        private Color[] srcColors;

        public BruteForce(Color[] srcColors, int dstPalLen)
        {
            this.srcColors = srcColors;
            paletteLength = dstPalLen;
            palette = new Color[paletteLength];
        }

        public byte[] GetQuantDat()
        {
            byte[] vs = new byte[srcColors.Length];

            for (int i = 0; i < srcColors.Length; i++)
                vs[i] = (byte)GetColorIndex(srcColors[i]);

            return vs;
        }

        public Color[] GetQuantPalette()
        {
            return palette.Copy();
        }

        public void StartQuantization()
        {
            Force();
        }

        private void Force()
        {
            Dictionary<Color, int> keyValuePairs = new Dictionary<Color, int>();

            foreach (var a in srcColors)
            {
                if (keyValuePairs.ContainsKey(a))
                    keyValuePairs[a]++;
                else
                    keyValuePairs.Add(a, 1);
            }

            var list = keyValuePairs.ToList();

            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            int end = list.Count > palette.Length ? palette.Length : list.Count;
            for (int i = 0; i < end; i++)
                palette[i] = list[i].Key;
        }

        public int GetColorIndex(Color srcColor)
        {
            double lengthColor = double.MaxValue;
            int index = 0;
            double srcAlpha = (double)srcColor.A / 255;

            for (int i = 0; i < palette.Length; i++)
            {
                double temp = 0;
                double alpha = (double)palette[i].A / 255;
                if (srcAlpha > alpha && alpha > 0)
                    temp = srcAlpha / alpha;
                else if (srcAlpha < alpha && srcAlpha > 0)
                    temp = alpha / srcAlpha;
                else
                    temp = double.MaxValue;

                double R = palette[i].R - srcColor.R;
                double G = palette[i].G - srcColor.G;
                double B = palette[i].B - srcColor.B;

                double length = Math.Sqrt(R * R + G * G + B * B) * temp;
                if (length < lengthColor)
                {
                    lengthColor = length;
                    index = i;
                }
            }

            return index;
        }
    }
}
