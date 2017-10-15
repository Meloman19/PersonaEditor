using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.SPR
{
    class SPRTextures :ISPR
    {
        public List<byte[]> List = new List<byte[]>();
        
        public SPRTextures(BinaryReader reader, List<int> offset)
        {
            var temp = offset.Select(x => x).ToList();
            temp.Add((int)reader.BaseStream.Length);

            for (int i = 1; i < temp.Count; i++)
            {
                reader.BaseStream.Position = temp[i - 1];
                List.Add(reader.ReadBytes(temp[i] - temp[i - 1]));
            }
        }

        public int Size
        {
            get
            {
                int returned = 0;
                foreach (var a in List) returned += a.Length;
                return returned;
            }
        }

        public void Get(BinaryWriter writer)
        {
            foreach(var a in List)
            {
                writer.Write(a);
            }
        }
    }
}