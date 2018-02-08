﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorGUI
{
    static class Static
    {
        public static class Paths
        {
            public static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirBackgrounds = Path.Combine(CurrentFolderEXE, "background");
            public static string DirFont = Path.Combine(CurrentFolderEXE, "font");
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

        public static PersonaEditorLib.PersonaEncoding.PersonaEncodingManager EncodingManager { get; } = new PersonaEditorLib.PersonaEncoding.PersonaEncodingManager(Paths.DirFont);
        public static PersonaEditorLib.PersonaEncoding.PersonaFontManager FontManager { get; } = new PersonaEditorLib.PersonaEncoding.PersonaFontManager(Paths.DirFont);
    }
}