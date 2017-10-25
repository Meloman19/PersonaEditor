using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.FileStructure.FNT
{
    public class FNTReserved
    {
        public FNTReserved(BinaryReader reader, uint size)
        {
            for (int i = 0; i < size; i++)
                Reserved.Add(reader.ReadInt32());
        }

        public List<int> Reserved = new List<int>();

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