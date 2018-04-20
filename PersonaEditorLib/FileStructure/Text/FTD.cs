using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersonaEditorLib.Extension;

namespace PersonaEditorLib.FileStructure.Text
{
    public class FTD : IPersonaFile, IFile
    {
        public const uint HeaderNumber = 0x00010000;
        public const uint MagicNumber = 0x46544430;

        public List<byte[][]> Entries = new List<byte[][]>();

        public FTD(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamFile(MS, MS.Length, 0));
        }

        private void Read(StreamFile streamFile)
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

                int entryCount = reader.ReadInt32();

                List<int> Entry = new List<int>();
                for (int i = 0; i < entryCount; i++)
                    Entry.Add(reader.ReadInt32());

                foreach (var a in Entry)
                {
                    streamFile.Stream.Position = a + 4;
                    int size = reader.ReadInt32();
                    int count = reader.ReadInt32();
                    streamFile.Stream.Position += 4;
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
        }

        private void GetEndian(StreamFile streamFile)
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

        public int Size()
        {
            int returned = 0x10;

            returned += Entries.Count * 4;
            returned += Utilities.Utilities.Alignment(returned, 0x10);

            foreach (var a in Entries)
            {
                returned += 0x10;
                returned += a.Sum(x => x.Length);
                returned += Utilities.Utilities.Alignment(returned, 0x10);
            }

            return returned;
        }

        public byte[] Get()
        {
            byte[] returned;

            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriterBE writer = new BinaryWriterBE(MS, Encoding.ASCII, true))
            {
                writer.Write(HeaderNumber);
                writer.Write(MagicNumber);
                writer.Write(Size());
                writer.Write(Entries.Count);
                MS.Position += Entries.Count * 4;
                MS.Position += Utilities.Utilities.Alignment(MS.Position, 0x10);

                List<int> newPos = new List<int>();
                foreach (var entry in Entries)
                {
                    newPos.Add((int)MS.Position);
                    MS.Position += 4;
                    writer.Write(entry.Sum(x => x.Length));
                    writer.Write(entry.Length);
                    MS.Position += 4;
                    foreach (var part in entry)
                        writer.Write(part);
                    MS.Position += Utilities.Utilities.Alignment(MS.Position, 0x10);
                }

                writer.Write(new byte[Utilities.Utilities.Alignment(MS.Position, 0x10)]);
                MS.Position = 0x10;
                newPos.ForEach(x => writer.Write(x));

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion IFile      
    }
}