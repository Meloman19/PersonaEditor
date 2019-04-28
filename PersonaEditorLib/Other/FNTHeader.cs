using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PersonaEditorLib.Other
{
    public class Glyph
    {
        public Glyph(BinaryReader reader)
        {
            Count = reader.ReadUInt16();
            Size1 = reader.ReadUInt16();
            Size2 = reader.ReadUInt16();
            ByteSize = reader.ReadUInt16();
        }

        public ushort Count { get; set; }
        public ushort Size1 { get; set; }
        public ushort Size2 { get; set; }
        public ushort ByteSize { get; set; }

        public byte BitsPerPixel { get { return Convert.ToByte((double)(ByteSize * 8) / (Size1 * Size2)); } }
        public int NumberOfColor { get { return Convert.ToInt32(Math.Pow(2, BitsPerPixel)); } }

        public void Get(BinaryWriter writer)
        {
            writer.Write(Count);
            writer.Write(Size1);
            writer.Write(Size2);
            writer.Write(ByteSize);
        }
    }

    public class FNTHeader
    {
        public FNTHeader(BinaryReader reader)
        {
            HeaderSize = reader.ReadInt32();
            FileSize = reader.ReadInt32();
            UnknownH = reader.ReadBytes(6);
            Glyphs = new Glyph(reader);
            UnknownUShort = reader.ReadUInt16();
            LastPosition = reader.ReadInt32();
        }

        public int HeaderSize { get; set; }
        public int FileSize { get; set; }
        public byte[] UnknownH { get; set; }
        public Glyph Glyphs { get; set; }
        public ushort UnknownUShort { get; set; }
        public int LastPosition { get; set; }

        public void Resize(int size)
        {
            Glyphs.Count = (ushort)size;
        }

        public int Size()
        {
            return HeaderSize;
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(HeaderSize);
            writer.Write(FileSize);
            writer.Write(UnknownH);
            Glyphs.Get(writer);
            writer.Write(UnknownUShort);
            writer.Write(LastPosition);
            writer.BaseStream.Position = HeaderSize;
        }
    }
}
