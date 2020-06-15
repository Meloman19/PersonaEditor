using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTWidthTable
    {
        public FNTWidthTable(BinaryReader reader)
        {
            uint Size = reader.ReadUInt32();

            for (int i = 0; i < Size / 2; i++)
            {
                byte Left = reader.ReadByte();
                byte Right = reader.ReadByte();
                WidthTable.Add(new VerticalCut(Left, Right));
            }
        }

        public List<VerticalCut> WidthTable { get; } = new List<VerticalCut>();

        public void Resize(int size)
        {
            if (size > WidthTable.Count)
            {
                WidthTable.AddRange(new VerticalCut[size - WidthTable.Count]);
            }
            else
            {
                WidthTable.RemoveRange(size, WidthTable.Count - size);
            }
        }

        public int Size()
        {
            return WidthTable.Count * 2 + 4;
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(WidthTable.Count * 2);
            foreach (var glyph in WidthTable)
                writer.Write(glyph.Get());
        }

        public int Count
        {
            get { return WidthTable.Count; }
        }

        public VerticalCut? this[int i]
        {
            get
            {
                if (i < WidthTable.Count)
                    return WidthTable[i];
                else
                    return null;
            }
            set
            {
                if (value != null)
                    WidthTable[i] = value.Value;
            }
        }
    }
}