using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.SPR
{
    public class SPR : IPersonaFile, IFile
    {
        SPRHeader Header;
        List<int> TextureOffsetList = new List<int>();
        List<int> KeyOffsetList = new List<int>();
        public SPRKeyList KeyList;

        public SPR(Stream stream)
        {
            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            Open(reader);
        }

        public SPR(string path) : this(File.OpenRead(path))
        {
        }

        public SPR(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Open(Utilities.IO.OpenReadFile(MS, IsLittleEndian));
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
                var tmx = new Graphic.TMX(reader.BaseStream, a);
                SubFiles.Add(new ObjectFile(tmx.Name, tmx));
            }
        }

        private void UpdateOffsets(List<int> list, int start)
        {
            list[0] = start;

            for (int i = 1; i < SubFiles.Count; i++)
            {
                start += (SubFiles[i - 1].Object as IFile).Size;
                int temp = Utilities.Utilities.Alignment(start, 16);
                start += temp == 0 ? 16 : temp;
                list[i] = start;
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public FileType Type => FileType.SPR;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Texture Count", SubFiles.Count);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size
        {
            get
            {
                int returned = 0;

                returned += Header.Size;
                returned += TextureOffsetList.Count * 8;
                returned += KeyOffsetList.Count * 8;
                returned += KeyList.Size;

                int temp = Utilities.Utilities.Alignment(returned, 16);
                returned += temp == 0 ? 16 : temp;

                returned += (SubFiles[0].Object as IFile).Size;
                for (int i = 1; i < SubFiles.Count; i++)
                {
                    temp = Utilities.Utilities.Alignment(returned, 16);
                    returned += temp == 0 ? 16 : temp;
                    returned += (SubFiles[i].Object as IFile).Size;
                }

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

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

                UpdateOffsets(TextureOffsetList, (int)writer.BaseStream.Position);

                writer.Write((SubFiles[0].Object as IFile).Get());
                for (int i = 1; i < SubFiles.Count; i++)
                {
                    int temp2 = Utilities.Utilities.Alignment(writer.BaseStream.Length, 16);
                    writer.Write(new byte[temp2 == 0 ? 16 : temp2]);
                    writer.Write((SubFiles[i].Object as IFile).Get());
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

        #endregion IFile
    }
}