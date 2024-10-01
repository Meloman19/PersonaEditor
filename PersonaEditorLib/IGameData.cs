using System.Collections.Generic;

namespace PersonaEditorLib
{
    public interface IGameData
    {
        List<GameFile> SubFiles { get; }
        int GetSize();
        byte[] GetData();
    }
}