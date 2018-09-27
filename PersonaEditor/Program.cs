using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.Text;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditor.ArgumentHandler;

namespace PersonaEditor
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
            Test(args);

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
        }

        static void DoSome(string[] args)
        {
            ArgumentsWork argwrk = new ArgumentsWork(args);
            if (argwrk.OpenedFile != "")
            {
                ObjectContainer file = null;
                if (argwrk.OpenedArgument.ToLower() == "/stringlist")
                {
                    file = new ObjectContainer(Path.GetFileName(argwrk.OpenedFile), new StringList(argwrk.OpenedFile, Static.OldEncoding()) { DestEncoding = Static.NewEncoding() });
                }
                else
                    file = GameFormatHelper.OpenFile(Path.GetFileName(argwrk.OpenedFile), File.ReadAllBytes(argwrk.OpenedFile),
                      GameFormatHelper.GetFormat(argwrk.OpenedFile));

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
                    if (objectFile.Object is AuxiliaryLibraries.GameFormat.Other.FNT fnt)
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
                    bmd.Open(PTP, Static.NewEncoding());
                }
            }
        }

        static void ExportText(ObjectContainer objectFile, string value, string openedFileDir, Parameters parameters)
        {
            if (objectFile.Object is PTP ptp)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                string[] exp = ptp.ExportTXT(objectFile.Name, parameters.RemoveSplit, Static.OldEncoding());

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
                    List<string[]> import = File.ReadAllLines(path, parameters.Encode).Select(x => x.Split('\t')).ToList();
                    LineMap MAP = new LineMap(parameters.Map);
                    if (MAP.CanGetText)
                    {
                        string[][] importedText = import.
                            FindAll(x => x[MAP[LineMap.Type.FileName]].Equals(Path.GetFileName(path), StringComparison.CurrentCultureIgnoreCase) && x.Length > MAP.MinLength && !x[MAP[LineMap.Type.NewText]].Equals("")).
                            Select(x => new string[] { x[MAP[LineMap.Type.MSGindex]], x[MAP[LineMap.Type.StringIndex]], x[MAP[LineMap.Type.NewText]] }).ToArray();
                        ptp.ImportText(importedText);
                    }
                    if (MAP.CanGetName)
                    {
                        Dictionary<string, string> NameDic = import.Where(x => x.Length >= MAP.MinLength).ToDictionary(x => x[MAP[LineMap.Type.OldName]], x => x[MAP[LineMap.Type.NewName]]);
                        ptp.ImportNames(NameDic, Static.NewEncoding());
                    }
                }
            }
            else if (objectFile.Object is StringList strlst)
            {
                string path = value == "" ? Path.Combine(openedFileDir, Path.GetFileNameWithoutExtension(objectFile.Name) + ".TXT") : value;
                if (File.Exists(path))
                {
                    string[][] importedtext = File.ReadAllLines(path, parameters.Encode).Select(x => x.Split('\t')).
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
                    File.WriteAllBytes(newpath, pFile.GetData());
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
                    BMD bmd = new BMD();
                    bmd.Open(objectFile.Object as PTP, encoding);
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