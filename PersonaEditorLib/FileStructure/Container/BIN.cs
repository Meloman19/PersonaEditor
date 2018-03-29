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
            {
                Old = false;
                IsLittleEndian = false;
                OpenNew(data);
            }                
            else if (data[3] == 0 && data[4] != 0)
            {
                Old = false;
                IsLittleEndian = true;
                OpenNew(data);
            }
            else
                OpenOld(data);
        }

        private void OpenOld(byte[] data)
        {
            IsLittleEndian = true;
            if (data.Length < 0x100)
                throw new System.Exception("BIN: data length unacceptable");
            using (BinaryReader reader = Utilities.IO.OpenReadFile(new MemoryStream(data), true))
                while (reader.BaseStream.Position < reader.BaseStream.Length - 0x100)
                {
                    string Name = Encoding.ASCII.GetString(reader.ReadBytes(0x100 - 4)).Trim('\0');
                    int Size = reader.ReadInt32();
                    byte[] Data = reader.ReadBytes(Size);
                    reader.BaseStream.Position += Utilities.Utilities.Alignment(reader.BaseStream.Position, 0x40);

                    ObjectFile objectFile = Utilities.PersonaFile.OpenFile(Name, Data, Utilities.PersonaFile.GetFileType(Name));
                    if (objectFile.Object == null)
                        objectFile = Utilities.PersonaFile.OpenFile(Name, Data, FileType.DAT);
                    SubFiles.Add(objectFile);
                }
        }
        
        private void OpenNew(byte[] data)
        {
            using (BinaryReader reader = Utilities.IO.OpenReadFile(new MemoryStream(data), IsLittleEndian))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string Name = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).Trim('\0');
                    int Size = reader.ReadInt32();
                    byte[] Data = reader.ReadBytes(Size);

                    ObjectFile objectFile = Utilities.PersonaFile.OpenFile(Name, Data, Utilities.PersonaFile.GetFileType(Name));
                    if (objectFile.Object == null)
                        objectFile = Utilities.PersonaFile.OpenFile(Name, Data, FileType.DAT);
                    SubFiles.Add(objectFile);
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

        bool Old = true;
        bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public FileType Type => FileType.BIN;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
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
            if (Old)
                return GetOld();
            else
                return GetNew();
        }

        #endregion IFile

        private byte[] GetOld()
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
                        byte[] data = pfile.Get();
                        int size = pfile.Size;
                        if (data.Length != size)
                        {

                        }
                        writer.Write(size);
                        writer.Write(data);
                        writer.Write(new byte[Utilities.Utilities.Alignment(MS.Position, 0x40)]);
                    }

                writer.Write(new byte[0x100]);

                return MS.ToArray();
            }
        }

        private byte[] GetNew()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                writer.Write((int)SubFiles.Count);
                foreach (var a in SubFiles)
                    if (a.Object is IFile file)
                        if (a.Object is IPersonaFile pfile)
                        {
                            writer.Write(Encoding.ASCII.GetBytes(a.Name));
                            writer.Write(new byte[Utilities.Utilities.Alignment(a.Name.Length, 0x20)]);
                            int size = file.Size;
                            int align = Utilities.Utilities.Alignment(size, 0x20);
                            writer.Write(size + align);
                            writer.Write(file.Get());
                            writer.Write(new byte[align]);
                        }

                return MS.ToArray();
            }
        }
    }
}