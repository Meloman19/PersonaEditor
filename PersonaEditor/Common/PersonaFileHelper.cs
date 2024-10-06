using System;
using System.Linq;
using PersonaEditorLib;
using PersonaEditorLib.Other;
using PersonaEditorLib.SpriteContainer;
using PersonaEditorLib.Text;

namespace PersonaEditor.Common
{
    public static class PersonaFileHelper
    {
        private static readonly Type[] _editableTypes = new[]
        {
            typeof(BMD),
            typeof(SPD),
            typeof(SPR),
            typeof(FTD),
            typeof(DAT),
            typeof(FNT),
            typeof(FNT0),
        };

        public static bool IsEditable(Type type)
        {
            return _editableTypes.Contains(type);
        }

        public static bool IsEditable<T>() =>
            IsEditable(typeof(T));

        public static bool IsEditable(GameFile gameFile)
        {
            if (gameFile == null)
                throw new System.ArgumentNullException(nameof(gameFile));

            return IsEditable(gameFile.GameData.GetType());
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