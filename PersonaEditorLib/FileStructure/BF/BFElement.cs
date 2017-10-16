using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.FileStructure.BF
{
    class BFElement : IBFElement
    {
        public List<byte[]> List = new List<byte[]>();

        private int _Size;
        private TypeMap _Type;

        public BFElement(BinaryReader reader, BFTable.Element element)
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
            }
        }
        public TypeMap Type { get { return _Type; } }
    }
}