using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.PM1
{
    interface IPM1Element
    {
        TypeMap Type { get; }
        void Get(BinaryWriter writer);
        int Size { get; }
        int TableSize { get; }
        int Count { get; }
        int TableCount { get; }
    }

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

    public class PM1
    {
        PM1Header Header;
        PM1Table Table;

        List<IPM1Element> List = new List<IPM1Element>();

        public PM1(Stream stream, bool IsLittleEndian)
        {
            BinaryReader reader;

            if (IsLittleEndian)
                reader = new BinaryReader(stream);
            else
                reader = new BinaryReaderBE(stream);

            Header = new PM1Header(reader);
            Table = new PM1Table(reader.ReadInt32ArrayArray(Header.TableLineCount, 4));

            foreach (var element in Table.Table)
                if (element.Count * element.Size > 0)
                    switch (element.Index)
                    {
                        case ((int)TypeMap.FileList):
                            List.Add(new PM1ElementFileNames(reader, element));
                            break;
                        case ((int)TypeMap.BMD):
                            List.Add(new PM1ElementBMD(reader, element));
                            break;
                        case ((int)TypeMap.RMDHead):
                            var rmdhead = new PM1ElementRMDHead(reader, element);
                            List.Add(rmdhead);
                            List.Add(new PM1ElementRMD(reader, rmdhead));
                            break;
                        case ((int)TypeMap.EPLHead):
                            List.Add(new PM1ElementEPLHead(reader, element));
                            break;
                        case ((int)TypeMap.RMD):
                            break;
                        case ((int)TypeMap.EPL):
                            List.Add(new PM1ElementEPL(reader, element, List.Find(x => x.Type == TypeMap.EPLHead) as PM1ElementEPLHead));
                            break;
                        default:
                            List.Add(new PM1Element(reader, element));
                            break;
                    }
        }

        public PM1(string path, bool IsLittleEndian) : this(File.OpenRead(path), IsLittleEndian)
        {

        }

        public byte[] GetBMD()
        {
            var temp = List.Find(x => x.Type == TypeMap.BMD);
            return temp == null ? new byte[0] : (temp as PM1ElementBMD).BMD.ToArray();
        }

        public void SetBMD(byte[] bmd)
        {
            var temp = List.Find(x => x.Type == TypeMap.BMD);
            if (temp != null)
                (temp as PM1ElementBMD).BMD = bmd;
        }



        public byte[] Get(bool IsLittleEndian)
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

                Update();
                Table.Update(List);

                Header.FileSize = Header.Size + Table.Size;
                List.ForEach(x => Header.FileSize += x.Size);

                Header.Get(writer);
                Table.Get(writer);

                List.OrderBy(x => x.Type).ToList().ForEach(x => x.Get(writer));
                returned = MS.ToArray();
            }

            return returned;
        }

        private void Update()
        {
            foreach (var element in List)
            {
                switch (element.Type)
                {
                    case (TypeMap.RMDHead):
                        PM1ElementRMDHead rmdhead = element as PM1ElementRMDHead;
                        PM1ElementRMD rmd = List.Find(x => x.Type == TypeMap.RMD) as PM1ElementRMD;
                        for (int i = 0; i < rmdhead.List.Count; i++)
                        {
                            int ret = 0;
                            List.ForEach(x =>
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
                        PM1ElementEPL epl = List.Find(x => x.Type == TypeMap.EPL) as PM1ElementEPL;
                        for (int i = 0; i < eplhead.List.Count; i++)
                        {
                            int ret = 0;
                            List.ForEach(x =>
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
    }
}