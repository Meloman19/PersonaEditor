using System;

namespace AuxiliaryLibraries.Media
{
    public sealed class PixelMap
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Pixel[] _pixels;

        public PixelMap()
        {
            _width = 0;
            _height = 0;
            _pixels = Array.Empty<Pixel>();
        }

        public PixelMap(int width, int height, Pixel[] pixels)
        {
            if (width * height != pixels.Length)
                throw new Exception("Wrong format");

            _width = width;
            _height = height;
            _pixels = pixels;
        }

        public int Width => _width;

        public int Height => _height;

        public Pixel[] Pixels => _pixels;
    }
}