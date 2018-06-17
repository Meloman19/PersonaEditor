using PersonaEditorLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PersonaEditorLib.FileStructure.Graphic
{
    public static class TMXHelper
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
    }
}