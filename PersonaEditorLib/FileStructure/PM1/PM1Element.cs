using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.FileStructure.PM1
{
    class PM1Element : IPM1Element
    {
        List<byte[]> List = new List<byte[]>();

        private int _Size;
        private TypeMap _Type;

        public PM1Element(BinaryReader reader, PM1Table.Element element)
        {
            _Size = element.Size;
            _Type = (TypeMap)element.Index;
            reader.BaseStream.Position = element.Position;
            for (int i = 0; i < element.Count; i++)
                List.Add(reader.ReadBytes(element.Size));
        }

        public int Size { get { return _Size * List.Count; } }
        public int Count { get { return List.Count; } }
        public int TableSize { get { return _Size; } }
        public int TableCount { get { return List.Count; } }

        public void Get(BinaryWriter writer)
        {
            foreach (var a in List)
            {
                writer.Write(a);
                writer.Write(new byte[Utilities.Utilities.Alignment(a.Length, 0x10)]);
            }
        }
        public TypeMap Type { get { return _Type; } }
    }
}