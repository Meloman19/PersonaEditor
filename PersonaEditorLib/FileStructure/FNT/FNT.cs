using PersonaEditorLib.Extension;
using PersonaEditorLib.FileTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PersonaEditorLib.FileStructure.FNT
{
    public class FNT
    {
        public FNTHeader Header { get; set; }
        public FNTPalette Palette { get; set; }
        public FNTWidthTable WidthTable { get; private set; }
        public FNTUnknown Unknown { get; set; }
        public FNTReserved Reserved { get; set; }
        public FNTCompressed Compressed { get; private set; }
        public FNTLast Last { get; private set; }

        public FNT(Stream stream, long position)
        {
            Read(stream, 0);
        }

        public FNT(string path)
        {
            using (FileStream FS = File.OpenRead(path))
                Read(FS, 0);
        }

        private void Read(Stream stream, long position)
        {
            stream.Position = position;
            BinaryReader reader = new BinaryReader(stream);

            Header = new FNTHeader(reader);
            reader.BaseStream.Position = Header.HeaderSize;
            Palette = new FNTPalette(reader, Header.Glyphs.NumberOfColor);
            WidthTable = new FNTWidthTable(reader);
            Unknown = new FNTUnknown(reader);
            Reserved = new FNTReserved(reader, Header.Glyphs.Count);
            Compressed = new FNTCompressed(reader);
            if (Header.LastPosition != 0)
            {
                reader.BaseStream.Position = Header.LastPosition;
                Last = new FNTLast(reader, Header.Glyphs.Count);
            }
        }

        public int Size()
        {
            return Header.HeaderSize + Palette.Size + WidthTable.Size() + Unknown.Size() + Reserved.Size + Compressed.Size();
        }

        private BitmapSource GetFontImage()
        {
            List<byte[]> data = Compressed.GetDecompressedData();

            PixelFormat currentPF;
            if (Header.Glyphs.BitsPerPixel == 4)
            {
                currentPF = PixelFormats.Indexed4;
                Util.ReverseByteInList(data);
            }
            else if (Header.Glyphs.BitsPerPixel == 8)
                currentPF = PixelFormats.Indexed8;
            else return null;

            ImageData BMP = new ImageData();
            ImageData Line = new ImageData();

            int glyphindex = 0;
            foreach (var a in data)
            {
                Line = ImageData.MergeLeftRight(Line, new ImageData(a, currentPF, Header.Glyphs.Size1, Header.Glyphs.Size2), 1);
                glyphindex++;
                if (glyphindex % 16 == 0)
                {
                    BMP = ImageData.MergeUpDown(BMP, Line, 0);
                    Line = new ImageData();
                }
            }
            BMP = ImageData.MergeUpDown(BMP, Line, 0);

            return BitmapSource.Create(BMP.PixelWidth, BMP.PixelHeight, 96, 96, BMP.PixelFormat, Palette.Pallete, BMP.Data, BMP.Stride);
        }

        private void SetFontImage(BitmapSource image)
        {
            int stride = (image.Format.BitsPerPixel * image.PixelWidth + 7) / 8;
            byte[] data = new byte[image.PixelHeight * stride];
            image.CopyPixels(data, stride, 0);

            ImageData BMP = new ImageData(data, image.Format, image.PixelWidth, image.PixelHeight);
            List<byte[]> BMPdata = new List<byte[]>();

            int row = 0;
            int column = 0;

            for (int i = 0; i < Header.Glyphs.Count; i++)
            {
                BMPdata.Add(ImageData.Crop(BMP, new ImageData.Rect(column * Header.Glyphs.Size1,
                    row * Header.Glyphs.Size2, Header.Glyphs.Size1, Header.Glyphs.Size2)).Data);
                column++;
                if (column == 16)
                {
                    row++;
                    column = 0;
                }
            }

            if (Header.Glyphs.BitsPerPixel == 4)
                Util.ReverseByteInList(BMPdata);

            Compressed.CompressData(BMPdata);
        }

        private XDocument GetWidthTable()
        {
            XDocument xDoc = new XDocument();
            XElement WT = new XElement("WidthTable");
            xDoc.Add(WT);

            XElement Line = null;
            int k = 0;
            for (int i = 0; i < WidthTable.Count; i++)
            {
                if (i % 16 == 0)
                {
                    k++;
                    Line = new XElement("Line_" + k);
                    WT.Add(Line);
                }
                XElement Glyph = new XElement("Glyph_" + ((i % 16) + 1));
                Line.Add(Glyph);
                Glyph.Add(new XElement("LeftCut", WidthTable[i]?.Left));
                Glyph.Add(new XElement("RightCut", WidthTable[i]?.Right));
            }

            Logging.Write("PersonaEditorLib", "Width Table was created.");
            return xDoc;
        }

        private void SetWidthTable(XDocument xDoc)
        {
            XElement WT = xDoc.Element("WidthTable");

            int index = 0;
            foreach (var line in WT.Elements())
            {
                int lineindex = Convert.ToInt32(line.Name.LocalName.Split('_')[1]);
                foreach (var glyph in line.Elements())
                {
                    int glyphindex = Convert.ToInt32(glyph.Name.LocalName.Split('_')[1]);
                    index = (lineindex - 1) * 16 + (glyphindex - 1);
                    WidthTable[index] = new VerticalCut(Convert.ToByte(glyph.Element("LeftCut").Value), Convert.ToByte(glyph.Element("RightCut").Value));
                }
            }

            Logging.Write("PersonaEditorLib", "Width Table was writed. Get " + index + " glyphs");
        }

        private byte[] Get()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(MS);

                Header.FileSize = 1 + Header.HeaderSize + Palette.Size + Reserved.Size + Compressed.Size();

                Header.Get(writer);
                Palette.Get(writer);
                WidthTable.Get(writer);
                Unknown.Get(writer);
                Reserved.Get(writer);
                Compressed.Get(writer);
                if (Last != null)
                    Last.Get(writer);

                returned = MS.ToArray();
            }

            return returned;
        }

        public BitmapSource Image
        {
            get { return GetFontImage(); }
            set { SetFontImage(value); }
        }
        public XDocument Table
        {
            get { return GetWidthTable(); }
            set { SetWidthTable(value); }
        }
        public byte[] This
        {
            get { return Get(); }
        }
    }
}