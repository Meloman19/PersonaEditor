using PersonaEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI
{
    static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds = Path.Combine(CurrentFolderEXE, Settings.AppSetting.Default.DirBackground);
            public static string DirFont = Path.Combine(CurrentFolderEXE, Settings.AppSetting.Default.DirFont);
            public static string DirLang = Path.Combine(CurrentFolderEXE, "lang");
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
        public static Classes.Visual.BackgroundManager BackManager { get; } = new Classes.Visual.BackgroundManager(Paths.DirBackgrounds);

        public static string OpenedFile = "";
    }
}