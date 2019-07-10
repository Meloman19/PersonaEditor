// Persona 3/4/5 font decompressor/compressor
// Based on RikuKH3's decompressor algorithm

using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PersonaEditorLib.Other
{
    public class FNT : IGameData, IImage, ITable
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

        public FNT(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(MS, 0);
        }

        public void Resize(int size)
        {
            Header.Resize(size);
            WidthTable.Resize(size);
            Unknown.Resize(size);
            Reserved.Resize(size);
            Compressed.Resize(size);
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

        #region IGameFile

        public FormatEnum Type => FormatEnum.FNT;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize() => Header.HeaderSize + Palette.Size + WidthTable.Size() + Unknown.Size() + Reserved.Size + Compressed.Size();

        public byte[] GetData()
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
                {
                    Header.LastPosition = Last.Get(writer);
                    writer.BaseStream.Position = 0;
                    Header.Get(writer);
                }

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion

        #region IImage

        public Bitmap GetBitmap()
        {
            List<byte[]> data = Compressed.GetDecompressedData();
            PixelFormat currentPF;
            if (Header.Glyphs.BitsPerPixel == 4)
            {
                currentPF = PixelFormats.Indexed4Reverse;
                //Tool.ArrayTool.ReverseByteInList(data);
            }
            else if (Header.Glyphs.BitsPerPixel == 8)
                currentPF = PixelFormats.Indexed8;
            else return null;

            int width = Header.Glyphs.Size1 * 16;
            int height = Header.Glyphs.Size2 * (int)Math.Ceiling(data.Count / 16d);

            int stride = ImageHelper.GetStride(currentPF, Header.Glyphs.Size1);
            int stridenew = ImageHelper.GetStride(currentPF, width);
            byte[] newData = new byte[ImageHelper.GetStride(currentPF, width) * height];

            for (int i = 0, offset = 0; i < data.Count; i++)
            {
                byte[] current = data[i];
                for (int y = 0; y < Header.Glyphs.Size2; y++)
                    for (int x = 0; x < stride; x++)
                        newData[offset + y * stridenew + x] = current[y * stride + x];

                if ((i + 1) % 16 == 0)
                {
                    offset += stridenew * (Header.Glyphs.Size2 - 1) + stride;
                }
                else
                    offset += stride;

            }

            return new Bitmap(width, height, currentPF, newData, Palette.GetImagePalette());
        }

        public void SetBitmap(Bitmap image)
        {
            PixelFormat pixelFormat;
            if (Header.Glyphs.BitsPerPixel == 4)
                pixelFormat = PixelFormats.Indexed4;
            else if (Header.Glyphs.BitsPerPixel == 8)
                pixelFormat = PixelFormats.Indexed8;
            else
                throw new Exception("FNT: Unknown Pixel Format");

            var palette = Palette.GetImagePalette();

            var tempBitmap = image.ConvertTo(pixelFormat, palette).CopyData();

            int srcStride = ImageHelper.GetStride(pixelFormat, image.Width);
            int rowSize = srcStride * Header.Glyphs.Size2;

            int columnStride = ImageHelper.GetStride(pixelFormat, Header.Glyphs.Size1);
            int dstDataSize = columnStride * Header.Glyphs.Size2;

            List<byte[]> BMPdata = new List<byte[]>();

            int row = 0;
            int column = 0;
            for (int i = 0; i < Header.Glyphs.Count; i++)
            {
                byte[] glyph = new byte[dstDataSize];

                for (int k = 0; k < Header.Glyphs.Size2; k++)
                {
                    byte[] temp = tempBitmap.SubArray
                        (row * rowSize + column * columnStride + k * srcStride,
                        columnStride);

                    Buffer.BlockCopy(temp, 0, glyph, k * columnStride, columnStride);
                }

                column++;
                if (column == 16)
                {
                    row++;
                    column = 0;
                }

                BMPdata.Add(glyph);
            }

            if (Header.Glyphs.BitsPerPixel == 4)
                AuxiliaryLibraries.Tools.ArrayTool.ReverseByteInList(BMPdata);

            Compressed.CompressData(BMPdata);
        }

        #endregion IImage

        #region ITable

        public XDocument GetTable()
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

            //Logging.Write("PersonaEditorLib", "Width Table was created.");
            return xDoc;
        }

        public void SetTable(XDocument xDoc)
        {
            XElement WT = xDoc.Element("WidthTable");

            int index = 0;

            try
            {
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
            }
            catch (Exception e)
            {
                // Logging.Write("PersonaEditorLib", e.Message);
            }

            //Logging.Write("PersonaEditorLib", "Width Table was writed. Get " + index + " glyphs");
        }

        #endregion ITable
    }
}