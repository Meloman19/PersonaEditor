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

        private StreamFile StreamFile;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public DAT(byte[] data)
        {
            Data = data;
        }

        public DAT(StreamFile streamFile)
        {
            StreamFile = streamFile;
        }

        #region IPersonaFile

        public FileType Type => FileType.DAT;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
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