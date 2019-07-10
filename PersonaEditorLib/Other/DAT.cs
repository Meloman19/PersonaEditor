using AuxiliaryLibraries.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PersonaEditorLib.Other
{
    public class DAT : IGameData
    {
        public byte[] Data { get; set; } = new byte[0];

        private StreamPart StreamFile;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

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

        public List<GameFile> GetSubFiles()
        {
            return SubFiles;
        }

        public int GetSize() => Data.Length;

        public byte[] GetData() => Data;

        #endregion

        public object Tag { get; set; }
    }
}