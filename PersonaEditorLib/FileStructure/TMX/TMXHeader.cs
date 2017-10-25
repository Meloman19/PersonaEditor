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
        int FileSize;
        byte[] Tag;
        byte PaletteCount;
        byte PaletteFormat;
        public ushort Width;
        public ushort Height;
        public PS2PixelFormat PixelFormat;
        byte MinMap;
        ushort mipKL;
        byte Reserved;

        byte mWrapModes;
        int UserTextureId;
        int UserClutId;
        public byte[] UserComment;

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
    }
}