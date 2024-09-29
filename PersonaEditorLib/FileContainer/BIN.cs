using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using AuxiliaryLibraries.Tools;

namespace PersonaEditorLib.FileContainer
{
    public class BIN : IGameData
    {
        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public BIN(string path)
        {
            Open(File.ReadAllBytes(path));
        }

        public BIN(byte[] data)
        {
            Open(data);
        }

        private void Open(byte[] data)
        {
            FES_format = false;
            Old = false;

            if (data[0] == 0)
            {
                IsLittleEndian = false;
                OpenNew(data);
            }
            else if (data[0] == 100 && data[1] == 0 && data[2] == 0 && data[3] == 0)
            {
                FES_format = true;
                IsLittleEndian = true;
                OpenSpecialFES(data);
            }
            else if (data[3] == 0 && data[4] != 0  && data[4] != 3)
            {
                IsLittleEndian = true;
                OpenNew(data);
            }
            else {
                Old = true;
                OpenOld(data);
            }
                
        }

        private void OpenOld(byte[] data)
        {
            IsLittleEndian = true;
            if (data.Length < 0x100)
                throw new System.Exception("BIN: data length unacceptable");
            using (BinaryReader reader = IOTools.OpenReadFile(new MemoryStream(data), IsLittleEndian, false))
                while (reader.BaseStream.Position < reader.BaseStream.Length - 0x100)
                {
                    string Name = Encoding.ASCII.GetString(reader.ReadBytes(0x100 - 4)).TrimEnd('\0');
                    if (string.IsNullOrWhiteSpace(Name) || Name.Contains('\0'))
                        throw new Exception("BIN: entry name is empty or have wrong char");

                    int Size = reader.ReadInt32();
                    byte[] Data = reader.ReadBytes(Size);
                    if (Data.Length != Size)
                        throw new Exception("BIN: readed size less than entry size");

                    reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position, 0x40);

                    GameFile objectFile = GameFormatHelper.OpenFile(Name, Data, GameFormatHelper.GetFormat(Name));
                    if (objectFile == null)
                        objectFile = GameFormatHelper.OpenFile(Name, Data, FormatEnum.DAT);
                    SubFiles.Add(objectFile);
                }
        }

        private void OpenNew(byte[] data)
        {
            using (BinaryReader reader = IOTools.OpenReadFile(new MemoryStream(data), IsLittleEndian))
            {
                int count = reader.ReadInt32();
                if (count == 0)
                    throw new Exception("BIN: count is zero");

                for (int i = 0; i < count; i++)
                {
                    string Name = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).TrimEnd('\0');
                    if (string.IsNullOrWhiteSpace(Name) || Name.Contains('\0'))
                        throw new Exception("BIN: entry name is empty or have wrong char");

                    int Size = reader.ReadInt32();
                    byte[] Data = reader.ReadBytes(Size);
                    if (Data.Length != Size)
                        throw new Exception("BIN: readed size less than entry size");
                    
                    GameFile objectFile = GameFormatHelper.OpenFile(Name, Data, GameFormatHelper.GetFormat(Name));
                    if (objectFile == null)
                        objectFile = GameFormatHelper.OpenFile(Name, Data, FormatEnum.DAT);
                    SubFiles.Add(objectFile);
                }

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new System.Exception("BIN: read error");
            }
        }

        private void OpenSpecialFES(byte[] data)
        {
            using (BinaryReader reader = IOTools.OpenReadFile(new MemoryStream(data), IsLittleEndian))
            {
                reader.BaseStream.Seek(4, SeekOrigin.Begin);

                int count = reader.ReadInt32();
                if (count == 0)
                    throw new Exception("BIN: incorrect special id");

                int[] blockPosition = new int[count];
                int[] blockSize = new int[count];

                for (int i = 0; i < count; i++)
                {
                    blockPosition[i] = reader.ReadInt32();
                    blockSize[i] = reader.ReadInt32();
                }

                for (int i = 0; i < count; i++)
                {
                    reader.BaseStream.Seek(blockPosition[i], SeekOrigin.Begin);

                    int Size = blockSize[i];
                    byte[] Data = reader.ReadBytes(Size);
                    
                    if (Data.Length != Size)
                        throw new Exception("BIN: readed size less than entry size");

                    FormatEnum format = GameFormatHelper.GetFormat(Data);
                    String Name = "Unnamed_FES_" + i + "." + format.ToString();

                    GameFile objectFile;

                    try {
                        objectFile = GameFormatHelper.OpenFile(Name, Data, format);
                    }
                    catch {
                        objectFile = GameFormatHelper.OpenFile(Name, Data, FormatEnum.DAT);
                    }
                    if (objectFile == null)
                        objectFile = GameFormatHelper.OpenFile(Name, Data, FormatEnum.DAT);
                    SubFiles.Add(objectFile);
                }
                
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new System.Exception("BIN: read error");
            }
        }

        public GameFile this[int index]
        {
            get
            {
                if (SubFiles.Count > index)
                {
                    return SubFiles[index];
                }
                return null;
            }
        }

        bool Old = true;
        bool FES_format = false;
        bool IsLittleEndian { get; set; } = true;

        #region IGameFile

        public FormatEnum Type => FormatEnum.BIN;

        public List<GameFile> GetSubFiles()
        {
            return SubFiles;
        }

        public int GetSize()
        {
            int returned = 0;

            if (Old)
            {
                foreach (var a in SubFiles)
                {
                    returned += 0x100;
                    returned += a.GameData.GetSize();
                    returned += IOTools.Alignment(returned, 0x40);
                }

                returned += 0x100;
            }
            else
            {
                returned += 4;
                foreach (var a in SubFiles)
                {
                    returned += 0x20 + 4;
                    int size = a.GameData.GetSize();
                    int align = IOTools.Alignment(size, 0x20);
                    returned += size + align;
                }
            }

            return returned;
        }

        public byte[] GetData()
        {
            if (FES_format)
                return GetFES();
            else if (Old)
                return GetOld();
            else
                return GetNew();
        }

        #endregion

        private byte[] GetOld()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

                foreach (var a in SubFiles)
                {
                    byte[] name = new byte[0x100 - 4];
                    Encoding.ASCII.GetBytes(a.Name, 0, a.Name.Length, name, 0);
                    writer.Write(name);
                    byte[] data = a.GameData.GetData();
                    int size = a.GameData.GetSize();
                    if (data.Length != size)
                    {

                    }
                    writer.Write(size);
                    writer.Write(data);
                    writer.Write(new byte[IOTools.Alignment(MS.Position, 0x40)]);
                }

                writer.Write(new byte[0x100]);

                return MS.ToArray();
            }
        }

        private byte[] GetNew()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

                writer.Write((int)SubFiles.Count);
                foreach (var a in SubFiles)
                {
                    writer.Write(Encoding.ASCII.GetBytes(a.Name));
                    writer.Write(new byte[IOTools.Alignment(a.Name.Length, 0x20)]);
                    int size = a.GameData.GetSize();
                    int align = IOTools.Alignment(size, 0x20);
                    writer.Write(size + align);
                    writer.Write(a.GameData.GetData());
                    writer.Write(new byte[align]);
                }

                return MS.ToArray();
            }
        }

        private byte[] GetFES()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

                writer.Write(100);
                writer.Write(SubFiles.Count);

                int fileAddress = 8 + (SubFiles.Count * 8);
                int align = 0;
                
                foreach (var a in SubFiles)
                {
                    int fileSize = a.GameData.GetSize();
                    writer.Write(fileAddress + align);
                    writer.Write(fileSize);

                    fileAddress += fileSize + align;
                    align = IOTools.Alignment(a.GameData.GetSize(), 0x4);
                }

                foreach (var a in SubFiles)
                {
                    align = IOTools.Alignment(a.GameData.GetSize(), 0x4);
                    writer.Write(a.GameData.GetData());
                    writer.Write(new byte[align]);
                }

                return MS.ToArray();
            }
        }
    }
}