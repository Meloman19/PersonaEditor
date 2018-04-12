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
    class Program
    {
        public static class Static
        {
            private static string CurrentFolderEXE = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            private static string DirFont = Path.Combine(CurrentFolderEXE, "font");
            private static PersonaEditorLib.PersonaEncoding.PersonaEncodingManager PersonaEncodingManager { get; } = new PersonaEditorLib.PersonaEncoding.PersonaEncodingManager(DirFont);
            private static PersonaEditorLib.PersonaEncoding.PersonaFontManager PersonaFontManager { get; } = new PersonaEditorLib.PersonaEncoding.PersonaFontManager(DirFont);

            private static PersonaEditorLib.PersonaEncoding.PersonaEncoding oldEncoding = null;
            private static PersonaEditorLib.PersonaEncoding.PersonaEncoding newEncoding = null;
            private static PersonaEditorLib.PersonaEncoding.PersonaFont newFont = null;

            public static string OldFontName { get; set; } = "P4";
            public static string NewFontName { get; set; } = "P4";

            public static PersonaEditorLib.PersonaEncoding.PersonaEncoding OldEncoding()
            {
                if (oldEncoding == null)
                    oldEncoding = Static.PersonaEncodingManager.GetPersonaEncoding(Static.OldFontName);
                return oldEncoding;
            }

            public static PersonaEditorLib.PersonaEncoding.PersonaEncoding NewEncoding()
            {
                if (newEncoding == null)
                    newEncoding = Static.PersonaEncodingManager.GetPersonaEncoding(Static.NewFontName);
                return newEncoding;
            }

            public static PersonaEditorLib.PersonaEncoding.PersonaFont NewFont()
            {
                if (newFont == null)
                    newFont = PersonaFontManager.GetPersonaFont(NewFontName);
                return newFont;
            }
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

                    Static.OldFontName = Setting.Element("OldFont").Value;
                    Static.NewFontName = Setting.Element("NewFont").Value;
                }
                catch
                {

                }
            }
            else
                CreateSetting(setting);
        }

        static void CreateSetting(string path)
        {
            try
            {
                XDocument xDoc = new XDocument();
                XElement Document = new XElement("Settings");
                xDoc.Add(Document);

                XElement Setting = new XElement("OldFont", Static.OldFontName);
                Document.Add(Setting);

                Setting = new XElement("NewFont", Static.NewFontName);
                Document.Add(Setting);

                xDoc.Save(path);
            }
            catch
            {

            }
        }

        static void Main(string[] args)
        {
            //    //  PersonaEditorLib.FileStructure.FNT.FNT fnt2 = new PersonaEditorLib.FileStructure.FNT.FNT("EQWEQWE.FNT");

            //    PersonaEditorLib.FileStructure.FNT.FNT fnt = new PersonaEditorLib.FileStructure.FNT.FNT(args[0]);
            //    //  Imaging.SavePNG(fnt.GetImage(), "P5.png");

            //    // fnt.SetImage(Imaging.OpenPNG("P5.png"));
            //    //fnt.Resize(4000);
            //    //fnt.SetImage(Imaging.OpenPNG("FONT0.png"));            

            //    fnt.Resize(int.Parse(args[1]));
            //    fnt.SetImage(Imaging.OpenPNG(args[2]));
            //    fnt.SetTable(XDocument.Load(args[3]));
            //    File.WriteAllBytes("P5(N).FNT", fnt.Get());

            //Test(args);
            LoadSetting();

            try
            {
                DoSome(args);
            }
            catch (Exception e)
            {
            }

#if DEBUG
            //Console.ReadKey();
#endif
        }

        static void Test(string[] args)
        {
            SubDir(args[0], TestAction);
        }

        static void TestAction(string path)
        {
            using (FileStream FS = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                FS.Position = 20;
                byte temp = (byte)FS.ReadByte();
                if (temp != 3)
                {

                }
            }
        }

        static void SubDir(string path, Action<string> action)
        {
            DirectoryInfo DI = new DirectoryInfo(path);
            foreach (var file in DI.GetFiles())
                if (file.Extension.ToLower() == ".pm1")
                    action.Invoke(file.FullName);

            foreach (var dir in DI.GetDirectories())
                SubDir(dir.FullName, action);
        }

        static void DoSome(string[] args)
        {
            ArgumentsWork argwrk = new ArgumentsWork(args);
            if (argwrk.OpenedFile != "")
            {
                ObjectFile file = null;
                if (argwrk.OpenedArgument.ToLower() == "/stringlist")
                {
                    file = new ObjectFile(Path.GetFileName(argwrk.OpenedFile), new PersonaEditorLib.FileStructure.Text.StringList(argwrk.OpenedFile, Static.OldEncoding()) { DestEncoding = Static.NewEncoding() });
                }
                else
                    file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(Path.GetFileName(argwrk.OpenedFile), File.ReadAllBytes(argwrk.OpenedFile),
                       PersonaEditorLib.Utilities.PersonaFile.GetFileType(argwrk.OpenedFile));

                if (file.Object != null)
                    foreach (var command in argwrk.ArgumentList)
                    {
                        Action<ObjectFile, string, string, Parameters> action = null;
                        if (command.Command == CommandType.Export)
                        {
                            if (command.Type == CommandSubType.Image)
                                action = ExportImage;
                            else if (command.Type == CommandSubType.Table)
                                action = ExportTable;
                            else if (command.Type == CommandSubType.All)
                                ExportAll(file, argwrk.OpenedFileDir);
                            else if (command.Type == CommandSubType.PTP)
                                action = ExportPTP;
                            else if (command.Type == CommandSubType.Text)
                                action = ExportText;
                            else
                                action = ExportByType;
                        }
                        else if (command.Command == CommandType.Import)
                        {
                            if (command.Type == CommandSubType.Image)
                                action = ImportImage;
                            else if (command.Type == CommandSubType.Table)
                                action = ImportTable;
                            else if (command.Type == CommandSubType.All)
                                ImportAll(file, argwrk.OpenedFileDir);
                            else if (command.Type == CommandSubType.PTP)
                                action = ImportPTP;
                            else if (command.Type == CommandSubType.Text)
                                action = ImportText;
                        }
                        else if (command.Command == CommandType.Save)
                            SaveFile(file, argwrk.OpenedFileDir, command.Value);

                        if (action != null)
                            SubFileAction(action, file, command.Value, argwrk.OpenedFileDir, command.Parameters);
                    }
            }
        }

        static void ExportImage(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".PNG");
            PersonaEditorLib.Utilities.PersonaFile.SaveImageFile(objectFile, path);
        }

        static void ImportImage(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is IImage image)
            {
                if (parameters.Size >= 0)
                    if (objectFile.Object is PersonaEditorLib.FileStructure.FNT.FNT fnt)
                        fnt.Resize(parameters.Size);

                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".PNG") : value;
                if (File.Exists(path))
                    image.SetImage(Imaging.OpenPNG(path));
            }
        }

        static void ExportTable(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is ITable table)
            {
                string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".XML");
                table.GetTable().Save(path);
            }
        }

        static void ImportTable(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is ITable table)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".XML") : value;
                if (File.Exists(path))
                    table.SetTable(XDocument.Load(path));
            }
        }

        static void ExportPTP(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PersonaEditorLib.FileStructure.Text.BMD bmd)
            {
                string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name.Replace('/', '+')) + ".PTP");
                PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP(bmd);
                if (parameters.CopyOld2New)
                    PTP.CopyOld2New(Static.OldEncoding());
                File.WriteAllBytes(path, PTP.Get());
            }
        }

        static void ImportPTP(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PersonaEditorLib.FileStructure.Text.BMD bmd)
            {
                string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name.Replace('/', '+')) + ".PTP");
                if (File.Exists(path))
                {
                    PersonaEditorLib.FileStructure.Text.PTP PTP = new PersonaEditorLib.FileStructure.Text.PTP(File.ReadAllBytes(path));
                    bmd.Open(PTP, Static.NewEncoding());
                }
            }
        }

        static void ExportText(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PersonaEditorLib.FileStructure.Text.PTP ptp)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                string[] exp = ptp.ExportTXT(parameters.Map, objectFile.Name, parameters.RemoveSplit, Static.OldEncoding(), Static.NewEncoding());

                File.AppendAllLines(path, exp);
            }
        }

        static void ImportText(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PersonaEditorLib.FileStructure.Text.PTP ptp)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                string[] importedtext;
                if (File.Exists(path))
                {
                    importedtext = File.ReadAllLines(path, parameters.Encode);
                    if (parameters.LineByLine)
                        ptp.ImportTXT_LBL(importedtext, parameters.Map, parameters.Width, parameters.SkipEmpty, Static.OldEncoding(), Static.NewEncoding(), Static.NewFont());
                    else
                        ptp.ImportTXT(importedtext, objectFile.Name, parameters.Map, parameters.Width, parameters.SkipEmpty, Static.OldEncoding(), Static.NewEncoding(), Static.NewFont());
                }
            }
            else if (objectFile.Object is PersonaEditorLib.FileStructure.Text.StringList strlst)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                string[] importedtext;
                if (File.Exists(path))
                {
                    importedtext = File.ReadAllLines(path, parameters.Encode);
                    strlst.ImportText(importedtext, parameters.Map, parameters.SkipEmpty);
                }
            }
        }

        static void ExportByType(ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.GetSubFiles();

                foreach (var a in sublist)
                {
                    string path = Path.Combine(openedFileDir, a.Name.Replace('/', '+'));
                    if (a.Object is IPersonaFile pfile && pFile.Type == GetFileType(value))
                        File.WriteAllBytes(path, pfile.Get());
                }
            }
        }

        static void SubFileAction(Action<ObjectFile, string, string, Parameters> action, ObjectFile objectFile, string value, string openedFileDir, Parameters parameters)
        {
            action.Invoke(objectFile, value, openedFileDir, parameters);

            if (parameters.Sub && objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.GetSubFiles();
                foreach (var a in sublist)
                    SubFileAction(action, a, value, openedFileDir, parameters);
            }
        }

        static FileType GetFileType(string type)
        {
            if (type == "BIN")
                return FileType.BIN;
            else if (type == "SPR")
                return FileType.SPR;
            else if (type == "TMX")
                return FileType.TMX;
            else if (type == "BF")
                return FileType.BF;
            else if (type == "PM1")
                return FileType.PM1;
            else if (type == "BMD")
                return FileType.BMD;
            else if (type == "FNT")
                return FileType.FNT;
            else if (type == "BVP")
                return FileType.BVP;
            else if (type == "HEX")
                return FileType.DAT;
            else
                return FileType.Unknown;
        }

        static void ExportAll(ObjectFile objectFile, string openedFileDir)
        {
            if (objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.GetSubFiles();

                foreach (var a in sublist)
                {
                    string newpath = Path.Combine(openedFileDir, a.Name.Replace('/', '+'));
                    if (a.Object is IPersonaFile pfile)
                        File.WriteAllBytes(newpath, pfile.Get());
                }
            }
        }

        static void ImportAll(ObjectFile objectFile, string openedFileDir)
        {
            if (objectFile.Object is IPersonaFile pFile)
            {
                var sublist = pFile.GetSubFiles();

                foreach (var a in sublist)
                {
                    string newpath = Path.Combine(openedFileDir, a.Name.Replace('/', '+'));
                    FileType fileType = ((IPersonaFile)a.Object).Type;

                    if (File.Exists(newpath))
                    {
                        var file = PersonaEditorLib.Utilities.PersonaFile.OpenFile(objectFile.Name, File.ReadAllBytes(newpath), fileType);
                        if (file.Object != null)
                            a.Object = file.Object;
                    }
                }
            }
        }

        static void SaveFile(ObjectFile objectFile, string openedFileDir, string savePath)
        {
            if (objectFile.Object is PersonaEditorLib.FileStructure.Text.PTP ptp)
            {
                string path = savePath == "" ? Path.Combine(openedFileDir, objectFile.Name) : savePath;
                File.WriteAllBytes(path, ptp.Get());
            }
            else if (objectFile.Object is IFile pFile)
            {
                string path = savePath == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + "(NEW)" + Path.GetExtension(objectFile.Name)) : savePath;
                File.WriteAllBytes(path, pFile.Get());
            }
        }
    }
}