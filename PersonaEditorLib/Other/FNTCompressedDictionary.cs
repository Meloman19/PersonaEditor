using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTCompressedDictionary
    {
        public FNTCompressedDictionary(BinaryReader reader, int size)
        {
            List<ushort[]> temp = new List<ushort[]>();
            for (int i = 0; i < size / 6; i++)
            {
                ushort[] added = new ushort[3];
                added[0] = reader.ReadUInt16();
                added[1] = reader.ReadUInt16();
                added[2] = reader.ReadUInt16();
                temp.Add(added);
            }
            Dictionary = temp.ToArray();
        }

        public ushort[][] Dictionary { get; set; }

        public void Get(BinaryWriter writer)
        {
            foreach (var b in Dictionary)
                foreach (var a in b)
                    writer.Write(a);
        }
    }
}
