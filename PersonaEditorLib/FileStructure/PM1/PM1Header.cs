using System.IO;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.PM1
{
    class PM1Header
    {
        public int EmptyHead { get; set; }
        public int FileSize { get; set; }
        public byte[] Name { get; set; }
        public int TableLineCount { get; set; }
        private byte[] Empty { get; set; }

        public PM1Header(BinaryReader reader)
        {
            EmptyHead = reader.ReadInt32();
            FileSize = reader.ReadInt32();
            Name = reader.ReadBytes(8);
            TableLineCount = reader.ReadInt32();
            Empty = reader.ReadBytes(12);
        }

        public PM1Header()
        {

        }
        
        public int Size
        {
            get
            {
                return 0x20;
            }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(EmptyHead);
            writer.Write(FileSize);
            writer.Write(Name);
            writer.Write(TableLineCount);
            writer.Write(Empty);
        }
    }
}