using PersonaEditorLib;
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

        public static bool IsEdited(GameFile gameFile)
        {
            if (gameFile == null)
                throw new System.ArgumentNullException(nameof(gameFile));

            return contextMenuItemsEdited.Contains(gameFile.GameData.Type);
        }

        public static bool HaveSubFiles(GameFile objectFile)
        {
            if (objectFile == null)
                throw new System.ArgumentNullException(nameof(objectFile));

            if (objectFile.GameData.SubFiles.Count != 0)
                return true;

            return false;
        }
    }
}