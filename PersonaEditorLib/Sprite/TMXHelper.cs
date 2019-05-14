using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PersonaEditorLib.Sprite
{
    public static class TMXHelper
    {
        static Dictionary<TMXPixelFormatEnum, PixelFormat> PS2ToAuxdic = new Dictionary<TMXPixelFormatEnum, PixelFormat>()
        {
            { TMXPixelFormatEnum.PSMT4,   PixelFormats.Indexed4Reverse  },
            { TMXPixelFormatEnum.PSMT8,   PixelFormats.Indexed8  },
            { TMXPixelFormatEnum.PSMTC32, PixelFormats.Rgba32PS2 }
        };

        public static PixelFormat PS2ToAux(TMXPixelFormatEnum pixelFormat)
        {
            if (PS2ToAuxdic.ContainsKey(pixelFormat))
                return PS2ToAuxdic[pixelFormat];
            else
                return PixelFormats.Undefined;
        }

        public static List<Color> TilePalette(IList<Color> colorArray)
        {
            List<Color> returned = new List<Color>();

            int Index = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int x = 0; x < 8; x++)
                    returned.Add(colorArray[Index++]);

                Index += 8;
                for (int x = 0; x < 8; x++)
                    returned.Add(colorArray[Index++]);

                Index -= 16;
                for (int x = 0; x < 8; x++)
                    returned.Add(colorArray[Index++]);

                Index += 8;
                for (int x = 0; x < 8; x++)
                    returned.Add(colorArray[Index++]);
            }
            return returned;
        }

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

        public static int ReadPalette(BinaryReader reader, TMXPixelFormatEnum pixelFormat, TMXPixelFormatEnum paletteFormat, out Color[] colors)
        {
            int returned = 0;

            var PaletteFormat = PS2ToAux(paletteFormat);

            byte[] paletteData = null;
            if (pixelFormat == TMXPixelFormatEnum.PSMT8)
            {
                paletteData = TilePalette(reader.ReadBytes(256 * 4));
                returned += 256 * 4;
            }
            else if (pixelFormat == TMXPixelFormatEnum.PSMT4)
            {
                paletteData = reader.ReadBytes(16 * 4);
                returned += 16 * 4;
            }

            if (PaletteFormat.IsIndexed)
                throw new Exception("TMX Palette is indexed!");
            else
                colors = PixelConverters.GetDataToColorConverter(PaletteFormat)(paletteData);

            return returned;
        }

        public static void WritePalette(BinaryWriter writer, TMXPixelFormatEnum pixelFormat, TMXPixelFormatEnum paletteFormat, Color[] colors)
        {
            var PaletteFormat = PS2ToAux(paletteFormat);
            byte[] paletteData = PixelConverters.GetColorToDataConverter(PaletteFormat)(colors);

            if (pixelFormat == TMXPixelFormatEnum.PSMT8)
                writer.Write(TilePalette(paletteData));
            else if (pixelFormat == TMXPixelFormatEnum.PSMT4)
                writer.Write(paletteData);
        }
    }
}