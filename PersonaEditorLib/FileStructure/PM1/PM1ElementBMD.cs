using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.PM1
{
    class PM1ElementBMD : IPM1Element
    {
        public byte[] BMD { get; set; }
        // public MemoryStream BMD { get; set; } = new MemoryStream();

        public PM1ElementBMD(BinaryReader reader, PM1Table.Element element)
        {
            reader.BaseStream.Position = element.Position;
            BMD = reader.ReadBytes(element.Size);
        }

        public int Size
        {
            get { return BMD.Length + Utilities.Alignment(BMD.Length, 0x10); }
        }
        public int Count
        {
            get { return 1; }
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
            writer.Write(BMD);
            writer.Write(new byte[Utilities.Alignment(BMD.Length, 0x10)]);
        }
        public TypeMap Type
        {
            get { return TypeMap.BMD; }
        }
    }
}