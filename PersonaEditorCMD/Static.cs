using PersonaEditorLib;
using System.IO;

namespace PersonaEditorCMD
{
    internal static class Static
    {
        private static readonly string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private static string DirFont = Path.Combine(CurrentFolderEXE, "font");

        private static PersonaEncoding oldEncoding = null;
        private static PersonaEncoding newEncoding = null;
        private static PersonaFont newFont = null;

        public static string OldFontName { get; set; } = "P4";
        public static string NewFontName { get; set; } = "P4";

        public static PersonaEncoding OldEncoding()
        {
            if (oldEncoding == null)
                oldEncoding = new PersonaEncoding(Path.Combine(DirFont, OldFontName + ".fntmap"));
            return oldEncoding;
        }

        public static PersonaEncoding NewEncoding()
        {
            if (newEncoding == null)
                newEncoding = new PersonaEncoding(Path.Combine(DirFont, NewFontName + ".fntmap"));
            return newEncoding;
        }

        public static PersonaFont NewFont()
        {
            if (newFont == null)
                newFont = new PersonaFont(Path.Combine(DirFont, NewFontName + ".fnt"));
            return newFont;
        }
    }
}