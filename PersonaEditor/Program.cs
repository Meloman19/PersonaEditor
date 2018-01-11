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
                PTP.CopyOld2New(Program.Settings.OldEncoding);
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
            //BMD.Open(PTP, New);
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
                PTP.CopyOld2New(Program.Settings.OldEncoding);
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
            //BMD.Open(PTP, New);
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
                PTP.CopyOld2New(Program.Settings.OldEncoding);
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
            PTP.ExportTXT(dest, ParList.Map, ParList.RemoveSplit, Program.Settings.OldEncoding, Program.Settings.NewEncoding);
        }

        private void ExportBMD(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).BMD");

            PersonaEditorLib.FileStructure.Text.BMD BMD = new PersonaEditorLib.FileStructure.Text.BMD();
            //BMD.Open(PTP, New);
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
            PTP.ImportTXT(src, ParList.Map, ParList.Auto, ParList.Width, ParList.SkipEmpty, ParList.Encode,
                Program.Settings.OldEncoding, Program.Settings.NewEncoding, Program.Settings.PersonaFontManager.GetPersonaFont(Program.Settings.NewFont));
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
            if (parameters.Old)
                StringList = new PersonaEditorLib.FileStructure.StringList(source, Program.Settings.OldEncoding);
            else
                StringList = new PersonaEditorLib.FileStructure.StringList(source, Program.Settings.NewEncoding);
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
            
            if (parameters.Old)
                File.WriteAllBytes(dest, StringList.Get(Program.Settings.OldEncoding));
            else
                File.WriteAllBytes(dest, StringList.Get(Program.Settings.NewEncoding));
        }
    }

    class Program
    {
        public static class Settings
        {
            public static string OldFont { get; set; } = "P5";
            public static string NewFont { get; set; } = "P5";

            public static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            public static string DirFont = Path.Combine(CurrentFolderEXE, "font");
            public static PersonaEditorLib.PersonaEncoding.PersonaEncodingManager PersonaEncodingManager { get; } = new PersonaEditorLib.PersonaEncoding.PersonaEncodingManager(DirFont);
            public static PersonaEditorLib.PersonaEncoding.PersonaFontManager PersonaFontManager { get; } = new PersonaEditorLib.PersonaEncoding.PersonaFontManager(DirFont);
            public static PersonaEditorLib.PersonaEncoding.PersonaEncoding OldEncoding { get; set; }
            public static PersonaEditorLib.PersonaEncoding.PersonaEncoding NewEncoding { get; set; }
        }

        static void LoadSetting()
        {
            string setting = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "PersonaEditor.xml");
            if (File.Exists(setting))
            {
                try
                {
                    XDocument xDoc = XDocument.Load(setting, LoadOptions.PreserveWhitespace);
                    XElement Setting = xDoc.Element("Settings");

                    Settings.OldFont = Setting.Element("OldFont").Value;
                    Settings.NewFont = Setting.Element("NewFont").Value;
                }
                catch
                {

                }
            }
            else
                CreateSetting(setting);

            Settings.OldEncoding = Settings.PersonaEncodingManager.GetPersonaEncoding(Settings.OldFont);
            Settings.NewEncoding = Settings.PersonaEncodingManager.GetPersonaEncoding(Settings.NewFont);
        }

        static void CreateSetting(string path)
        {
            try
            {
                XDocument xDoc = new XDocument();
                XElement Document = new XElement("Settings");
                xDoc.Add(Document);

                XElement Setting = new XElement("FontDir");
                Document.Add(Setting);

                Setting = new XElement("OldFont", Settings.OldFont);
                Document.Add(Setting);

                Setting = new XElement("NewFont", Settings.NewFont);
                Document.Add(Setting);

                xDoc.Save(path);
            }
            catch
            {

            }
        }

        static void Main(string[] args)
        {
            LoadSetting();

            try
            {
                DoSome(args);
            }
            catch (Exception e)
            {
            }
        }

        static void Test()
        {
        }

        static void DoSome(string[] args)
        {
            ArgumentsWork argwrk = new ArgumentsWork(args);
            if (argwrk.FileSource != "")
            {
                var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(argwrk.FileSource), File.ReadAllBytes(argwrk.FileSource), argwrk.FileType);

                if (file != null)
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
                            else if (command.Type == CommandSubType.PTP)
                                ExportPTP(file, argwrk.FileSource, command.Value, command.Parameters.CopyOld2New);
                            else if (command.Type == CommandSubType.TXT)
                            {
                                if (file.Object is PersonaEditorLib.FileStructure.Text.PTP ptp)
                                {
                                    ptp.ExportTXT(command.Value, command.Parameters.Map, command.Parameters.RemoveSplit, Settings.OldEncoding, Settings.NewEncoding);
                                }
                            }
                            else if (command.Type == CommandSubType.ALL)
                                ExportAllFiles(file, argwrk.FileSource);
                            else if (command.Type == CommandSubType.Empty)
                                ExportFileByType(file, argwrk.FileSource, command.Value, command.Parameters.Sub);
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

        static void ExportTextPTP()
        {

        }

        static void ExportPTP(ObjectFile objectFile, string sourcePath, string ptpPath, bool copyold)
        {
            if (objectFile.Object is PersonaEditorLib.FileStructure.Text.BMD bmd)
            {
                if (ptpPath == "") ptpPath = Util.GetNewPath(sourcePath, ".PTP");
                PersonaEditorLib.FileStructure.Text.PTP ptp = new PersonaEditorLib.FileStructure.Text.PTP();
                ptp.Open(objectFile.Name, bmd);

                if (copyold)
                    ptp.CopyOld2New(Settings.OldEncoding);

                File.WriteAllBytes(ptpPath, ptp.Get());
            }
        }

        static void ExportAllFiles(ObjectFile objectFile, string sourcePath)
        {
            if (objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.GetSubFiles();

                foreach (var a in sublist)
                {
                    string newpath = Path.Combine(Path.GetDirectoryName(sourcePath), a.Name.Replace('/', '+'));
                    if (a.Object is IPersonaFile pfile)
                        File.WriteAllBytes(newpath, pfile.Get());
                }
            }
        }

        static void ExportFileByType(ObjectFile objectFile, string sourcePath, string ext, bool sub)
        {
            if (objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.GetSubFiles();

                foreach (var a in sublist)
                {
                    string newpath = Path.Combine(Path.GetDirectoryName(sourcePath), a.Name.Replace('/', '+'));
                    if (Path.GetExtension(a.Name).ToLower() == ("." + ext.ToLower()) && a.Object is IPersonaFile pfile)
                        File.WriteAllBytes(newpath, pfile.Get());

                    if (sub)
                        ExportFileByType(a, sourcePath, ext, sub);
                }
            }
        }
    }
}