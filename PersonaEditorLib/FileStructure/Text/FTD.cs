using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersonaEditorLib.Extension;
using System.Collections.ObjectModel;

namespace PersonaEditorLib.FileStructure.Text
{
    public class FTD : IPersonaFile, IFile
    {
        public const uint HeaderNumber = 0x00010000;
        public const uint MagicNumber = 0x46544430;
        readonly List<byte[]> Unknown = new List<byte[]>();

        public List<byte[][]> Entries = new List<byte[][]>();

        private uint type = 0;

        public FTD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, MS.Length, 0));
        }

        public void ImportText(List<string[]> text, Encoding newEcoding)
        {
            foreach (var a in Entries)
            {
                if (a.Length > 1)
                    foreach (var b in a)
                    {
                        int ind = text.FindIndex(x => x[0] == newEcoding.GetString(b).TrimEnd('\0') && x[0] != "" && x[1] != "");
                        if (ind >= 0)
                        {
                            byte[] temp = newEcoding.GetBytes(text[ind][1]);

                            for (int i = 0; i < b.Length; i++)
                            {
                                if (i < temp.Length)
                                    b[i] = temp[i];
                                else
                                    b[i] = 0;
                            }
                        }
                    }
                else
                {
                    int ind = text.FindIndex(x => x[0] == newEcoding.GetString(a[0]).TrimEnd('\0') && x[0] != "" && x[1] != "");
                    if (ind >= 0)
                    {
                        byte[] temp = newEcoding.GetBytes(text[ind][1]);
                        Array.Resize(ref temp, temp.Length + 1);
                        a[0] = temp;
                    }
                }
            }
        }

        private void Read(StreamPart streamFile)
        {
            GetEndian(streamFile);

            streamFile.Stream.Position = streamFile.Position;
            using (BinaryReader reader = Utilities.IO.OpenReadFile(streamFile.Stream, IsLittleEndian))
            {
                if (reader.ReadInt32() != HeaderNumber)
                    throw new Exception("FTD: header read error");

                if (reader.ReadUInt32() != MagicNumber)
                    throw new Exception("FTD: wrong magic number");

                if (reader.ReadInt32() > streamFile.Size)
                    throw new Exception("FTD: wrong file size");

                uint typeandcount = reader.ReadUInt32();
                type = typeandcount >> 16;
                uint count = typeandcount << 16 >> 16;

                if (type == 0)
                    ReadT0(reader, count);
                else if (type == 1)
                    ReadT1(reader, count);
                else
                    throw new Exception("FTD: unknown type");
            }
        }

        private void ReadT0(BinaryReader reader, uint entryCount)
        {
            List<int> Entry = new List<int>();
            for (int i = 0; i < entryCount; i++)
                Entry.Add(reader.ReadInt32());

            foreach (var a in Entry)
            {
                reader.BaseStream.Position = a + 4;
                int size = reader.ReadInt32();
                int count = reader.ReadInt32();
                reader.BaseStream.Position += 4;
                if (size % count != 0)
                    throw new Exception("FTD: Hm...");

                byte[] data = reader.ReadBytes(size);

                int subEntry_Size = size / count;

                byte[][] entries = new byte[count][];
                for (int i = 0; i < count; i++)
                    entries[i] = data.SubArray(i * subEntry_Size, subEntry_Size);

                Entries.Add(entries);
            }
        }

        private void ReadT1(BinaryReader reader, uint entryCount)
        {
            List<int> Entry = new List<int>();
            for (int i = 0; i < entryCount; i++)
                Entry.Add(reader.ReadInt32());

            foreach (var a in Entry)
            {
                reader.BaseStream.Position = a;
                byte size = reader.ReadByte();
                byte count = reader.ReadByte();
                reader.BaseStream.Position += 2;

                byte[] data = reader.ReadBytes(size);
                int subEntry_Size = size / count;
                byte[][] entries = new byte[count][];
                for (int i = 0; i < count; i++)
                    entries[i] = data.SubArray(i * subEntry_Size, subEntry_Size);

                Entries.Add(entries);
            }
        }

        private void GetEndian(StreamPart streamFile)
        {
            streamFile.Stream.Position = streamFile.Position + 4;
            byte[] magicnum = new byte[4];
            streamFile.Stream.Read(magicnum, 0, 4);

            if (BitConverter.ToUInt32(magicnum, 0) == MagicNumber)
                IsLittleEndian = true;
            else if (BitConverter.ToUInt32(magicnum.Reverse().ToArray(), 0) == MagicNumber)
                IsLittleEndian = false;
            else
                throw new Exception("FTD: wrong magic number");
        }

        private bool IsLittleEndian = true;

        #region IPersonaFile

        public FileType Type => FileType.FTD;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public ReadOnlyObservableCollection<PropertyClass> GetProperties => null;

        #endregion IPersonaFile

        #region IFile

        public int Size()
        {
            int returned = 0x10;

            returned += Entries.Count * 4;
            returned += Utilities.UtilitiesTool.Alignment(returned, 0x10);

            int entryoffset = 0;

            if (type == 0)
                entryoffset = 0x10;
            else if (type == 1)
                entryoffset = 4;

            foreach (var a in Entries)
            {
                returned += entryoffset;
                returned += a.Sum(x => x.Length);
                returned += Utilities.UtilitiesTool.Alignment(returned, 0x10);
            }

            return returned;
        }

        public byte[] Get()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian))
            {
                writer.Write(HeaderNumber);
                writer.Write(MagicNumber);
                writer.Write(Size());
                writer.Write((uint)((type << 16) + Entries.Count));
                writer.BaseStream.Position += Entries.Count * 4;
                writer.BaseStream.Position += Utilities.UtilitiesTool.Alignment(writer.BaseStream.Position, 0x10);

                if (type == 0)
                    GetT0(writer);
                else if (type == 1)
                    GetT1(writer);

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile      

        private void GetT0(BinaryWriter writer)
        {
            List<int> newPos = new List<int>();
            foreach (var entry in Entries)
            {
                newPos.Add((int)writer.BaseStream.Position);
                writer.BaseStream.Position += 4;
                writer.Write(entry.Sum(x => x.Length));
                writer.Write(entry.Length);
                writer.BaseStream.Position += 4;
                foreach (var part in entry)
                    writer.Write(part);
                writer.BaseStream.Position += Utilities.UtilitiesTool.Alignment(writer.BaseStream.Position, 0x10);
            }

            writer.Write(new byte[Utilities.UtilitiesTool.Alignment(writer.BaseStream.Position, 0x10)]);
            writer.BaseStream.Position = 0x10;
            newPos.ForEach(x => writer.Write(x));
        }

        private void GetT1(BinaryWriter writer)
        {
            List<int> newPos = new List<int>();
            foreach (var entry in Entries)
            {
                newPos.Add((int)writer.BaseStream.Position);
                writer.Write((byte)entry.Sum(x => x.Length));
                writer.Write((byte)entry.Length);
                writer.BaseStream.Position += 2;
                foreach (var part in entry)
                    writer.Write(part);
                writer.BaseStream.Position += Utilities.UtilitiesTool.Alignment(writer.BaseStream.Position, 0x10);
            }

            writer.Write(new byte[Utilities.UtilitiesTool.Alignment(writer.BaseStream.Position, 0x10)]);
            writer.BaseStream.Position = 0x10;
            newPos.ForEach(x => writer.Write(x));
        }
    }
}