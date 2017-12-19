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
        public static byte[] GetBMD(BF bf)
        {
            var bmd = bf.List.Find(x => (x.Object as IPersonaFile).Type == FileType.BMD).Object as IPersonaFile;
            if (bmd != null)
            {
                return bmd.Get();
            }
            else
                return new byte[0];
        }

        public static void SetBMD(BF bf, object bmd)
        {
            if (bmd is IPersonaFile pFile)
                if (pFile.Type == FileType.BMD)
                {
                    var temp = bf.List.Find(x => (x.Object as IPersonaFile).Type == FileType.BMD);
                    temp.Object = bmd;
                }
        }

        private static List<Tuple<FileType, int>> MAP = new List<Tuple<FileType, int>>()
        {
            new Tuple<FileType, int>(FileType.HEX, 0x0),
            new Tuple<FileType, int>(FileType.HEX, 0x1),
            new Tuple<FileType, int>(FileType.HEX, 0x2),
            new Tuple<FileType, int>(FileType.BMD, 0x3),
            new Tuple<FileType, int>(FileType.HEX, 0x4)
        };

        private static FileType getType(int index)
        {
            if (MAP.Find(x => x.Item2 == index) is Tuple<FileType, int> a)
                return a.Item1;
            else
                return FileType.ObjList;
        }

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
        ushort Unknown;

        public List<ObjectFile> List { get; } = new List<ObjectFile>();

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
            foreach (var a in List)
            {
                FileType fileType = (a.Object as IPersonaFile).Type;
                if (fileType == FileType.HEX)
                    a.Name = Path.GetFileNameWithoutExtension(name) + "(" + ((int)a.Tag).ToString().PadLeft(2, '0') + ").DAT";
                else
                    a.Name = Path.GetFileNameWithoutExtension(name) + "." + fileType.ToString();
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
            Unknown = reader.ReadUInt16();

            stream.Position = 0x20;
            Table = reader.ReadInt32ArrayArray(tablecount, 4);

            foreach (var element in Table)
                if (element[1] * element[2] > 0)
                {
                    reader.BaseStream.Position = element[3];

                    string tempN;
                    var type = getType(element[0]);
                    if (type == FileType.ObjList)
                        tempN = "(" + element[0].ToString().PadLeft(2, '0') + ").DAT";
                    else
                        tempN = "." + type.ToString();

                    var item = Utilities.PersonaFile.OpenFile(tempN, reader.ReadBytes(element[1] * element[2]), type);
                    item.Tag = element[0];

                    List.Add(item);
                }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IPersonaFile

        public FileType Type => FileType.BF;

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

        #region IFile

        public int Size
        {
            get
            {
                int returned = 0;

                returned += 0x20 + Table.Length * 0x10;
                List.ForEach(x => returned += (x.Object as IPersonaFile).Size);

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                if (GetTable(Table, List, 0x20))
                {
                    var temp = Table.FirstOrDefault(x => x[0] == 0x4);
                    if (temp == null)
                        return new byte[0];

                    writer.Write(0x0);
                    writer.Write(Size - temp[1] * temp[2]);
                    writer.Write(Encoding.ASCII.GetBytes("FLW0"));
                    writer.Write(Table.Length);
                    writer.Write(Unknown);
                    writer.Write(Utilities.Utilities.Alignment(writer.BaseStream.Length, 0x20));

                    foreach (var a in Table)
                        foreach (var b in a)
                            writer.Write(b);
                }
                else
                    return new byte[0];

                List.ForEach(x => writer.Write((x.Object as IPersonaFile).Get()));

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile

        #endregion IPersonaFile
    }
}