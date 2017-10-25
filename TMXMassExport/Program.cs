using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PersonaEditorLib.Extension;
using PersonaEditorLib.FileTypes;

namespace TMXMassExport
{
    class Program
    {
        static void Main(string[] args)
        {
            string SourceDir;
            string DestDir;
            if (args.Length > 1)
            {
                SourceDir = args[0];
                DestDir = args[1];
            }
            else
            {
                Console.WriteLine("Use: TMXMassExport.exe \"SourceDirPath\" \"DestDirPath\"");
                Console.ReadKey();
                return;
            }

            if (Directory.Exists(SourceDir))
            {
                string[] Files = PersonaEditorLib.Utilities.Utilities.GetAllFiles(new DirectoryInfo(SourceDir), new List<string>());

                Directory.CreateDirectory(DestDir);

                for (int i = 0; i < Files.Length; i++)
                {
                    SaveTMX(Path.Combine(SourceDir, Files[i]), Path.Combine(DestDir, Files[i]));
                    Console.Clear();
                    Console.WriteLine("Current file:" + i + "/" + Files.Length);
                }
            }
        }

        static void SaveTMX(string source, string dest)
        {
            using (FileStream FS = File.OpenRead(source))
            {
                byte[] TMXtag = new byte[] { 0x54, 0x4d, 0x58, 0x30, 0x00, 0x00, 0x00, 0x00 };

                int index = 0;
                while (FS.Position < FS.Length)
                {
                    if (FS.CheckEntrance(TMXtag))
                    {
                        if (FS.Position - TMXtag.Length - 8 >= 0)
                        {
                            PersonaEditorLib.FileStructure.TMX.TMX TMX = new PersonaEditorLib.FileStructure.TMX.TMX(FS, FS.Position - TMXtag.Length - 8, true);
                            Imaging.SavePNG(TMX.Image, Path.Combine(Path.GetDirectoryName(dest), Path.GetFileNameWithoutExtension(dest) + "(" + index++ + ").PNG"));
                        }
                    }
                }
            }
        }

    }
}
