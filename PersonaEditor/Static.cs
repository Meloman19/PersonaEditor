using System;
using System.IO;
using PersonaEditor.Common.Managers;
using PersonaEditor.Common.Settings;

namespace PersonaEditor
{
    internal static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE { get; } = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds { get; } = Path.Combine(CurrentFolderEXE, "background");
            public static string DirFont { get; } = Path.Combine(CurrentFolderEXE, "font");
            public static string DirLang { get; } = Path.Combine(CurrentFolderEXE, "lang");
            public static string AppData { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PersonaEditor");
            public static string AppSettings { get; } = Path.Combine(AppData, "settings.json");
        }

        public static PersonaEncodingManager EncodingManager { get; } = new PersonaEncodingManager(Paths.DirFont);
        public static PersonaFontManager FontManager { get; } = new PersonaFontManager(Paths.DirFont);

        public static SettingsProvider SettingsProvider { get; } = new SettingsProvider(Paths.AppSettings);

        public static string OpenedFile = "";
    }
}