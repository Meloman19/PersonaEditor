using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.PM1
{
    class PM1ElementEPLHead : IPM1Element
    {
        public class Structure
        {
            public int Index;
            public int IndexIntertal;
            public int Position;

            public Structure(BinaryReader reader)
            {
                Index = reader.ReadInt32();
                Position = reader.ReadInt32();
                IndexIntertal = reader.ReadInt32();
                int temp = reader.ReadInt32();
                if (temp != 0) throw new Exception("PM1ElementEPLHead -> Structure Init -> LastInt != 0");
            }
        }

        private int _Size = 0;

        public List<Structure> List { get; private set; } = new List<Structure>();

        public PM1ElementEPLHead(BinaryReader reader, PM1Table.Element element)
        {
            List.Clear();
            _Size = element.Size;
            reader.BaseStream.Position = element.Position;
            for (int i = 0; i < element.Count; i++)
                List.Add(new Structure(reader));
        }

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
                writer.Write(a.Index);
                writer.Write(a.Position);
                writer.Write(a.IndexIntertal);
                writer.Write((int)0);
            }
        }
        public TypeMap Type
        {
            get { return TypeMap.EPLHead; }
        }
    }
}