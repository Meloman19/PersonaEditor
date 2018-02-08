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

        List<ObjectFile> list = new List<ObjectFile>();

        public TBL(byte[] data, string name)
        {
            int index = 0;
            using (BinaryReader reader = Utilities.IO.OpenReadFile(new MemoryStream(data), IsLittleEndian))
                do
                {
                    int Size = reader.ReadInt32();
                    byte[] tempdata = reader.ReadBytes(Size);
                    FileType fileType = Utilities.PersonaFile.GetFileType(tempdata);
                    string ext = Path.GetExtension(name);
                    string tempName = name.Substring(0, name.Length - ext.Length) + "(" + index++.ToString().PadLeft(2, '0') + ")";
                    if (fileType == FileType.Unknown)
                        tempName += ".DAT";
                    else
                        tempName += "." + fileType.ToString();

                    list.Add(Utilities.PersonaFile.OpenFile(tempName, tempdata, fileType == FileType.Unknown ? FileType.DAT : fileType));
                    long temp = Utilities.Utilities.Alignment(reader.BaseStream.Position, 16);
                    reader.BaseStream.Position += temp;
                } while (reader.BaseStream.Position < reader.BaseStream.Length);
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

        public List<ObjectFile> GetSubFiles()
        {
            return list;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.Replace);
                returned.Add(ContextMenuItems.Separator);
                returned.Add(ContextMenuItems.SaveAs);

                return returned;
            }
        }

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

        public int Size
        {
            get
            {
                return Get().Length;
            }
        }

        public byte[] Get()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(MS, IsLittleEndian))
            {
                foreach (var element in list)
                {
                    if (element.Object is IPersonaFile pFile)
                    {
                        writer.Write(pFile.Size);
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