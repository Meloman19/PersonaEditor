using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AuxiliaryLibraries.Media.Quantization
{
    public class NeuQuant
    {
        public static int ncycles = 100;          // no. of learning cycles

        public static int netsize = 256;      // number of colours used
        public static int specials = 3;       // number of reserved colours used
        public static int bgColour = specials - 1;    // reserved background colour
        public static int cutnetsize = netsize - specials;
        public static int maxnetpos = netsize - 1;

        public static int initrad = netsize / 8;   // for 256 cols, radius starts at 32
        public static int radiusbiasshift = 6;
        public static int radiusbias = 1 << radiusbiasshift;
        public static int initBiasRadius = initrad * radiusbias;
        public static int radiusdec = 30; // factor of 1/30 each cycle

        public static int alphabiasshift = 10;            // alpha starts at 1
        public static int initalpha = 1 << alphabiasshift; // biased by 10 bits

        public static double gamma = 1024.0;
        public static double beta = 1.0 / 1024.0;
        public static double betagamma = beta * gamma;

        private int networkSize = 3;
        private int colormapSize = 4;

        private double[][] network = new double[netsize][]; // the network itself
        protected int[][] colormap = new int[netsize][]; // the network itself

        private int[] netindex = new int[256]; // for network lookup - really 256

        private double[] bias = new double[netsize];  // bias and freq arrays for learning
        private double[] freq = new double[netsize];

        // four primes near 500 - assume no image has a length so large
        // that it is divisible by all four primes

        public static int prime1 = 499;
        public static int prime2 = 491;
        public static int prime3 = 487;
        public static int prime4 = 503;
        public static int maxprime = prime4;

        private Color[] pixelsC = null;
        //protected int[] pixels = null;
        private int samplefac = 0;

        public NeuQuant(Color[] colors, int sample = 1)
        {
            SampleSet(sample);
            pixelsC = colors;
            setUpArrays();
        }

        private void SampleSet(int sample)
        {
            if (sample < 1) throw new Exception("Sample must be 1..30");
            if (sample > 30) throw new Exception("Sample must be 1..30");
            samplefac = sample;
        }

        public int getColorCount()
        {
            return netsize;
        }

        public Color GetColor(int index)
        {
            if (index < 0 || index >= netsize) throw new ArgumentOutOfRangeException("index", index, "Index must be < 0 and >=netsize");
            byte bb = (byte)colormap[index][0];
            byte gg = (byte)colormap[index][1];
            byte rr = (byte)colormap[index][2];
            byte aa = (byte)colormap[index][3];
            return Color.FromArgb(aa, rr, gg, bb);
        }

        public Color[] writeColourMap()
        {
            Color[] returned = new Color[netsize];

            for (int i = 0; i < netsize; i++)
            {
                byte bb = (byte)colormap[i][0];
                byte gg = (byte)colormap[i][1];
                byte rr = (byte)colormap[i][2];
                byte aa = (byte)colormap[i][3];
                returned[i] = Color.FromArgb(aa, rr, gg, bb);
            }

            return returned;
        }

        public byte[] GetNewData()
        {
            byte[] returned = new byte[pixelsC.Length];

            for (int i = 0; i < returned.Length; i++)
                returned[i] = (byte)lookup(pixelsC[i]);

            return returned;
        }

        protected void setUpArrays()
        {
            for (int i = 0; i < network.Length; i++)
                network[i] = new double[networkSize];
            for (int i = 0; i < colormap.Length; i++)
                colormap[i] = new int[colormapSize];

            network[0][0] = 0.0;    // black
            network[0][1] = 0.0;
            network[0][2] = 0.0;

            network[1][0] = 255.0;  // white
            network[1][1] = 255.0;
            network[1][2] = 255.0;

            // RESERVED bgColour	// background

            for (int i = 0; i < specials; i++)
            {
                freq[i] = 1.0 / netsize;
                bias[i] = 0.0;
            }

            for (int i = specials; i < netsize; i++)
            {
                double[] p = network[i];
                p[0] = (255.0 * (i - specials)) / cutnetsize;
                p[1] = (255.0 * (i - specials)) / cutnetsize;
                p[2] = (255.0 * (i - specials)) / cutnetsize;

                freq[i] = 1.0 / netsize;
                bias[i] = 0.0;
            }
        }

        public void init()
        {
            learn();
            fix();
            inxbuild();

            foreach (var a in colormap)
                a[3] = 0xFF;
        }

        private void altersingle(double alpha, int i, double b, double g, double r)
        {
            // Move neuron i towards biased (b,g,r) by factor alpha
            double[] n = network[i];                // alter hit neuron
            n[0] -= (alpha * (n[0] - b));
            n[1] -= (alpha * (n[1] - g));
            n[2] -= (alpha * (n[2] - r));
        }

        private void alterneigh(double alpha, int rad, int i, double b, double g, double r)
        {

            int lo = i - rad; if (lo < specials - 1) lo = specials - 1;
            int hi = i + rad; if (hi > netsize) hi = netsize;

            int j = i + 1;
            int k = i - 1;
            int q = 0;
            while ((j < hi) || (k > lo))
            {
                double a = (alpha * (rad * rad - q * q)) / (rad * rad);
                q++;
                if (j < hi)
                {
                    double[] p = network[j];
                    p[0] -= (a * (p[0] - b));
                    p[1] -= (a * (p[1] - g));
                    p[2] -= (a * (p[2] - r));
                    j++;
                }
                if (k > lo)
                {
                    double[] p = network[k];
                    p[0] -= (a * (p[0] - b));
                    p[1] -= (a * (p[1] - g));
                    p[2] -= (a * (p[2] - r));
                    k--;
                }
            }
        }

        private int contest(double b, double g, double r)
        {    // Search for biased BGR values
             // finds closest neuron (min dist) and updates freq 
             // finds best neuron (min dist-bias) and returns position 
             // for frequently chosen neurons, freq[i] is high and bias[i] is negative 
             // bias[i] = gamma*((1/netsize)-freq[i]) 

            double bestd = float.MaxValue;
            double bestbiasd = bestd;
            int bestpos = -1;
            int bestbiaspos = bestpos;

            for (int i = specials; i < netsize; i++)
            {
                double[] n = network[i];
                double dist = n[0] - b; if (dist < 0) dist = -dist;
                double a = n[1] - g; if (a < 0) a = -a;
                dist += a;
                a = n[2] - r; if (a < 0) a = -a;
                dist += a;
                if (dist < bestd) { bestd = dist; bestpos = i; }
                double biasdist = dist - bias[i];
                if (biasdist < bestbiasd) { bestbiasd = biasdist; bestbiaspos = i; }
                freq[i] -= beta * freq[i];
                bias[i] += betagamma * freq[i];
            }
            freq[bestpos] += beta;
            bias[bestpos] -= betagamma;
            return bestbiaspos;
        }

        private int specialFind(double b, double g, double r)
        {
            for (int i = 0; i < specials; i++)
            {
                double[] n = network[i];
                if (n[0] == b && n[1] == g && n[2] == r) return i;
            }
            return -1;
        }

        private void learn()
        {
            int biasRadius = initBiasRadius;
            int alphadec = 30 + ((samplefac - 1) / 3);
            int lengthcount = pixelsC.Length;
            int samplepixels = lengthcount / samplefac;
            int delta = samplepixels / ncycles;
            int alpha = initalpha;

            int i = 0;
            int rad = biasRadius >> radiusbiasshift;
            if (rad <= 1) rad = 0;

            int step = 0;
            int pos = 0;

            if ((lengthcount % prime1) != 0) step = prime1;
            else
            {
                if ((lengthcount % prime2) != 0) step = prime2;
                else
                {
                    if ((lengthcount % prime3) != 0) step = prime3;
                    else step = prime4;
                }
            }

            i = 0;
            while (i < samplepixels)
            {
                int red = pixelsC[pos].R;
                int green = pixelsC[pos].G;
                int blue = pixelsC[pos].B;
                int alphach = pixelsC[pos].A;

                double b = blue;
                double g = green;
                double r = red;

                if (i == 0)
                {   // remember background colour
                    network[bgColour][0] = b;
                    network[bgColour][1] = g;
                    network[bgColour][2] = r;
                }

                int j = specialFind(b, g, r);
                j = j < 0 ? contest(b, g, r) : j;

                if (j >= specials)
                {   // don't learn for specials
                    double a = (1.0 * alpha) / initalpha;
                    altersingle(a, j, b, g, r);
                    if (rad > 0) alterneigh(a, rad, j, b, g, r);   // alter neighbours
                }

                pos += step;
                while (pos >= lengthcount) pos -= lengthcount;

                i++;
                if (i % delta == 0)
                {
                    alpha -= alpha / alphadec;
                    biasRadius -= biasRadius / radiusdec;
                    rad = biasRadius >> radiusbiasshift;
                    if (rad <= 1) rad = 0;
                }
            }
        }

        private void fix()
        {
            for (int i = 0; i < netsize; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int x = (int)(0.5 + network[i][j]);
                    if (x < 0) x = 0;
                    if (x > 255) x = 255;
                    colormap[i][j] = x;
                }
                colormap[i][3] = i;
            }
        }

        private void inxbuild()
        {
            // Insertion sort of network and building of netindex[0..255]

            int previouscol = 0;
            int startpos = 0;

            for (int i = 0; i < netsize; i++)
            {
                int[] p = colormap[i];
                int[] q = null;
                int smallpos = i;
                int smallval = p[1];            // index on g
                                                // find smallest in i..netsize-1
                for (int j = i + 1; j < netsize; j++)
                {
                    q = colormap[j];
                    if (q[1] < smallval)
                    {       // index on g
                        smallpos = j;
                        smallval = q[1];    // index on g
                    }
                }
                q = colormap[smallpos];
                // swap p (i) and q (smallpos) entries
                if (i != smallpos)
                {
                    int j = q[0]; q[0] = p[0]; p[0] = j;
                    j = q[1]; q[1] = p[1]; p[1] = j;
                    j = q[2]; q[2] = p[2]; p[2] = j;
                    j = q[3]; q[3] = p[3]; p[3] = j;
                }
                // smallval entry is now in position i
                if (smallval != previouscol)
                {
                    netindex[previouscol] = (startpos + i) >> 1;
                    for (int j = previouscol + 1; j < smallval; j++) netindex[j] = i;
                    previouscol = smallval;
                    startpos = i;
                }
            }
            netindex[previouscol] = (startpos + maxnetpos) >> 1;
            for (int j = previouscol + 1; j < 256; j++) netindex[j] = maxnetpos; // really 256
        }

        public Color convert(Color pixel)
        {
            byte alfa = pixel.A;
            int r = pixel.R;
            int g = pixel.G;
            int b = pixel.B;
            int i = inxsearch(b, g, r);
            byte bb = (byte)colormap[i][0];
            byte gg = (byte)colormap[i][1];
            byte rr = (byte)colormap[i][2];
            return Color.FromArgb(alfa, rr, gg, bb);
        }

        public int lookup(Color c)
        {
            int r = c.R;
            int g = c.G;
            int b = c.B;
            int i = inxsearch(b, g, r);
            return i;
        }

        private int not_used_slow_inxsearch(int b, int g, int r)
        {
            // Search for BGR values 0..255 and return colour index
            int bestd = 1000;       // biggest possible dist is 256*3
            int best = -1;
            for (int i = 0; i < netsize; i++)
            {
                int[] p = colormap[i];
                int dist = p[1] - g;
                if (dist < 0) dist = -dist;
                int a = p[0] - b; if (a < 0) a = -a;
                dist += a;
                a = p[2] - r; if (a < 0) a = -a;
                dist += a;
                if (dist < bestd) { bestd = dist; best = i; }
            }
            return best;
        }

        protected int inxsearch(int b, int g, int r)
        {
            // Search for BGR values 0..255 and return colour index
            int bestd = 1000;       // biggest possible dist is 256*3
            int best = -1;
            int i = netindex[g];    // index on g
            int j = i - 1;      // start at netindex[g] and work outwards

            while ((i < netsize) || (j >= 0))
            {
                if (i < netsize)
                {
                    int[] p = colormap[i];
                    int dist = p[1] - g;        // inx key
                    if (dist >= bestd) i = netsize; // stop iter
                    else
                    {
                        if (dist < 0) dist = -dist;
                        int a = p[0] - b; if (a < 0) a = -a;
                        dist += a;
                        if (dist < bestd)
                        {
                            a = p[2] - r; if (a < 0) a = -a;
                            dist += a;
                            if (dist < bestd) { bestd = dist; best = i; }
                        }
                        i++;
                    }
                }
                if (j >= 0)
                {
                    int[] p = colormap[j];
                    int dist = g - p[1]; // inx key - reverse dif
                    if (dist >= bestd) j = -1; // stop iter
                    else
                    {
                        if (dist < 0) dist = -dist;
                        int a = p[0] - b; if (a < 0) a = -a;
                        dist += a;
                        if (dist < bestd)
                        {
                            a = p[2] - r; if (a < 0) a = -a;
                            dist += a;
                            if (dist < bestd) { bestd = dist; best = j; }
                        }
                        j--;
                    }
                }
            }

            return best;
        }
    }
}