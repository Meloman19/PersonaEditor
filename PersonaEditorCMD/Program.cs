using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PersonaEditorLib;
using PersonaEditorLib.Text;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorCMD.ArgumentHandler;
using PersonaEditorLib.Other;

namespace PersonaEditorCMD
{
    class Program
    {
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
            LoadSetting();
            //Test(args);

            try
            {
                DoSome(args);
            }
            catch (Exception e)
            {
            }
        }

        static void Test(string[] args)
        {
            string testPath = @"d:\Persona 5\DATA_PS3_JAP\";
            var par = new Parameters(new string[][] { new string[] { "/sub" } });
            var files = Directory.EnumerateFiles(testPath, "*.*", SearchOption.AllDirectories).ToArray();
            int index = 0;

            foreach (var filePath in files)
            {
                Console.Write($"{index++}/{files.Length}\r");

                var OpenedFileDir = Path.GetDirectoryName(filePath);
                if (new FileInfo(filePath).Length > 10000000)
                    continue;
                ObjectContainer file = GameFormatHelper.OpenFile(Path.GetFileName(filePath), File.ReadAllBytes(filePath));
                if (file.Object != null)
                {
                    SubFileAction((a, b, c, d) =>
                    {
                        try
                        {
                            if (a.Object is BMD bmd)
                            {
                                PTP ptp = new PTP(bmd);
                                var newName = a.Name.Replace('/', '+');
                                string path = Path.Combine(c, Path.GetFileNameWithoutExtension(newName) + ".TXT");

                                var exp = ptp.ExportTXT(true, Static.OldEncoding());
                                File.WriteAllLines(path, exp);
                            }
                        }
                        catch { }
                    }, file, "", OpenedFileDir, par);
                }
            }
        }

        static void DoSome(string[] args)
        {
            ArgumentsWork argwrk = new ArgumentsWork(args);
            if (argwrk.OpenedFile != "")
            {
                ObjectContainer file = GameFormatHelper.OpenFile(Path.GetFileName(argwrk.OpenedFile), File.ReadAllBytes(argwrk.OpenedFile));

                if (file.Object != null)
                    foreach (var command in argwrk.ArgumentList)
                    {
                        Action<ObjectContainer, string, string, Parameters> action = null;
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
                            SaveFile(file, command.Value, argwrk.OpenedFileDir, command.Parameters);

                        if (action != null)
                            SubFileAction(action, file, command.Value, argwrk.OpenedFileDir, command.Parameters);
                    }
            }
        }

        static void ExportImage(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".PNG");
            PersonaEditorTools.SaveImageFile(objectFile, path);
        }

        static void ImportImage(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is IImage image)
            {
                if (parameters.Size >= 0)
                    if (objectFile.Object is FNT fnt)
                        fnt.Resize(parameters.Size);

                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".PNG") : value;
                if (File.Exists(path))
                    image.SetBitmap(AuxiliaryLibraries.WPF.Tools.ImageTools.OpenPNG(path).GetBitmap());
            }
        }

        static void ExportTable(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is ITable table)
            {
                string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".XML");
                table.GetTable().Save(path);
            }
        }

        static void ImportTable(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is ITable table)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".XML") : value;
                if (File.Exists(path))
                    table.SetTable(XDocument.Load(path));
            }
        }

        static void ExportPTP(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is BMD bmd)
            {
                string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name.Replace('/', '+')) + ".PTP");
                PTP PTP = new PTP(bmd);
                if (parameters.CopyOld2New)
                    PTP.CopyOld2New(Static.OldEncoding());
                File.WriteAllBytes(path, PTP.GetData());
            }
        }

        static void ImportPTP(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is BMD bmd)
            {
                string path = Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name.Replace('/', '+')) + ".PTP");
                if (File.Exists(path))
                {
                    PTP PTP = new PTP(File.ReadAllBytes(path));
                    var temp = new BMD(PTP, Static.NewEncoding());
                    temp.IsLittleEndian = bmd.IsLittleEndian;
                    objectFile.Object = temp;
                }
            }
        }

        static void ExportText(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PTP ptp)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                var exp = ptp.ExportTXT(parameters.RemoveSplit, Static.OldEncoding()).Select(x => $"{objectFile.Name}\t{x}");

                File.AppendAllLines(path, exp);
            }
            else if (objectFile.Object is StringList strlst)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                string[] exp = strlst.ExportText();

                File.AppendAllLines(path, exp);
            }
        }

        static void ImportText(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PTP ptp)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;

                if (File.Exists(path))
                {
                    List<string[]> import = File.ReadAllLines(path, parameters.FileEncoding).Select(x => x.Split('\t')).ToList();
                    LineMap MAP = new LineMap(parameters.Map);

                    if (parameters.LineByLine)
                    {
                        if (MAP[LineMap.Type.NewText] >= 0)
                        {
                            string[] importedText = import
                                .Select(x => x[MAP[LineMap.Type.NewText]])
                                .ToArray();
                            ptp.ImportTextLBL(importedText);
                        }
                    }
                    else
                    {
                        if (MAP[LineMap.Type.FileName] >= 0
                            & MAP[LineMap.Type.MSGindex] >= 0
                            & MAP[LineMap.Type.StringIndex] >= 0
                            & MAP[LineMap.Type.NewText] >= 0)
                        {
                            string[][] importedText = import
                                .Where(x => x.Length >= MAP.MinLength)
                                .Where(x => x[MAP[LineMap.Type.FileName]].Equals(objectFile.Name, StringComparison.CurrentCultureIgnoreCase))
                                .Where(x => x[MAP[LineMap.Type.NewText]] != "")
                                .Select(x => new string[]
                                {
                                    x[MAP[LineMap.Type.MSGindex]],
                                    x[MAP[LineMap.Type.StringIndex]],
                                    x[MAP[LineMap.Type.NewText]]
                                })
                                .ToArray();

                            if (parameters.Width > 0)
                            {
                                var charWidth = Static.NewFont().GetCharWidth(Static.NewEncoding());
                                ptp.ImportText(importedText, charWidth, parameters.Width);
                            }
                            else
                                ptp.ImportText(importedText);
                        }
                    }

                    if (MAP[LineMap.Type.OldName] >= 0 & MAP[LineMap.Type.NewName] >= 0)
                    {
                        Dictionary<string, string> importedText = import
                                .Where(x => x.Length >= MAP.MinLength)
                                .GroupBy(x => x[MAP[LineMap.Type.OldName]])
                                .ToDictionary(x => x.Key, x => x.First()[MAP[LineMap.Type.NewName]]);
                        ptp.ImportNames(importedText, Static.OldEncoding());
                    }
                }
            }
            else if (objectFile.Object is StringList strlst)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                if (File.Exists(path))
                {
                    string[][] importedtext = File.ReadAllLines(path, parameters.FileEncoding).Select(x => x.Split('\t')).
                        Where(x => x.Length > 1 && x[1] != "").ToArray();
                    strlst.ImportText(importedtext);
                }
            }
        }

        static void ExportByType(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is IGameFile pFile)
            {
                var sublist = pFile.SubFiles;

                foreach (var a in sublist)
                {
                    string path = Path.Combine(openedFileDir, a.Name.Replace('/', '+'));
                    if (pFile.Type == GetFileType(value))
                        File.WriteAllBytes(path, pFile.GetData());
                }
            }
        }

        static void SubFileAction(Action<ObjectContainer, string, string, Parameters> action, ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            action.Invoke(objectFile, value, openedFileDir, parameters);

            if (parameters.Sub && objectFile.Object is IGameFile pFile)
            {
                var sublist = pFile.SubFiles;
                foreach (var a in sublist)
                    SubFileAction(action, a, value, openedFileDir, parameters);
            }
        }

        static FormatEnum GetFileType(string type)
        {
            if (Enum.TryParse(type, out FormatEnum formatEnum))
                return formatEnum;
            else
                return FormatEnum.Unknown;
        }

        static void ExportAll(ObjectContainer objectFile, string openedFileDir)
        {
            if (objectFile.Object is IGameFile pFile)
            {
                var sublist = pFile.SubFiles;

                foreach (var a in sublist)
                {
                    string newpath = Path.Combine(openedFileDir, a.Name.Replace('/', '+'));
                    File.WriteAllBytes(newpath, (a.Object as IGameFile).GetData());
                }
            }
        }

        static void ImportAll(ObjectContainer objectFile, string openedFileDir)
        {
            if (objectFile.Object is IGameFile pFile)
            {
                var sublist = pFile.SubFiles;

                foreach (var a in sublist)
                {
                    string newpath = Path.Combine(openedFileDir, a.Name.Replace('/', '+'));
                    FormatEnum fileType = ((IGameFile)a.Object).Type;

                    if (File.Exists(newpath))
                    {
                        var file = GameFormatHelper.OpenFile(objectFile.Name, File.ReadAllBytes(newpath), fileType);
                        if (file.Object != null)
                            a.Object = file.Object;
                    }
                }
            }
        }

        static void SaveFile(ObjectContainer objectFile, string savePath, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PTP ptp)
            {
                if (parameters.AsBMD)
                {
                    string path = savePath == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".BMD") : savePath;
                    Encoding encoding = Static.NewEncoding();

                    BMD bmd = new BMD(objectFile.Object as PTP, encoding);
                    File.WriteAllBytes(path, bmd.GetData());
                }
                else
                {
                    string path = savePath == "" ? Path.Combine(openedFileDir, objectFile.Name) : savePath;
                    File.WriteAllBytes(path, ptp.GetData());
                }
            }
            else if (objectFile.Object is IGameFile pFile)
            {
                string path = savePath == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + "(NEW)" + Path.GetExtension(objectFile.Name)) : savePath;
                File.WriteAllBytes(path, pFile.GetData());
            }
        }
    }
}