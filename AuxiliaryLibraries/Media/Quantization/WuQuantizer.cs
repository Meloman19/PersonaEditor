using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace AuxiliaryLibraries.Media.Quantization
{
    public sealed class WuQuantizer : IQuantization
    {
        #region Private Structures
        struct Box
        {
            public byte AlphaMinimum;
            public byte AlphaMaximum;
            public byte RedMinimum;
            public byte RedMaximum;
            public byte GreenMinimum;
            public byte GreenMaximum;
            public byte BlueMinimum;
            public byte BlueMaximum;
            public int Size;
        }

        struct ColorMoment
        {
            public long Alpha;
            public long Red;
            public long Green;
            public long Blue;
            public int Weight;
            public float Moment;

            public static ColorMoment operator +(ColorMoment c1, ColorMoment c2)
            {
                c1.Alpha += c2.Alpha;
                c1.Red += c2.Red;
                c1.Green += c2.Green;
                c1.Blue += c2.Blue;
                c1.Weight += c2.Weight;
                c1.Moment += c2.Moment;
                return c1;
            }

            public static ColorMoment operator -(ColorMoment c1, ColorMoment c2)
            {
                c1.Alpha -= c2.Alpha;
                c1.Red -= c2.Red;
                c1.Green -= c2.Green;
                c1.Blue -= c2.Blue;
                c1.Weight -= c2.Weight;
                c1.Moment -= c2.Moment;
                return c1;
            }

            public static ColorMoment operator -(ColorMoment c1)
            {
                c1.Alpha = -c1.Alpha;
                c1.Red = -c1.Red;
                c1.Green = -c1.Green;
                c1.Blue = -c1.Blue;
                c1.Weight = -c1.Weight;
                c1.Moment = -c1.Moment;
                return c1;
            }

            public void Add(WuPixel p)
            {
                byte pAlpha = p.Alpha;
                byte pRed = p.Red;
                byte pGreen = p.Green;
                byte pBlue = p.Blue;
                Alpha += pAlpha;
                Red += pRed;
                Green += pGreen;
                Blue += pBlue;
                Weight++;
                Moment += pAlpha * pAlpha + pRed * pRed + pGreen * pGreen + pBlue * pBlue;
            }

            public void Add(Color p)
            {
                byte pAlpha = p.A;
                byte pRed = p.R;
                byte pGreen = p.G;
                byte pBlue = p.B;
                Alpha += pAlpha;
                Red += pRed;
                Green += pGreen;
                Blue += pBlue;
                Weight++;
                Moment += pAlpha * pAlpha + pRed * pRed + pGreen * pGreen + pBlue * pBlue;
            }

            public void AddFast(ref ColorMoment c2)
            {
                Alpha += c2.Alpha;
                Red += c2.Red;
                Green += c2.Green;
                Blue += c2.Blue;
                Weight += c2.Weight;
                Moment += c2.Moment;
            }

            public long Amplitude()
            {
                return Alpha * Alpha + Red * Red + Green * Green + Blue * Blue;
            }

            public long WeightedDistance()
            {
                return Amplitude() / Weight;
            }

            public float Variance()
            {
                var result = Moment - (float)Amplitude() / Weight;
                return float.IsNaN(result) ? 0.0f : result;
            }
        }

        struct CubeCut
        {
            public readonly byte? Position;
            public readonly float Value;

            public CubeCut(byte? cutPoint, float result)
            {
                Position = cutPoint;
                Value = result;
            }
        }

        struct PaletteColorHistory
        {
            public int Alpha;
            public int Red;
            public int Green;
            public int Blue;
            public int Sum;

            public Color ToNormalizedColorWPF()
            {
                return (Sum != 0) ? Color.FromArgb((byte)(Alpha / Sum), (byte)(Red / Sum), (byte)(Green / Sum), (byte)(Blue / Sum)) : Color.Transparent;
            }

            public void AddPixel(WuPixel pixel)
            {
                Alpha += pixel.Alpha;
                Red += pixel.Red;
                Green += pixel.Green;
                Blue += pixel.Blue;
                Sum++;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        struct WuPixel
        {
            public WuPixel(byte alpha, byte red, byte green, byte blue)
                : this()
            {
                Alpha = alpha;
                Red = red;
                Green = green;
                Blue = blue;

                System.Diagnostics.Debug.Assert(Argb == (alpha << 24 | red << 16 | green << 8 | blue));
            }

            public WuPixel(int argb)
                : this()
            {
                Argb = argb;
                System.Diagnostics.Debug.Assert(Alpha == ((uint)argb >> 24));
                System.Diagnostics.Debug.Assert(Red == ((uint)(argb >> 16) & 255));
                System.Diagnostics.Debug.Assert(Green == ((uint)(argb >> 8) & 255));
                System.Diagnostics.Debug.Assert(Blue == ((uint)argb & 255));
            }

            [FieldOffsetAttribute(3)]
            public byte Alpha;
            [FieldOffsetAttribute(2)]
            public byte Red;
            [FieldOffsetAttribute(1)]
            public byte Green;
            [FieldOffsetAttribute(0)]
            public byte Blue;
            [FieldOffsetAttribute(0)]
            public int Argb;

            public override string ToString()
            {
                return string.Format("Alpha:{0} Red:{1} Green:{2} Blue:{3}", Alpha, Red, Green, Blue);
            }
        }

        #endregion

        class PaletteLookup
        {
            private int mMask;
            private Dictionary<int, LookupNode[]> mLookup;
            private readonly LookupNode[] Palette;

            public PaletteLookup(WuPixel[] palette)
            {
                Palette = new LookupNode[palette.Length];
                for (int paletteIndex = 0; paletteIndex < palette.Length; paletteIndex++)
                {
                    Palette[paletteIndex] = new LookupNode { Pixel = palette[paletteIndex], PaletteIndex = (byte)paletteIndex };
                }
                BuildLookup(palette);
            }

            public byte GetPaletteIndex(WuPixel pixel)
            {
                int pixelKey = pixel.Argb & mMask;
                LookupNode[] bucket;
                if (!mLookup.TryGetValue(pixelKey, out bucket))
                {
                    bucket = Palette;
                }

                if (bucket.Length == 1)
                {
                    return bucket[0].PaletteIndex;
                }

                int bestDistance = int.MaxValue;
                byte bestMatch = 0;
                foreach (var lookup in bucket)
                {
                    var lookupPixel = lookup.Pixel;

                    var deltaAlpha = pixel.Alpha - lookupPixel.Alpha;
                    int distance = deltaAlpha * deltaAlpha;

                    var deltaRed = pixel.Red - lookupPixel.Red;
                    distance += deltaRed * deltaRed;

                    var deltaGreen = pixel.Green - lookupPixel.Green;
                    distance += deltaGreen * deltaGreen;

                    var deltaBlue = pixel.Blue - lookupPixel.Blue;
                    distance += deltaBlue * deltaBlue;

                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    bestMatch = lookup.PaletteIndex;
                }

                if ((bucket == Palette) && (pixelKey != 0))
                {
                    mLookup[pixelKey] = new LookupNode[] { bucket[bestMatch] };
                }

                return bestMatch;
            }

            private void BuildLookup(WuPixel[] palette)
            {
                int mask = GetMask(palette);
                Dictionary<int, List<LookupNode>> tempLookup = new Dictionary<int, List<LookupNode>>();
                foreach (LookupNode lookup in Palette)
                {
                    int pixelKey = lookup.Pixel.Argb & mask;

                    List<LookupNode> bucket;
                    if (!tempLookup.TryGetValue(pixelKey, out bucket))
                    {
                        bucket = new List<LookupNode>();
                        tempLookup[pixelKey] = bucket;
                    }
                    bucket.Add(lookup);
                }

                mLookup = new Dictionary<int, LookupNode[]>(tempLookup.Count);
                foreach (var key in tempLookup.Keys)
                {
                    mLookup[key] = tempLookup[key].ToArray();
                }
                mMask = mask;
            }

            private static int GetMask(WuPixel[] palette)
            {
                IEnumerable<byte> alphas = from pixel in palette
                                           select pixel.Alpha;
                byte maxAlpha = alphas.Max();
                int uniqueAlphas = alphas.Distinct().Count();

                IEnumerable<byte> reds = from pixel in palette
                                         select pixel.Red;
                byte maxRed = reds.Max();
                int uniqueReds = reds.Distinct().Count();

                IEnumerable<byte> greens = from pixel in palette
                                           select pixel.Green;
                byte maxGreen = greens.Max();
                int uniqueGreens = greens.Distinct().Count();

                IEnumerable<byte> blues = from pixel in palette
                                          select pixel.Blue;
                byte maxBlue = blues.Max();
                int uniqueBlues = blues.Distinct().Count();

                double totalUniques = uniqueAlphas + uniqueReds + uniqueGreens + uniqueBlues;

                double AvailableBits = 1.0 + Math.Log(uniqueAlphas * uniqueReds * uniqueGreens * uniqueBlues);

                byte alphaMask = ComputeBitMask(maxAlpha, Convert.ToInt32(Math.Round(uniqueAlphas / totalUniques * AvailableBits)));
                byte redMask = ComputeBitMask(maxRed, Convert.ToInt32(Math.Round(uniqueReds / totalUniques * AvailableBits)));
                byte greenMask = ComputeBitMask(maxGreen, Convert.ToInt32(Math.Round(uniqueGreens / totalUniques * AvailableBits)));
                byte blueMask = ComputeBitMask(maxAlpha, Convert.ToInt32(Math.Round(uniqueBlues / totalUniques * AvailableBits)));

                WuPixel maskedPixel = new WuPixel(alphaMask, redMask, greenMask, blueMask);

                return maskedPixel.Argb;
            }

            private static byte ComputeBitMask(byte max, int bits)
            {
                byte mask = 0;

                if (bits != 0)
                {
                    byte highestSetBitIndex = HighestSetBitIndex(max);


                    for (int i = 0; i < bits; i++)
                    {
                        mask <<= 1;
                        mask++;
                    }

                    for (int i = 0; i <= highestSetBitIndex - bits; i++)
                    {
                        mask <<= 1;
                    }
                }
                return mask;
            }

            private static byte HighestSetBitIndex(byte value)
            {
                byte index = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (0 != (value & 1))
                    {
                        index = (byte)i;
                    }
                    value >>= 1;
                }
                return index;
            }

            private struct LookupNode
            {
                public WuPixel Pixel;
                public byte PaletteIndex;
            }
        }

        protected const int Alpha = 3;
        protected const int Red = 2;
        protected const int Green = 1;
        protected const int Blue = 0;
        /// <summary><para>Shift color values right this many bits.</para><para>This reduces the granularity of the color maps produced, making it much faster.</para></summary>
        /// 3 = value error of 8 (0 and 7 will look the same to it, 0 and 8 different); Takes ~4MB for color tables; ~.25 -> .50 seconds
        /// 2 = value error of 4; Takes ~64MB for color tables; ~3 seconds
        /// RAM usage roughly estimated with: ( ( 256 >> SidePixShift ) ^ 4 ) * 60
        /// Default SidePixShift = 3
        private const int SidePixShift = 3;
        private const int MaxSideIndex = 256 / (1 << SidePixShift);
        private const int SideSize = MaxSideIndex + 1;

        private Color[] quantPalette = null;
        private byte[] quantData = null;

        public int AlphaThreshold { get; set; } = 10;
        public int AlphaFader { get; set; } = 70;

        public Color[] QuantPalette => quantPalette;
        public byte[] QuantData => quantData;

        public WuQuantizer()
        {
        }

        //public bool StartQuantization(Bitmap bitmapSource, int colorMax)
        //{
        //    var colorconvert = DataToColorConverter.GetDataToColorConverter(bitmapSource.PixelFormat);
        //    if (colorconvert == null)
        //        return false;

        //    byte[] imageData = bitmapSource.CopyData();

        //    Color[] image = colorconvert(imageData, bitmapSource.Palette);
        //    int maxColors = colorMax;
        //    var result = QuantizeImage(image, AlphaThreshold, AlphaFader, maxColors);
        //    quantPalette = result.Item1;
        //    quantData = result.Item2;
        //    return true;
        //}

        public bool StartQuantization(Color[] image, int colorMax)
        {
            if (image == null)
                return false;

            int maxColors = colorMax;
            var result = QuantizeImage(image, AlphaThreshold, AlphaFader, maxColors);
            quantPalette = result.Item1;
            quantData = result.Item2;
            return true;
        }

        public static Tuple<Color[], byte[]> QuantizeImage(Color[] image, int alphaThreshold = 10, int alphaFader = 70, int maxColors = 256)
        {
            var colorCount = maxColors;
            var moments = BuildHistogram(image, alphaThreshold, alphaFader);
            CalculateMoments(moments);
            var cubes = SplitData(ref colorCount, maxColors, moments);
            var lookups = BuildLookups(cubes, moments);
            var paletteHistogram = new PaletteColorHistory[colorCount + 1];
            var returnedData = IndexedPixels(image, lookups, alphaThreshold, maxColors, paletteHistogram);
            var returnedColors = BuildPalette(new Color[maxColors], paletteHistogram);

            return new Tuple<Color[], byte[]>(returnedColors, returnedData);
        }

        private static Color[] BuildPalette(Color[] palette, PaletteColorHistory[] paletteHistogram)
        {
            for (int paletteColorIndex = 0; paletteColorIndex < paletteHistogram.Length; paletteColorIndex++)
            {
                palette[paletteColorIndex] = paletteHistogram[paletteColorIndex].ToNormalizedColorWPF();
            }
            return palette;
        }

        private static byte[] IndexedPixels(Color[] pixels, WuPixel[] lookups, int alphaThreshold, int maxColors, PaletteColorHistory[] paletteHistogram)
        {
            var length = maxColors == 256 ? pixels.Length : maxColors == 16 ? pixels.Length / 2 : throw new Exception("IndexedPixels: unknown pixelformat");
            byte[] returned = new byte[length];
            PaletteLookup lookup = new PaletteLookup(lookups);
            --maxColors;
            for (int i = 0; i < pixels.Length; i++)
            {
                WuPixel pixel = new WuPixel(pixels[i].A, pixels[i].R, pixels[i].G, pixels[i].B);
                byte bestMatch = (byte)maxColors;
                if (pixel.Alpha > alphaThreshold)
                {
                    bestMatch = lookup.GetPaletteIndex(pixel);
                    paletteHistogram[bestMatch].AddPixel(pixel);
                }
                switch (maxColors)
                {
                    case 256 - 1:
                        returned[i] = bestMatch;
                        break;
                    case 16 - 1:
                        if (i % 2 == 0) { returned[i / 2] = (byte)(bestMatch << 4); }
                        else { returned[i / 2] |= (byte)(bestMatch & 0x0F); }
                        break;
                    case 2 - 1:
                        if (i % 8 == 0) { returned[i / 8] = (byte)(bestMatch << 7); }
                        else { returned[i / 8] |= (byte)((bestMatch & 0x01) << (7 - (i % 8))); }
                        break;
                }
            }

            return returned;
        }

        #region Histogram

        private static ColorMoment[,,,] BuildHistogram(WuPixel[] sourcePixels, int alphaThreshold, int alphaFader)
        {
            var moments = new ColorMoment[SideSize, SideSize, SideSize, SideSize];

            foreach (var pixel in sourcePixels)
            {
                byte pixelAlpha = pixel.Alpha;
                if (pixelAlpha > alphaThreshold)
                {
                    if (pixelAlpha < 255)
                    {
                        var alpha = pixel.Alpha + (pixel.Alpha % alphaFader);
                        pixelAlpha = (byte)(alpha > 255 ? 255 : alpha);
                    }
                    byte pixelRed = pixel.Red;
                    byte pixelGreen = pixel.Green;
                    byte pixelBlue = pixel.Blue;

                    pixelAlpha = (byte)((pixelAlpha >> SidePixShift) + 1);
                    pixelRed = (byte)((pixelRed >> SidePixShift) + 1);
                    pixelGreen = (byte)((pixelGreen >> SidePixShift) + 1);
                    pixelBlue = (byte)((pixelBlue >> SidePixShift) + 1);
                    moments[pixelAlpha, pixelRed, pixelGreen, pixelBlue].Add(pixel);
                }
            }

            return moments;
        }

        private static ColorMoment[,,,] BuildHistogram(Color[] sourcePixels, int alphaThreshold, int alphaFader)
        {
            var moments = new ColorMoment[SideSize, SideSize, SideSize, SideSize];

            foreach (var pixel in sourcePixels)
            {
                byte pixelAlpha = pixel.A;
                if (pixelAlpha > alphaThreshold)
                {
                    if (pixelAlpha < 255)
                    {
                        var alpha = pixel.A + (pixel.A % alphaFader);
                        pixelAlpha = (byte)(alpha > 255 ? 255 : alpha);
                    }
                    byte pixelRed = pixel.R;
                    byte pixelGreen = pixel.G;
                    byte pixelBlue = pixel.B;

                    pixelAlpha = (byte)((pixelAlpha >> SidePixShift) + 1);
                    pixelRed = (byte)((pixelRed >> SidePixShift) + 1);
                    pixelGreen = (byte)((pixelGreen >> SidePixShift) + 1);
                    pixelBlue = (byte)((pixelBlue >> SidePixShift) + 1);
                    moments[pixelAlpha, pixelRed, pixelGreen, pixelBlue].Add(pixel);
                }
            }

            return moments;
        }

        #endregion Histogram

        private static void CalculateMoments(ColorMoment[,,,] moments)
        {
            var xarea = new ColorMoment[SideSize, SideSize];
            var area = new ColorMoment[SideSize];
            for (var alphaIndex = 1; alphaIndex < SideSize; alphaIndex++)
            {
                for (var redIndex = 1; redIndex < SideSize; redIndex++)
                {
                    Array.Clear(area, 0, area.Length);
                    for (var greenIndex = 1; greenIndex < SideSize; greenIndex++)
                    {
                        ColorMoment line = new ColorMoment();
                        for (var blueIndex = 1; blueIndex < SideSize; blueIndex++)
                        {
                            line.AddFast(ref moments[alphaIndex, redIndex, greenIndex, blueIndex]);
                            area[blueIndex].AddFast(ref line);
                            xarea[greenIndex, blueIndex].AddFast(ref area[blueIndex]);

                            ColorMoment moment = moments[alphaIndex - 1, redIndex, greenIndex, blueIndex];
                            moment.AddFast(ref xarea[greenIndex, blueIndex]);
                            moments[alphaIndex, redIndex, greenIndex, blueIndex] = moment;
                        }
                    }
                }
            }
        }

        #region Split

        private static Box[] SplitData(ref int colorCount, int maxColors, ColorMoment[,,,] moments)
        {
            --colorCount;
            var next = 0;
            var volumeVariance = new float[maxColors];
            var cubes = new Box[maxColors];
            cubes[0].AlphaMaximum = MaxSideIndex;
            cubes[0].RedMaximum = MaxSideIndex;
            cubes[0].GreenMaximum = MaxSideIndex;
            cubes[0].BlueMaximum = MaxSideIndex;
            for (var cubeIndex = 1; cubeIndex < colorCount; ++cubeIndex)
            {
                if (Cut(moments, ref cubes[next], ref cubes[cubeIndex]))
                {
                    volumeVariance[next] = cubes[next].Size > 1 ? CalculateVariance(moments, cubes[next]) : 0.0f;
                    volumeVariance[cubeIndex] = cubes[cubeIndex].Size > 1 ? CalculateVariance(moments, cubes[cubeIndex]) : 0.0f;
                }
                else
                {
                    volumeVariance[next] = 0.0f;
                    cubeIndex--;
                }

                next = 0;
                var temp = volumeVariance[0];

                for (var index = 1; index <= cubeIndex; ++index)
                {
                    if (volumeVariance[index] <= temp) continue;
                    temp = volumeVariance[index];
                    next = index;
                }

                if (temp > 0.0) continue;
                colorCount = cubeIndex + 1;
                break;
            }
            return cubes.Take(colorCount).ToArray();
        }

        private static float CalculateVariance(ColorMoment[,,,] moments, Box cube)
        {
            ColorMoment volume = Volume(cube, moments);
            return volume.Variance();
        }

        private static ColorMoment Top(Box cube, int direction, int position, ColorMoment[,,,] moment)
        {
            switch (direction)
            {
                case Alpha:
                    return (moment[position, cube.RedMaximum, cube.GreenMaximum, cube.BlueMaximum] -
                            moment[position, cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] -
                            moment[position, cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] +
                            moment[position, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum]) -
                           (moment[position, cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[position, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] -
                            moment[position, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                            moment[position, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                case Red:
                    return (moment[cube.AlphaMaximum, position, cube.GreenMaximum, cube.BlueMaximum] -
                            moment[cube.AlphaMaximum, position, cube.GreenMinimum, cube.BlueMaximum] -
                            moment[cube.AlphaMinimum, position, cube.GreenMaximum, cube.BlueMaximum] +
                            moment[cube.AlphaMinimum, position, cube.GreenMinimum, cube.BlueMaximum]) -
                           (moment[cube.AlphaMaximum, position, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[cube.AlphaMaximum, position, cube.GreenMinimum, cube.BlueMinimum] -
                            moment[cube.AlphaMinimum, position, cube.GreenMaximum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, position, cube.GreenMinimum, cube.BlueMinimum]);

                case Green:
                    return (moment[cube.AlphaMaximum, cube.RedMaximum, position, cube.BlueMaximum] -
                            moment[cube.AlphaMaximum, cube.RedMinimum, position, cube.BlueMaximum] -
                            moment[cube.AlphaMinimum, cube.RedMaximum, position, cube.BlueMaximum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, position, cube.BlueMaximum]) -
                           (moment[cube.AlphaMaximum, cube.RedMaximum, position, cube.BlueMinimum] -
                            moment[cube.AlphaMaximum, cube.RedMinimum, position, cube.BlueMinimum] -
                            moment[cube.AlphaMinimum, cube.RedMaximum, position, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, position, cube.BlueMinimum]);

                case Blue:
                    return (moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMaximum, position] -
                            moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMinimum, position] -
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMaximum, position] +
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, position]) -
                           (moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMaximum, position] -
                            moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, position] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, position] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, position]);

                default:
                    return new ColorMoment();
            }
        }

        private static ColorMoment Bottom(Box cube, int direction, ColorMoment[,,,] moment)
        {
            switch (direction)
            {
                case Alpha:
                    return (-moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMaximum] +
                            moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum]) -
                           (-moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                case Red:
                    return (-moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] +
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum]) -
                           (-moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                case Green:
                    return (-moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] +
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] +
                            moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum]) -
                           (-moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                case Blue:
                    return (-moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] +
                            moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]) -
                           (-moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                default:
                    return new ColorMoment();
            }
        }

        private static CubeCut Maximize(ColorMoment[,,,] moments, Box cube, int direction, byte first, byte last, ColorMoment whole)
        {
            var bottom = Bottom(cube, direction, moments);
            var result = 0.0f;
            byte? cutPoint = null;

            for (var position = first; position < last; ++position)
            {
                var half = bottom + Top(cube, direction, position, moments);
                if (half.Weight == 0) continue;

                var temp = half.WeightedDistance();

                half = whole - half;
                if (half.Weight != 0)
                {
                    temp += half.WeightedDistance();

                    if (temp > result)
                    {
                        result = temp;
                        cutPoint = position;
                    }
                }
            }

            return new CubeCut(cutPoint, result);
        }

        private static bool Cut(ColorMoment[,,,] moments, ref Box first, ref Box second)
        {
            int direction;
            var whole = Volume(first, moments);
            var maxAlpha = Maximize(moments, first, Alpha, (byte)(first.AlphaMinimum + 1), first.AlphaMaximum, whole);
            var maxRed = Maximize(moments, first, Red, (byte)(first.RedMinimum + 1), first.RedMaximum, whole);
            var maxGreen = Maximize(moments, first, Green, (byte)(first.GreenMinimum + 1), first.GreenMaximum, whole);
            var maxBlue = Maximize(moments, first, Blue, (byte)(first.BlueMinimum + 1), first.BlueMaximum, whole);

            if ((maxAlpha.Value >= maxRed.Value) && (maxAlpha.Value >= maxGreen.Value) && (maxAlpha.Value >= maxBlue.Value))
            {
                direction = Alpha;
                if (!maxAlpha.Position.HasValue) return false;
            }
            else if ((maxRed.Value >= maxAlpha.Value) && (maxRed.Value >= maxGreen.Value) && (maxRed.Value >= maxBlue.Value))
                direction = Red;
            else
            {
                if ((maxGreen.Value >= maxAlpha.Value) && (maxGreen.Value >= maxRed.Value) && (maxGreen.Value >= maxBlue.Value))
                    direction = Green;
                else
                    direction = Blue;
            }

            second.AlphaMaximum = first.AlphaMaximum;
            second.RedMaximum = first.RedMaximum;
            second.GreenMaximum = first.GreenMaximum;
            second.BlueMaximum = first.BlueMaximum;

            switch (direction)
            {
                case Alpha:
                    second.AlphaMinimum = first.AlphaMaximum = maxAlpha.Position.Value;
                    second.RedMinimum = first.RedMinimum;
                    second.GreenMinimum = first.GreenMinimum;
                    second.BlueMinimum = first.BlueMinimum;
                    break;

                case Red:
                    second.RedMinimum = first.RedMaximum = maxRed.Position.Value;
                    second.AlphaMinimum = first.AlphaMinimum;
                    second.GreenMinimum = first.GreenMinimum;
                    second.BlueMinimum = first.BlueMinimum;
                    break;

                case Green:
                    second.GreenMinimum = first.GreenMaximum = maxGreen.Position.Value;
                    second.AlphaMinimum = first.AlphaMinimum;
                    second.RedMinimum = first.RedMinimum;
                    second.BlueMinimum = first.BlueMinimum;
                    break;

                case Blue:
                    second.BlueMinimum = first.BlueMaximum = maxBlue.Position.Value;
                    second.AlphaMinimum = first.AlphaMinimum;
                    second.RedMinimum = first.RedMinimum;
                    second.GreenMinimum = first.GreenMinimum;
                    break;
            }

            first.Size = (first.AlphaMaximum - first.AlphaMinimum) * (first.RedMaximum - first.RedMinimum) * (first.GreenMaximum - first.GreenMinimum) * (first.BlueMaximum - first.BlueMinimum);
            second.Size = (second.AlphaMaximum - second.AlphaMinimum) * (second.RedMaximum - second.RedMinimum) * (second.GreenMaximum - second.GreenMinimum) * (second.BlueMaximum - second.BlueMinimum);

            return true;
        }

        #endregion Split

        private static ColorMoment Volume(Box cube, ColorMoment[,,,] moment)
        {
            return (moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMaximum] -
                    moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] -
                    moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] +
                    moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] -
                    moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMaximum] +
                    moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] +
                    moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] -
                    moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum]) -

                   (moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] -
                    moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] -
                    moment[cube.AlphaMaximum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                    moment[cube.AlphaMinimum, cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] -
                    moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                    moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                    moment[cube.AlphaMaximum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum] -
                    moment[cube.AlphaMinimum, cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);
        }

        private static WuPixel[] BuildLookups(Box[] cubes, ColorMoment[,,,] moments)
        {
            WuPixel[] lookups = new WuPixel[cubes.Length];

            for (int cubeIndex = 0; cubeIndex < cubes.Length; cubeIndex++)
            {
                var volume = Volume(cubes[cubeIndex], moments);

                if (volume.Weight <= 0) continue;

                var lookup = new WuPixel
                {
                    Alpha = (byte)(volume.Alpha / volume.Weight),
                    Red = (byte)(volume.Red / volume.Weight),
                    Green = (byte)(volume.Green / volume.Weight),
                    Blue = (byte)(volume.Blue / volume.Weight)
                };
                lookups[cubeIndex] = lookup;
            }
            return lookups;
        }

        private static Color[] BuildLookupsColor(Box[] cubes, ColorMoment[,,,] moments)
        {
            Color[] lookups = new Color[cubes.Length];

            for (int cubeIndex = 0; cubeIndex < cubes.Length; cubeIndex++)
            {
                var volume = Volume(cubes[cubeIndex], moments);

                if (volume.Weight <= 0) continue;

                lookups[cubeIndex] = Color.FromArgb
                    ((byte)(volume.Alpha / volume.Weight),
                    (byte)(volume.Red / volume.Weight),
                    (byte)(volume.Green / volume.Weight),
                    (byte)(volume.Blue / volume.Weight));
            }

            return lookups;
        }

    }
}