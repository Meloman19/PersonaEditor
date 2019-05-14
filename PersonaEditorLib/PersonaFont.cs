using PersonaEditorLib.Other;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PersonaEditorLib
{
    public class PersonaFont
    {
        public Dictionary<int, byte[]> DataList { get; } = new Dictionary<int, byte[]>();
        public Dictionary<int, VerticalCut> CutList { get; } = new Dictionary<int, VerticalCut>();
        public Color[] Palette { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelFormat PixelFormat { get; set; }

        public PersonaFont(string fontPath)
        {
            string cache = Path.Combine(Path.GetDirectoryName(fontPath), Path.GetFileNameWithoutExtension(fontPath) + ".FNTCACHE");
            if (File.Exists(cache))
                OpenCache(cache);
            else
            {
                OpenFont(new FNT(fontPath));
                CreateCache(cache);
            }
        }

        public Tuple<byte[], VerticalCut> GetGlyph(int index)
        {
            byte[] data = null;
            VerticalCut verticalCut = new VerticalCut();

            if (DataList.ContainsKey(index))
                data = DataList[index];
            if (CutList.ContainsKey(index))
                verticalCut = CutList[index];

            return new Tuple<byte[], VerticalCut>(data, verticalCut);
        }

        public VerticalCut GetVerticalCut(int index)
        {
            if (CutList.ContainsKey(index))
                return CutList[index];
            else
                return new VerticalCut();
        }

        public Dictionary<char, int> GetCharWidth(PersonaEncoding encoding)
        {
            Dictionary<char, int> returned = new Dictionary<char, int>();

            foreach (var a in encoding.Dictionary)
            {
                if (CutList.ContainsKey(a.Key))
                {
                    if (!returned.ContainsKey(a.Value))
                    {
                        var temp = CutList[a.Key];
                        returned.Add(a.Value, temp.Right - temp.Left);
                    }
                }
            }

            return returned;
        }

        private void OpenFont(string FontName)
        {
            using (FileStream FS = File.OpenRead(FontName))
                OpenFont(new FNT(FS, 0));
        }

        private void OpenFont(FNT FNT)
        {
            try
            {
                ReadFONT(FNT);
                if (CutList.ContainsKey(32))
                    CutList[32] = new VerticalCut(10, 20);
            }
            catch (Exception e)
            {
                //Logging.Write("PersonaEditorLib", e);
            }
        }

        private void ReadFONT(FNT FNT)
        {
            Height = FNT.Header.Glyphs.Size1;
            Width = FNT.Header.Glyphs.Size2;
            Palette = FNT.Palette.Pallete;
            if (FNT.Header.Glyphs.BitsPerPixel == 4)
                PixelFormat = PixelFormats.Indexed4;
            else if (FNT.Header.Glyphs.BitsPerPixel == 8)
                PixelFormat = PixelFormats.Indexed8;
            else throw new Exception("ReadFONT Error: unknown PixelFormat");

            var DecList = FNT.Compressed.GetDecompressedData();
            if (FNT.Header.Glyphs.BitsPerPixel == 4)
                ArrayTool.ReverseByteInList(DecList);

            for (int i = 0; i < DecList.Count; i++)
            {
                var Cut = FNT.WidthTable[i] == null ? new VerticalCut(0, (byte)Width) : FNT.WidthTable[i].Value;

                int index = i + 32;
                if (DataList.ContainsKey(index))
                    DataList[index] = DecList[i];
                else
                    DataList.Add(index, DecList[i]);

                if (CutList.ContainsKey(index))
                    CutList[index] = Cut;
                else
                    CutList.Add(index, Cut);
            }
        }

        private void CreateCache(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Palette.Length);
                writer.Write(DataList.Count);
                writer.Write(CutList.Count);

                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);
                foreach (var a in Palette)
                {
                    writer.Write(a.R);
                    writer.Write(a.G);
                    writer.Write(a.B);
                    writer.Write(a.A);
                }

                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);
                foreach (var a in DataList)
                    writer.Write(a.Value);

                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);
                foreach (var a in CutList)
                {
                    writer.Write(a.Value.Left);
                    writer.Write(a.Value.Right);
                }

                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);
            }
        }

        private void OpenCache(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                int colorcount = reader.ReadInt32();
                int datacount = reader.ReadInt32();
                int cutcount = reader.ReadInt32();

                if (colorcount == 16)
                    PixelFormat = PixelFormats.Indexed4;
                else if (colorcount == 256)
                    PixelFormat = PixelFormats.Indexed8;
                else
                {

                }

                reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position, 0x10);
                Palette = ReadPalette(reader, colorcount).ToArray();

                int glyphsize = (PixelFormat.BitsPerPixel * Width * Height) / 8;
                reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position, 0x10);
                for (int i = 32; i < 32 + datacount; i++)
                    DataList.Add(i, reader.ReadBytes(glyphsize));

                reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position, 0x10);
                for (int i = 32; i < 32 + cutcount; i++)
                    CutList.Add(i, new VerticalCut(reader.ReadBytes(2)));
            }
        }

        public static List<Color> ReadPalette(BinaryReader reader, int Count)
        {
            List<Color> Colors = new List<Color>();
            for (int i = 0; i < Count; i++)
            {
                byte R = reader.ReadByte();
                byte G = reader.ReadByte();
                byte B = reader.ReadByte();
                byte A = reader.ReadByte();
                Colors.Add(Color.FromArgb(A, R, G, B));
            }
            return Colors;
        }
    }
}
