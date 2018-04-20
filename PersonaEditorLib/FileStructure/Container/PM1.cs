using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PersonaEditorLib.Extension;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorLib.FileStructure.Container
{
    public class PM1 : IPersonaFile
    {
        enum TypeMap
        {
            FileList = 0x1,
            T3HeadList = 0x2,
            RMDHead = 0x3,
            BMD = 0x6,
            EPLHead = 0x7,
            EPL = 0x8,
            RMD = 0x9
        }

        byte[] Unknown;

        int textsize = 0x20;

        int[][] Table;

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

        private static int[] MainFileList = new int[]
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

            stream.Position = 0x10;
            int tablelinecount = reader.ReadInt32();
            Unknown = reader.ReadBytes(12);
            stream.Position = 0x20;
            Table = reader.ReadInt32ArrayArray(tablelinecount, 4);

            stream.Position = 0x20;

            string[] list = ReadFileList(reader, Table.FirstOrDefault(x => x[0] == (int)TypeMap.FileList));
            int ind = 0;

            var RMDHead = Table.FirstOrDefault(x => x[0] == (int)TypeMap.RMDHead);
            if (RMDHead != null && RMDHead[1] * RMDHead[2] > 0)
            {
                ReadRMD(reader, list.SubArray(ind, RMDHead[2]), RMDHead);
                ind += RMDHead[2];
            }

            var BMD = Table.FirstOrDefault(x => x[0] == (int)TypeMap.BMD);
            if (BMD != null && BMD[1] * BMD[2] > 0)
            {
                ReadBMD(reader, list.SubArray(ind, BMD[2]), BMD);
                ind += BMD[2];
            }

            var EPLHead = Table.FirstOrDefault(x => x[0] == (int)TypeMap.EPLHead);
            if (EPLHead != null && EPLHead[1] * EPLHead[2] > 0)
            {
                ReadEPL(reader, list.SubArray(ind, EPLHead[2]), EPLHead, Table.FirstOrDefault(x => x[0] == (int)TypeMap.EPL));
                ind += EPLHead[2];
            }

            if (ind != list.Length)
            {
                throw new Exception("PM1");
            }

            foreach (var a in Table)
                if (a[1] * a[2] > 0)
                    if (!(MainFileList.Contains(a[0])))
                    {
                        reader.BaseStream.Position = a[3];
                        for (int i = 0; i < a[2]; i++)
                        {
                            var temp = Utilities.PersonaFile.OpenFile("", reader.ReadBytes(a[1]), FileType.DAT);
                            temp.Tag = a[0];
                            HidList.Add(temp);
                        }
                    }
        }

        private void ReadRMD(BinaryReader reader, string[] names, int[] rmdhead)
        {
            reader.BaseStream.Position = rmdhead[3];
            int[][] RMD = reader.ReadInt32ArrayArray(rmdhead[2], 8);

            for (int i = 0; i < RMD.Length; i++)
            {
                reader.BaseStream.Position = RMD[i][4];
                var returned = Utilities.PersonaFile.OpenFile(names[i], reader.ReadBytes(RMD[i][5]), FileType.DAT);
                returned.Tag = new object[] { (int)TypeMap.RMD, RMD[i] };
                SubFiles.Add(returned);
            }
        }

        private void ReadBMD(BinaryReader reader, string[] names, int[] bmd)
        {
            reader.BaseStream.Position = bmd[3];
            var returned = Utilities.PersonaFile.OpenFile(names[0], reader.ReadBytes(bmd[1]), FileType.BMD);
            returned.Tag = new object[] { bmd[0] };
            SubFiles.Add(returned);
        }

        private void ReadEPL(BinaryReader reader, string[] names, int[] eplhead, int[] epl)
        {
            reader.BaseStream.Position = eplhead[3];
            int[][] eplpos = reader.ReadInt32ArrayArray(eplhead[2], 4);

            reader.BaseStream.Position = epl[3];
            byte[] EPL = reader.ReadBytes(epl[1]);

            var splited = Utilities.Array.SplitArray(EPL, eplpos.Select(x => x[1] - epl[3]).ToArray());

            for (int i = 0; i < splited.Count; i++)
            {
                var returned = Utilities.PersonaFile.OpenFile(names[i], splited[i], FileType.DAT);
                returned.Tag = new object[] { (int)TypeMap.EPL, eplpos[i] };
                SubFiles.Add(returned);
            }
        }

        private string[] ReadFileList(BinaryReader reader, int[] element)
        {
            List<string> fileList = new List<string>();

            if (element != null)
            {
                reader.BaseStream.Position = element[3];
                for (int i = 0; i < element[2]; i++)
                    fileList.Add(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(element[1]).Where(x => x != 0).ToArray()));
            }
            return fileList.ToArray();
        }

        #region IPersonaFile

        public FileType Type => FileType.PM1;

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

        public bool IsLittleEndian { get; set; } = true;

        #endregion IPersonaFile

        #region IFile

        public int Size() => Get().Length;

        public byte[] Get()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                var RMD = SubFiles.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.RMD);
                var BMD = SubFiles.Find(x => (int)(x.Tag as object[])[0] == (int)TypeMap.BMD);
                var EPL = SubFiles.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.EPL);

                MS.Position = 0x20 + 0x10 * (1 + (RMD.Count == 0 ? 0 : 2) + (BMD == null ? 0 : 1) + (EPL.Count == 0 ? 0 : 2) + HidList.GroupBy(x => (int)x.Tag).Count());

                List<int[]> table = new List<int[]>();

                var filelist = SubFiles.Select(x => x.Name).ToArray();
                table.Add(new int[]
                {
                    (int)TypeMap.FileList,
                    textsize,
                    filelist.Length,
                    (int)MS.Position
                });
                foreach (var file in filelist)
                    writer.WriteString(file, textsize);

                long RMDHeadPos = 0;

                if (RMD.Count != 0)
                {
                    table.Add(new int[]
                    {
                        (int)TypeMap.RMDHead,
                        0x20,
                        RMD.Count,
                        (int)MS.Position
                    });

                    RMDHeadPos = MS.Position;
                    MS.Position += RMD.Count * 0x20;

                    table.Add(new int[]
                    {
                        (int)TypeMap.RMD,
                        0,
                        1,
                        (int)MS.Position
                    });
                }

                if (BMD != null)
                {
                    byte[] bmd = (BMD.Object as IPersonaFile).Get();
                    table.Add(new int[]
                    {
                        (int)TypeMap.BMD,
                        bmd.Length + Utilities.Utilities.Alignment(bmd.Length, 0x10),
                        1,
                        (int)MS.Position
                    });
                    writer.Write(bmd);
                    writer.Write(new byte[Utilities.Utilities.Alignment(bmd.Length, 0x10)]);
                }

                long EPLHeadPos = 0;

                if (EPL.Count != 0)
                {
                    table.Add(new int[]
                    {
                        (int)TypeMap.EPLHead,
                        0x10,
                        EPL.Count,
                        (int)MS.Position
                    });

                    EPLHeadPos = MS.Position;
                    MS.Position += EPL.Count * 0x10;

                    table.Add(new int[]
                    {
                        (int)TypeMap.EPL,
                        0,
                        1,
                        (int)MS.Position
                    });
                }

                foreach (var a in EPL)
                {
                    byte[] epl = (a.Object as IPersonaFile).Get();
                    table.Find(x => x[0] == (int)TypeMap.EPL)[1] += epl.Length + Utilities.Utilities.Alignment(epl.Length, 0x10);
                    int[] eplhead = (int[])(a.Tag as object[])[1];
                    eplhead[1] = (int)MS.Position;
                    writer.Write(epl);
                    MS.Position += Utilities.Utilities.Alignment(epl.Length, 0x10);
                }

                foreach (var a in RMD)
                {
                    byte[] rmd = (a.Object as IPersonaFile).Get();
                    table.Find(x => x[0] == (int)TypeMap.RMD)[1] += rmd.Length + Utilities.Utilities.Alignment(rmd.Length, 0x10);
                    int[] rmdhead = (int[])(a.Tag as object[])[1];
                    rmdhead[4] = (int)MS.Position;
                    rmdhead[5] = rmd.Length;
                    writer.Write(rmd);
                    MS.Position += Utilities.Utilities.Alignment(rmd.Length, 0x10);
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

                //  table.AddRange(Table.Where(x => !table.Exists(y => y[0] == x[0])));
                table = table.OrderBy(x => x[0]).ToList();

                MS.Position = 0x4;
                writer.Write((int)MS.Length);
                writer.Write(Encoding.ASCII.GetBytes("PMD1"));
                MS.Position = 0x10;
                writer.Write(table.Count);
                writer.Write(Unknown);
                MS.Position = 0x20;
                foreach (var a in table)
                    writer.WriteInt32Array(a);

                return MS.ToArray();
            }
        }

        #endregion IFile
    }
}