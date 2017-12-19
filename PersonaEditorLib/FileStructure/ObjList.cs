using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure
{
    class ObjList : IPersonaFile
    {
        public List<ObjectFile> list { get; } = new List<ObjectFile>();

        public ObjList(byte[] array, int size, int count)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(array)))
                for (int i = 0; i < count; i++)
                {
                    string name = i.ToString().PadLeft(2, '0');
                    list.Add(Utilities.PersonaFile.OpenFile(name, reader.ReadBytes(size), FileType.HEX));
                }
        }

        #region IPersonaFile

        public FileType Type => FileType.ObjList;

        public List<ObjectFile> GetSubFiles()
        {
            return list;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.Replace);
                returned.Add(ContextMenuItems.Separator);
                returned.Add(ContextMenuItems.SaveAs);
                returned.Add(ContextMenuItems.SaveAll);

                return returned;
            }
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Entry Count", list.Count);
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
                foreach (IFile file in list)
                    returned += file.Size;
                return returned;
            }
        }

        public byte[] Get()
        {
            List<byte> returned = new List<byte>();

            foreach (IFile file in list)
                returned.AddRange(file.Get());

            return returned.ToArray();
        }

        #endregion IFile

        public object Tag { get; set; }
    }
}