using System;
using System.Collections.Generic;

namespace PersonaEditorLib.Other
{
    public class DAT : IGameData
    {
        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public DAT(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Data = data;
        }

        public byte[] Data { get; }

        #region IGameFile

        public FormatEnum Type => FormatEnum.DAT;

        public int GetSize() => Data.Length;

        public byte[] GetData() => Data;

        #endregion
    }
}