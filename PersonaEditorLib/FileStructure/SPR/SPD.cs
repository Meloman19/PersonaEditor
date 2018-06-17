using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PersonaEditorLib.Interfaces;
using System.Collections.ObjectModel;

namespace PersonaEditorLib.FileStructure.SPR
{
    public class SPD : IPersonaFile
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
                    streamFile.Stream.Position += 8;
                    long texPos = reader.ReadUInt32();
                    int texSize = reader.ReadInt32();
                    streamFile.Stream.Position += 16;
                    string name = Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd('\0');

                    long tempPos = streamFile.Stream.Position;
                    streamFile.Stream.Position = texPos;
                    var text = Utilities.PersonaFile.OpenFile(name + ".dds", reader.ReadBytes(texSize), FileType.DDS);
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

        #region IPersonaFile

        public FileType Type => FileType.SPD;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public ReadOnlyObservableCollection<PropertyClass> GetProperties => null;

        #endregion IPersonaFile

        #region IFile

        public int Size()
        {
            int returned = 0;

            returned += 0x20; // Add Header
            returned += SubFiles.Count * 0x30; // Add Textures Header
            returned += KeyList.Count * 0xa0; // Add Keys
            SubFiles.ForEach(x => returned += (x.Object as Graphic.DDS).Size()); // Add Textures

            return returned;
        }

        public byte[] Get()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS, Encoding.ASCII, true))
            {
                writer.Write(MagicNumber);
                writer.Write(0x2);
                writer.Write(Size());
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
                        writer.Write(i);
                        writer.Write(0);
                        writer.Write(pos[i]);
                        writer.Write(dds.Size());
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

        #endregion IFile
    }
}