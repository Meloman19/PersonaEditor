using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.TMX
{
    public class TMXHeader
    {
        int ID;
        public int FileSize { get; set; }
        byte[] Tag;
        byte PaletteCount;
        byte PaletteFormat;
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public PS2PixelFormat PixelFormat { get; private set; }
        byte MinMap;
        ushort mipKL;
        byte Reserved;

        byte mWrapModes;
        int UserTextureId;
        int UserClutId;
        public byte[] UserComment { get; private set; }

        public TMXHeader(BinaryReader reader)
        {
            ID = reader.ReadInt32();
            FileSize = reader.ReadInt32();
            Tag = reader.ReadBytes(8);

            PaletteCount = reader.ReadByte();
            if (PaletteCount > 1)
            {

            }
            PaletteFormat = reader.ReadByte();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            PixelFormat = (PS2PixelFormat)reader.ReadByte();
            MinMap = reader.ReadByte();
            mipKL = reader.ReadUInt16();
            Reserved = reader.ReadByte();
            mWrapModes = reader.ReadByte();
            UserTextureId = reader.ReadInt32();

            UserClutId = reader.ReadInt32();
            UserComment = reader.ReadBytes(28);
        }

        public int Size
        {
            get { return 64; }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(ID);
            writer.Write(FileSize);
            writer.Write(Tag);
            writer.Write(PaletteCount);
            writer.Write(PaletteFormat);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write((byte)PixelFormat);
            writer.Write(MinMap);
            writer.Write(mipKL);
            writer.Write(Reserved);
            writer.Write(mWrapModes);
            writer.Write(UserTextureId);

            writer.Write(UserClutId);
            writer.Write(UserComment);
        }
    }
}