using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.FileStructure.TMX
{
    public enum PS2PixelFormat
    {
        PSMTC32 = 0x00,
        PSMTC24 = 0x01,
        PSMTC16 = 0x02,
        PSMTC16S = 0x0A,
        PSMT8 = 0x13,
        PSMT4 = 0x14,
        PSMT8H = 0x1B,
        PSMT4HL = 0x24,
        PSMT4HH = 0x2C,
        PSMZ32 = 0x30,
        PSMZ24 = 0x31,
        PSMZ16 = 0x32,
        PSMZ16S = 0x3A
    }

    public class TMXPalette
    {
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

        public static PixelFormat GetPixelFormat(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return PixelFormats.Indexed8;
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return PixelFormats.Indexed4;
                case PS2PixelFormat.PSMTC24:
                    return PixelFormats.Rgb24;
                case PS2PixelFormat.PSMTC32:
                    return PixelFormats.Pbgra32;
                default:
                    return PixelFormats.Default;
            }
        }

        public TMXPalette(BinaryReader reader, PS2PixelFormat format)
        {
            Format = GetPixelFormat(format);

            if (Format == PixelFormats.Indexed8)
                Pallete = new BitmapPalette(TilePalette(Utilities.Utilities.ReadPalette(reader, 256)));
            else if (Format == PixelFormats.Indexed4)
                Pallete = new BitmapPalette(Utilities.Utilities.ReadPalette(reader, 16));
        }

        public BitmapPalette Pallete { get; set; } = null;
        public PixelFormat Format { get; set; }

        public int Size
        {
            get
            {
                if (Pallete == null)
                    return 0;
                else
                    return Pallete.Colors.Count * 4;
            }
        }

        public void Get(BinaryWriter writer)
        {
            if (Pallete != null)
            {
                List<Color> colors = Format == PixelFormats.Indexed8 ? TilePalette(Pallete.Colors) : Pallete.Colors.ToList();
                foreach (var color in colors)
                {
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);
                    writer.Write(color.A);
                }
            }
        }
    }
}