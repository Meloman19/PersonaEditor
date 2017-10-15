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

        public MemoryStream GetBMD()
        {
            var temp = List.Find(x => x.Type == TypeMap.BMD);
            return temp == null ? new MemoryStream() : temp.Count == 0 ? new MemoryStream() : new MemoryStream((temp as BFElement).List[0]);
        }

        public void SetBMD(MemoryStream bmd)
        {
            var temp = List.Find(x => x.Type == TypeMap.BMD) as BFElement;
            if (temp != null)
            {
                temp.List.Clear();
                temp.List.Add(bmd.ToArray());
            }
        }

        public MemoryStream Get(bool IsLittleEndian)
        {
            MemoryStream returned = new MemoryStream();
            BinaryWriter writer;

            if (IsLittleEndian)
                writer = new BinaryWriter(returned);
            else
                writer = new BinaryWriterBE(returned);

            Table.Update(List);

            Header.FileSize = Header.Size + Table.Size;
            List.ForEach(x => Header.FileSize += x.Size);

            Header.Get(writer);
            Table.Get(writer);

            List.OrderBy(x => x.Type).ToList().ForEach(x => x.Get(writer));

            return returned;
        }
    }
}