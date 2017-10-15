using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.FileStructure.FNT
{
    public class FNTPalette
    {
        public FNTPalette(BinaryReader reader, int NumberOfColor)
        {
            List<Color> Colors = new List<Color>();
            for (int i = 0; i < NumberOfColor; i++)
                Colors.Add(new Color()
                {
                    R = reader.ReadByte(),
                    G = reader.ReadByte(),
                    B = reader.ReadByte(),
                    A = reader.ReadByte()
                });
            Pallete = new BitmapPalette(Colors);
        }

        public BitmapPalette Pallete { get; set; }

        public int Size()
        {
            return Pallete.Colors.Count * 4;
        }

        public void Get(BinaryWriter writer)
        {
            foreach (var color in Pallete.Colors)
            {
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);
                writer.Write(color.A);
            }
        }
    }
}