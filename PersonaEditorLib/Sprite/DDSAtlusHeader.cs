using AuxiliaryLibraries;
using AuxiliaryLibraries.Extensions;
using System;
using System.IO;
using System.Linq;

namespace PersonaEditorLib.Sprite
{
    public class DDSAtlusHeader
    {
        public int Size { get; } = 124;

        public int HeaderFlags { get; } // Header flags?
        public int SizeWOHeader { get; set; } // Size texture without header
        public int Unknown0x08 { get; } // Version?
        public int Unknown0x0C { get; } // Padding?

        public int HeaderSize { get; }
        public int SizeTexture { get; set; }
        public DDSAtlusPixelFormat PixelFormat { get; }
        public byte MipMapCount { get; }
        public byte Unknown0x1A { get; }
        public byte TileByte { get; private set; }
        public uint UnknownFlags { get; }

        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public uint Unknown0x24 { get; }
        public int[] Reserved { get; }

        public int TileCount { get; } = 1;
        public bool Tile { get; private set; }

        private void CheckHeader()
        {
            if (Unknown0x08 != 0x1) throw new Exception("DDSHeaderV2: exception 0x08");
            if (Unknown0x0C != 0x0) throw new Exception("DDSHeaderV2: exception 0x0C");
            if (HeaderSize != 0x80) throw new Exception("DDSHeaderV2: exception 0x10");
            if (Unknown0x1A != 0x02) throw new Exception("DDSHeaderV2: exception 0x1A");
            if (Unknown0x24 != 0x10000) throw new Exception("DDSHeaderV2: exception 0x24");
            if (Reserved.Contains<int>(0, new ReverseStructComparer<int>()))
                throw new Exception("DDSHeaderV2: exception 0x28 array");
            
        }

        public DDSAtlusHeader(BinaryReader reader)
        {
            // 0x00-0x10
            HeaderFlags = reader.ReadInt32();
            SizeWOHeader = reader.ReadInt32();
            Unknown0x08 = reader.ReadInt32();
            Unknown0x0C = reader.ReadInt32();

            // 0x10-0x20
            HeaderSize = reader.ReadInt32();
            SizeTexture = reader.ReadInt32();
            PixelFormat = (DDSAtlusPixelFormat)reader.ReadByte();
            MipMapCount = reader.ReadByte();
            Unknown0x1A = reader.ReadByte();
            TileByte = reader.ReadByte();
            UnknownFlags = reader.ReadUInt32();

            // 0x20-0x80
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Unknown0x24 = reader.ReadUInt32();
            Reserved = reader.ReadInt32Array(22);

            CheckHeader();

            GetTile(TileByte);

            if (Tile)
            {
                int BytePerBlock = PixelFormat == DDSAtlusPixelFormat.DXT1 ? 8 : 16;
                int size = Width * Height * BytePerBlock / 16;
                TileCount = SizeTexture % size == 0 ? SizeTexture / size : throw new Exception("DDSAtlus: tile read error");
            }

        }

        public byte[] Get()
        {
            byte[] returned = null;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new AuxiliaryLibraries.IO.BinaryWriterEndian(MS))
            {
                writer.Write(HeaderFlags);
                writer.Write(SizeWOHeader);
                writer.Write(Unknown0x08);
                writer.Write(Unknown0x0C);

                writer.Write(HeaderSize);
                writer.Write(SizeWOHeader);
                writer.Write((byte)PixelFormat);
                writer.Write(MipMapCount);
                writer.Write(Unknown0x1A);
                writer.Write(TileByte);
                writer.Write(UnknownFlags);

                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Unknown0x24);
                writer.WriteInt32Array(Reserved);

                returned = MS.ToArray();
            }

            return returned;
        }

        private void GetTile(byte tile)
        {
            if (tile == 0)
                Tile = false;
            else if (tile == 1)
                Tile = true;
            else
                throw new Exception("DDSHeaderV2: exception 0x1B");
        }
    }
}