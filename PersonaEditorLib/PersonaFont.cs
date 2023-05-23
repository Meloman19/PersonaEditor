using AuxiliaryLibraries.Media;
using PersonaEditorLib.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonaEditorLib
{
    public sealed class PersonaFont
    {
        public Dictionary<int, Pixel[]> DataList { get; } = new Dictionary<int, Pixel[]>();
        public Dictionary<int, VerticalCut> CutList { get; } = new Dictionary<int, VerticalCut>();
        public int Height { get; set; }
        public int Width { get; set; }

        public PersonaFont(string fontPath)
        {
            OpenFont(new FNT(fontPath));
        }

        public Tuple<Pixel[], VerticalCut> GetGlyph(int index)
        {
            Pixel[] data = null;
            VerticalCut verticalCut = new VerticalCut();

            if (DataList.ContainsKey(index))
                data = DataList[index];
            if (CutList.ContainsKey(index))
                verticalCut = CutList[index];

            return new Tuple<Pixel[], VerticalCut>(data, verticalCut);
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
            var palette = FNT.Palette.Pallete;
            var decList = FNT.Compressed.GetDecompressedData();

            List<Pixel[]> pixelData;
            switch (FNT.Header.Glyphs.BitsPerPixel)
            {
                case 4:
                    pixelData = decList.Select(x => DecodingHelper.FromIndexed4Reverse(x, palette, Width)).ToList();
                    break;
                case 8:
                    pixelData = decList.Select(x => DecodingHelper.FromIndexed8(x, palette)).ToList();
                    break;
                default:
                    throw new Exception("ReadFONT Error: unknown PixelFormat");
            }

            for (int i = 0; i < pixelData.Count; i++)
            {
                var Cut = FNT.WidthTable[i] == null ? new VerticalCut(0, (byte)Width) : FNT.WidthTable[i].Value;

                int index = i + 32;
                DataList[index] = pixelData[i];
                CutList[index] = Cut;
            }
        }
    }
}