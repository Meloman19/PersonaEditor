using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.Container
{
    public class TBL : IPersonaFile
    {
        List<byte[]> List = new List<byte[]>();

        public TBL(byte[] data, string name)
        {
            using (MemoryStream MS = new MemoryStream(data))
                Read(new StreamFile(MS, MS.Length, 0), name);
        }

        public TBL(StreamFile streamFile, string name)
        {
            Read(streamFile, name);
        }

        private void GetType(StreamFile streamFile)
        {
            try
            {
                streamFile.Stream.Position = streamFile.Position;
                using (BinaryReader reader = Utilities.IO.OpenReadFile(streamFile.Stream, true))
                    do
                    {
                        int Size = reader.ReadInt32();

                        if (streamFile.Position + streamFile.Size < Size + streamFile.Stream.Position)
                            throw new Exception("TBL error");

                        reader.BaseStream.Position += Size;
                        reader.BaseStream.Position += Utilities.Utilities.Alignment(reader.BaseStream.Position - streamFile.Position, 16);
                    } while (streamFile.Stream.Position < streamFile.Position + streamFile.Size);
                IsLittleEndian = true;
            }
            catch
            {
                try
                {
                    streamFile.Stream.Position = streamFile.Position;
                    using (BinaryReader reader = Utilities.IO.OpenReadFile(streamFile.Stream, false))
                        do
                        {
                            int Size = reader.ReadInt32();

                            if (streamFile.Position + streamFile.Size < Size + streamFile.Stream.Position)
                                throw new Exception("TBL error");

                            reader.BaseStream.Position += Size;
                            reader.BaseStream.Position += Utilities.Utilities.Alignment(reader.BaseStream.Position - streamFile.Position, 16);
                        } while (streamFile.Stream.Position < streamFile.Position + streamFile.Size);
                    IsLittleEndian = false;
                }
                catch
                {
                    throw new Exception("TBL error");
                }
            }
        }

        private void Read(StreamFile streamFile, string name)
        {
            GetType(streamFile);

            int index = 0;
            streamFile.Stream.Position = streamFile.Position;
            using (BinaryReader reader = Utilities.IO.OpenReadFile(streamFile.Stream, IsLittleEndian))
                do
                {
                    int Size = reader.ReadInt32();

                    if (streamFile.Position + streamFile.Size < Size + streamFile.Stream.Position)
                        throw new Exception("TBL error");

                    byte[] tempdata = reader.ReadBytes(Size);
                    FileType fileType = Utilities.PersonaFile.GetFileType(tempdata);
                    string ext = Path.GetExtension(name);
                    string tempName = name.Substring(0, name.Length - ext.Length) + "(" + index++.ToString().PadLeft(2, '0') + ")";
                    if (fileType == FileType.Unknown)
                        tempName += ".DAT";
                    else
                        tempName += "." + fileType.ToString();

                    SubFiles.Add(Utilities.PersonaFile.OpenFile(tempName, tempdata, fileType == FileType.Unknown ? FileType.DAT : fileType));
                    reader.BaseStream.Position += Utilities.Utilities.Alignment(reader.BaseStream.Position - streamFile.Position, 16);
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

        #region IPersonaFile

        public FileType Type => FileType.TBL;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Entry Count", List.Count);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size() => Get().Length;

        public byte[] Get()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian))
            {
                foreach (var element in SubFiles)
                {
                    if (element.Object is IPersonaFile pFile)
                    {
                        writer.Write(pFile.Size());
                        writer.Write(pFile.Get());
                        writer.Write(new byte[Utilities.Utilities.Alignment(writer.BaseStream.Position, 16)]);
                    }
                }
                return MS.ToArray();
            }
        }

        #endregion IFile

    }
}