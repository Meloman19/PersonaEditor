using System.IO;

namespace PersonaEditorLib.SpriteContainer
{
    class SPRHeader
    {
        public ushort sprID;
        public ushort userID;
        public int unusedLength;
        public byte[] Tag;
        public int headerlength;
        public int filesize;
        public ushort TextureCount;
        public ushort KeyFrameCount;
        public int TextureOffset;
        public int KeyFrameOffset;

        public SPRHeader(BinaryReader reader)
        {
            sprID = reader.ReadUInt16();
            userID = reader.ReadUInt16();
            unusedLength = reader.ReadInt32();
            Tag = reader.ReadBytes(4);
            headerlength = reader.ReadInt32();

            filesize = reader.ReadInt32();
            TextureCount = reader.ReadUInt16();
            KeyFrameCount = reader.ReadUInt16();
            TextureOffset = reader.ReadInt32();
            KeyFrameOffset = reader.ReadInt32();
        }

        public int Size
        {
            get { return 0x20; }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(sprID);
            writer.Write(userID);
            writer.Write(unusedLength);
            writer.Write(Tag);
            writer.Write(headerlength);
            writer.Write(filesize);
            writer.Write(TextureCount);
            writer.Write(KeyFrameCount);
            writer.Write(TextureOffset);
            writer.Write(KeyFrameOffset);
        }
    }
}