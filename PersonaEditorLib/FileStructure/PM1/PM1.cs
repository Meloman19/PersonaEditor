using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorLib.FileStructure.PM1
{
    public interface IPM1Element
    {
        TypeMap Type { get; }
        void Get(BinaryWriter writer);
        int Size { get; }
        int TableSize { get; }
        int Count { get; }
        int TableCount { get; }
    }

    public enum TypeMap
    {
        FileList = 0x1,
        T3HeadList = 0x2,
        RMDHead = 0x3,
        BMD = 0x6,
        EPLHead = 0x7,
        EPL = 0x8,
        RMD = 0x9
    }

    public class PM1 : IPersonaFile
    {
        public static byte[] GetBMD(PM1 pm1)
        {
            var temp = pm1.List2.Find(x => x.Type == TypeMap.BMD);
            return temp == null ? new byte[0] : (temp as PM1ElementBMD).BMD.ToArray();
        }

        public static void SetBMD(PM1 pm1, byte[] bmd)
        {
            var temp = pm1.List2.Find(x => x.Type == TypeMap.BMD);
            if (temp != null)
                (temp as PM1ElementBMD).BMD = bmd;
        }

        PM1Header Header;
        PM1Table Table;

        public List<ObjectFile> List { get; } = new List<ObjectFile>();

        public List<ObjectFile> HidList { get; } = new List<ObjectFile>();

        public List<IPM1Element> List2 { get; } = new List<IPM1Element>();

        public PM1(Stream stream)
        {
            Read(stream);
        }

        public PM1(string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Open, FileAccess.Read))
                Read(FS);
        }

        public PM1(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(MS);
        }

        private static int[] FileListE = new int[]
        {
            0x3,
            0x6,
            0x7
        };

        private static int[] FileListEE = new int[]
        {
            0x1,
            0x3,
            0x6,
            0x7,
            0x8,
            0x9
        };

        private void Read(Stream stream)
        {
            BinaryReader reader = Utilities.IO.OpenReadFile(stream, IsLittleEndian);

            Header = new PM1Header(reader);
            Table = new PM1Table(reader.ReadInt32ArrayArray(Header.TableLineCount, 4));

            foreach (var element in Table.Table)
                if (element.Count * element.Size > 0)
                    switch (element.Index)
                    {
                        case ((int)TypeMap.FileList):
                            List2.Add(new PM1ElementFileNames(reader, element));
                            break;
                        case ((int)TypeMap.BMD):
                            List2.Add(new PM1ElementBMD(reader, element));
                            break;
                        case ((int)TypeMap.RMDHead):
                            var rmdhead = new PM1ElementRMDHead(reader, element);
                            List2.Add(rmdhead);
                            List2.Add(new PM1ElementRMD(reader, rmdhead));
                            break;
                        case ((int)TypeMap.EPLHead):
                            List2.Add(new PM1ElementEPLHead(reader, element));
                            break;
                        case ((int)TypeMap.RMD):
                            break;
                        case ((int)TypeMap.EPL):
                            List2.Add(new PM1ElementEPL(reader, element, List2.Find(x => x.Type == TypeMap.EPLHead) as PM1ElementEPLHead));
                            break;
                        default:
                            List2.Add(new PM1Element(reader, element));
                            break;
                    }

            string[] list = ReadFileList(reader);
            int ind = 0;

            foreach (var a in FileListE)
            {
                var element = Table.Table.Find(x => x.Index == a);
                if (element.Size * element.Count > 0)
                {

                    ReadFile(reader, list.SubArray(ind, element.Count), element);
                    ind += element.Count;
                }
            }

            foreach (var a in Table.Table)
            {
                if (a.Size * a.Count > 0)
                {
                    if (!(FileListEE.Contains(a.Index)))
                    {
                        reader.BaseStream.Position = a.Position;
                        for (int i = 0; i < a.Count; i++)
                        {
                            var temp = Utilities.PersonaFile.OpenFile("", reader.ReadBytes(a.Size), FileType.HEX);
                            temp.Tag = a.Index;
                            HidList.Add(temp);
                        }
                    }
                }
            }
        }

        private void ReadFile(BinaryReader reader, string[] name, PM1Table.Element element)
        {
            if (element.Index == (int)TypeMap.BMD)
            {
                reader.BaseStream.Position = element.Position;
                var returned = Utilities.PersonaFile.OpenFile(name[0], reader.ReadBytes(element.Size), FileType.BMD);
                returned.Tag = element.Index;
                List.Add(returned);
            }
            else if (element.Index == (int)TypeMap.EPLHead)
            {
                var elementEPL = Table.Table.Find(x => x.Index == (int)TypeMap.EPL);

                reader.BaseStream.Position = element.Position;
                int[] eplpos = new int[element.Count];

                for (int i = 0; i < element.Count; i++)
                {
                    reader.BaseStream.Position += 4;
                    eplpos[i] = reader.ReadInt32() - elementEPL.Position;
                    reader.BaseStream.Position += 8;
                }
                reader.BaseStream.Position = elementEPL.Position;
                byte[] EPL = reader.ReadBytes(elementEPL.Size);

                var splited = Utilities.Array.SplitArray(EPL, eplpos);

                for (int i = 0; i < splited.Count; i++)
                {
                    var returned = Utilities.PersonaFile.OpenFile(name[i], splited[i], FileType.HEX);
                    returned.Tag = element.Index;
                    List.Add(returned);
                }
            }
            else if (element.Index == (int)TypeMap.RMDHead)
            {
                int[][] RMD = new int[element.Count][];

                for (int i = 0; i < element.Count; i++)
                {
                    reader.BaseStream.Position += 0x10;
                    int pos = reader.ReadInt32();
                    int size = reader.ReadInt32();
                    reader.BaseStream.Position += 0x8;
                    RMD[i] = new int[2] { pos, size };
                }

                for (int i = 0; i < RMD.Length; i++)
                {
                    reader.BaseStream.Position = RMD[i][0];
                    var returned = Utilities.PersonaFile.OpenFile(name[i], reader.ReadBytes(RMD[i][1]), FileType.HEX);
                    returned.Tag = element.Index;
                    List.Add(returned);
                }
            }
            else
            {
            }
        }

        private string[] ReadFileList(BinaryReader reader)
        {
            var FileList = Table.Table.Find(x => x.Index == (int)TypeMap.FileList);
            List<string> fileList = new List<string>();

            reader.BaseStream.Position = FileList.Position;
            for (int i = 0; i < FileList.Count; i++)
                fileList.Add(Encoding.ASCII.GetString(reader.ReadBytes(FileList.Size).Where(x => x != 0).ToArray()));
            return fileList.ToArray();
        }

        private void Update()
        {
            foreach (var element in List2)
            {
                switch (element.Type)
                {
                    case (TypeMap.RMDHead):
                        PM1ElementRMDHead rmdhead = element as PM1ElementRMDHead;
                        PM1ElementRMD rmd = List2.Find(x => x.Type == TypeMap.RMD) as PM1ElementRMD;
                        for (int i = 0; i < rmdhead.List.Count; i++)
                        {
                            int ret = 0;
                            List2.ForEach(x =>
                            {
                                if (x.Type < TypeMap.RMD)
                                    ret += x.Size;
                            });

                            rmdhead.List[i].Position = rmd.Position(i) + Header.Size + Table.Size + ret;
                            rmdhead.List[i].Size = (int)rmd.List[i].Length;
                        }
                        break;
                    case (TypeMap.EPLHead):
                        PM1ElementEPLHead eplhead = element as PM1ElementEPLHead;
                        PM1ElementEPL epl = List2.Find(x => x.Type == TypeMap.EPL) as PM1ElementEPL;
                        for (int i = 0; i < eplhead.List.Count; i++)
                        {
                            int ret = 0;
                            List2.ForEach(x =>
                            {
                                if (x.Type < TypeMap.EPL)
                                    ret += x.Size;
                            });

                            eplhead.List[i].Position = epl.Position(i) + Header.Size + Table.Size + ret;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #region IPersonaFile

        public FileType Type => FileType.PM1;

        public List<ObjectFile> GetSubFiles()
        {
            return List;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

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

        public bool IsLittleEndian { get; set; } = true;

        #region IFile

        public int Size
        {
            get
            {
                int returned = 0;

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                Update();
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

        #endregion IFile

        #endregion IPersonaFile
    }
}