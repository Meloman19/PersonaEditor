using System.IO;

namespace PersonaEditorLib.Other
{
    public class FNTCompressedHeader
    {
        public FNTCompressedHeader(BinaryReader reader)
        {
            HeaderSize = reader.ReadInt32();
            DictionarySize = reader.ReadInt32();
            CompressedBlockSize = reader.ReadInt32();
            Unknown = reader.ReadInt32();
            BytesPerGlyph = reader.ReadUInt16();
            UnknownU = reader.ReadUInt16();
            GlyphTableCount = reader.ReadInt32();
            GlyphPosTableSize = reader.ReadInt32();
            UncompressedFontSize = reader.ReadInt32();
        }

        public int HeaderSize { get; set; }
        public int DictionarySize { get; set; }
        public int CompressedBlockSize { get; set; }
        public int Unknown { get; set; }
        public ushort BytesPerGlyph { get; set; }
        public ushort UnknownU { get; set; }
        public int GlyphTableCount { get; set; }
        public int GlyphPosTableSize { get; set; }
        public int UncompressedFontSize { get; set; }

        public void Resize(int size)
        {
            GlyphTableCount = size + 1;
            GlyphPosTableSize = GlyphTableCount * 4;
            UncompressedFontSize = size * BytesPerGlyph;
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(HeaderSize);
            writer.Write(DictionarySize);
            writer.Write(CompressedBlockSize);
            writer.Write(Unknown);
            writer.Write(BytesPerGlyph);
            writer.Write(UnknownU);
            writer.Write(GlyphTableCount);
            writer.Write(GlyphPosTableSize);
            writer.Write(UncompressedFontSize);
        }
    }
}