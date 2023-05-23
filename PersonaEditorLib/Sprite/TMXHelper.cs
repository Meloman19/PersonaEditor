using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Sprite
{
    public static class TMXHelper
    {
        public static byte[] TilePalette(byte[] palette)
        {
            if (palette == null)
                throw new ArgumentNullException("palette");
            if (palette.Length != 1024)
                throw new ArgumentOutOfRangeException("palette.Length", palette.Length, "Must be 1024");

            List<byte> returned = new List<byte>();

            int index = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 8; x++)
                    returned.AddRange(palette.SubArray((index + x) * 4, 4));
                index += 16;

                for (int x = 0; x < 8; x++)
                    returned.AddRange(palette.SubArray((index + x) * 4, 4));
                index -= 8;

                for (int x = 0; x < 8; x++)
                    returned.AddRange(palette.SubArray((index + x) * 4, 4));
                index += 16;

                for (int x = 0; x < 8; x++)
                    returned.AddRange(palette.SubArray((index + x) * 4, 4));
                index += 8;
            }

            return returned.ToArray();
        }

        public static byte[] ReadPallete(BinaryReader reader, TMXPixelFormatEnum pixelFormat)
        {
            switch (pixelFormat)
            {
                case TMXPixelFormatEnum.PSMT8:
                    return reader.ReadBytes(256 * 4);
                case TMXPixelFormatEnum.PSMT4:
                    return reader.ReadBytes(16 * 4);
                default:
                    throw new Exception("Unknown pallete");
            }
        }

        public static int GetStride(TMXPixelFormatEnum pixelFormat, int width)
        {
            int bitsPerPixel;
            switch (pixelFormat)
            {
                case TMXPixelFormatEnum.PSMT8:
                    bitsPerPixel = 8;
                    break;
                case TMXPixelFormatEnum.PSMT4:
                    bitsPerPixel = 4;
                    break;
                case TMXPixelFormatEnum.PSMTC32:
                    bitsPerPixel = 32;
                    break;
                default:
                    throw new Exception("Unknown format");
            }

            return ImageHelper.GetStride(bitsPerPixel, width);
        }
    }
}