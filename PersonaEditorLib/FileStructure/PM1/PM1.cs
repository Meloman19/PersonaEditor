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
            var temp = pm1.List.Find(x => (int)(x.Tag as object[])[0] == (int)TypeMap.BMD);
            return temp == null ? new byte[0] : (temp.Object as IPersonaFile).Get();
        }

        public static void SetBMD(PM1 pm1, byte[] bmd)
        {
            var bmdfile = Utilities.PersonaFile.OpenFile("", bmd, FileType.BMD);
            var temp = pm1.List.Find(x => (int)(x.Tag as object[])[0] == (int)TypeMap.BMD);
            if (bmdfile != null && temp != null)
                temp.Object = bmdfile.Object;
        }

        PM1Header Header;
        PM1Table Table;
        int textsize = 0x20;

        public List<ObjectFile> List { get; } = new List<ObjectFile>();

        public List<ObjectFile> HidList { get; } = new List<ObjectFile>();

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
                            var temp = Utilities.PersonaFile.OpenFile("", reader.ReadBytes(a.Size), FileType.DAT);
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
                returned.Tag = new object[] { element.Index };
                List.Add(returned);
            }
            else if (element.Index == (int)TypeMap.EPLHead)
            {
                var elementEPL = Table.Table.Find(x => x.Index == (int)TypeMap.EPL);

                reader.BaseStream.Position = element.Position;
                int[][] eplpos = reader.ReadInt32ArrayArray(element.Count, 4);

                reader.BaseStream.Position = elementEPL.Position;
                byte[] EPL = reader.ReadBytes(elementEPL.Size);

                var splited = Utilities.Array.SplitArray(EPL, eplpos.Select(x => x[1] - elementEPL.Position).ToArray());

                for (int i = 0; i < splited.Count; i++)
                {
                    var returned = Utilities.PersonaFile.OpenFile(name[i], splited[i], FileType.DAT);
                    returned.Tag = new object[] { element.Index, eplpos[i] };
                    List.Add(returned);
                }
            }
            else if (element.Index == (int)TypeMap.RMDHead)
            {
                reader.BaseStream.Position = element.Position;
                int[][] RMD = reader.ReadInt32ArrayArray(element.Count, 8);

                for (int i = 0; i < RMD.Length; i++)
                {
                    reader.BaseStream.Position = RMD[i][4];
                    var returned = Utilities.PersonaFile.OpenFile(name[i], reader.ReadBytes(RMD[i][5]), FileType.DAT);
                    returned.Tag = new object[] { element.Index, RMD[i] };
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

            if (FileList != null)
            {
                reader.BaseStream.Position = FileList.Position;
                for (int i = 0; i < FileList.Count; i++)
                    fileList.Add(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(FileList.Size).Where(x => x != 0).ToArray()));
            }
            return fileList.ToArray();
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

        #endregion IPersonaFile

        #region IFile

        public int Size
        {
            get
            {
                //  int returned = 0;

                return Get().Length;

                //  return returned;
            }
        }

        public byte[] Get()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                Header.Get(writer);
                Table.Get(writer);

                List<int[]> table = new List<int[]>();

                var filelist = List.Select(x => x.Name).ToArray();
                foreach (var file in filelist)
                    writer.WriteString(file, textsize);

                var RMD = List.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.RMDHead);
                long RMDHeadPos = 0;

                if (RMD.Count != 0)
                {
                    RMDHeadPos = writer.BaseStream.Position;
                    writer.Write(new byte[RMD.Count * 0x20]);
                }

                var BMD = List.Find(x => (int)(x.Tag as object[])[0] == (int)TypeMap.BMD);
                if (BMD != null)
                {
                    writer.Write((BMD.Object as IPersonaFile).Get());
                    writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Length, 0x10)]);
                }

                var EPL = List.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.EPLHead);
                long EPLHeadPos = 0;

                if (EPL.Count != 0)
                {
                    EPLHeadPos = writer.BaseStream.Position;
                    writer.Write(new byte[EPL.Count * 0x10]);

                    foreach (var a in EPL)
                    {
                        int[] eplhead = (int[])(a.Tag as object[])[1];
                        eplhead[1] = (int)writer.BaseStream.Position;
                        writer.Write((a.Object as IPersonaFile).Get());
                        writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Length, 0x10)]);
                    }
                }

                if (RMD.Count != 0)
                {
                    foreach (var a in RMD)
                    {
                        int[] rmdhead = (int[])(a.Tag as object[])[1];
                        rmdhead[4] = (int)writer.BaseStream.Position;
                        writer.Write((a.Object as IPersonaFile).Get());
                        writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Length, 0x10)]);
                    }
                }

                if (EPLHeadPos != 0)
                {
                    writer.BaseStream.Position = EPLHeadPos;
                    foreach (var a in EPL)
                        writer.WriteInt32Array((int[])(a.Tag as object[])[1]);
                }

                if (RMDHeadPos != 0)
                {
                    writer.BaseStream.Position = RMDHeadPos;
                    foreach (var a in RMD)
                        writer.WriteInt32Array((int[])(a.Tag as object[])[1]);
                }

                return MS.ToArray();
            }
        }

        #endregion IFile
    }
}