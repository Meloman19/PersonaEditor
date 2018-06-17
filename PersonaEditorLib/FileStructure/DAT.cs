using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure
{
    public class DAT : IPersonaFile
    {
        public byte[] Data { get; set; } = new byte[0];

        private StreamPart StreamFile;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public DAT(byte[] data)
        {
            Data = data;
        }

        public DAT(StreamPart streamFile)
        {
            StreamFile = streamFile;
        }

        #region IPersonaFile

        public FileType Type => FileType.DAT;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
        }

        public ReadOnlyObservableCollection<PropertyClass> GetProperties => null;

        #endregion IPersonaFile

        #region IFile

        public int Size() => Data.Length;

        public byte[] Get() => Data;

        #endregion IFile

        public object Tag { get; set; }
    }
}