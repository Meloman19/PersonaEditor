using AuxiliaryLibraries.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PersonaEditorLib.SpriteContainer
{
    public class SPD : IGameData, ITable
    {
        public const int MagicNumber = 0x30525053;

        public List<SPDKey> KeyList { get; } = new List<SPDKey>();

        public SPD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, MS.Length, 0));
        }

        private void Read(StreamPart streamFile)
        {
            streamFile.Stream.Position = streamFile.Position;

            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
            {
                if (reader.ReadInt32() != MagicNumber)
                    throw new Exception("SPD: wrong Magic Number");
                if (reader.ReadInt32() != 0x2)
                { }
                if (reader.ReadInt32() != streamFile.Size)
                { }
                if (reader.ReadInt32() != 0)
                { }
                if (reader.ReadInt32() != 0x20)
                { }
                ushort TextureCount = reader.ReadUInt16();
                ushort KeyCount = reader.ReadUInt16();
                int TextureKeyPosition = reader.ReadInt32();
                int KeyPosition = reader.ReadInt32();

                ReadTexture(streamFile, TextureKeyPosition, TextureCount);
                ReadKey(streamFile, KeyPosition, KeyCount);
            }

        }

        private void ReadTexture(StreamPart streamFile, int pos, int count)
        {
            streamFile.Stream.Position = streamFile.Position + pos;

            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
                for (int i = 0; i < count; i++)
                {
                    int tag = reader.ReadInt32();
                    streamFile.Stream.Position += 4;
                    long texPos = reader.ReadUInt32();
                    int texSize = reader.ReadInt32();
                    streamFile.Stream.Position += 16;
                    string name = Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0');

                    long tempPos = streamFile.Stream.Position;
                    streamFile.Stream.Position = texPos;
                    var text = GameFormatHelper.OpenFile(name + ".dds", reader.ReadBytes(texSize), FormatEnum.DDS);
                    text.Tag = tag;
                    SubFiles.Add(text);
                    streamFile.Stream.Position = tempPos;
                }
        }

        private void ReadKey(StreamPart streamFile, int pos, int count)
        {
            streamFile.Stream.Position = streamFile.Position + pos;

            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
                for (int i = 0; i < count; i++)
                    KeyList.Add(new SPDKey(reader));
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.SPD;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            int returned = 0;

            returned += 0x20; // Add Header
            returned += SubFiles.Count * 0x30; // Add Textures Header
            returned += KeyList.Count * 0xa0; // Add Keys
            SubFiles.ForEach(x => returned += (x.GameData as Sprite.DDS).GetSize()); // Add Textures

            return returned;
        }

        public byte[] GetData()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS, Encoding.ASCII, true))
            {
                writer.Write(MagicNumber);
                writer.Write(0x2);
                writer.Write(GetSize());
                writer.Write(0);
                writer.Write(0x20);
                writer.Write((ushort)SubFiles.Count);
                writer.Write((ushort)KeyList.Count);
                writer.Write(0x20);
                writer.Write(0x20 + SubFiles.Count * 0x30);

                List<int> pos = new List<int>();

                SubFiles.ForEach(x => writer.Write(new byte[0x30]));
                KeyList.ForEach(x => x.Get(writer));

                SubFiles.ForEach(x =>
                {
                    pos.Add((int)MS.Position);
                    writer.Write((x.GameData as Sprite.DDS).GetData());
                });

                MS.Position = 0x20;
                for (int i = 0; i < SubFiles.Count; i++)
                    if (SubFiles[i].GameData is Sprite.DDS dds)
                    {
                        writer.Write((int)SubFiles[i].Tag);
                        writer.Write(0);
                        writer.Write(pos[i]);
                        writer.Write(dds.GetSize());
                        writer.Write(dds.Width);
                        writer.Write(dds.Height);
                        writer.Write(0);
                        writer.Write(0);
                        byte[] temp = new byte[0x10];
                        string temp2 = Path.GetFileNameWithoutExtension(SubFiles[i].Name);
                        Encoding.ASCII.GetBytes(temp2, 0, temp2.Length, temp, 0);
                        writer.Write(temp);
                    }

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion

        #region ITable

        public XDocument GetTable()
        {
            XDocument xDoc = new XDocument();
            XElement WT = new XElement("SpriteInfo");
            xDoc.Add(WT);

            foreach (var key in KeyList)
            {
                XElement Key = new XElement("Key");

                string temp = Encoding.GetEncoding("shift-jis").GetString(key.Comment).TrimEnd('\0');
                if (temp.Contains("\0"))
                {

                }

                XAttribute attribute = new XAttribute("Name", temp);
                Key.Add(attribute);

                XElement index = new XElement("Index", key.ListIndex.ToString());
                Key.Add(index);
                XElement texIndex = new XElement("TextureIndex", key.TextureIndex.ToString());
                Key.Add(texIndex);
                XElement X0 = new XElement("X", key.X0.ToString());
                Key.Add(X0);
                XElement Y0 = new XElement("Y", key.Y0.ToString());
                Key.Add(Y0);
                XElement width = new XElement("Width", key.Xdel.ToString());
                Key.Add(width);
                XElement height = new XElement("Height", key.Ydel.ToString());
                Key.Add(height);

                WT.Add(Key);
            }

            return xDoc;
        }

        public void SetTable(XDocument xDoc)
        {
            try
            {
                XElement SI = xDoc.Element("SpriteInfo");

                foreach (var key in SI.Elements())
                {
                    int index = int.Parse(key.Element("Index").Value);
                    int texIndex = int.Parse(key.Element("TextureIndex").Value);
                    int X0 = int.Parse(key.Element("X").Value);
                    int Y0 = int.Parse(key.Element("Y").Value);
                    int width = int.Parse(key.Element("Width").Value);
                    int height = int.Parse(key.Element("Height").Value);

                    var findKey = KeyList.Find(x => x.ListIndex == index && x.TextureIndex == texIndex);
                    if (findKey != null)
                    {
                        findKey.X0 = X0;
                        findKey.Y0 = Y0;
                        findKey.Xdel = width;
                        findKey.Ydel = height;
                    }
                    else
                    {

                    }
                }
            }
            catch { }
        }

        #endregion ITable
    }
}