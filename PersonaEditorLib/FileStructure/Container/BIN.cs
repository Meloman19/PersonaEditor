using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorLib.FileStructure.Container
{
    public class BIN : IPersonaFile
    {
        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public BIN(string path)
        {
            Open(File.ReadAllBytes(path));
        }

        public BIN(byte[] data)
        {
            Open(data);
        }

        private void Open(byte[] data)
        {
            if (data[0] == 0)
                OpenBE(data);
            else
                OpenLE(data);
        }

        private void OpenLE(byte[] data)
        {
            IsLittleEndian = true;
            using (BinaryReader reader = Utilities.IO.OpenReadFile(new MemoryStream(data), true))
                while (reader.BaseStream.Position < reader.BaseStream.Length - 0x100)
                {
                    string Name = Encoding.ASCII.GetString(reader.ReadBytes(0x100 - 4)).Trim('\0');
                    int Size = reader.ReadInt32();
                    byte[] Data = reader.ReadBytes(Size);
                    reader.BaseStream.Position += Utilities.Utilities.Alignment(reader.BaseStream.Position, 0x40);
                    SubFiles.Add(Utilities.PersonaFile.OpenFile(Name, Data, Utilities.PersonaFile.GetFileType(Name)));
                }
        }

        private void OpenBE(byte[] data)
        {
            IsLittleEndian = false;
            using (BinaryReader reader = Utilities.IO.OpenReadFile(new MemoryStream(data), false))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string Name = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).Trim('\0');
                    int Size = reader.ReadInt32();
                    byte[] Data = reader.ReadBytes(Size);
                    SubFiles.Add(Utilities.PersonaFile.OpenFile(Name, Data, Utilities.PersonaFile.GetFileType(Name)));
                }
            }
        }

        public object this[int index]
        {
            get
            {
                if (SubFiles.Count > index)
                {
                    return SubFiles[index];
                }
                return null;
            }
            set
            {
                if (SubFiles.Count > index)
                {
                    SubFiles[index].Object = value;
                }
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public FileType Type => FileType.BIN;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
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

                returned.Add("Entry Count", SubFiles.Count);
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

                foreach (IFile a in SubFiles)
                    returned += a.Size;

                return returned;
            }
        }

        public byte[] Get()
        {
            if (IsLittleEndian)
                return GetLE();
            else
                return GetBE();
        }

        #endregion IFile

        private byte[] GetLE()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                foreach (var a in SubFiles)
                    if (a.Object is IPersonaFile pfile)
                    {
                        byte[] name = new byte[0x100 - 4];
                        Encoding.ASCII.GetBytes(a.Name, 0, a.Name.Length, name, 0);
                        writer.Write(name);
                        writer.Write(pfile.Size);
                        writer.Write(pfile.Get());
                        writer.Write(new byte[Utilities.Utilities.Alignment(MS.Position, 0x40)]);
                    }

                writer.Write(new byte[0x100]);

                return MS.ToArray();
            }
        }

        private byte[] GetBE()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                writer.Write((int)SubFiles.Count);
                foreach (var a in SubFiles)
                    if (a is IFile file)
                        if (a is IPersonaFile pfile)
                        {
                            writer.Write(Encoding.ASCII.GetBytes(a.Name));
                            writer.Write(new byte[Utilities.Utilities.Alignment(a.Name.Length, 0x20)]);
                            writer.Write(file.Size);
                            writer.Write(file.Get());
                        }

                return MS.ToArray();
            }
        }
    }
}