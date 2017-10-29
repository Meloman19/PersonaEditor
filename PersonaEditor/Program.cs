using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using PersonaEditorLib.FileTypes;
using PersonaEditorLib.Extension;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using PersonaEditorLib;
using System.Text.RegularExpressions;

namespace PersonaEditor
{
    public class ArgumentWork
    {
        public class Parameters
        {
            public string NewFont { get; private set; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_NEW.FNT");
            public string OldFont { get; private set; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_OLD.FNT");
            public string NewMap { get; private set; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_NEW.TXT");
            public string OldMap { get; private set; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_OLD.TXT");
            public bool IsLittleEndian { get; private set; } = true;
            public string Map { get; private set; } = "";
            public bool Auto { get; private set; } = false;
            public int Width { get; private set; } = 0;
            public bool RemoveSplit { get; private set; } = false;
            public bool CopyOld2New { get; private set; } = false;
            public int Length { get; private set; } = 0;
            public bool Old { get; private set; } = true;
            public bool SkipEmpty { get; private set; } = false;

            public Parameters(List<Argument.Parameter> ParList)
            {
                foreach (var a in ParList)
                {
                    if (a.Param == "nf") NewFont = a.Value;
                    else if (a.Param == "of") OldFont = a.Value;
                    else if (a.Param == "nm") NewMap = a.Value;
                    else if (a.Param == "om") OldMap = a.Value;
                    else if (a.Param == "be") IsLittleEndian = false;
                    else if (a.Param == "map") Map = a.Value;
                    else if (a.Param == "auto")
                    {
                        Auto = true;
                        Width = Convert.ToInt32(a.Value);
                    }
                    else if (a.Param == "rmvspl") RemoveSplit = true;
                    else if (a.Param == "co2n") CopyOld2New = true;
                    else if (a.Param == "len") Length = Convert.ToInt32(a.Value);
                    else if (a.Param == "new") Old = false;
                    else if (a.Param == "skipempty") SkipEmpty = true;
                }
            }
        }

        public class Argument
        {
            public class Parameter
            {
                public string Param { get; set; }
                public string Value { get; set; }
            }

            public string Command { get; set; } = "";
            public string Value { get; set; } = "";
            public List<Parameter> Params { get; set; } = new List<Parameter>();
        }

        public struct ArgumentSourceFile
        {
            public enum FileType
            {
                Empty,
                FNT,
                PM1,
                BF,
                BMD,
                PTP,
                TMX,
                BIN,
                BVP,
                TBL,
                TEXT
            }

            public FileType SourceType { get; private set; }
            public string Value { get; private set; }
            public Parameters Parameters { get; private set; }

            public ArgumentSourceFile(Argument arg)
            {
                if (arg.Command == "fnt") SourceType = FileType.FNT;
                else if (arg.Command == "pm1") SourceType = FileType.PM1;
                else if (arg.Command == "bf") SourceType = FileType.BF;
                else if (arg.Command == "bmd") SourceType = FileType.BMD;
                else if (arg.Command == "ptp") SourceType = FileType.PTP;
                else if (arg.Command == "tmx") SourceType = FileType.TMX;
                else if (arg.Command == "bin") SourceType = FileType.BIN;
                else if (arg.Command == "bvp") SourceType = FileType.BVP;
                else if (arg.Command == "tbl") SourceType = FileType.TBL;
                else if (arg.Command == "text") SourceType = FileType.TEXT;
                else SourceType = FileType.Empty;

                Value = arg.Value;
                Parameters = new Parameters(arg.Params);
            }
        }

        public struct ArgumentAction
        {
            public enum ActType
            {
                Empty = 0,
                ExportImg = 10,
                ExportWT = 11,
                ExportBMD = 12,
                ExportPTP = 13,
                ExportTXT = 14,
                ExportAll = 99,
                ImportImg = 100,
                ImportWT = 101,
                ImportBMD = 102,
                ImportPTP = 103,
                ImportTXT = 104,
                ImportAll = 199,
                Save = 200
            }

            public ActType Action { get; private set; }
            public string Value { get; private set; }
            public Parameters Parameters { get; private set; }

            public ArgumentAction(Argument arg)
            {
                if (arg.Command == "expimg") Action = ActType.ExportImg;
                else if (arg.Command == "expwt") Action = ActType.ExportWT;
                else if (arg.Command == "expbmd") Action = ActType.ExportBMD;
                else if (arg.Command == "expptp") Action = ActType.ExportPTP;
                else if (arg.Command == "exptxt") Action = ActType.ExportTXT;
                else if (arg.Command == "expall") Action = ActType.ExportAll;
                else if (arg.Command == "impimg") Action = ActType.ImportImg;
                else if (arg.Command == "impwt") Action = ActType.ImportWT;
                else if (arg.Command == "impbmd") Action = ActType.ImportBMD;
                else if (arg.Command == "impptp") Action = ActType.ImportPTP;
                else if (arg.Command == "imptxt") Action = ActType.ImportTXT;
                else if (arg.Command == "impall") Action = ActType.ImportAll;
                else if (arg.Command == "save") Action = ActType.Save;
                else Action = ActType.Empty;

                Value = arg.Value;
                Parameters = new Parameters(arg.Params);
            }
        }

        List<Argument> args = new List<Argument>();

        public ArgumentWork(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i][0] == '-')
                    this.args.Add(new Argument() { Command = args[i].Substring(1).ToLower() });
                else if (args[i][0] == '/')
                {
                    if (this.args.Count > 0)
                    {
                        Argument a = this.args.Last();
                        a.Params.Add(new Argument.Parameter() { Param = args[i].Substring(1).ToLower() });
                    }
                }
                else
                {
                    if (this.args.Count > 0)
                    {
                        Argument a = this.args.Last();
                        if (a.Params.Count > 0)
                        {
                            Argument.Parameter b = a.Params.Last();
                            b.Value = args[i];
                        }
                        else
                            a.Value = args[i];
                    }
                }
            }
        }

        public ArgumentSourceFile GetSourceFile()
        {
            if (args.Count > 0) return new ArgumentSourceFile(args[0]);
            else return new ArgumentSourceFile();
        }

        public List<ArgumentAction> GetActList()
        {
            List<ArgumentAction> returned = new List<ArgumentAction>();

            if (args.Count > 1)
                for (int i = 1; i < args.Count; i++)
                    returned.Add(new ArgumentAction(args[i]));

            return returned;
        }
    }

    public static class FNTWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("-----Font decompressor/compressor by Meloman19-----");
            Console.WriteLine("-------------------Persona 3/4/5-------------------");
            Console.WriteLine("----------Based on RikuKH3's decompressor----------");
            Console.WriteLine("---------------------------------------------------");

            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.FNT.FNT FNT = new PersonaEditorLib.FileStructure.FNT.FNT(SourceFile.Value);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportImg) ExportImg(FNT, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportWT) ExportWT(FNT, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(FNT, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportImg) ImportImg(FNT, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportWT) ImportWT(FNT, SourceFile.Value, command.Value);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportImg(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".BMP");

            Imaging.SaveBMP(FNT.Image, filename);
        }

        public static void ExportWT(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".XML");

            FNT.Table.Save(filename);
        }

        public static void ImportImg(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".BMP");

            FNT.Image = Imaging.OpenImage(filename);
        }

        public static void ImportWT(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".XML");

            FNT.Table = XDocument.Load(filename);
        }

        public static void Save(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, "(NEW).FNT");

            FNT.This.SaveToFile(filename);
        }
    }

    public static class TMXWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.TMX.TMX FNT = new PersonaEditorLib.FileStructure.TMX.TMX(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportImg) ExportImg(FNT, SourceFile.Value, command.Value);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportImg(PersonaEditorLib.FileStructure.TMX.TMX TMX, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".PNG");

            Imaging.SavePNG(TMX.Image, filename);
        }
    }

    public static class PM1Work
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.PM1.PM1 PM1 = new PersonaEditorLib.FileStructure.PM1.PM1(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportBMD) ExportBMD(PM1, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportPTP) ExportPTP(PM1, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportBMD) ImportBMD(PM1, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportPTP) ImportPTP(PM1, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(PM1, SourceFile.Value, command.Value, command.Parameters);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportBMD(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".BMD");

            PM1.GetBMD().SaveToFile(filename);
        }

        public static void ExportPTP(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".PTP");

            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PM1.GetBMD(), Path.GetFileNameWithoutExtension(filename), true);
            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP(ParList.OldFont, ParList.OldMap, ParList.NewFont, ParList.NewMap);
            PTP.Open(BMD, ParList.CopyOld2New);
            PTP.SaveProject(filename);
        }

        public static void ImportBMD(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BMD");

            using (var FS = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                MemoryStream MS = new MemoryStream();
                FS.CopyTo(MS);
                PM1.SetBMD(MS);
            }
        }

        public static void ImportPTP(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".PTP");

            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP(ParList.OldFont, ParList.OldMap, ParList.NewFont, ParList.NewMap);
            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PTP);
            PM1.SetBMD(BMD.Get(ParList.IsLittleEndian));
        }

        public static void Save(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).PM1");

            PM1.Get(ParList.IsLittleEndian).SaveToFile(filename);
        }
    }

    public static class BFWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.BF.BF BF = new PersonaEditorLib.FileStructure.BF.BF(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportBMD) ExportBMD(BF, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportPTP) ExportPTP(BF, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportBMD) ImportBMD(BF, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportPTP) ImportPTP(BF, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(BF, SourceFile.Value, command.Value, command.Parameters);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportBMD(PersonaEditorLib.FileStructure.BF.BF BF, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".BMD");

            BF.GetBMD().SaveToFile(filename);
        }

        public static void ExportPTP(PersonaEditorLib.FileStructure.BF.BF BF, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".PTP");

            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(BF.GetBMD(), Path.GetFileNameWithoutExtension(filename), true);
            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP(ParList.OldFont, ParList.OldMap, ParList.NewFont, ParList.NewMap);
            PTP.Open(BMD, ParList.CopyOld2New);
            PTP.SaveProject(filename);
        }

        public static void ImportBMD(PersonaEditorLib.FileStructure.BF.BF BF, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BMD");

            using (var FS = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {

                MemoryStream MS = new MemoryStream();
                FS.CopyTo(MS);
                BF.SetBMD(MS);
            }
        }

        public static void ImportPTP(PersonaEditorLib.FileStructure.BF.BF BF, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".PTP");

            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP(ParList.OldFont, ParList.OldMap, ParList.NewFont, ParList.NewMap);
            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PTP);
            BF.SetBMD(BMD.Get(ParList.IsLittleEndian));
        }

        public static void Save(PersonaEditorLib.FileStructure.BF.BF BF, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BF");

            BF.Get(ParList.IsLittleEndian).SaveToFile(filename);
        }
    }

    public static class BMDWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
                BMD.Open(File.OpenRead(SourceFile.Value), SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportPTP) ExportPTP(BMD, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(BMD, SourceFile.Value, command.Value, command.Parameters);


                return true;
            }
            catch (Exception e)
            {
                PersonaEditorLib.Logging.Write("BMDWork.log", e);
                return false;
            }
        }

        public static void ExportPTP(PersonaEditorLib.FileStructure.BMD.BMD BMDfile, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".PTP");

            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP(ParList.OldFont, ParList.OldMap, ParList.NewFont, ParList.NewMap);
            PTP.Open(BMDfile, ParList.CopyOld2New);
            PTP.SaveProject(filename);
        }

        public static void Save(PersonaEditorLib.FileStructure.BMD.BMD BMD, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BMD");

            BMD.Get(ParList.IsLittleEndian).SaveToFile(filename);
        }
    }

    public static class PTPWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP(SourceFile.Parameters.OldFont, SourceFile.Parameters.OldMap, SourceFile.Parameters.NewFont, SourceFile.Parameters.NewMap);

                PTP.Open(SourceFile.Value);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportTXT) ExportTXT(PTP, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportTXT) ImportTXT(PTP, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportBMD) ExportBMD(PTP, SourceFile.Value, command.Value, command.Parameters);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(PTP, SourceFile.Value, command.Value);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportTXT(PersonaEditorLib.FileStructure.PTP.PTP PTP, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            PTP.ExportTXT(filename, ParList.Map, ParList.RemoveSplit);
        }

        public static void ImportTXT(PersonaEditorLib.FileStructure.PTP.PTP PTP, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            PTP.ImportTXT(filename, ParList.Map, ParList.Auto, ParList.Width, ParList.SkipEmpty);
        }

        public static void ExportBMD(PersonaEditorLib.FileStructure.PTP.PTP PTP, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BMD");

            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PTP);
            BMD.Get(ParList.IsLittleEndian).SaveToFile(filename);
        }

        public static void Save(PersonaEditorLib.FileStructure.PTP.PTP PTP, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".PTP");

            PTP.SaveProject(filename);
        }
    }

    public static class BINWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.BIN.BIN BIN = new PersonaEditorLib.FileStructure.BIN.BIN(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportAll) ExportAll(BIN, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportAll) ImportAll(BIN, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(BIN, SourceFile.Value, command.Value, command.Parameters);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportAll(PersonaEditorLib.FileStructure.BIN.BIN BIN, string sourcefile, string filename)
        {
            List<string> filelist = BIN.GetFileList.Select(x => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(sourcefile)), Path.GetFileName(sourcefile) + "+" + x.Replace('/', '+'))).ToList();

            for (int i = 0; i < filelist.Count; i++)
                using (BinaryWriter writer = new BinaryWriter(File.Create(filelist[i])))
                    writer.Write(BIN[i]);
        }

        public static void ImportAll(PersonaEditorLib.FileStructure.BIN.BIN BIN, string sourcefile, string filename)
        {
            List<string> filelist = BIN.GetFileList.Select(x => Path.Combine(Path.GetDirectoryName(sourcefile), Path.GetFileName(sourcefile) + "+" + x.Replace('/', '+')))
                .Select(x => Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x) + "(NEW)" + Path.GetExtension(x))).ToList();

            List<FileInfo> Files = new List<FileInfo>(new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(sourcefile))).GetFiles());

            for (int i = 0; i < filelist.Count; i++)
            {
                var temp = Files.Find(x => x.Name == filelist[i]);
                if (temp != null)
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(temp.FullName)))
                        BIN[i] = reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static void Save(PersonaEditorLib.FileStructure.BIN.BIN BIN, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "")
            {
                string fullpath = Path.GetFullPath(sourcefile);
                filename = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + "(NEW)" + Path.GetExtension(fullpath);
            }

            BIN.Get().SaveToFile(filename);
        }
    }

    public static class BVPWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.BVP.BVP BVP = new PersonaEditorLib.FileStructure.BVP.BVP(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportAll) ExportAll(BVP, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportAll) ImportAll(BVP, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(BVP, SourceFile.Value, command.Value, command.Parameters);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportAll(PersonaEditorLib.FileStructure.BVP.BVP BVP, string sourcefile, string filename)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(sourcefile));
            string temp2 = Path.GetFileNameWithoutExtension(sourcefile);

            for (int i = 0; i < BVP.Count; i++)
                File.WriteAllBytes(Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(3, '0') + ").BMD"), BVP[i]);
        }

        public static void ImportAll(PersonaEditorLib.FileStructure.BVP.BVP BVP, string sourcefile, string filename)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(sourcefile));
            string temp2 = Path.GetFileNameWithoutExtension(sourcefile);

            for (int i = 0; i < BVP.Count; i++)
            {
                string name = Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(3, '0') + ")(NEW).BMD");
                if (File.Exists(name))
                    BVP[i] = File.ReadAllBytes(name);
            }
        }

        public static void Save(PersonaEditorLib.FileStructure.BVP.BVP BVP, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BVP");

            File.WriteAllBytes(filename, BVP.Get(ParList.IsLittleEndian));
        }
    }

    public static class TBLWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                PersonaEditorLib.FileStructure.TBL.TBL TBL = new PersonaEditorLib.FileStructure.TBL.TBL(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportAll) ExportAll(TBL, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportAll) ImportAll(TBL, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(TBL, SourceFile.Value, command.Value, command.Parameters);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportAll(PersonaEditorLib.FileStructure.TBL.TBL TBL, string sourcefile, string filename)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(sourcefile));
            string temp2 = Path.GetFileName(sourcefile);

            for (int i = 0; i < TBL.Count; i++)
                File.WriteAllBytes(Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(2, '0') + ").DAT"), TBL[i]);
        }

        public static void ImportAll(PersonaEditorLib.FileStructure.TBL.TBL TBL, string sourcefile, string filename)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(sourcefile));
            string temp2 = Path.GetFileName(sourcefile);

            for (int i = 0; i < TBL.Count; i++)
            {
                string name = Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(2, '0') + ")(NEW).DAT");
                if (File.Exists(name))
                    TBL[i] = File.ReadAllBytes(name);
            }
        }

        public static void Save(PersonaEditorLib.FileStructure.TBL.TBL TBL, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).TBL");

            File.WriteAllBytes(filename, TBL.Get(ParList.IsLittleEndian));
        }
    }

    public static class TEXTWork
    {
        public static bool Work(ArgumentWork argWRK)
        {
            try
            {
                var SourceFile = argWRK.GetSourceFile();
                var ActList = argWRK.GetActList();

                if (SourceFile.Parameters.Length <= 0) return false;

                PersonaEditorLib.FileStructure.StringList StringList = new PersonaEditorLib.FileStructure.StringList(SourceFile.Value, SourceFile.Parameters.Length,
                    SourceFile.Parameters.Old ? new CharList(SourceFile.Parameters.OldMap, SourceFile.Parameters.OldFont) : new CharList(SourceFile.Parameters.NewMap, SourceFile.Parameters.NewFont));

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportTXT) ExportTXT(StringList, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportTXT) ImportTXT(StringList, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(StringList, SourceFile.Value, command.Value, command.Parameters);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static void ExportTXT(PersonaEditorLib.FileStructure.StringList StringList, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".TXT");

            for (int i = 0; i < StringList.Count; i++)
                File.AppendAllText(filename, StringList[i] + "\r\n");
        }

        public static void ImportTXT(PersonaEditorLib.FileStructure.StringList StringList, string sourcefile, string filename)
        {
            StringList.Import(filename);
        }

        public static void Save(PersonaEditorLib.FileStructure.StringList StringList, string sourcefile, string filename, ArgumentWork.Parameters ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).DAT");
            
            File.WriteAllBytes(filename, StringList.Get(ParList.Length, ParList.Old ? new CharList(ParList.OldMap, ParList.OldFont) : new CharList(ParList.NewMap, ParList.NewFont)));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool complete = false;

            ArgumentWork argWRK = new ArgumentWork(args);
            var arg = argWRK.GetSourceFile();

            switch (arg.SourceType)
            {
                case (ArgumentWork.ArgumentSourceFile.FileType.FNT):
                    complete = FNTWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.PM1):
                    complete = PM1Work.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.BMD):
                    complete = BMDWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.PTP):
                    complete = PTPWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.BF):
                    complete = BFWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.TMX):
                    complete = TMXWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.BIN):
                    complete = BINWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.BVP):
                    complete = BVPWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.TBL):
                    complete = TBLWork.Work(argWRK);
                    break;
                case (ArgumentWork.ArgumentSourceFile.FileType.TEXT):
                    complete = TEXTWork.Work(argWRK);
                    break;
                default:
                    break;
            }

            if (complete) Console.WriteLine(arg.Value + " - Success");
            else Console.WriteLine("Failure");

#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}