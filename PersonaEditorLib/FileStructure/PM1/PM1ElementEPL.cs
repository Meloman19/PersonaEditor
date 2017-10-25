using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.PM1
{
    class PM1ElementEPL : IPM1Element
    {
        public List<byte[]> List = new List<byte[]>();

        public PM1ElementEPL(BinaryReader reader, PM1Table.Element element, PM1ElementEPLHead headers)
        {
            reader.BaseStream.Position = element.Position;
            using (BinaryReader temp = new BinaryReader(new MemoryStream(reader.ReadBytes(element.Size))))
            {
                var pos = headers.List.Select(x => x.Position - headers.List.First().Position).ToList();
                pos.Add((int)temp.BaseStream.Length);

                for (int i = 0; i < pos.Count - 1; i++)
                    List.Add(temp.ReadBytes(pos[i + 1] - pos[i]));
            }
        }

        public int Position(int index)
        {
            if (index >= List.Count)
            {
                return -1;
            }
            else
            {
                int returned = 0;
                for (int i = 0; i < index; i++)
                    returned += (int)List[i].Length + Utilities.Utilities.Alignment(List[i].Length, 0x10);
                return returned;
            }
        }

        public int Size
        {
            get
            {
                int returned = 0;
                foreach (var a in List)
                    returned += (int)a.Length + Utilities.Utilities.Alignment(a.Length, 0x10);
                return returned;
            }
        }
        public int Count
        {
            get { return List.Count; }
        }
        public int TableSize
        {
            get { return Size; }
        }
        public int TableCount
        {
            get { return 1; }
        }
        public void Get(BinaryWriter writer)
        {
            foreach (var a in List)
            {
                writer.Write(a);
                writer.Write(new byte[Utilities.Utilities.Alignment(a.Length, 0x10)]);
            }
        }
        public TypeMap Type
        {
            get { return TypeMap.EPL; }
        }
    }
}