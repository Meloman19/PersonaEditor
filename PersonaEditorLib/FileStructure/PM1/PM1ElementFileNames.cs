using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.PM1
{
    class PM1ElementFileNames : IPM1Element
    {
        public PM1ElementFileNames(BinaryReader reader, PM1Table.Element element)
        {
            List.Clear();
            _Size = element.Size;
            reader.BaseStream.Position = element.Position;
            for (int i = 0; i < element.Count; i++)
                List.Add(Encoding.ASCII.GetString(reader.ReadBytes(element.Size).Where(x => x != 0).ToArray()));
        }

        private int _Size = 0;

        public List<string> List { get; private set; } = new List<string>();

        public int Size
        {
            get { return _Size * List.Count; }
        }
        public int Count
        {
            get { return List.Count; }
        }
        public int TableSize
        {
            get { return _Size; }
        }
        public int TableCount
        {
            get { return Count; }
        }

        public void Get(BinaryWriter writer)
        {
            foreach (var a in List)
            {
                writer.Write(Encoding.ASCII.GetBytes(a));
                writer.Write(new byte[Utilities.Utilities.Alignment(a.Length, 0x20)]);
            }
        }
        public TypeMap Type
        {
            get { return TypeMap.FileList; }
        }
    }
}