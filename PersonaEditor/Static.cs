using PersonaEditor.Classes.Managers;
using System.Collections.Generic;
using System.IO;

namespace PersonaEditor
{
    internal static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE { get; } = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds { get; } = Path.Combine(CurrentFolderEXE, ApplicationSettings.AppSetting.Default.DirBackground);
            public static string DirFont { get; } = Path.Combine(CurrentFolderEXE, ApplicationSettings.AppSetting.Default.DirFont);
            public static string DirLang { get; } = Path.Combine(CurrentFolderEXE, "lang");
        }

        public static class FontMap
        {
            public static Dictionary<int, byte> Shift = new Dictionary<int, byte>()
            {
                { 81, 2 },
                { 103, 2 },
                { 106, 2 },
                { 112, 2 },
                { 113, 2 },
                { 121, 2 }
            };
        }

        public static PersonaEncodingManager EncodingManager { get; } = new PersonaEncodingManager(Paths.DirFont);
        public static PersonaFontManager FontManager { get; } = new PersonaFontManager(Paths.DirFont);
        public static BackgroundManager BackManager { get; } = new BackgroundManager(Paths.DirBackgrounds);

        public static string OpenedFile = "";
    }
}