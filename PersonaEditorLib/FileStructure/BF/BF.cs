using PersonaEditorLib.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.BF
{

    interface IBFElement
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
        BMD = 0x3
    }

    public class BF
    {
        BFHeader Header;
        BFTable Table;

        List<IBFElement> List = new List<IBFElement>();

        public BF(string path, bool IsLittleEndian) : this(File.OpenRead(path), IsLittleEndian)
        {

        }

        public BF(Stream Stream, bool IsLittleEndian)
        {
            BinaryReader reader;

            if (IsLittleEndian)
                reader = new BinaryReader(Stream);
            else
                reader = new BinaryReaderBE(Stream);

            Header = new BFHeader(reader);
            Table = new BFTable(reader.ReadInt32ArrayArray(Header.TableLineCount, 4));

            foreach (var element in Table.Table)
                if (element.Count * element.Size > 0)
                    List.Add(new BFElement(reader, element));
        }

        public byte[] GetBMD()
        {
            var temp = List.Find(x => x.Type == TypeMap.BMD);
            return temp == null ? new byte[0] : temp.Count == 0 ? new byte[0] : (temp as BFElement).List[0].ToArray();
        }

        public void SetBMD(byte[] bmd)
        {
            var temp = List.Find(x => x.Type == TypeMap.BMD) as BFElement;
            if (temp != null)
            {
                temp.List.Clear();
                temp.List.Add(bmd);
            }
        }

        public byte[] Get(bool IsLittleEndian)
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian);

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
    }
}