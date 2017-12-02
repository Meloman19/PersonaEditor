using System.Collections.Generic;
using System.IO;
using PersonaEditorLib.Interfaces;
using System;

namespace PersonaEditorLib.FileStructure.BF
{
    class BFElement
    {
        public List<byte[]> List = new List<byte[]>();

        private int _Size;
        private FileType _Type = FileType.HEX;
        public int Index { get; set; }

        public BFElement(BinaryReader reader, BFTable.Element element)
        {
            _Size = element.Size;
            Index = element.Index;
            if (BFTable.MAP.Find(x => x.Item2 == element.Index) is Tuple<FileType, int> a)
                _Type = a.Item1;

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
        public FileType Type { get { return _Type; } }
    }
}