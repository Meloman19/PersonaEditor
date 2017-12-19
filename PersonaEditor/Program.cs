using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using PersonaEditorLib.Extension;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using PersonaEditorLib;
using System.Text.RegularExpressions;
using PersonaEditorLib.Interfaces;

namespace PersonaEditor
{
    public class PM1Console : IConsoleWork
    {
        PersonaEditorLib.FileStructure.PM1.PM1 PM1;
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public PM1Console(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            PM1 = new PersonaEditorLib.FileStructure.PM1.PM1(source);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.BMD)
                ExportBMD(dest);
            else if (com == CommandSubType.PTP)
                ExportPTP(dest, parameters);
        }

        #region ExportMethods

        private void ExportBMD(string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".BMD");

            File.WriteAllBytes(dest, PersonaEditorLib.FileStructure.PM1.PM1.GetBMD(PM1));
        }

        private void ExportPTP(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD(PersonaEditorLib.FileStructure.PM1.PM1.GetBMD(PM1));
            PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP();
            PTP.Open(Path.GetFileNameWithoutExtension(dest) + ".BMD", BMD);
            if (ParList.CopyOld2New)
                PTP.CopyOld2New(Old);
            PTP.SaveProject(dest);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.BMD)
                ImportBMD(src);
            else if (com == CommandSubType.PTP)
                ImportPTP(src, parameters);
        }

        #region ImportMethods

        private void ImportBMD(string src)
        {
            if (src == "") src = Util.GetNewPath(source, "(NEW).BMD");

            PersonaEditorLib.FileStructure.PM1.PM1.SetBMD(PM1, File.ReadAllBytes(src));
        }

        private void ImportPTP(string src, ArgumentsWork.Parameters ParList)
        {
            if (src == "") src = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP();
            PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD();
            BMD.Open(PTP, New);
            PersonaEditorLib.FileStructure.PM1.PM1.SetBMD(PM1, BMD.Get());
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).PM1");

            File.WriteAllBytes(dest, PM1.Get());
        }
    }

    public class BFConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.Container.BF BF;
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public BFConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            BF = new PersonaEditorLib.FileStructure.Container.BF(source);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.BMD)
                ExportBMD(dest);
            else if (com == CommandSubType.PTP)
                ExportPTP(dest, parameters);
        }

        #region ExportMethods

        private void ExportBMD(string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".BMD");

            File.WriteAllBytes(dest, PersonaEditorLib.FileStructure.Container.BF.GetBMD(BF));
        }

        private void ExportPTP(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD(PersonaEditorLib.FileStructure.Container.BF.GetBMD(BF));
            PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP();
            PTP.Open(Path.GetFileNameWithoutExtension(dest), BMD);
            if (ParList.CopyOld2New)
                PTP.CopyOld2New(Old);
            PTP.SaveProject(dest);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.BMD)
                ImportBMD(src);
            else if (com == CommandSubType.PTP)
                ImportPTP(src, parameters);
        }

        #region ImportMethods

        private void ImportBMD(string src)
        {
            if (src == "") src = Util.GetNewPath(source, "(NEW).BMD");

            PersonaEditorLib.FileStructure.Container.BF.SetBMD(BF, File.ReadAllBytes(src));
        }

        private void ImportPTP(string src, ArgumentsWork.Parameters ParList)
        {
            if (src == "") src = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP();
            PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD();
            BMD.Open(PTP, New);
            BMD.IsLittleEndian = ParList.IsLittleEndian;
            PersonaEditorLib.FileStructure.Container.BF.SetBMD(BF, BMD.Get());
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).BF");

            File.WriteAllBytes(dest, BF.Get());
        }
    }

    public class BMDConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD();
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public BMDConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            using (FileStream FS = File.OpenRead(source))
                BMD.Open(FS);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.PTP)
                ExportPTP(dest, parameters);
        }

        #region ExportMethods

        private void ExportPTP(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP();
            PTP.Open(Path.GetFileName(source), BMD);
            if (ParList.CopyOld2New)
                PTP.CopyOld2New(Old);
            PTP.SaveProject(dest);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string source)
        {
        }

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).BMD");

            File.WriteAllBytes(dest, BMD.Get());
        }
    }

    public class PTPConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.Text.PTP PTP;
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public PTPConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            PTP = new PersonaEditorLib.FileStructure.Text.PTP();
            PTP.Open(source);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.TXT)
                ExportTXT(dest, parameters);
            else if (com == CommandSubType.BMD)
                ExportBMD(dest, parameters);
        }

        #region ExportMethods

        private void ExportTXT(string dest, ArgumentsWork.Parameters ParList)
        {
            PTP.ExportTXT(dest, ParList.Map, ParList.RemoveSplit, Old, New);
        }

        private void ExportBMD(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).BMD");

            PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD();
            BMD.Open(PTP, New);
            BMD.IsLittleEndian = ParList.IsLittleEndian;
            File.WriteAllBytes(dest, BMD.Get());
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.TXT)
                ImportTXT(src, parameters);
        }

        #region ImportMethods

        private void ImportTXT(string src, ArgumentsWork.Parameters ParList)
        {
            PTP.ImportTXT(src, ParList.Map, ParList.Auto, ParList.Width, ParList.SkipEmpty, ParList.Encode, Old, New);
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PTP");

            PTP.SaveProject(dest);
        }
    }

    public class TEXTConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.StringList StringList;
        string source = "";

        public TEXTConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            StringList = new PersonaEditorLib.FileStructure.StringList(source,
                     parameters.Old ? new CharList(parameters.OldMap, parameters.OldFont) : new CharList(parameters.NewMap, parameters.NewFont));
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.TXT)
                ExportTXT(dest, parameters);
        }

        #region ExportMethods

        private void ExportTXT(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".TXT");

            for (int i = 0; i < StringList.Count; i++)
                File.AppendAllText(dest, StringList[i].Item1 + "\t" + StringList[i].Item2 + "\r\n");
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.TXT)
                ImportTXT(src, parameters);
        }

        #region ImportMethods

        private void ImportTXT(string src, ArgumentsWork.Parameters ParList)
        {
            StringList.Import(src);
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW)" + Path.GetExtension(source));

            File.WriteAllBytes(dest, StringList.Get(parameters.Old ? new CharList(parameters.OldMap, parameters.OldFont) : new CharList(parameters.NewMap, parameters.NewFont)));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoSome(args);
            }
            catch (Exception e)
            {
            }

            //            bool complete = Do(args);

            //            if (complete)
            //                Console.WriteLine(args[1] + " - Success");
            //            else
            //                Console.WriteLine("Failure");


        }

        static void Test()
        {
            string OldMap = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_OLD.TXT");
            string OldFont = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_OLD.FNT");
            CharList charList = new CharList(OldMap, OldFont);
            PersonaEditorLib.FileStructure.StringList StringList = new PersonaEditorLib.FileStructure.StringList("MSG.TBL(03).DAT", charList);
            File.WriteAllBytes("111.DAT", StringList.Get(charList, 1));
        }

        static void DoSome(string[] args)
        {
            ArgumentsWork argwrk = new ArgumentsWork(args);
            var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(argwrk.FileSource), File.ReadAllBytes(argwrk.FileSource), argwrk.FileType);

            foreach (var command in argwrk.ArgumentList)
                if (command.Command == CommandType.Export)
                {
                    if (command.Type == CommandSubType.Image)
                    {
                        if (file.Object is IImage image)
                        {
                            if (command.Value == "") command.Value = Util.GetNewPath(argwrk.FileSource, ".PNG");
                            Imaging.SavePNG(image.GetImage(), command.Value);
                        }
                    }
                    else if (command.Type == CommandSubType.Table)
                    {
                        if (file.Object is PersonaEditorLib.FileStructure.FNT.FNT fnt)
                        {
                            if (command.Value == "") command.Value = Util.GetNewPath(argwrk.FileSource, ".XML");
                            fnt.GetWidthTable().Save(command.Value);
                        }
                    }
                    else if (command.Type == CommandSubType.ALL)
                    {
                        if (file.Object is IPersonaFile pFile)
                        {
                            var sublist = pFile.GetSubFiles();

                            foreach (var a in sublist)
                            {
                                string newpath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(argwrk.FileSource)), Path.GetFileName(argwrk.FileSource) + "+" + a.Name.Replace('/', '+'));
                                if (a.Object is IPersonaFile pfile)
                                    File.WriteAllBytes(newpath, pfile.Get());
                            }
                        }
                    }
                }
                else if (command.Command == CommandType.Import)
                {
                    if (command.Type == CommandSubType.Image)
                    {
                        if (file.Object is IImage image)
                        {
                            if (command.Value == "") command.Value = Util.GetNewPath(argwrk.FileSource, ".PNG");
                            image.SetImage(Imaging.OpenPNG(command.Value));
                        }
                    }
                    else if (command.Type == CommandSubType.Table)
                    {
                        if (file.Object is PersonaEditorLib.FileStructure.FNT.FNT fnt)
                        {
                            if (command.Value == "") command.Value = Util.GetNewPath(argwrk.FileSource, ".XML");
                            fnt.SetWidthTable(XDocument.Load(command.Value));
                        }
                    }
                    else if (command.Type == CommandSubType.ALL)
                    {
                        if (file.Object is IPersonaFile pFile)
                        {
                            var sublist = pFile.GetSubFiles();

                            foreach (var a in sublist)
                            {
                                string name = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(argwrk.FileSource)), Path.GetFileName(argwrk.FileSource) + "+" + a.Name.Replace('/', '+'));
                                string newpath = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name) + "(NEW)" + Path.GetExtension(name));

                                if (File.Exists(newpath))
                                    if (a.Object is IPersonaFile pfile)
                                    {
                                        var newfile = PersonaEditorLib.Utilities.PersonaFile.OpenFile(a.Name, File.ReadAllBytes(newpath), pfile.Type);
                                        if (newfile != null)
                                            a.Object = newfile.Object;
                                    }
                            }
                        }
                    }
                }
                else if (command.Command == CommandType.Save)
                {
                    if (file.Object is IFile pFile)
                    {
                        if (command.Value == "") command.Value = Util.GetNewPath(Path.GetFileNameWithoutExtension(argwrk.FileSource), "(NEW)" + Path.GetExtension(argwrk.FileSource));

                        File.WriteAllBytes(command.Value, pFile.Get());
                    }
                }
        }
    }
}