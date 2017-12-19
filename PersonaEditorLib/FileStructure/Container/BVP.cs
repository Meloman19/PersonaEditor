using System;
using System.Collections.Generic;
using System.IO;
using PersonaEditorLib.Extension;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorLib.FileStructure.Container
{
    public class BVP : IPersonaFile
    {
        List<int> ListUnknown = new List<int>();
        public List<ObjectFile> List = new List<ObjectFile>();

        public BVP(string path)
        {
            Name = Path.GetFileName(path);
            Open(File.ReadAllBytes(path));
        }

        public BVP(string name, byte[] data)
        {
            Name = name;
            Open(data);
        }

        private void Open(byte[] data)
        {
            using (BinaryReader reader = Utilities.IO.OpenReadFile(new MemoryStream(data), IsLittleEndian))
            {
                List<int[]> Entry = new List<int[]>();
                int[] temp = reader.ReadInt32Array(3);

                while (temp[1] != 0)
                {
                    Entry.Add(temp);
                    ListUnknown.Add(temp[0]);
                    temp = reader.ReadInt32Array(3);
                }

                for (int i = 0; i < Entry.Count; i++)
                {
                    reader.BaseStream.Position = Entry[i][1];
                    string name = Path.GetFileNameWithoutExtension(Name) + "(" + i.ToString().PadLeft(3, '0') + ").BMD";
                    List.Add(Utilities.PersonaFile.OpenFile(name, reader.ReadBytes(Entry[i][2]), FileType.BMD));
                }
            }
        }

        public int Count
        {
            get { return List.Count; }
        }

        public object this[int index]
        {
            get
            {
                if (List.Count > index)
                    return List[index].Object;

                return null;
            }
            set
            {
                if (List.Count > index)
                    List[index].Object = value;
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public string Name { get; private set; } = "";

        public FileType Type => FileType.BVP;

        public List<ObjectFile> GetSubFiles()
        {
            return List;
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

                foreach (IFile a in List)
                    returned += a.Size;

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned = null;

            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(new MemoryStream(), IsLittleEndian))
            {
                writer.Write(new byte[ListUnknown.Count * 12]);
                writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);

                List<int[]> Entry = new List<int[]>();

                for (int i = 0; i < List.Count; i++)
                {
                    var temp = List[i] as IFile;
                    Entry.Add(new int[] { ListUnknown[i], (int)writer.BaseStream.Position, temp.Size });

                    writer.Write(temp.Size);
                    writer.Write(temp.Get());
                    writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);
                }

                writer.BaseStream.Position = 0;

                foreach (var a in Entry)
                    writer.WriteInt32Array(a);

                writer.BaseStream.Position = 0;
                returned = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(returned, 0, returned.Length);
            }

            return returned;
        }

        #endregion IFile
    }
}