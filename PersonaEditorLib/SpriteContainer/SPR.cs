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

        private void UpdateOffsets(List<int> list, int start)
        {
            list[0] = start;

            for (int i = 1; i < SubFiles.Count; i++)
            {
                start += SubFiles[i - 1].GameData.GetSize();
                int temp = IOTools.Alignment(start, 16);
                start += temp == 0 ? 16 : temp;
                list[i] = start;
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

            int temp = IOTools.Alignment(returned, 16);
            returned += temp == 0 ? 16 : temp;

            returned += (SubFiles[0].GameData as IGameData).GetSize();
            for (int i = 1; i < SubFiles.Count; i++)
            {
                temp = IOTools.Alignment(returned, 16);
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

                int temp = IOTools.Alignment(writer.BaseStream.Position, 16);
                writer.Write(new byte[temp == 0 ? 16 : temp]);

                UpdateOffsets(TextureOffsetList, (int)writer.BaseStream.Position);

                writer.Write(SubFiles[0].GameData.GetData());
                for (int i = 1; i < SubFiles.Count; i++)
                {
                    int temp2 = IOTools.Alignment(writer.BaseStream.Length, 16);
                    writer.Write(new byte[temp2 == 0 ? 16 : temp2]);
                    writer.Write(SubFiles[i].GameData.GetData());
                }

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
