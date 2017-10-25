using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.FNT
{
    public class FNTLast
    {
        List<byte[]> List = new List<byte[]>();

        public FNTLast(BinaryReader reader, int count)
        {
            for (int i = 0; i < count & reader.BaseStream.Position < reader.BaseStream.Length; i++)
                List.Add(reader.ReadBytes(2));
        }

        public int Size
        {
            get { return List.Count * 2; }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Length, 16)]);
            foreach (var a in List)
                writer.Write(a);
        }
    }
}