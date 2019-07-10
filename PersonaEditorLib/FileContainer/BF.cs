using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PersonaEditorLib.FileContainer
{
    public class BF : IGameData
    {
        private static Dictionary<int, FormatEnum> MAP = new Dictionary<int, FormatEnum>()
        {
            { 0x3, FormatEnum.BMD }
        };

        int[][] Table;
        int[] Sizes;
        byte[] Unknown;
        int endIndex = -1;

        public BF(string path)
        {
            using (FileStream FS = File.OpenRead(path))
                Open(FS);

            SetName(Path.GetFileNameWithoutExtension(path));
        }

        public BF(Stream Stream, string name)
        {
            Open(Stream);

            SetName(name);
        }

        public BF(byte[] data, string name)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Open(MS);

            SetName(name);
        }

        public void SetName(string name)
        {
            foreach (var a in SubFiles)
            {
                FormatEnum fileType = a.GameData.Type;
                string ext = Path.GetExtension(name);
                if (fileType == FormatEnum.DAT)
                    a.Name = name.Substring(0, name.Length - ext.Length) + "(" + ((int)a.Tag).ToString().PadLeft(2, '0') + ").DAT";
                else
                    a.Name = name.Substring(0, name.Length - ext.Length) + "." + fileType.ToString();
            }
        }

        private void Open(Stream stream)
        {
            stream.Position = 0x10;
            if (stream.ReadByte() == 0)
                IsLittleEndian = false;
            else
                IsLittleEndian = true;

            BinaryReader reader = IOTools.OpenReadFile(stream, IsLittleEndian);

            stream.Position = 0x4;
            int fileSize = reader.ReadInt32();
            stream.Position = 0x10;
            int tablecount = reader.ReadInt32();
            Unknown = reader.ReadBytes(12);

            stream.Position = 0x20;
            Table = reader.ReadInt32ArrayArray(tablecount, 4);

            Sizes = new int[Table.Length];
            for (int i = 0; i < Table.Length; i++)
                Sizes[i] = Table[i][1];

            foreach (var element in Table)
                if (element[1] * element[2] > 0)
                {
                    reader.BaseStream.Position = element[3];

                    string tempN;

                    FormatEnum type = FormatEnum.DAT;
                    if (MAP.ContainsKey(element[0]))
                        type = MAP[element[0]];

                    tempN = "." + type.ToString();

                    if (fileSize == element[3] + element[1] * element[2])
                        endIndex = element[0];

                    byte[] data = reader.ReadBytes(element[1] * element[2]);

                    var item = GameFormatHelper.OpenFile(tempN, data, type);
                    if (item == null)
                        item = GameFormatHelper.OpenFile(tempN, data, FormatEnum.DAT);

                    item.Tag = element[0];

                    SubFiles.Add(item);
                }

            if (endIndex == -1)
                throw new Exception("BF: endIndex");
        }

        public bool IsLittleEndian { get; set; } = true;

        public void TableUpdate()
        {
            int startOffset = 0x20 + Table.Length * 0x10;

            foreach (var element in Table)
            {
                int tempsize = 0;
                var sub = SubFiles.Find(x => (int)x.Tag == element[0]);
                if (sub != null)
                {
                    tempsize = sub.GameData.GetSize();
                    if (tempsize % element[1] == 0)
                        element[2] = tempsize / element[1];
                    else
                        throw new Exception("BF: Wrong subfile");
                }
                else
                    element[2] = 0;

                element[3] = startOffset;
                startOffset += tempsize;
            }
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.BF;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize()
        {
            int returned = 0;

            returned += 0x20 + 0x10 * Table.Length;
            SubFiles.ForEach(x => returned += x.GameData.GetSize());

            return returned;
        }

        public byte[] GetData()
        {
            byte[] returned = new byte[0];

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian);

                writer.Write(0);

                int tempSize = GetSize();
                var temp = SubFiles.FindAll(x => (int)x.Tag > endIndex);
                foreach (var a in temp)
                    tempSize -= a.GameData.GetSize();

                writer.Write(tempSize);
                writer.Write(Encoding.ASCII.GetBytes("FLW0"));
                writer.Write(0);
                writer.Write(Table.Length);
                writer.Write(Unknown);

                TableUpdate();
                foreach (var line in Table)
                    writer.WriteInt32Array(line);

                SubFiles.ForEach(x => writer.Write(x.GameData.GetData()));

                returned = MS.ToArray();
            }

            return returned;
        }

        #endregion
    }
}