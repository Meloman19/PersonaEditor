using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using PersonaEditorLib.Interfaces;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.SPR
{
    public class SPDKey : BindingObject
    {
        public int ListIndex { get; private set; }
        public int TextureIndex { get; private set; }
        public int[] Unk0x08 { get; private set; } // x 6
        public int X0 { get; set; }
        public int Y0 { get; set; }
        public int Xdel { get; set; }
        public int Ydel { get; set; }
        public int[] Unk0x30 { get; private set; } // x 2
        public int[] Unk0x40 { get; private set; } // x 12
        public byte[] Comment { get; private set; }

        public SPDKey(BinaryReader reader)
        {
            ListIndex = reader.ReadInt32();
            TextureIndex = reader.ReadInt32();
            Unk0x08 = reader.ReadInt32Array(6);
            X0 = reader.ReadInt32();
            Y0 = reader.ReadInt32();
            Xdel = reader.ReadInt32();
            Ydel = reader.ReadInt32();
            Unk0x30 = reader.ReadInt32Array(2);

            if (reader.ReadInt32() != Xdel)
                throw new Exception("SPDKey: Xdel wrong");
            if (reader.ReadInt32() != Ydel)
                throw new Exception("SPDKey: Ydel wrong");

            Unk0x40 = reader.ReadInt32Array(12);
            Comment = reader.ReadBytes(0x30);
        }

        public int Size
        {
            get { return 0xa0; }
        }

        public void Get(BinaryWriter writer)
        {
            writer.Write(ListIndex);
            writer.Write(TextureIndex);
            writer.WriteInt32Array(Unk0x08);
            writer.Write(X0);
            writer.Write(Y0);
            writer.Write(Xdel);
            writer.Write(Ydel);
            writer.WriteInt32Array(Unk0x30);
            writer.Write(Xdel);
            writer.Write(Ydel);
            writer.WriteInt32Array(Unk0x40);
            writer.Write(Comment);
        }
    }

    public class SPDTextureKey
    {
        public string Name { get; set; }
    }

    public class SPD : IPersonaFile
    {
        public const int MagicNumber = 0x30525053;

        public List<SPDKey> KeyList { get; } = new List<SPDKey>();

        public SPD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamFile(MS, MS.Length, 0));
        }

        private void Read(StreamFile streamFile)
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

        private void ReadTexture(StreamFile streamFile, int pos, int count)
        {
            streamFile.Stream.Position = streamFile.Position + pos;

            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
                for (int i = 0; i < count; i++)
                {
                    streamFile.Stream.Position += 8;
                    long texPos = reader.ReadUInt32();
                    int texSize = reader.ReadInt32();
                    streamFile.Stream.Position += 16;
                    string name = Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0');

                    long tempPos = streamFile.Stream.Position;
                    streamFile.Stream.Position = texPos;
                    SubFiles.Add(Utilities.PersonaFile.OpenFile(name + ".dds", reader.ReadBytes(texSize), Interfaces.FileType.DDS));
                    streamFile.Stream.Position = tempPos;
                }
        }

        private void ReadKey(StreamFile streamFile, int pos, int count)
        {
            streamFile.Stream.Position = streamFile.Position + pos;

            using (BinaryReader reader = new BinaryReader(streamFile.Stream, Encoding.ASCII, true))
                for (int i = 0; i < count; i++)
                    KeyList.Add(new SPDKey(reader));
        }

        #region IPersonaFile

        public FileType Type => FileType.SPD;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size
        {
            get
            {
                int returned = 0;

                returned += 0x20; // Add Header
                returned += SubFiles.Count * 0x30; // Add Textures Header
                returned += KeyList.Count * 0xa0; // Add Keys
                SubFiles.ForEach(x => returned += (x.Object as Graphic.DDS).Size); // Add Textures

                return returned;
            }
        }

        public byte[] Get()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS, Encoding.ASCII, true))
            {
                writer.Write(MagicNumber);
                writer.Write(0x2);
                writer.Write(Size);
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
                    writer.Write((x.Object as Graphic.DDS).Get());
                });

                MS.Position = 0x20;
                for (int i = 0; i < SubFiles.Count; i++)
                    if (SubFiles[i].Object is Graphic.DDS dds)
                    {
                        writer.Write(i + 1);
                        writer.Write(0);
                        writer.Write(pos[i]);
                        writer.Write(dds.Size);
                        writer.Write(dds.Header.Width);
                        writer.Write(dds.Header.Height);
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

        #endregion IFile
    }
}