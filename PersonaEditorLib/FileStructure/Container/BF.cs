using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.Container
{
    public class BF : IPersonaFile
    {
        private static Dictionary<int, FileType> MAP = new Dictionary<int, FileType>()
        {
            {0x0, FileType.DAT },
            {0x1, FileType.DAT },
            {0x2, FileType.DAT },
            {0x3, FileType.BMD },
            {0x4, FileType.DAT }
        };

        private static bool GetTable(int[][] table, List<ObjectFile> list, int tableoffset)
        {
            int offset = tableoffset + 0x10 * table.Length;

            foreach (var a in table)
            {
                a[3] = offset;
                int count = 0;

                var item = list.Find(x => (int)x.Tag == a[0]);
                if (item?.Object is IPersonaFile pFile)
                {
                    int tempsize = pFile.Size;
                    if (tempsize % a[1] == 0)
                        count = Convert.ToInt32(tempsize / a[1]);
                    else
                        return false;
                }

                a[2] = count;
                offset += a[1] * a[2];
            }

            return true;
        }

        int[][] Table;
        byte[] Unknown;

        public BF(string path)
        {
            using (FileStream FS = File.OpenRead(path))
                Open(FS);

            SetName(Path.GetFileNameWithoutExtension(path));
        }

        public BF(Stream Stream, string name = "")
        {
            Open(Stream);

            SetName(name);
        }

        public BF(byte[] data, string name = "")
        {
            using (MemoryStream MS = new MemoryStream(data))
                Open(MS);

            SetName(name);
        }

        public void SetName(string name)
        {
            foreach (var a in SubFiles)
            {
                FileType fileType = (a.Object as IPersonaFile).Type;
                string ext = Path.GetExtension(name);
                if (fileType == FileType.DAT)
                    a.Name = name.Substring(0, name.Length - ext.Length) + "(" + ((int)a.Tag).ToString().PadLeft(2, '0') + ").DAT";
                else
                    a.Name = name.Substring(0, name.Length - ext.Length) + "." + fileType.ToString();
            }
        }

        private void Open(Stream stream)
        {
            stream.Position = 0x10;
            if (stream.ReadByte() == 0)
                IsLittleEndian = false;
            else
                IsLittleEndian = true;

            stream.Position = 0x10;
            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            int tablecount = reader.ReadInt32();
            Unknown = reader.ReadBytes(12);

            stream.Position = 0x20;
            Table = reader.ReadInt32ArrayArray(tablecount, 4);

            foreach (var element in Table)
                if (element[1] * element[2] > 0)
                {
                    reader.BaseStream.Position = element[3];

                    string tempN;
                    FileType type = MAP[element[0]];
                    tempN = "." + type.ToString();

                    byte[] data = reader.ReadBytes(element[1] * element[2]);
                    var item = Utilities.PersonaFile.OpenFile(tempN, data, type);
                    if (item.Object == null)
                        item = Utilities.PersonaFile.OpenFile(tempN, data, FileType.DAT);

                    item.Tag = element[0];

                    SubFiles.Add(item);
                }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public FileType Type => FileType.BF;
        
        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();
        
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

                returned += 0x20 + Table.Length * 0x10;
                SubFiles.ForEach(x => returned += (x.Object as IPersonaFile).Size);

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                if (GetTable(Table, SubFiles, 0x20))
                {
                    var temp = Table.FirstOrDefault(x => x[0] == 0x4);
                    if (temp == null)
                        return new byte[0];

                    writer.Write(0);
                    writer.Write(Size - temp[1] * temp[2]);
                    writer.Write(Encoding.ASCII.GetBytes("FLW0"));
                    writer.Write(0);
                    writer.Write(Table.Length);
                    writer.Write(Unknown);

                    foreach (var a in Table)
                        foreach (var b in a)
                            writer.Write(b);
                }
                else
                    return new byte[0];

                SubFiles.ForEach(x => writer.Write((x.Object as IPersonaFile).Get()));

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile
    }
}