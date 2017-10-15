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

        public static List<Color> TilePalette(List<Color> colorArray)
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

        public static int GetIndexedColorCount(PS2PixelFormat fmt)
        {
            switch (fmt)
            {
                case PS2PixelFormat.PSMT8:
                case PS2PixelFormat.PSMT8H:
                    return 256;
                case PS2PixelFormat.PSMT4:
                case PS2PixelFormat.PSMT4HL:
                case PS2PixelFormat.PSMT4HH:
                    return 16;
                default:
                    return 0;
            }
        }

        public TMXPalette(BinaryReader reader, PS2PixelFormat format)
        {
            List<Color> Colors = new List<Color>();
            for (int i = 0; i < GetIndexedColorCount(format); i++)
                Colors.Add(new Color()
                {
                    R = reader.ReadByte(),
                    G = reader.ReadByte(),
                    B = reader.ReadByte(),
                    A = reader.ReadByte()
                });

            if (GetIndexedColorCount(format) == 256)
                Pallete = new BitmapPalette(TilePalette(Colors));
            else
                Pallete = new BitmapPalette(Colors);
        }

        public BitmapPalette Pallete { get; set; }
    }
}