using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure
{
    public class DAT : IPersonaFile
    {
        public byte[] Data { get; set; } = new byte[0];

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public DAT(byte[] data)
        {
            Data = data;
        }

        #region IPersonaFile

        public FileType Type => FileType.DAT;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.SaveAs);
                returned.Add(ContextMenuItems.Replace);

                return returned;
            }
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Size", Size);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size
        {
            get { return Data.Length; }
        }

        public byte[] Get()
        {
            return Data;
        }

        #endregion IFile

        public object Tag { get; set; }
    }
}