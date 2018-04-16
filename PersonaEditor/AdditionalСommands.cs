using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditor
{
    static class AdditionalСommands
    {
        public static Dictionary<string, Action<string[]>> commands { get; } = new Dictionary<string, Action<string[]>>()
        {
            { "/find_all_dds", FindAllDDS }
        };

        public static void FindAllDDS(string[] args)
        {
            int index = 0;
            using (FileStream FS = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(FS, Encoding.ASCII, true))
                {
                    long len = FS.Length - 3;
                    for (int i = 0; i < len; i++)
                    {
                        FS.Position = i;
                        if (reader.ReadInt32() == PersonaEditorLib.FileStructure.Graphic.DDS.MagicNumber)
                        {
                            FS.Position = i;
                            PersonaEditorLib.FileStructure.Graphic.DDS dds = null;
                            try
                            {
                                dds = new PersonaEditorLib.FileStructure.Graphic.DDS(new PersonaEditorLib.StreamFile(FS, FS.Length, i));
                            }
                            catch(Exception e)
                            {
                                continue;
                            }

                            string dir = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]));
                            Directory.CreateDirectory(dir);
                            File.WriteAllBytes(Path.Combine(dir, Path.GetFileNameWithoutExtension(args[0]) + "(" + index++ + ").dds"), dds.Get());
                        }
                    }
                }
            }
        }
    }
}
