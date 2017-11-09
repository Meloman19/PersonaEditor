using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.SPR
{
    interface ISPR
    {
        void Get(BinaryWriter writer);
        int Size { get; }
    }

    public class SPR
    {
        SPRHeader Header;
        List<int> TextureOffsetList = new List<int>();
        List<int> KeyOffsetList = new List<int>();
        public SPRKeyList KeyList;
        SPRTextures Textures;

        public SPR(Stream stream, bool IsLittleEndian)
        {
            try
            {
                BinaryReader reader;

                if (IsLittleEndian)
                    reader = new BinaryReader(stream);
                else
                    reader = new BinaryReaderBE(stream);

                Header = new SPRHeader(reader);
                for (int i = 0; i < Header.TextureCount; i++)
                {
                    reader.ReadUInt32();
                    TextureOffsetList.Add(reader.ReadInt32());
                }
                for (int i = 0; i < Header.KeyFrameCount; i++)
                {
                    reader.ReadUInt32();
                    KeyOffsetList.Add(reader.ReadInt32());
                }
                KeyList = new SPRKeyList(reader, Header.KeyFrameCount);
                Textures = new SPRTextures(reader, TextureOffsetList);
            }
            catch (Exception e)
            {
                Logging.Write("L", e);
            }
        }

        public SPR(string path, bool IsLittleEndian) : this(File.OpenRead(path), IsLittleEndian)
        {
        }

        public List<byte[]> GetTextureList()
        {
            return Textures.List;
        }

        public byte[] Get(bool IsLittleEndian)
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer;

                if (IsLittleEndian)
                    writer = new BinaryWriter(MS);
                else
                    writer = new BinaryWriterBE(MS);

                Header.Get(writer);
                foreach (var a in TextureOffsetList)
                {
                    writer.Write((int)0);
                    writer.Write(a);
                }
                foreach (var a in KeyOffsetList)
                {
                    writer.Write((int)0);
                    writer.Write(a);
                }
                KeyList.Get(writer);
                int temp = Utilities.Utilities.Alignment(writer.BaseStream.Position, 16);
                writer.Write(new byte[temp == 0 ? 16 : temp]);
                Textures.Get(writer);

                returned = MS.ToArray();
            }

            return returned;
        }
    }
}