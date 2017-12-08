using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.BF
{
    public class BF : IPersonaFile, IFile
    {
        BFHeader Header;
        BFTable Table;

        List<object> List = new List<object>();

        List<BFElement> List2 = new List<BFElement>();

        public BF(string path)
        {
            using (FileStream FS = File.OpenRead(path))
                Open(FS);

            Name = Path.GetFileName(path);
        }

        public BF(string name, Stream Stream)
        {
            Name = name;
            Open(Stream);
        }

        public BF(string name, byte[] data)
        {
            Name = name;
            using (MemoryStream MS = new MemoryStream(data))
                Open(MS);
        }

        private void Open(Stream stream)
        {
            stream.Position = 0x10;
            if (stream.ReadByte() == 0)
                IsLittleEndian = false;
            else
                IsLittleEndian = true;

            stream.Position = 0;
            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            Header = new BFHeader(reader);
            Table = new BFTable(reader.ReadInt32ArrayArray(Header.TableLineCount, 4));

            foreach (var element in Table.Table)
                if (element.Count * element.Size > 0)
                {
                    reader.BaseStream.Position = element.Position;

                    List.Add(Utilities.PersonaFile.OpenFile(Name, reader.ReadBytes(element.Count * element.Size), element.FileType));

                    List2.Add(new BFElement(reader, element));
                }

        }

        public byte[] GetBMD()
        {
            var temp = List2.Find(x => x.Type == FileType.BMD);
            return temp == null ? new byte[0] : temp.Count == 0 ? new byte[0] : (temp as BFElement).List[0].ToArray();
        }

        public void SetBMD(byte[] bmd)
        {
            var temp = List2.Find(x => x.Type == FileType.BMD) as BFElement;
            if (temp != null)
            {
                temp.List.Clear();
                temp.List.Add(bmd);
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public string Name { get; private set; } = "";

        public FileType Type => FileType.BF;

        public List<object> GetSubFiles()
        {
            return List;
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

                returned.Add("Entry Count", List2.Count);
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

                returned += Header.Size + Table.Size;
                List2.ForEach(x => returned += x.Size);

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                Table.Update(List2);

                Header.FileSize = Header.Size + Table.Size;
                List2.ForEach(x => Header.FileSize += x.Size);

                Header.Get(writer);
                Table.Get(writer);

                List2.OrderBy(x => x.Type).ToList().ForEach(x => x.Get(writer));

                returned = MS.ToArray();
            }

            return returned;
        }

        public bool Replace(object a)
        {
            throw new NotImplementedException();
        }

        #endregion IFile
    }
}