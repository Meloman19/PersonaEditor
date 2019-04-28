using AuxiliaryLibraries.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PersonaEditorLib.Other
{
    public class DAT : IGameFile
    {
        public byte[] Data { get; set; } = new byte[0];

        private StreamPart StreamFile;

        public List<ObjectContainer> SubFiles { get; } = new List<ObjectContainer>();

        public DAT(byte[] data)
        {
            Data = data;
        }

        public DAT(StreamPart streamFile)
        {
            StreamFile = streamFile;
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.DAT;

        public List<ObjectContainer> GetSubFiles()
        {
            return SubFiles;
        }

        public int GetSize() => Data.Length;

        public byte[] GetData() => Data;

        #endregion

        public object Tag { get; set; }
    }
}