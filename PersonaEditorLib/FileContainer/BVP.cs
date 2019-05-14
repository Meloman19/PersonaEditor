using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PersonaEditorLib.FileContainer
{
    public class BVP : IGameFile
    {
        List<int> FlagList = new List<int>();

        public BVP(string path)
        {
            Name = Path.GetFileName(path);
            Open(File.ReadAllBytes(path));
        }

        public BVP(string name, byte[] data)
        {
            Name = name;
            Open(data);
        }

        private void Open(byte[] data)
        {
            using (BinaryReader reader = IOTools.OpenReadFile(new MemoryStream(data), IsLittleEndian))
            {
                List<int[]> Entry = new List<int[]>();

                do
                {
                    Entry.Add(reader.ReadInt32Array(3));
                } while (Entry[Entry.Count - 1][1] != 0);

                for (int i = 0; i < Entry.Count - 1; i++)
                {
                    FlagList.Add(Entry[i][0]);
                    reader.BaseStream.Position = Entry[i][1];
                    string name = Path.GetFileNameWithoutExtension(Name) + "(" + i.ToString().PadLeft(3, '0') + ").BMD";
                    SubFiles.Add(GameFormatHelper.OpenFile(name, reader.ReadBytes(Entry[i][2]), FormatEnum.BMD));
                }
            }
        }

        public int Count
        {
            get { return SubFiles.Count; }
        }

        public object this[int index]
        {
            get
            {
                if (SubFiles.Count > index)
                    return SubFiles[index].Object;

                return null;
            }
            set
            {
                if (SubFiles.Count > index)
                    SubFiles[index].Object = value;
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        public string Name { get; private set; } = "";

        #region IGameFile

        public FormatEnum Type => FormatEnum.BVP;

        public List<ObjectContainer> SubFiles { get; } = new List<ObjectContainer>();

        public int GetSize() => GetData().Length;

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
            {
                writer.BaseStream.Position = (SubFiles.Count + 1) * 12;

                List<int[]> Entry = new List<int[]>();

                for (int i = 0; i < SubFiles.Count; i++)
                {
                    var temp = SubFiles[i].Object as IGameFile;
                    Entry.Add(new int[] { FlagList[i], (int)writer.BaseStream.Position, temp.GetSize() });

                    writer.Write(temp.GetData());
                    writer.Write(new byte[IOTools.Alignment(writer.BaseStream.Position, 16)]);
                }

                writer.BaseStream.Position = 0;

                foreach (var a in Entry)
                    writer.WriteInt32Array(a);

                return MS.ToArray();
            }
        }

        #endregion
    }
}