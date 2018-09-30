using AuxiliaryLibraries.GameFormat;
using System.Collections.Generic;

namespace PersonaEditor.Classes
{
    public static class PersonaFileHelper
    {
        private static List<FormatEnum> contextMenuItemsEdited = new List<FormatEnum>()
        {
            FormatEnum.BMD,
            FormatEnum.PTP,
            FormatEnum.SPD,
            FormatEnum.SPR,
            FormatEnum.FTD,
            FormatEnum.DAT,
            FormatEnum.FNT,
            FormatEnum.FNT0
        };

        public static bool IsEdited(FormatEnum fileType)
        {
            return contextMenuItemsEdited.Contains(fileType);
        }

        public static bool IsEdited(ObjectContainer objectFile)
        {
            if (objectFile?.Object is IGameFile personaFile)
                return contextMenuItemsEdited.Contains(personaFile.Type);

            return false;
        }

        public static bool HaveSubFiles(ObjectContainer objectFile)
        {
            if (objectFile?.Object is IGameFile personaFile && personaFile.SubFiles.Count != 0)
                return true;

            return false;
        }
    }
}