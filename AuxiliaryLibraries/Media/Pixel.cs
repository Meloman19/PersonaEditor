namespace AuxiliaryLibraries.Media
{
    public struct Pixel
    {
        private readonly byte _a;
        private readonly byte _r;
        private readonly byte _g;
        private readonly byte _b;

        private Pixel(byte a, byte r, byte g, byte b)
        {
            _a = a;
            _r = r;
            _g = g;
            _b = b;
        }

        public byte A => _a;
        public byte R => _r;
        public byte G => _g;
        public byte B => _b;

        public static Pixel FromArgb(byte a, byte r, byte g, byte b) =>
            new Pixel(a, r, g, b);

        public static Pixel FromArgb(byte a, Pixel basePixel) =>
            FromArgb(a, basePixel.R, basePixel.G, basePixel.B);

        public static Pixel FromArgb(byte r, byte g, byte b) =>
            FromArgb(byte.MaxValue, r, g, b);
    }
}