using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.Other
{
    public class FTD : IGameData
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

        private void Read(StreamPart streamFile)
        {
            GetEndian(streamFile);

            streamFile.Stream.Position = streamFile.Position;
            using (BinaryReader reader = IOTools.OpenReadFile(streamFile.Stream, IsLittleEndian))
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

        #region IGameFile

        public FormatEnum Type => FormatEnum.FTD;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            int returned = 0x10;

            returned += Entries.Count * 4;
            returned += IOTools.Alignment(returned, 0x10);

            int entryoffset = 0;

            if (type == 0)
                entryoffset = 0x10;
            else if (type == 1)
                entryoffset = 4;

            foreach (var a in Entries)
            {
                returned += entryoffset;
                returned += a.Sum(x => x.Length);
                returned += IOTools.Alignment(returned, 0x10);
            }

            return returned;
        }

        public byte[] GetData()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
            {
                writer.Write(HeaderNumber);
                writer.Write(MagicNumber);
                writer.Write(GetSize());
                writer.Write((uint)((type << 16) + Entries.Count));
                writer.BaseStream.Position += Entries.Count * 4;
                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);

                if (type == 0)
                    GetT0(writer);
                else if (type == 1)
                    GetT1(writer);

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion

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
                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);
            }

            writer.Write(new byte[IOTools.Alignment(writer.BaseStream.Position, 0x10)]);
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
                writer.BaseStream.Position += IOTools.Alignment(writer.BaseStream.Position, 0x10);
            }

            writer.Write(new byte[IOTools.Alignment(writer.BaseStream.Position, 0x10)]);
            writer.BaseStream.Position = 0x10;
            newPos.ForEach(x => writer.Write(x));
        }
    }
}