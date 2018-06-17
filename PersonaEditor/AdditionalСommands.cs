using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
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
            { "/find_all_dds", FindAllDDS },
            { "/p5_tbl_import", Persona5TBL_Import },
            { "/p5_ftd_import", Persona5FTD_ImportText },
            { "/find_bmd", Find_BMD }
        };

        public static void Persona5TBL_Import(string[] args)
        {
            PersonaEditorLib.FileStructure.Text.StringList stringList = new PersonaEditorLib.FileStructure.Text.StringList(args[0], Program.Static.OldEncoding());
            var names = stringList.List.Select(x => x.OldString).ToArray();
            var newnames = File.ReadAllLines(args[1]).Select(x => x.Split('\t')).ToList();

            for (int i = 0; i < names.Length; i++)
            {
                var ind = newnames.FindIndex(x => x[0] == names[i]);
                if (ind >= 0 && newnames[ind][1] != "")
                    names[i] = newnames[ind][1];
            }

            Encoding encoding = Program.Static.NewEncoding();

            using (MemoryStream newText = new MemoryStream())
            using (BinaryWriter newTextWriter = new PersonaEditorLib.BinaryWriterBE(newText, Encoding.ASCII, true))
            using (MemoryStream newPos = new MemoryStream())
            using (BinaryWriter newPosWriter = new PersonaEditorLib.BinaryWriterBE(newPos, Encoding.ASCII, true))
            {
                ushort pos = 0;

                foreach (var a in names)
                {
                    newPosWriter.Write(pos);
                    byte[] temp = encoding.GetBytes(a);
                    newTextWriter.Write(temp);
                    newTextWriter.Write((byte)0);
                    pos += Convert.ToUInt16(temp.Length + 1);
                }

                string full = Path.GetFullPath(args[0]);

                string nameWE = Path.GetFileNameWithoutExtension(full);
                string nameE = Path.GetExtension(full);

                string newTextName = Path.Combine(Path.GetDirectoryName(full), nameWE + "_TEXT" + nameE);
                string newPosName = Path.Combine(Path.GetDirectoryName(full), nameWE + "_POS" + nameE);

                File.WriteAllBytes(newTextName, newText.ToArray());
                File.WriteAllBytes(newPosName, newPos.ToArray());
            }
        }

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
                        if (reader.ReadBytes(4).SequenceEqual(PersonaEditorLib.FileStructure.Graphic.DDS.MagicNumber))
                        {
                            FS.Position = i;
                            PersonaEditorLib.FileStructure.Graphic.DDS dds = null;
                            try
                            {
                                dds = new PersonaEditorLib.FileStructure.Graphic.DDS(new PersonaEditorLib.StreamPart(FS, FS.Length, i));
                            }
                            catch (Exception e)
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

        public static void Persona5FTD_ImportText(string[] args)
        {
            PersonaEditorLib.FileStructure.Text.FTD ftd = new PersonaEditorLib.FileStructure.Text.FTD(File.ReadAllBytes(args[0]));

            var text = File.ReadAllLines(args[1]).Select(x => x.Split('\t')).ToList();

            ftd.ImportText(text, Program.Static.NewEncoding());

            string full = Path.GetFullPath(args[0]);

            string nameWE = Path.GetFileNameWithoutExtension(full);
            string nameE = Path.GetExtension(full);

            string newname = Path.Combine(Path.GetDirectoryName(full), nameWE + "(NEW)" + nameE);

            File.WriteAllBytes(newname, ftd.Get());
        }

        public static void Find_BMD(string[] args)
        {
            List<string> result = new List<string>();

            SubDirAction(args[0], (x) =>
            {
                try
                {
                    var ptp = new PersonaEditorLib.FileStructure.Text.PTP(File.ReadAllBytes(x));
                    if (ptp.names.Count > 30)
                    {
                        result.Add(ptp.names.Count + "||||" + x);
                    }
                }
                catch(Exception ex)
                {

                }
            });

            File.WriteAllLines(args[1], result);
        }

        public static void SubFileAction(ObjectFile objectFile, Action<ObjectFile> action)
        {
            action.Invoke(objectFile);

            if (objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.SubFiles;
                foreach (var a in sublist)
                    SubFileAction(a, action);
            }
        }

        public static void SubDirAction(string dir, Action<string> action)
        {
            foreach (var a in Directory.EnumerateDirectories(dir))
                SubDirAction(a, action);

            foreach (var a in Directory.EnumerateFiles(dir))
                action(a);
        }
    }
}
