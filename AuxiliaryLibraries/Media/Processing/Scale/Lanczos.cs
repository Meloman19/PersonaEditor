using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media.Processing.Scale
{
    public class Lanczos
    {
        private int lanczosSize = 2;

        public Bitmap imageScale(Bitmap bufferedImage, float widthScale, float heightScale)
        {
            widthScale = 1 / widthScale;
            heightScale = 1 / heightScale;
            lanczosSize = widthScale > 1 ? 3 : 2;
            int srcW = bufferedImage.Width;
            int srcH = bufferedImage.Height;
            int destW = (int)(bufferedImage.Width / widthScale);
            int destH = (int)(bufferedImage.Height / heightScale);

            Color[] inPixels = bufferedImage.CopyPixels();
            Color[] outPixels = new Color[destW * destH];

            for (int col = 0; col < destW; col++)
            {
                double x = col * widthScale;
                double fx = Math.Floor(col * widthScale);
                for (int row = 0; row < destH; row++)
                {
                    double y = row * heightScale;
                    double fy = Math.Floor(y);
                    double[] argb = { 0, 0, 0, 0 };
                    int[] pargb = { 0, 0, 0, 0 };
                    double totalWeight = 0;

                    for (int subrow = (int)(fy - lanczosSize + 1); subrow <= fy + lanczosSize; subrow++)
                    {
                        if (subrow < 0 || subrow >= srcH)
                            continue;

                        for (int subcol = (int)(fx - lanczosSize + 1); subcol <= fx + lanczosSize; subcol++)
                        {
                            if (subcol < 0 || subcol >= srcW)
                                continue;

                            double weight = getLanczosFactor(x - subcol) * getLanczosFactor(y - subrow);

                            if (weight > 0)
                            {
                                int index = (subrow * srcW + subcol);

                                pargb[0] = inPixels[index].A;
                                pargb[1] = inPixels[index].R;
                                pargb[2] = inPixels[index].G;
                                pargb[3] = inPixels[index].B;

                                totalWeight += weight;
                                for (int i = 0; i < 4; i++)
                                    argb[i] += weight * pargb[i];
                            }
                        }
                    }
                    for (int i = 0; i < 4; i++)
                        pargb[i] = (int)(argb[i] / totalWeight);

                    outPixels[row * destW + col] = Color.FromArgb(
                        clamp(pargb[0]),
                        clamp(pargb[1]),
                        clamp(pargb[2]),
                        clamp(pargb[3]));
                }
            }

            return new Bitmap(destW, destH, outPixels);
        }
        private byte clamp(int v)
        {
            return (byte)(v > 255 ? 255 : (v < 0 ? 0 : v));
        }

        private double getLanczosFactor(double x)
        {
            if (x >= lanczosSize)
                return 0;
            if (Math.Abs(x) < 1e-16)
                return 1;
            x *= Math.PI;
            return Math.Sin(x) * Math.Sin(x / lanczosSize) / (x * x);
        }
    }
}
