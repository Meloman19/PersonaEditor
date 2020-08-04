using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media.Quantization
{
    // fork https://github.com/JeremyAnsel/JeremyAnsel.ColorQuant
    // Licensed under the MIT license.

    public sealed class WuAlphaColorQuantizer : IQuantization
    {
        #region Nested Class

        /// <summary>
        /// A box color cube.
        /// </summary>
        private sealed class Box
        {
            /// <summary>
            /// Gets or sets the min red value, exclusive.
            /// </summary>
            public int R0 { get; set; }

            /// <summary>
            /// Gets or sets the max red value, inclusive.
            /// </summary>
            public int R1 { get; set; }

            /// <summary>
            /// Gets or sets the min green value, exclusive.
            /// </summary>
            public int G0 { get; set; }

            /// <summary>
            /// Gets or sets the max green value, inclusive.
            /// </summary>
            public int G1 { get; set; }

            /// <summary>
            /// Gets or sets the min blue value, exclusive.
            /// </summary>
            public int B0 { get; set; }

            /// <summary>
            /// Gets or sets the max blue value, inclusive.
            /// </summary>
            public int B1 { get; set; }

            /// <summary>
            /// Gets or sets the min alpha value, exclusive.
            /// </summary>
            public int A0 { get; set; }

            /// <summary>
            /// Gets or sets the max alpha value, inclusive.
            /// </summary>
            public int A1 { get; set; }

            /// <summary>
            /// Gets or sets the volume.
            /// </summary>
            public int Volume { get; set; }
        }

        #endregion

        #region Fields & Consts

        /// <summary>
        /// The index bits.
        /// </summary>
        private const int IndexBits = 6;

        /// <summary>
        /// The index alpha bits.
        /// </summary>
        private const int IndexAlphaBits = 4;

        /// <summary>
        /// The index count.
        /// </summary>
        private const int IndexCount = (1 << WuAlphaColorQuantizer.IndexBits) + 1;

        /// <summary>
        /// The index alpha count.
        /// </summary>
        private const int IndexAlphaCount = (1 << WuAlphaColorQuantizer.IndexAlphaBits) + 1;

        /// <summary>
        /// The table length.
        /// </summary>
        private const int TableLength = WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount;

        /// <summary>
        /// Moment of <c>P(c)</c>.
        /// </summary>
        private readonly long[] vwt = new long[WuAlphaColorQuantizer.TableLength];

        /// <summary>
        /// Moment of <c>r*P(c)</c>.
        /// </summary>
        private readonly long[] vmr = new long[WuAlphaColorQuantizer.TableLength];

        /// <summary>
        /// Moment of <c>g*P(c)</c>.
        /// </summary>
        private readonly long[] vmg = new long[WuAlphaColorQuantizer.TableLength];

        /// <summary>
        /// Moment of <c>b*P(c)</c>.
        /// </summary>
        private readonly long[] vmb = new long[WuAlphaColorQuantizer.TableLength];

        /// <summary>
        /// Moment of <c>a*P(c)</c>.
        /// </summary>
        private readonly long[] vma = new long[WuAlphaColorQuantizer.TableLength];

        /// <summary>
        /// Moment of <c>c^2*P(c)</c>.
        /// </summary>
        private readonly double[] m2 = new double[WuAlphaColorQuantizer.TableLength];

        /// <summary>
        /// Color space tag.
        /// </summary>
        private readonly byte[] tag = new byte[WuAlphaColorQuantizer.TableLength];

        #endregion

        public Color[] QuantPalette { get; private set; }

        public byte[] QuantData { get; private set; }

        public bool StartQuantization(Color[] image, int colorCount)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (colorCount < 1 || colorCount > 256)
            {
                throw new ArgumentOutOfRangeException(nameof(colorCount));
            }

            this.Clear();

            this.Build3DHistogram(image);
            this.Get3DMoments();

            Box[] cube;
            var realColorCount = colorCount;
            this.BuildCube(out cube, ref realColorCount);
            this.GenerateResult(image, colorCount, cube);

            return true;
        }

        /// <summary>
        /// Gets an index.
        /// </summary>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        /// <param name="a">The alpha value.</param>
        /// <returns>The index.</returns>
        private static int GetIndex(int r, int g, int b, int a)
        {
            return (r << ((WuAlphaColorQuantizer.IndexBits * 2) + WuAlphaColorQuantizer.IndexAlphaBits))
                + (r << (WuAlphaColorQuantizer.IndexBits + WuAlphaColorQuantizer.IndexAlphaBits + 1))
                + (g << (WuAlphaColorQuantizer.IndexBits + WuAlphaColorQuantizer.IndexAlphaBits))
                + (r << (WuAlphaColorQuantizer.IndexBits * 2))
                + (r << (WuAlphaColorQuantizer.IndexBits + 1))
                + (g << WuAlphaColorQuantizer.IndexBits)
                + ((r + g + b) << WuAlphaColorQuantizer.IndexAlphaBits)
                + r + g + b + a;
        }

        /// <summary>
        /// Computes sum over a box of any given statistic.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="moment">The moment.</param>
        /// <returns>The result.</returns>
        private static double Volume(Box cube, long[] moment)
        {
            return moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B1, cube.A1)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B1, cube.A0)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A1)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A1)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A1)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A0)];
        }

        /// <summary>
        /// Computes part of Volume(cube, moment) that doesn't depend on r1, g1, or b1 (depending on direction).
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="moment">The moment.</param>
        /// <returns>The result.</returns>
        private static long Bottom(Box cube, int direction, long[] moment)
        {
            switch (direction)
            {
                // Red
                case 3:
                    return -moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                // Green
                case 2:
                    return -moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                // Blue
                case 1:
                    return -moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                // Alpha
                case 0:
                    return -moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B1, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        /// <summary>
        /// Computes remainder of Volume(cube, moment), substituting position for r1, g1, or b1 (depending on direction).
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="position">The position.</param>
        /// <param name="moment">The moment.</param>
        /// <returns>The result.</returns>
        private static long Top(Box cube, int direction, int position, long[] moment)
        {
            switch (direction)
            {
                // Red
                case 3:
                    return moment[WuAlphaColorQuantizer.GetIndex(position, cube.G1, cube.B1, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(position, cube.G1, cube.B1, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(position, cube.G1, cube.B0, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(position, cube.G1, cube.B0, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(position, cube.G0, cube.B1, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(position, cube.G0, cube.B1, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(position, cube.G0, cube.B0, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(position, cube.G0, cube.B0, cube.A0)];

                // Green
                case 2:
                    return moment[WuAlphaColorQuantizer.GetIndex(cube.R1, position, cube.B1, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, position, cube.B1, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, position, cube.B0, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, position, cube.B0, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, position, cube.B1, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, position, cube.B1, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, position, cube.B0, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, position, cube.B0, cube.A0)];

                // Blue
                case 1:
                    return moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, position, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, position, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, position, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, position, cube.A0)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, position, cube.A1)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, position, cube.A0)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, position, cube.A1)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, position, cube.A0)];

                // Alpha
                case 0:
                    return moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B1, position)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, position)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, position)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, position)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, position)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, position)]
                        + moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, position)]
                        - moment[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, position)];

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        /// <summary>
        /// Clears the tables.
        /// </summary>
        private void Clear()
        {
            Array.Clear(this.vwt, 0, WuAlphaColorQuantizer.TableLength);
            Array.Clear(this.vmr, 0, WuAlphaColorQuantizer.TableLength);
            Array.Clear(this.vmg, 0, WuAlphaColorQuantizer.TableLength);
            Array.Clear(this.vmb, 0, WuAlphaColorQuantizer.TableLength);
            Array.Clear(this.vma, 0, WuAlphaColorQuantizer.TableLength);
            Array.Clear(this.m2, 0, WuAlphaColorQuantizer.TableLength);

            Array.Clear(this.tag, 0, WuAlphaColorQuantizer.TableLength);
        }

        /// <summary>
        /// Builds a 3-D color histogram of <c>counts, r/g/b, c^2</c>.
        /// </summary>
        /// <param name="image">The image.</param>
        private void Build3DHistogram(Color[] image)
        {
            foreach (var color in image)
            {
                int a = color.A;
                int r = color.R;
                int g = color.G;
                int b = color.B;

                int inr = r >> (8 - IndexBits);
                int ing = g >> (8 - IndexBits);
                int inb = b >> (8 - IndexBits);
                int ina = a >> (8 - IndexAlphaBits);

                int ind = WuAlphaColorQuantizer.GetIndex(inr + 1, ing + 1, inb + 1, ina + 1);

                this.vwt[ind]++;
                this.vmr[ind] += r;
                this.vmg[ind] += g;
                this.vmb[ind] += b;
                this.vma[ind] += a;
                this.m2[ind] += (r * r) + (g * g) + (b * b) + (a * a);
            }
        }

        /// <summary>
        /// Converts the histogram into moments so that we can rapidly calculate
        /// the sums of the above quantities over any desired box.
        /// </summary>
        private void Get3DMoments()
        {
            long[] volume = new long[WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount];
            long[] volumeR = new long[WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount];
            long[] volumeG = new long[WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount];
            long[] volumeB = new long[WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount];
            long[] volumeA = new long[WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount];
            double[] volume2 = new double[WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount];

            long[] area = new long[WuAlphaColorQuantizer.IndexAlphaCount];
            long[] areaR = new long[WuAlphaColorQuantizer.IndexAlphaCount];
            long[] areaG = new long[WuAlphaColorQuantizer.IndexAlphaCount];
            long[] areaB = new long[WuAlphaColorQuantizer.IndexAlphaCount];
            long[] areaA = new long[WuAlphaColorQuantizer.IndexAlphaCount];
            double[] area2 = new double[WuAlphaColorQuantizer.IndexAlphaCount];

            for (int r = 1; r < WuAlphaColorQuantizer.IndexCount; r++)
            {
                Array.Clear(volume, 0, WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount);
                Array.Clear(volumeR, 0, WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount);
                Array.Clear(volumeG, 0, WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount);
                Array.Clear(volumeB, 0, WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount);
                Array.Clear(volumeA, 0, WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount);
                Array.Clear(volume2, 0, WuAlphaColorQuantizer.IndexCount * WuAlphaColorQuantizer.IndexAlphaCount);

                for (int g = 1; g < WuAlphaColorQuantizer.IndexCount; g++)
                {
                    Array.Clear(area, 0, WuAlphaColorQuantizer.IndexAlphaCount);
                    Array.Clear(areaR, 0, WuAlphaColorQuantizer.IndexAlphaCount);
                    Array.Clear(areaG, 0, WuAlphaColorQuantizer.IndexAlphaCount);
                    Array.Clear(areaB, 0, WuAlphaColorQuantizer.IndexAlphaCount);
                    Array.Clear(areaA, 0, WuAlphaColorQuantizer.IndexAlphaCount);
                    Array.Clear(area2, 0, WuAlphaColorQuantizer.IndexAlphaCount);

                    for (int b = 1; b < WuAlphaColorQuantizer.IndexCount; b++)
                    {
                        long line = 0;
                        long lineR = 0;
                        long lineG = 0;
                        long lineB = 0;
                        long lineA = 0;
                        double line2 = 0;

                        for (int a = 1; a < WuAlphaColorQuantizer.IndexAlphaCount; a++)
                        {
                            int ind1 = WuAlphaColorQuantizer.GetIndex(r, g, b, a);

                            line += this.vwt[ind1];
                            lineR += this.vmr[ind1];
                            lineG += this.vmg[ind1];
                            lineB += this.vmb[ind1];
                            lineA += this.vma[ind1];
                            line2 += this.m2[ind1];

                            area[a] += line;
                            areaR[a] += lineR;
                            areaG[a] += lineG;
                            areaB[a] += lineB;
                            areaA[a] += lineA;
                            area2[a] += line2;

                            int inv = (b * WuAlphaColorQuantizer.IndexAlphaCount) + a;

                            volume[inv] += area[a];
                            volumeR[inv] += areaR[a];
                            volumeG[inv] += areaG[a];
                            volumeB[inv] += areaB[a];
                            volumeA[inv] += areaA[a];
                            volume2[inv] += area2[a];

                            int ind2 = ind1 - WuAlphaColorQuantizer.GetIndex(1, 0, 0, 0);

                            this.vwt[ind1] = this.vwt[ind2] + volume[inv];
                            this.vmr[ind1] = this.vmr[ind2] + volumeR[inv];
                            this.vmg[ind1] = this.vmg[ind2] + volumeG[inv];
                            this.vmb[ind1] = this.vmb[ind2] + volumeB[inv];
                            this.vma[ind1] = this.vma[ind2] + volumeA[inv];
                            this.m2[ind1] = this.m2[ind2] + volume2[inv];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes the weighted variance of a box.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <returns>The result.</returns>
        private double Variance(Box cube)
        {
            double dr = WuAlphaColorQuantizer.Volume(cube, this.vmr);
            double dg = WuAlphaColorQuantizer.Volume(cube, this.vmg);
            double db = WuAlphaColorQuantizer.Volume(cube, this.vmb);
            double da = WuAlphaColorQuantizer.Volume(cube, this.vma);

            double xx =
                this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B1, cube.A1)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B1, cube.A0)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A1)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G1, cube.B0, cube.A0)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A1)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B1, cube.A0)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A1)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R1, cube.G0, cube.B0, cube.A0)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A1)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B1, cube.A0)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A1)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G1, cube.B0, cube.A0)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A1)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B1, cube.A0)]
                - this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A1)]
                + this.m2[WuAlphaColorQuantizer.GetIndex(cube.R0, cube.G0, cube.B0, cube.A0)];

            return xx - (((dr * dr) + (dg * dg) + (db * db) + (da * da)) / WuAlphaColorQuantizer.Volume(cube, this.vwt));
        }

        /// <summary>
        /// We want to minimize the sum of the variances of two sub-boxes.
        /// The sum(c^2) terms can be ignored since their sum over both sub-boxes
        /// is the same (the sum for the whole box) no matter where we split.
        /// The remaining terms have a minus sign in the variance formula,
        /// so we drop the minus sign and maximize the sum of the two terms.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="first">The first position.</param>
        /// <param name="last">The last position.</param>
        /// <param name="cut">The cutting point.</param>
        /// <param name="wholeR">The whole red.</param>
        /// <param name="wholeG">The whole green.</param>
        /// <param name="wholeB">The whole blue.</param>
        /// <param name="wholeA">The whole alpha.</param>
        /// <param name="wholeW">The whole weight.</param>
        /// <returns>The result.</returns>
        private double Maximize(Box cube, int direction, int first, int last, out int cut, double wholeR, double wholeG, double wholeB, double wholeA, double wholeW)
        {
            long baseR = WuAlphaColorQuantizer.Bottom(cube, direction, this.vmr);
            long baseG = WuAlphaColorQuantizer.Bottom(cube, direction, this.vmg);
            long baseB = WuAlphaColorQuantizer.Bottom(cube, direction, this.vmb);
            long baseA = WuAlphaColorQuantizer.Bottom(cube, direction, this.vma);
            long baseW = WuAlphaColorQuantizer.Bottom(cube, direction, this.vwt);

            double max = 0.0;
            cut = -1;

            for (int i = first; i < last; i++)
            {
                double halfR = baseR + WuAlphaColorQuantizer.Top(cube, direction, i, this.vmr);
                double halfG = baseG + WuAlphaColorQuantizer.Top(cube, direction, i, this.vmg);
                double halfB = baseB + WuAlphaColorQuantizer.Top(cube, direction, i, this.vmb);
                double halfA = baseA + WuAlphaColorQuantizer.Top(cube, direction, i, this.vma);
                double halfW = baseW + WuAlphaColorQuantizer.Top(cube, direction, i, this.vwt);

                if (halfW == 0)
                {
                    continue;
                }

                double temp = ((halfR * halfR) + (halfG * halfG) + (halfB * halfB) + (halfA * halfA)) / halfW;

                halfR = wholeR - halfR;
                halfG = wholeG - halfG;
                halfB = wholeB - halfB;
                halfA = wholeA - halfA;
                halfW = wholeW - halfW;

                if (halfW == 0)
                {
                    continue;
                }

                temp += ((halfR * halfR) + (halfG * halfG) + (halfB * halfB) + (halfA * halfA)) / halfW;

                if (temp > max)
                {
                    max = temp;
                    cut = i;
                }
            }

            return max;
        }

        /// <summary>
        /// Cuts a box.
        /// </summary>
        /// <param name="set1">The first set.</param>
        /// <param name="set2">The second set.</param>
        /// <returns>Returns a value indicating whether the box has been split.</returns>
        private bool Cut(Box set1, Box set2)
        {
            double wholeR = WuAlphaColorQuantizer.Volume(set1, this.vmr);
            double wholeG = WuAlphaColorQuantizer.Volume(set1, this.vmg);
            double wholeB = WuAlphaColorQuantizer.Volume(set1, this.vmb);
            double wholeA = WuAlphaColorQuantizer.Volume(set1, this.vma);
            double wholeW = WuAlphaColorQuantizer.Volume(set1, this.vwt);

            int cutr;
            int cutg;
            int cutb;
            int cuta;

            double maxr = this.Maximize(set1, 3, set1.R0 + 1, set1.R1, out cutr, wholeR, wholeG, wholeB, wholeA, wholeW);
            double maxg = this.Maximize(set1, 2, set1.G0 + 1, set1.G1, out cutg, wholeR, wholeG, wholeB, wholeA, wholeW);
            double maxb = this.Maximize(set1, 1, set1.B0 + 1, set1.B1, out cutb, wholeR, wholeG, wholeB, wholeA, wholeW);
            double maxa = this.Maximize(set1, 0, set1.A0 + 1, set1.A1, out cuta, wholeR, wholeG, wholeB, wholeA, wholeW);

            int dir;

            if ((maxr >= maxg) && (maxr >= maxb) && (maxr >= maxa))
            {
                dir = 3;

                if (cutr < 0)
                {
                    return false;
                }
            }
            else if ((maxg >= maxr) && (maxg >= maxb) && (maxg >= maxa))
            {
                dir = 2;
            }
            else if ((maxb >= maxr) && (maxb >= maxg) && (maxb >= maxa))
            {
                dir = 1;
            }
            else
            {
                dir = 0;
            }

            set2.R1 = set1.R1;
            set2.G1 = set1.G1;
            set2.B1 = set1.B1;
            set2.A1 = set1.A1;

            switch (dir)
            {
                // Red
                case 3:
                    set2.R0 = set1.R1 = cutr;
                    set2.G0 = set1.G0;
                    set2.B0 = set1.B0;
                    set2.A0 = set1.A0;
                    break;

                // Green
                case 2:
                    set2.G0 = set1.G1 = cutg;
                    set2.R0 = set1.R0;
                    set2.B0 = set1.B0;
                    set2.A0 = set1.A0;
                    break;

                // Blue
                case 1:
                    set2.B0 = set1.B1 = cutb;
                    set2.R0 = set1.R0;
                    set2.G0 = set1.G0;
                    set2.A0 = set1.A0;
                    break;

                // Alpha
                case 0:
                    set2.A0 = set1.A1 = cuta;
                    set2.R0 = set1.R0;
                    set2.G0 = set1.G0;
                    set2.B0 = set1.B0;
                    break;
            }

            set1.Volume = (set1.R1 - set1.R0) * (set1.G1 - set1.G0) * (set1.B1 - set1.B0) * (set1.A1 - set1.A0);
            set2.Volume = (set2.R1 - set2.R0) * (set2.G1 - set2.G0) * (set2.B1 - set2.B0) * (set2.A1 - set2.A0);

            return true;
        }

        /// <summary>
        /// Marks a color space tag.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="label">A label.</param>
        private void Mark(Box cube, byte label)
        {
            for (int r = cube.R0 + 1; r <= cube.R1; r++)
            {
                for (int g = cube.G0 + 1; g <= cube.G1; g++)
                {
                    for (int b = cube.B0 + 1; b <= cube.B1; b++)
                    {
                        for (int a = cube.A0 + 1; a <= cube.A1; a++)
                        {
                            this.tag[WuAlphaColorQuantizer.GetIndex(r, g, b, a)] = label;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the cube.
        /// </summary>
        /// <param name="cube">The cube.</param>
        /// <param name="colorCount">The color count.</param>
        private void BuildCube(out Box[] cube, ref int colorCount)
        {
            cube = new Box[colorCount];
            double[] vv = new double[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                cube[i] = new Box();
            }

            cube[0].R0 = cube[0].G0 = cube[0].B0 = cube[0].A0 = 0;
            cube[0].R1 = cube[0].G1 = cube[0].B1 = WuAlphaColorQuantizer.IndexCount - 1;
            cube[0].A1 = WuAlphaColorQuantizer.IndexAlphaCount - 1;

            int next = 0;

            for (int i = 1; i < colorCount; i++)
            {
                if (this.Cut(cube[next], cube[i]))
                {
                    vv[next] = cube[next].Volume > 1 ? this.Variance(cube[next]) : 0.0;
                    vv[i] = cube[i].Volume > 1 ? this.Variance(cube[i]) : 0.0;
                }
                else
                {
                    vv[next] = 0.0;
                    i--;
                }

                next = 0;

                double temp = vv[0];
                for (int k = 1; k <= i; k++)
                {
                    if (vv[k] > temp)
                    {
                        temp = vv[k];
                        next = k;
                    }
                }

                if (temp <= 0.0)
                {
                    colorCount = i + 1;
                    break;
                }
            }
        }

        /// <summary>
        /// Generates the quantized result.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="colorCount">The color count.</param>
        /// <param name="cube">The cube.</param>
        /// <returns>The result.</returns>
        private void GenerateResult(Color[] image, int colorCount, Box[] cube)
        {
            List<Color> palette = new List<Color>();
            for (int k = 0; k < colorCount; k++)
            {
                this.Mark(cube[k], (byte)k);

                double weight = WuAlphaColorQuantizer.Volume(cube[k], this.vwt);

                byte A;
                byte R;
                byte G;
                byte B;
                if (weight != 0)
                {
                    A = (byte)(Volume(cube[k], this.vma) / weight);
                    R = (byte)(Volume(cube[k], this.vmr) / weight);
                    G = (byte)(Volume(cube[k], this.vmg) / weight);
                    B = (byte)(Volume(cube[k], this.vmb) / weight);
                }
                else
                {
                    A = 0xff;
                    R = 0;
                    G = 0;
                    B = 0;
                }
                palette.Add(Color.FromArgb(A, R, G, B));
            }
            QuantPalette = palette.ToArray();

            var length = colorCount == 256 ? image.Length : colorCount == 16 ? image.Length / 2 : throw new Exception("IndexedPixels: unknown pixelformat");
            var returned = new byte[length];
            for (int i = 0; i < image.Length; i++)
            {
                var color = image[i];
                int a = color.A >> (8 - IndexAlphaBits);
                int r = color.R >> (8 - IndexBits);
                int g = color.G >> (8 - IndexBits);
                int b = color.B >> (8 - IndexBits);

                int ind = GetIndex(r + 1, g + 1, b + 1, a + 1);
                var value = tag[ind];

                switch (colorCount)
                {
                    case 256:
                        returned[i] = value;
                        break;
                    case 16:
                        if (i % 2 == 0) { returned[i / 2] = (byte)(value << 4); }
                        else { returned[i / 2] |= (byte)(value & 0x0F); }
                        break;
                    case 2:
                        if (i % 8 == 0) { returned[i / 8] = (byte)(value << 7); }
                        else { returned[i / 8] |= (byte)((value & 0x01) << (7 - (i % 8))); }
                        break;
                }
            }
            QuantData = returned;
        }
    }
}