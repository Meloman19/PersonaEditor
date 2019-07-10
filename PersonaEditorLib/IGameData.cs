using System.Collections.Generic;

namespace PersonaEditorLib
{
    public interface IGameData
    {
        FormatEnum Type { get; }
        List<GameFile> SubFiles { get; }
        int GetSize();
        byte[] GetData();
    }
}