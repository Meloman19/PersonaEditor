using AuxiliaryLibraries.IO;
using AuxiliaryLibraries.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.FileContainer
{
    public class TBL : IGameData
    {
        List<byte[]> List = new List<byte[]>();

        public TBL(byte[] data, string name)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamPart(MS, MS.Length, 0), name);
        }

        public TBL(StreamPart streamFile, string name)
        {
            Read(streamFile, name);
        }

        private void GetType(StreamPart streamFile)
        {
            try
            {
                streamFile.Stream.Position = streamFile.Position;
                using (BinaryReader reader = IOTools.OpenReadFile(streamFile.Stream, true))
                    do
                    {
                        int Size = reader.ReadInt32();

                        if (streamFile.Position + streamFile.Size < Size + streamFile.Stream.Position)
                            throw new Exception("TBL error");

                        reader.BaseStream.Position += Size;
                        reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position - streamFile.Position, 16);
                    } while (streamFile.Stream.Position < streamFile.Position + streamFile.Size);
                IsLittleEndian = true;
            }
            catch
            {
                try
                {
                    streamFile.Stream.Position = streamFile.Position;
                    using (BinaryReader reader = IOTools.OpenReadFile(streamFile.Stream, false))
                        do
                        {
                            int Size = reader.ReadInt32();

                            if (streamFile.Position + streamFile.Size < Size + streamFile.Stream.Position)
                                throw new Exception("TBL error");

                            reader.BaseStream.Position += Size;
                            reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position - streamFile.Position, 16);
                        } while (streamFile.Stream.Position < streamFile.Position + streamFile.Size);
                    IsLittleEndian = false;
                }
                catch
                {
                    throw new Exception("TBL error");
                }
            }
        }

        private void Read(StreamPart streamFile, string name)
        {
            GetType(streamFile);

            int index = 0;
            streamFile.Stream.Position = streamFile.Position;
            using (BinaryReader reader = IOTools.OpenReadFile(streamFile.Stream, IsLittleEndian))
                do
                {
                    int Size = reader.ReadInt32();

                    if (streamFile.Position + streamFile.Size < Size + streamFile.Stream.Position)
                        throw new Exception("TBL error");

                    byte[] tempdata = reader.ReadBytes(Size);
                    FormatEnum fileType = GameFormatHelper.GetFormat(tempdata);
                    string ext = Path.GetExtension(name);
                    string tempName = name.Substring(0, name.Length - ext.Length) + "(" + index++.ToString().PadLeft(2, '0') + ")";
                    if (fileType == FormatEnum.Unknown)
                        tempName += ".DAT";
                    else
                        tempName += "." + fileType.ToString();

                    SubFiles.Add(GameFormatHelper.OpenFile(tempName, tempdata, fileType == FormatEnum.Unknown ? FormatEnum.DAT : fileType));
                    reader.BaseStream.Position += IOTools.Alignment(reader.BaseStream.Position - streamFile.Position, 16);
                } while (streamFile.Stream.Position < streamFile.Position + streamFile.Size);
        }

        public int Count
        {
            get { return List.Count; }
        }

        public byte[] this[int index]
        {
            get
            {
                if (List.Count > index)
                {
                    return List[index].ToArray();
                }
                return null;
            }
            set
            {
                if (List.Count > index)
                {
                    List[index] = value;
                }
            }
        }

        public bool IsLittleEndian { get; set; } = true;

        #region IGameFile

        public FormatEnum Type => FormatEnum.TBL;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize() => GetData().Length;

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = IOTools.OpenWriteFile(MS, IsLittleEndian))
            {
                foreach (var element in SubFiles)
                {
                    writer.Write(element.GameData.GetSize());
                    writer.Write(element.GameData.GetData());
                    writer.Write(new byte[IOTools.Alignment(writer.BaseStream.Position, 16)]);
                }
                return MS.ToArray();
            }
        }

        #endregion

    }
}