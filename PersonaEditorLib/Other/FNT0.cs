using AuxiliaryLibraries;
using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PersonaEditorLib.Other
{
    public class FNT0 : IGameData, IImage, ITable
    {
        private List<byte[]> glyphs = null;
        private bool glyphsChanged = false;

        private readonly uint MagicNumber = 0x30544E46;
        private bool IsLittleEndian = true;
        private byte[] Unknown = null;
        private int UnknownInt;

        public ushort Width { get; private set; } = 0;
        public ushort Height { get; private set; } = 0;

        public FNTCompressed Compressed { get; private set; }

        public FNT0(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            using (MemoryStream MS = new MemoryStream(data))
                Read(MS);
        }

        private void Read(Stream stream)
        {
            using (BinaryReader reader = IOTools.OpenReadFile(stream, IsLittleEndian))
            {
                // Read Header
                if (reader.ReadUInt32() != MagicNumber)
                    throw new Exception("FNT0 Header: wrong magicnumber");
                if (reader.ReadUInt32() != 0x00010000)
                    throw new Exception("FNT0 Header: wrong unknown");
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                UnknownInt = reader.ReadInt32();
                //if (reader.ReadInt32() != 0)
                //    throw new Exception("FNT0 Header: wrong padding");

                int unknownPos = reader.ReadInt32();
                int dicPos = reader.ReadInt32();

                // Read Unknown Block
                Unknown = reader.ReadBytes(dicPos - unknownPos);

                // Read Combressed Block
                Compressed = new FNTCompressed(reader);

                if (stream.Position != stream.Length)
                    throw new Exception("FNT0 Header: wrong size");
            }
        }

        public Rectangle[] GetGlyphRect()
        {
            if (glyphs == null)
                glyphs = Compressed.GetDecompressedData();
            return glyphs.Select(x => new Rectangle(x[0], x[1], x[2], x[3])).ToArray();
        }

        public void SetGlyphRect(Rectangle[] rectangles)
        {
            if (rectangles == null)
                throw new ArgumentNullException("rectangles");

            int count = Compressed.Header.GlyphTableCount - 1;
            if (rectangles.Length == count)
            {
                if (glyphs == null)
                    glyphs = Compressed.GetDecompressedData();

                for (int i = 0; i < glyphs.Count; i++)
                {
                    glyphs[i][0] = (byte)rectangles[i].X;
                    glyphs[i][1] = (byte)rectangles[i].Y;
                    glyphs[i][2] = (byte)rectangles[i].Width;
                    glyphs[i][3] = (byte)rectangles[i].Height;
                }
                glyphsChanged = true;
            }
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.FNT0;

        public List<GameFile> SubFiles => new List<GameFile>();

        public int GetSize()
        {
            return 0x18 + Unknown.Length + Compressed.Size();
        }

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
            {
                writer.Write(MagicNumber);
                writer.Write(0x00010000);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(UnknownInt);
                writer.Write(0x18);
                writer.Write(0x18 + Unknown.Length);

                writer.Write(Unknown);

                if (glyphsChanged)
                    Compressed.CompressData(glyphs);
                Compressed.Get(writer);

                return MS.ToArray();
            }
        }

        #endregion

        #region IImage

        public Bitmap GetBitmap()
        {
            if (glyphs == null)
                glyphs = Compressed.GetDecompressedData();

            PixelFormat pixelFormat = PixelFormats.Indexed8;
            Color[] palette = ImageHelper.GetGrayPalette(8);

            int stride = ImageHelper.GetStride(pixelFormat, Width);

            int imageWidth = Width * 16;
            int imageHeight = Height * (int)Math.Ceiling(glyphs.Count / 16d);
            int imageStride = ImageHelper.GetStride(pixelFormat, imageWidth);

            byte[] newData = new byte[imageStride * imageHeight];

            for (int i = 0, offset = 0; i < glyphs.Count; i++)
            {
                ArraySection<byte> current = new ArraySection<byte>(glyphs[i], 6, Width * Height);
                // ArraySegment<byte> current = new ArraySegment<byte>(data[i], 6, width * height);
                //  byte[] current = System.ArraySegment< data[i];
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < stride; x++)
                        newData[offset + y * imageStride + x] = current[y * stride + x];

                if ((i + 1) % 16 == 0)
                {
                    offset += imageStride * (Height - 1) + stride;
                }
                else
                    offset += stride;

            }

            return new Bitmap(imageWidth, imageHeight, pixelFormat, newData, palette);
        }

        public void SetBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            if (glyphs == null)
                glyphs = Compressed.GetDecompressedData();

            PixelFormat pixelFormat = PixelFormats.Indexed8;
            Color[] palette = ImageHelper.GetGrayPalette(8);

            var tempBitmap = bitmap.ConvertTo(pixelFormat, palette).CopyData();

            int srcStride = ImageHelper.GetStride(pixelFormat, bitmap.Width);
            int rowSize = srcStride * Height;
            int columnStride = ImageHelper.GetStride(pixelFormat, Width);

            int row = 0;
            int column = 0;
            for (int i = 0; i < glyphs.Count; i++)
            {
                byte[] glyph = glyphs[i];

                for (int k = 0; k < Height; k++)
                {
                    byte[] temp = tempBitmap.SubArray
                        (row * rowSize + column * columnStride + k * srcStride,
                        columnStride);

                    Buffer.BlockCopy(temp, 0, glyph, 6 + k * columnStride, columnStride);
                }

                column++;
                if (column == 16)
                {
                    row++;
                    column = 0;
                }
            }

            glyphsChanged = true;
        }

        #endregion

        #region ITable

        public XDocument GetTable()
        {
            if (glyphs == null)
                glyphs = Compressed.GetDecompressedData();

            XDocument xDoc = new XDocument();
            XElement WT = new XElement("RectTable");
            xDoc.Add(WT);

            XElement Line = null;
            int k = 0;
            for (int i = 0; i < glyphs.Count; i++)
            {
                if (i % 16 == 0)
                {
                    k++;
                    Line = new XElement("Line_" + k);
                    WT.Add(Line);
                }
                XElement Glyph = new XElement("Glyph_" + ((i % 16) + 1));
                Line.Add(Glyph);
                Glyph.Add(new XElement("X", glyphs[i][0]));
                Glyph.Add(new XElement("Y", glyphs[i][1]));
                Glyph.Add(new XElement("Width", glyphs[i][2]));
                Glyph.Add(new XElement("Height", glyphs[i][3]));
            }

            //Logging.Write("PersonaEditorLib", "Width Table was created.");
            return xDoc;
        }

        public void SetTable(XDocument xDoc)
        {
            if (xDoc == null)
                throw new ArgumentNullException("xDoc");
            if (glyphs == null)
                glyphs = Compressed.GetDecompressedData();

            int index = 0;

            try
            {
                XElement WT = xDoc.Element("RectTable");
                foreach (var line in WT.Elements())
                {
                    int lineindex = Convert.ToInt32(line.Name.LocalName.Split('_')[1]);
                    foreach (var glyph in line.Elements())
                    {
                        int glyphindex = Convert.ToInt32(glyph.Name.LocalName.Split('_')[1]);
                        index = (lineindex - 1) * 16 + (glyphindex - 1);
                        glyphs[index][0] = Convert.ToByte(glyph.Element("X").Value);
                        glyphs[index][1] = Convert.ToByte(glyph.Element("Y").Value);
                        glyphs[index][2] = Convert.ToByte(glyph.Element("Width").Value);
                        glyphs[index][3] = Convert.ToByte(glyph.Element("Height").Value);
                    }
                }
            }
            catch (Exception e)
            {
                return;
                // Logging.Write("PersonaEditorLib", e.Message);
            }

            glyphsChanged = true;

            //Logging.Write("PersonaEditorLib", "Width Table was writed. Get " + index + " glyphs");
        }

        #endregion
    }
}