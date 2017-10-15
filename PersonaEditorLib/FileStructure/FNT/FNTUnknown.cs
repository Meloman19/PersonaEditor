using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.FileStructure.FNT
{

    public class FNTUnknown
    {
        public FNTUnknown(BinaryReader reader)
        {
            uint Size = reader.ReadUInt32();
            for (int i = 0; i < Size; i++)
                Unknown.Add(reader.ReadByte());
        }

        public List<byte> Unknown { get; set; } = new List<byte>();

        public int Size()
        {
            return Unknown.Count + 4;
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(Unknown.Count);
            foreach (var b in Unknown)
                writer.Write(b);
        }
    }

}