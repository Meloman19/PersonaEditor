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
        List<int> FlagList = new List<int>();
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

                do
                {
                    Entry.Add(reader.ReadInt32Array(3));
                } while (Entry[Entry.Count - 1][1] != 0);

                for (int i = 0; i < Entry.Count - 1; i++)
                {
                    FlagList.Add(Entry[i][0]);
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
                return Get().Length;
            }
        }

        public byte[] Get()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian))
            {
                writer.BaseStream.Position = (List.Count + 1) * 12;

                List<int[]> Entry = new List<int[]>();

                for (int i = 0; i < List.Count; i++)
                {
                    var temp = List[i].Object as IFile;
                    Entry.Add(new int[] { FlagList[i], (int)writer.BaseStream.Position, temp.Size });

                    writer.Write(temp.Get());
                    writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);
                }

                writer.BaseStream.Position = 0;

                foreach (var a in Entry)
                    writer.WriteInt32Array(a);

                return MS.ToArray();
            }
        }

        #endregion IFile
    }
}