using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTReserved
    {
        public FNTReserved(BinaryReader reader, uint size)
        {
            for (int i = 0; i < size; i++)
                Reserved.Add(reader.ReadInt32());
        }

        public List<int> Reserved = new List<int>();

        public void Resize(int size)
        {
            if (size > Reserved.Count)
                Reserved.AddRange(new int[size - Reserved.Count]);
            else
                Reserved.RemoveRange(size, Reserved.Count - size);
        }

        public int Size
        {
            get { return Reserved.Count * 4; }
        }

        public void Get(BinaryWriter writer)
        {
            foreach (var b in Reserved)
                writer.Write(b);
        }
    }
}