using AuxiliaryLibraries.Extensions;
using PersonaEditorLib.Other;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.FileContainer
{
    public class PM1 : IGameData
    {
        #region Private Classes and Enum

        enum TypeMap : int
        {
            FileList = 0x1,
            T3 = 0x2,
            RMDHead = 0x3,
            BMD = 0x6,
            EPLHead = 0x7,
            EPL = 0x8,
            RMD = 0x9,
            TMXHead = 0x16,
            TMX = 0x17,
            CTable = 0x1B
        }

        #endregion

        #region Read Only

        readonly int textsize = 0x20;
        readonly int MagicNumber = 0x31444D50;

        #endregion

        #region Private Fields

        private byte[] Unknown;

        private List<GameFile> HidList = new List<GameFile>();

        private static int[] MainFileList = new int[]
        {
            0x1,
            0x2,
            0x3,
            0x6,
            0x7,
            0x8,
            0x9,
            0x16,
            0x17,
            0x1B
        };

        private static int[] NamedFilesList = new int[]
        {
            0x2,
            0x3,
            0x6,
            0x7,
            0x16,
            0x1B
        };

        private bool IsLittleEndian { get; set; } = true;

        #endregion

        #region Constructors

        public PM1(Stream stream)
        {
            Read(stream);
        }

        public PM1(string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Open, FileAccess.Read))
                Read(FS);
        }

        public PM1(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(MS);
        }

        #endregion

        private void Read(Stream stream)
        {
            using (BinaryReader reader = IOTools.OpenReadFile(stream, IsLittleEndian))
            {
                if (reader.ReadInt32() != 0)
                    throw new Exception("PM1 Read (0x0): Not equal 0");

                int fileSize = reader.ReadInt32();

                if (reader.ReadInt32() != MagicNumber)
                    throw new Exception("PM1 Read (0x8): Not equal MagicNumber");
                if (reader.ReadInt32() != 0)
                    throw new Exception("PM1 Read (0xC): Not equal 0 (padding?)");

                int tableElementCount = reader.ReadInt32();

                Unknown = reader.ReadBytes(12);
                var Table = reader.ReadInt32ArrayArray(tableElementCount, 4);

                int readSize = 0x20 + tableElementCount * 0x10;

                Dictionary<int, byte[][]> blocks = new Dictionary<int, byte[][]>();

                foreach (var a in Table.Where(x => x[1] * x[2] > 0))
                {
                    stream.Position = a[3];
                    byte[][] data = new byte[a[2]][];

                    for (int i = 0; i < a[2]; i++)
                    {
                        readSize += a[1];
                        data[i] = reader.ReadBytes(a[1]);
                    }

                    blocks.Add(a[0], data);
                }

                if (readSize != fileSize)
                    throw new Exception("PM1 Read: wrong file size");

                ReadNamed(blocks);
                ReadUnnamed(blocks);
            }
        }

        private void ReadNamed(Dictionary<int, byte[][]> data)
        {
            string[] fileNameList = new string[0];
            int fileNameIndex = 0;

            void ReadSingleFile(TypeMap typeMap)
            {
                if (data[(int)typeMap].Length > 1)
                    throw new Exception($"PM1 Read: {typeMap.ToString()}'s count more than 1");

                string name = fileNameList[fileNameIndex++];
                var singleFile = GameFormatHelper.OpenFile(name, data[(int)typeMap][0], GameFormatHelper.GetFormat(name));

                if (singleFile == null)
                    singleFile = GameFormatHelper.OpenFile(name, data[(int)typeMap][0], FormatEnum.DAT);

                singleFile.Tag = new object[] { (int)typeMap };
                SubFiles.Add(singleFile);
            }

            if (data.ContainsKey((int)TypeMap.FileList))
                fileNameList = data[(int)TypeMap.FileList].Select(x => Encoding.ASCII.GetString(x).TrimEnd('\0')).ToArray();
            else
            {
                foreach (var a in MainFileList)
                    if (data.ContainsKey(a))
                        throw new Exception("PM1 Read: file contains named files");
            }

            // Read T3
            if (data.ContainsKey((int)TypeMap.T3))
                ReadSingleFile(TypeMap.T3);

            // Read RMD
            if (data.ContainsKey((int)TypeMap.RMDHead))
            {
                if (data.ContainsKey((int)TypeMap.RMD))
                {
                    if (data[(int)TypeMap.RMD].Length > 1)
                        throw new Exception("PM1 Read: RMD's count more than 1");

                    using (BinaryReader RMDreader = IOTools.OpenReadFile(data[(int)TypeMap.RMD][0], IsLittleEndian))
                    {
                        var rmdHeaders = data[(int)TypeMap.RMDHead]
                            .Select(x =>
                            {
                                using (BinaryReader BR = IOTools.OpenReadFile(x, IsLittleEndian))
                                    return BR.ReadInt32Array(8);
                            })
                            .ToArray();

                        for (int i = 0; i < rmdHeaders.Length; i++)
                        {
                            RMDreader.BaseStream.Position = rmdHeaders[i][4] - rmdHeaders[0][4];
                            var rmd = GameFormatHelper.OpenFile(fileNameList[fileNameIndex++], RMDreader.ReadBytes(rmdHeaders[i][5]), FormatEnum.DAT);
                            rmd.Tag = new object[] { (int)TypeMap.RMD, rmdHeaders[i] };
                            SubFiles.Add(rmd);
                        }
                    }
                }
                else
                    throw new Exception("PM1 Read: file contain RMD Header, but not RMD");
            }

            // Read BMD
            if (data.ContainsKey((int)TypeMap.BMD))
                ReadSingleFile(TypeMap.BMD);

            // Read EPL
            if (data.ContainsKey((int)TypeMap.EPLHead))
            {
                if (data.ContainsKey((int)TypeMap.EPL))
                {
                    if (data[(int)TypeMap.EPL].Length > 1)
                        throw new Exception("PM1 Read: EPL's count more than 1");

                    var eplHeaders = data[(int)TypeMap.EPLHead]
                        .Select(x =>
                        {
                            using (BinaryReader BR = IOTools.OpenReadFile(x, IsLittleEndian))
                                return BR.ReadInt32Array(4);
                        })
                        .ToArray();

                    var eplList = data[(int)TypeMap.EPL][0].Split(eplHeaders.Select(x => x[1] - eplHeaders[0][1]).ToArray()).ToArray();

                    for (int i = 0; i < eplList.Length; i++)
                    {
                        var epl = GameFormatHelper.OpenFile(fileNameList[fileNameIndex++], eplList[i], FormatEnum.DAT);
                        epl.Tag = new object[] { (int)TypeMap.EPL, eplHeaders[i] };
                        SubFiles.Add(epl);
                    }
                }
                else
                    throw new Exception("PM1 Read: file contain EPL Header, but not EPL");
            }

            // Read TMX
            if (data.ContainsKey((int)TypeMap.TMXHead))
            {
                if (data.ContainsKey((int)TypeMap.TMX))
                {
                    if (data[(int)TypeMap.TMX].Length > 1)
                        throw new Exception("PM1 Read: TMX's count more than 1");

                    var tmxHeaders = data[(int)TypeMap.TMXHead]
                        .Select(x =>
                        {
                            using (BinaryReader BR = IOTools.OpenReadFile(x, IsLittleEndian))
                                return BR.ReadInt32Array(4);
                        })
                        .ToArray();

                    var tmxList = data[(int)TypeMap.TMX][0].Split(tmxHeaders.Select(x => x[1] - tmxHeaders[0][1]).ToArray()).ToArray();

                    for (int i = 0; i < tmxList.Length; i++)
                    {
                        var name = fileNameList[fileNameIndex++];
                        var tmx = GameFormatHelper.OpenFile(name, tmxList[i], GameFormatHelper.GetFormat(name));
                        tmx.Tag = new object[] { (int)TypeMap.TMX, tmxHeaders[i] };
                        SubFiles.Add(tmx);
                    }
                }
                else
                    throw new Exception("PM1 Read: file contain TMX Header, but not TMX");
            }

            // Read CTable
            if (data.ContainsKey((int)TypeMap.CTable))
                ReadSingleFile(TypeMap.CTable);

            if (fileNameIndex != fileNameList.Length)
                throw new Exception("PM1 Read: not all files are read");
        }

        private void ReadUnnamed(Dictionary<int, byte[][]> data)
        {
            int index = 0;
            foreach (var el in data)
                if (!MainFileList.Contains(el.Key))
                    foreach (var a in el.Value)
                    {
                        //throw new Exception("PM1: Unknown");
                        string name = $"Noname({index.ToString().PadLeft(2, '0')}).DAT";
                        while (SubFiles.Exists(x => x.Name == name))
                        {
                            index++;
                            name = $"Noname({index.ToString().PadLeft(2, '0')}).DAT";
                        }

                        var temp = GameFormatHelper.OpenFile(name, a, FormatEnum.DAT);
                        temp.Tag = new object[] { el.Key };
                        SubFiles.Add(temp);
                    }
        }

        private void Write(Stream stream)
        {
            Dictionary<int, byte[][]> blocks = new Dictionary<int, byte[][]>();

            var namedFiles = SubFiles.FindAll(x => MainFileList.Contains((int)(x.Tag as object[])[0]));

            byte[][] fileNames = new byte[namedFiles.Count][];
            for (int i = 0; i < namedFiles.Count; i++)
            {
                var temp = Encoding.ASCII.GetBytes(namedFiles[i].Name);
                var name = new byte[32];
                Buffer.BlockCopy(temp, 0, name, 0, temp.Length);

                fileNames[i] = name;
            }
            blocks.Add((int)TypeMap.FileList, fileNames);

            WriteNamed(blocks);
            WriteUnnamed(blocks);
            UpdateOffsets(blocks);

            var table = CreateTable(blocks);

            using (BinaryWriter writer = IOTools.OpenWriteFile(stream, IsLittleEndian))
            {
                writer.Write(0);
                writer.Write(0);
                writer.Write(MagicNumber);
                writer.Write(0);
                writer.Write(table.Length);
                writer.Write(Unknown);

                foreach (var a in table)
                    writer.WriteInt32Array(a);

                foreach (var a in table)
                {
                    var temp = blocks[a[0]];

                    foreach (var b in temp)
                        writer.Write(b);
                }

                int fileSize = (int)stream.Position;
                stream.Position = 4;
                writer.Write(fileSize);
            }
        }

        private void WriteNamed(Dictionary<int, byte[][]> data)
        {
            void WriteSingleFile(TypeMap typeMap)
            {
                var TYPE = SubFiles.Find(x => (int)(x.Tag as object[])[0] == (int)typeMap);
                if (TYPE != null)
                {
                    byte[][] type = new byte[1][];
                    var temp = TYPE.GameData.GetData();

                    int align = IOTools.Alignment(temp.Length, 16);
                    byte[] tempType = null;
                    if (align == 0)
                        tempType = temp;
                    else
                    {
                        tempType = new byte[temp.Length + align];
                        Buffer.BlockCopy(temp, 0, tempType, 0, temp.Length);
                    }

                    type[0] = tempType;
                    data.Add((int)typeMap, type);
                }
            }

            // Write T3
            WriteSingleFile(TypeMap.T3);

            // Write RMD
            var RMD = SubFiles.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.RMD);
            if (RMD.Count != 0)
            {
                byte[][] rmdHeader = new byte[RMD.Count][];
                byte[][] rmd = new byte[1][];

                int offset = 0;

                using (MemoryStream MS = new MemoryStream())
                {
                    for (int i = 0; i < RMD.Count; i++)
                    {
                        byte[] temp = RMD[i].GameData.GetData();
                        byte[] tempRMD = new byte[temp.Length + IOTools.Alignment(temp.Length, 16)];
                        Buffer.BlockCopy(temp, 0, tempRMD, 0, temp.Length);
                        MS.Write(tempRMD, 0, tempRMD.Length);

                        var tempHeader = (RMD[i].Tag as object[])[1] as int[];
                        tempHeader[4] = offset;
                        tempHeader[5] = temp.Length;

                        using (MemoryStream headerMS = new MemoryStream())
                        using (BinaryWriter writer = IOTools.OpenWriteFile(headerMS, IsLittleEndian))
                        {
                            writer.WriteInt32Array(tempHeader);
                            rmdHeader[i] = headerMS.ToArray();
                        }

                        offset += tempRMD.Length;
                    }

                    rmd[0] = MS.ToArray();
                }

                data.Add((int)TypeMap.RMD, rmd);
                data.Add((int)TypeMap.RMDHead, rmdHeader);
            }

            // Write BMD
            WriteSingleFile(TypeMap.BMD);

            // Write EPL
            var EPL = SubFiles.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.EPL);
            if (EPL.Count != 0)
            {
                byte[][] eplHeader = new byte[EPL.Count][];
                byte[][] epl = new byte[1][];

                int offset = 0;

                using (MemoryStream MS = new MemoryStream())
                {
                    for (int i = 0; i < EPL.Count; i++)
                    {
                        byte[] temp = EPL[i].GameData.GetData();
                        byte[] tempEPL = new byte[temp.Length + IOTools.Alignment(temp.Length, 16)];
                        Buffer.BlockCopy(temp, 0, tempEPL, 0, temp.Length);
                        MS.Write(tempEPL, 0, tempEPL.Length);

                        var tempHeader = (EPL[i].Tag as object[])[1] as int[];
                        tempHeader[1] = offset;

                        using (MemoryStream headerMS = new MemoryStream())
                        using (BinaryWriter writer = IOTools.OpenWriteFile(headerMS, IsLittleEndian))
                        {
                            writer.WriteInt32Array(tempHeader);
                            eplHeader[i] = headerMS.ToArray();
                        }

                        offset += tempEPL.Length;
                    }

                    epl[0] = MS.ToArray();
                }

                data.Add((int)TypeMap.EPL, epl);
                data.Add((int)TypeMap.EPLHead, eplHeader);
            }

            // Write TMX
            var TMX = SubFiles.FindAll(x => (int)(x.Tag as object[])[0] == (int)TypeMap.TMX);
            if (TMX.Count != 0)
            {
                byte[][] tmxHeader = new byte[TMX.Count][];
                byte[][] tmx = new byte[1][];

                int offset = 0;

                using (MemoryStream MS = new MemoryStream())
                {
                    for (int i = 0; i < TMX.Count; i++)
                    {
                        byte[] temp = TMX[i].GameData.GetData();
                        byte[] tempTMX = new byte[temp.Length + IOTools.Alignment(temp.Length, 16)];
                        Buffer.BlockCopy(temp, 0, tempTMX, 0, temp.Length);
                        MS.Write(tempTMX, 0, tempTMX.Length);

                        var tempHeader = (TMX[i].Tag as object[])[1] as int[];
                        tempHeader[1] = offset;

                        using (MemoryStream headerMS = new MemoryStream())
                        using (BinaryWriter writer = IOTools.OpenWriteFile(headerMS, IsLittleEndian))
                        {
                            writer.WriteInt32Array(tempHeader);
                            tmxHeader[i] = headerMS.ToArray();
                        }

                        offset += tempTMX.Length;
                    }

                    tmx[0] = MS.ToArray();
                }

                data.Add((int)TypeMap.TMX, tmx);
                data.Add((int)TypeMap.TMXHead, tmxHeader);
            }

            // Write CTable
            WriteSingleFile(TypeMap.CTable);
        }

        private void WriteUnnamed(Dictionary<int, byte[][]> data)
        {
            var unnamedFiles = SubFiles.FindAll(x => !MainFileList.Contains((int)(x.Tag as object[])[0]));
            foreach (var a in unnamedFiles)
            {
                byte[][] type = new byte[1][];
                var temp = a.GameData.GetData();

                int align = IOTools.Alignment(temp.Length, 16);
                byte[] tempType = null;
                if (align == 0)
                    tempType = temp;
                else
                {
                    tempType = new byte[temp.Length + align];
                    Buffer.BlockCopy(temp, 0, tempType, 0, temp.Length);
                }

                type[0] = tempType;
                data.Add((int)(a.Tag as object[])[0], type);
            }
        }

        private void UpdateOffsets(Dictionary<int, byte[][]> data)
        {
            if (data.TryGetValue((int)TypeMap.RMDHead, out byte[][] rmdHead))
            {
                int offset = 0x20 + data.Count * 0x10;
                for (int i = 0; i < (int)TypeMap.RMD; i++)
                {
                    if (data.TryGetValue(i, out byte[][] current))
                    {
                        foreach (var a in current)
                            offset += a.Length;
                    }
                }

                for (int i = 0; i < rmdHead.Length; i++)
                {
                    using (MemoryStream MS = new MemoryStream(rmdHead[i]))
                    using (BinaryReader reader = IOTools.OpenReadFile(MS, IsLittleEndian))
                    using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
                    {
                        MS.Position = 0x10;
                        int headerOffset = reader.ReadInt32();
                        headerOffset += offset;
                        MS.Position = 0x10;
                        writer.Write(headerOffset);
                    }
                }
            }

            if (data.TryGetValue((int)TypeMap.EPLHead, out byte[][] eplHead))
            {
                int offset = 0x20 + data.Count * 0x10;
                for (int i = 0; i < (int)TypeMap.EPL; i++)
                {
                    if (data.TryGetValue(i, out byte[][] current))
                    {
                        foreach (var a in current)
                            offset += a.Length;
                    }
                }

                for (int i = 0; i < eplHead.Length; i++)
                {
                    using (MemoryStream MS = new MemoryStream(eplHead[i]))
                    using (BinaryReader reader = IOTools.OpenReadFile(MS, IsLittleEndian))
                    using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
                    {
                        MS.Position = 0x4;
                        int headerOffset = reader.ReadInt32();
                        headerOffset += offset;
                        MS.Position = 0x4;
                        writer.Write(headerOffset);
                    }
                }
            }

            if (data.TryGetValue((int)TypeMap.TMXHead, out byte[][] tmxHead))
            {
                int offset = 0x20 + data.Count * 0x10;
                for (int i = 0; i < (int)TypeMap.TMX; i++)
                {
                    if (data.TryGetValue(i, out byte[][] current))
                    {
                        foreach (var a in current)
                            offset += a.Length;
                    }
                }

                for (int i = 0; i < tmxHead.Length; i++)
                {
                    using (MemoryStream MS = new MemoryStream(tmxHead[i]))
                    using (BinaryReader reader = IOTools.OpenReadFile(MS, IsLittleEndian))
                    using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
                    {
                        MS.Position = 0x4;
                        int headerOffset = reader.ReadInt32();
                        headerOffset += offset;
                        MS.Position = 0x4;
                        writer.Write(headerOffset);
                    }
                }
            }

        }

        private int[][] CreateTable(Dictionary<int, byte[][]> data)
        {
            int[][] returned = new int[data.Count][];
            int index = 0;

            int maxIndex = data.Max(x => x.Key);
            int offset = 0x20 + data.Count * 0x10;

            for (int i = 0; i <= maxIndex; i++)
            {
                if (data.TryGetValue(i, out byte[][] current))
                {
                    int[] temp = new int[4];
                    temp[0] = i;
                    temp[1] = current[0].Length;
                    temp[2] = current.Length;
                    temp[3] = offset;

                    foreach (var a in current)
                        offset += a.Length;

                    returned[index++] = temp;
                }
            }

            return returned;
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.PM1;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize() => GetData().Length;

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                Write(MS);

                return MS.ToArray();
            }
        }

        #endregion IFile
    }
}