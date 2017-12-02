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
    public class BF //: IPersonaFile, IFile
    {
        BFHeader Header;
        BFTable Table;

        List<BFElement> List = new List<BFElement>();

        public BF(string path, bool IsLittleEndian) : this(File.OpenRead(path), IsLittleEndian)
        {

        }

        public BF(Stream Stream, bool IsLittleEndian)
        {
            BinaryReader reader = Utilities.IO.OpenReadFile(Stream, IsLittleEndian);

            Open(reader);
        }

        public BF(byte[] data, bool IsLittleEndian)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Open(Utilities.IO.OpenReadFile(MS, IsLittleEndian));
        }

        private void Open(BinaryReader reader)
        {
            Header = new BFHeader(reader);
            Table = new BFTable(reader.ReadInt32ArrayArray(Header.TableLineCount, 4));

            foreach (var element in Table.Table)
                if (element.Count * element.Size > 0)
                    List.Add(new BFElement(reader, element));
        }

        public byte[] GetBMD()
        {
            var temp = List.Find(x => x.Type == FileType.BMD);
            return temp == null ? new byte[0] : temp.Count == 0 ? new byte[0] : (temp as BFElement).List[0].ToArray();
        }               

        public void SetBMD(byte[] bmd)
        {
            var temp = List.Find(x => x.Type == FileType.BMD) as BFElement;
            if (temp != null)
            {
                temp.List.Clear();
                temp.List.Add(bmd);
            }
        }

        #region IPersonaFile

        public FileType Type => FileType.BF;

        public List<TreeItem> GetTreeItemsList()
        {
            List<TreeItem> returned = new List<TreeItem>();

            returned.Add(new TreeItem()
            {
                Data = GetBMD(),
                Type = FileType.BMD
            });

            return returned;
        }

        public void SetTreeItemsList(List<Tuple<string, byte[]>> list)
        {
            SetBMD(list[0].Item2);
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

                returned.Add("Entry Count", List.Count);
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
                List.ForEach(x => returned += x.Size);

                return returned;
            }
        }

        public string Name => throw new NotImplementedException();

        public byte[] Get(bool IsLittleEndian)
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                Get(writer);

                returned = MS.ToArray();
            }

            return returned;
        }

        public void Get(BinaryWriter writer)
        {
            Table.Update(List);

            Header.FileSize = Header.Size + Table.Size;
            List.ForEach(x => Header.FileSize += x.Size);

            Header.Get(writer);
            Table.Get(writer);

            List.OrderBy(x => x.Type).ToList().ForEach(x => x.Get(writer));
        }

        public List<object> GetSubFiles()
        {
            throw new NotImplementedException();
        }

        public bool Replace(object a)
        {
            throw new NotImplementedException();
        }

        #endregion IFile
    }
}