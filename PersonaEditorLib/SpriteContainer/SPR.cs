using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PersonaEditorLib.SpriteContainer
{
    public class SPR : IGameData
    {
        SPRHeader Header;
        List<int> TextureOffsetList = new List<int>();
        List<int> KeyOffsetList = new List<int>();
        public SPRKeyList KeyList;

        public SPR(Stream stream)
        {
            BinaryReader reader = IOTools.OpenReadFile(stream, IsLittleEndian);

            Open(reader);
        }

        public SPR(string path) : this(File.OpenRead(path))
        {
        }

        public SPR(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Open(IOTools.OpenReadFile(MS, IsLittleEndian));
        }

        private void Open(BinaryReader reader)
        {
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

            foreach (var a in TextureOffsetList)
            {
                var tmx = new Sprite.TMX(new StreamPart(reader.BaseStream, -1, a));
                SubFiles.Add(new GameFile(tmx.Comment + ".tmx", tmx));
            }
        }

        private void UpdateOffsets(List<int> list, int? start)
        {
            if (!start.HasValue)
                return;

            var offset = start.Value;

            for (int i = 0; i < SubFiles.Count; i++)
            {
                if (i == 0)
                {
                    list[0] = offset;
                }
                else
                {
                    offset += SubFiles[i - 1].GameData.GetSize();
                    int temp = IOTools.Alignment(offset, 16);
                    offset += temp == 0 ? 16 : temp;
                    list[i] = offset;
                }
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IGameFile

        public FormatEnum Type => FormatEnum.SPR;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            int returned = 0;

            returned += Header.Size;
            returned += TextureOffsetList.Count * 8;
            returned += KeyOffsetList.Count * 8;
            returned += KeyList.Size;

            for (int i = 0; i < SubFiles.Count; i++)
            {
                int temp = IOTools.Alignment(returned, 16);
                returned += temp == 0 ? 16 : temp;
                returned += SubFiles[i].GameData.GetSize();
            }

            return returned;
        }

        public byte[] GetData()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

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

                int? startTextureOffset = null;

                for (int i = 0; i < SubFiles.Count; i++)
                {
                    int temp = IOTools.Alignment(writer.BaseStream.Position, 16);
                    writer.Write(new byte[temp == 0 ? 16 : temp]);
                    startTextureOffset ??= (int)writer.BaseStream.Position;
                    writer.Write(SubFiles[i].GameData.GetData());
                }

                UpdateOffsets(TextureOffsetList, startTextureOffset);

                writer.BaseStream.Position = Header.Size;
                foreach (var a in TextureOffsetList)
                {
                    writer.Write((int)0);
                    writer.Write(a);
                }

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion
    }
}
