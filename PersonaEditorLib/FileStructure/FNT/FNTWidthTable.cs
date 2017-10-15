using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.FileStructure.FNT
{
    public class FNTWidthTable
    {
        public FNTWidthTable(BinaryReader reader)
        {
            uint Size = reader.ReadUInt32();

            for (int i = 0; i < Size / 2; i++)
            {
                VerticalCut cut = new VerticalCut();
                cut.Left = reader.ReadByte();
                cut.Right = reader.ReadByte();
                WidthTable.Add(cut);
            }
        }

        public List<VerticalCut> WidthTable = new List<VerticalCut>();

        public int Size()
        {
            return WidthTable.Count * 4 + 4;
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(WidthTable.Count * 2);
            foreach (var glyph in WidthTable)
                writer.Write(glyph.Get());
        }
    }

}