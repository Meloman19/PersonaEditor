using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTUnknown
    {
        public FNTUnknown(BinaryReader reader)
        {
            uint Size = reader.ReadUInt32();
            for (int i = 0; i < Size; i++)
                Unknown.Add(reader.ReadByte());
        }

        public List<byte> Unknown { get; } = new List<byte>();

        public void Resize(int size)
        {
            int temp = size % 4 == 0 ? size : size + (4 - (size % 4));
            if (temp > Unknown.Count)
                Unknown.AddRange(new byte[temp - Unknown.Count]);
            else
                Unknown.RemoveRange(temp, Unknown.Count - temp);
        }

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