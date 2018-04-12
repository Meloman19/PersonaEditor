using System;
using System.Collections.Generic;
using System.IO;

namespace PersonaEditorLib.FileStructure.FNT
{
    public class CompressedHeader
    {
        public CompressedHeader(BinaryReader reader)
        {
            HeaderSize = reader.ReadInt32();
            DictionarySize = reader.ReadInt32();
            CompressedBlockSize = reader.ReadInt32();
            Unknown = reader.ReadInt32();
            BytesPerGlyph = reader.ReadUInt16();
            UnknownU = reader.ReadUInt16();
            GlyphTableCount = reader.ReadInt32();
            GlyphPosTableSize = reader.ReadInt32();
            UncompressedFontSize = reader.ReadInt32();
        }

        public int HeaderSize { get; set; }
        public int DictionarySize { get; set; }
        public int CompressedBlockSize { get; set; }
        public int Unknown { get; set; }
        public ushort BytesPerGlyph { get; set; }
        public ushort UnknownU { get; set; }
        public int GlyphTableCount { get; set; }
        public int GlyphPosTableSize { get; set; }
        public int UncompressedFontSize { get; set; }

        public void Resize(int size)
        {
            GlyphTableCount = size + 1;
            GlyphPosTableSize = GlyphTableCount * 4;
            UncompressedFontSize = size * BytesPerGlyph;
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(HeaderSize);
            writer.Write(DictionarySize);
            writer.Write(CompressedBlockSize);
            writer.Write(Unknown);
            writer.Write(BytesPerGlyph);
            writer.Write(UnknownU);
            writer.Write(GlyphTableCount);
            writer.Write(GlyphPosTableSize);
            writer.Write(UncompressedFontSize);
        }
    }

    public class CompressedDictionary
    {
        public CompressedDictionary(BinaryReader reader, int size)
        {
            List<ushort[]> temp = new List<ushort[]>();
            for (int i = 0; i < size / 6; i++)
            {
                ushort[] added = new ushort[3];
                added[0] = reader.ReadUInt16();
                added[1] = reader.ReadUInt16();
                added[2] = reader.ReadUInt16();
                temp.Add(added);
            }
            Dictionary = temp.ToArray();
        }

        public ushort[][] Dictionary { get; set; }

        public void Get(BinaryWriter writer)
        {
            foreach (var b in Dictionary)
                foreach (var a in b)
                    writer.Write(a);
        }
    }

    public class CompressedGlyphTable
    {
        public CompressedGlyphTable(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
                Table.Add(reader.ReadInt32());
        }

        public List<int> Table = new List<int>();

        public void Get(BinaryWriter writer)
        {
            foreach (var a in Table)
                writer.Write(a);
        }
    }

    public class FNTCompressed
    {
        public FNTCompressed(BinaryReader reader)
        {
            Header = new CompressedHeader(reader);
            Dictionary = new CompressedDictionary(reader, Header.DictionarySize);
            GlyphTable = new CompressedGlyphTable(reader, Header.GlyphTableCount);
            CompressedData = reader.ReadBytes(Header.CompressedBlockSize);
        }

        public CompressedHeader Header { get; set; }
        public CompressedDictionary Dictionary { get; set; }
        public CompressedGlyphTable GlyphTable { get; set; }
        public byte[] CompressedData { get; set; }

        public void Resize(int size)
        {
            Header.Resize(size);
        }

        public List<byte[]> GetDecompressedData()
        {
            List<byte[]> returned = new List<byte[]>();
            List<byte> Decompress = new List<byte>();


            int temp = 0;

            for (int i = 0; i < CompressedData.Length; i++)
            {
                int Byte = CompressedData[i];
                for (int k = 0; k < 8; k++)
                {
                    temp = Dictionary.Dictionary[temp][Byte % 2 + 1];
                    Byte = Byte >> 1;

                    if (Dictionary.Dictionary[temp][1] == 0)
                    {
                        if (Decompress.Count == Header.BytesPerGlyph)
                        {
                            returned.Add(Decompress.ToArray());
                            Decompress = new List<byte>();
                        }
                        Decompress.Add((byte)(Dictionary.Dictionary[temp][2]));
                        temp = 0;
                    }
                }
            }

            return returned;
        }

        public void CompressData(List<byte[]> list)
        {
            BitWriter BitW = new BitWriter();

            int DictPart = FindDictPart();

            List<bool> returned = new List<bool>();

            for (int i1 = list.Count - 1; i1 >= 0; i1--)
            {
                Console.Write("\r{0} glyph left             ", i1);

                for (int i2 = list[i1].Length - 1; i2 >= 0; i2--)
                {
                    int s4 = list[i1][i2];
                    int i = 1;

                    while (Dictionary.Dictionary[i][2] != s4)
                    {
                        i++;
                        if (Dictionary.Dictionary[i][1] != 0)
                        {
                            if ((s4 >> 4) > ((s4 << 4) >> 4))
                            {
                                s4 = s4 - (1 << 4);
                            }
                            else
                            {
                                s4 = s4 - 1;
                            }
                            i = 1;
                        }
                    }
                    int v0 = i;
                    while (v0 != 0)
                        v0 = FindDictIndex(v0, DictPart, returned);
                }
            }
            for (int i = returned.Count - 1; i >= 0; i--)
                BitW.Write(returned[i]);


            CompressedData = BitW.GetArray();
            Header.CompressedBlockSize = Convert.ToInt32(CompressedData.Length);
            WriteGlyphPosition();

            Console.WriteLine("\rComplete             ");
        }

        private int FindDictIndex(int v0, int DictPart, List<bool> list)
        {
            if (Dictionary.Dictionary[0][1] == v0)
            {
                list.Add(false);
                return 0;
            }
            else if (Dictionary.Dictionary[0][2] == v0)
            {
                list.Add(true);
                return 0;
            }

            for (int i = DictPart; i < Dictionary.Dictionary.Length; i++)
            {
                if (Dictionary.Dictionary[i][1] == v0)
                {
                    list.Add(false);
                    return i;
                }
                else if (Dictionary.Dictionary[i][2] == v0)
                {
                    list.Add(true);
                    return i;
                }
            }
            return -1;
        }

        private int FindDictPart()
        {
            for (int i = 1; i < Dictionary.Dictionary.Length; i++)
            {
                if (Dictionary.Dictionary[i][1] != 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private void WriteGlyphPosition()
        {
            List<int> GlyphNewPosition = new List<int>(Header.GlyphTableCount) { 0 };

            int temp = 0;
            int a = 0;
            int b = 0;

            foreach (var by in CompressedData)
            {
                int s4 = by;
                a++;
                for (int i = 1; i < 9; i++)
                {
                    temp = Dictionary.Dictionary[temp][s4 % 2 + 1];
                    s4 = s4 >> 1;

                    if (Dictionary.Dictionary[temp][1] == 0)
                    {
                        b++;
                        if (b % Header.BytesPerGlyph == 0)
                            GlyphNewPosition.Add(((a - 1) << 3) + i);
                        temp = 0;
                    }
                }
            }

            Header.Unknown = GlyphNewPosition[GlyphNewPosition.Count - 1];
            GlyphTable.Table = GlyphNewPosition;

            Logging.Write("PersonaEditorLib", "Writed new Glyph Position Table");
        }

        public int Size()
        {
            return Header.HeaderSize + Header.DictionarySize + Header.GlyphTableCount * 4 + Header.CompressedBlockSize;
        }

        public void Get(BinaryWriter writer)
        {
            Header.Get(writer);
            Dictionary.Get(writer);
            GlyphTable.Get(writer);
            writer.Write(CompressedData);
        }
    }
}