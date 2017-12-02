using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.BF
{
    class BFTable
    {
        public static List<Tuple<FileType, int>> MAP = new List<Tuple<FileType, int>>()
        {
            new Tuple<FileType, int>(FileType.BMD, 0x3)
        };

        public class Element
        {
            public int Index { get; set; } = -1;
            public int Size { get; set; } = 0;
            public int Count { get; set; } = 0;
            public int Position { get; set; } = 0;

            public Element(int index, int size, int count, int position)
            {
                Index = index;
                Size = size;
                Count = count;
                Position = position;
            }
        }

        public List<Element> Table { get; private set; } = new List<Element>();

        public BFTable(int[][] array)
        {
            for (int i = 0; i < 3; i++)
                Table.Add(new Element(array[i][0], array[i][1], array[i][2], array[i][3]));
            for (int i = 3; i < array.Length; i++)
                Table.Add(new Element(array[i][0], array[i][2], array[i][1], array[i][3]));
        }

        public int Size
        {
            get { return Table.Count * 0x10; }
        }

        public void Get(BinaryWriter writer)
        {
            for (int i = 0; i < 3; i++)
            {
                writer.Write(Table[i].Index);
                writer.Write(Table[i].Size);
                writer.Write(Table[i].Count);
                writer.Write(Table[i].Position);
            }
            for (int i = 3; i < Table.Count; i++)
            {
                writer.Write(Table[i].Index);
                writer.Write(Table[i].Count);
                writer.Write(Table[i].Size);
                writer.Write(Table[i].Position);
            }
        }

        public void Update(List<BFElement> List)
        {
            if (Table.Count > 0)
            {
                var temp = List.Find(x => x.Index == Table[0].Index);
                if (temp != null)
                {
                    Table[0].Size = temp.TableSize;
                    Table[0].Count = temp.TableCount;
                }
            }
            for (int i = 1; i < Table.Count; i++)
            {
                Table[i].Position = Table[i - 1].Position + Table[i - 1].Size * Table[i - 1].Count;

                var temp = List.Find(x => x.Index == Table[i].Index);
                if (temp != null)
                {
                    Table[i].Size = temp.TableSize;
                    Table[i].Count = temp.TableCount;
                }
            }
        }
    }
}