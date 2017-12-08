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
        string _Name = "";

        SPRHeader Header;
        List<int> TextureOffsetList = new List<int>();
        List<int> KeyOffsetList = new List<int>();
        public SPRKeyList KeyList;

        public List<object> TextureList { get; private set; } = new List<object>();

        public SPR(string name, Stream stream, bool IsLittleEndian)
        {
            _Name = name;
            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            Open(reader);
        }

        public SPR(string path, bool IsLittleEndian) : this(Path.GetFileName(path), File.OpenRead(path), IsLittleEndian)
        {
        }

        public SPR(string name, byte[] data, bool IsLittleEndian)
        {
            _Name = name;

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
                TextureList.Add(new TMX.TMX(reader.BaseStream, a, true));
        }

        private void UpdateOffsets(List<int> list, int start)
        {
            list[0] = start;

            for (int i = 1; i < TextureList.Count; i++)
            {
                start += (TextureList[i - 1] as IFile).Size;
                int temp = Utilities.Utilities.Alignment(start, 16);
                start += temp == 0 ? 16 : temp;
                list[i] = start;
            }
        }

        #region IPersonaFile

        public string Name => _Name;

        public FileType Type => FileType.SPR;

        public List<object> GetSubFiles()
        {
            return TextureList;
        }

        public bool Replace(object newdata)
        {
            if (newdata is SPR spr)
            {
                Header = spr.Header;
                TextureOffsetList = spr.TextureOffsetList;
                KeyOffsetList = spr.KeyOffsetList;
                KeyList = spr.KeyList;
                TextureList = spr.TextureList;

                return true;
            }

            return false;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.Export);
                returned.Add(ContextMenuItems.Import);

                return returned;
            }
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Texture Count", TextureList.Count);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public bool IsLittleEndian { get; set; } = true;

        public int Size
        {
            get
            {
                int returned = 0;

                returned += Header.Size;
                returned += TextureOffsetList.Count * 4;
                returned += KeyOffsetList.Count * 4;
                returned += KeyList.Size;
                returned += (TextureList[0] as IFile).Size;
                for (int i = 1; i < TextureList.Count; i++)
                {
                    int temp = Utilities.Utilities.Alignment(returned, 16);
                    returned += temp == 0 ? 16 : temp;
                    returned += (TextureList[i] as IFile).Size;
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

                writer.Write((TextureList[0] as IFile).Get());
                for (int i = 1; i < TextureList.Count; i++)
                {
                    int temp2 = Utilities.Utilities.Alignment(writer.BaseStream.Length, 16);
                    writer.Write(new byte[temp2 == 0 ? 16 : temp2]);
                    writer.Write((TextureList[i] as IFile).Get());
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