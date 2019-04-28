using System.Collections.Generic;

namespace PersonaEditorLib
{
    public interface IGameFile
    {
        FormatEnum Type { get; }
        List<ObjectContainer> SubFiles { get; }
        int GetSize();
        byte[] GetData();
    }
}