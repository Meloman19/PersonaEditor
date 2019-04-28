using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTCompressedGlyphTable
    {
        public FNTCompressedGlyphTable(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
                Table.Add(reader.ReadInt32());
        }

        public List<int> Table = new List<int>();

        public void Get(BinaryWriter writer)
        {
            foreach (var a in Table)
                writer.Write(a);
        }
    }
}
