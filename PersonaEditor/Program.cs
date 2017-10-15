using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using PersonaEditorLib.FileTypes;
using PersonaEditorLib.Extension;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PersonaEditor
{
    public class ArgumentWork
    {
        public class Argument
        {
            public class Parameter
            {
                public string Param { get; set; }
                public string Value { get; set; }
            }

            public string Command { get; set; }
            public string Value { get; set; }
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
                PTP
            }

            public FileType SourceType { get; private set; }
            public string Value { get; private set; }
            public List<Argument.Parameter> Parameters { get; set; }

            public ArgumentSourceFile(Argument arg)
            {
                if (arg.Command == "fnt") SourceType = FileType.FNT;
                else if (arg.Command == "pm1") SourceType = FileType.PM1;
                else if (arg.Command == "bf") SourceType = FileType.BF;
                else if (arg.Command == "bmd") SourceType = FileType.BMD;
                else if (arg.Command == "ptp") SourceType = FileType.PTP;
                else SourceType = FileType.Empty;

                Value = arg.Value;
                Parameters = arg.Params;
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
                ImportImg = 100,
                ImportWT = 101,
                ImportBMD = 102,
                ImportPTP = 103,
                ImportTXT = 104,
                Save = 200
            }

            public ActType Action { get; private set; }
            public string Value { get; private set; }
            public List<Argument.Parameter> Parameters { get; set; }

            public ArgumentAction(Argument arg)
            {
                if (arg.Command == "expimg") Action = ActType.ExportImg;
                else if (arg.Command == "expwt") Action = ActType.ExportWT;
                else if (arg.Command == "expbmd") Action = ActType.ExportBMD;
                else if (arg.Command == "expptp") Action = ActType.ExportPTP;
                else if (arg.Command == "exptxt") Action = ActType.ExportTXT;
                else if (arg.Command == "impimg") Action = ActType.ImportImg;
                else if (arg.Command == "impwt") Action = ActType.ImportWT;
                else if (arg.Command == "impbmd") Action = ActType.ImportBMD;
                else if (arg.Command == "impptp") Action = ActType.ImportPTP;
                else if (arg.Command == "imptxt") Action = ActType.ImportTXT;
                else if (arg.Command == "save") Action = ActType.Save;
                else Action = ActType.Empty;

                Value = arg.Value;
                Parameters = arg.Params;
            }
        }

        List<Argument> args = new List<Argument>();

        public ArgumentWork(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i][0] == '-')
                {
                    if (i + 1 < args.Length)
                    {
                        if (args[i + 1][0] == '-' | args[i + 1][0] == '/') this.args.Add(new Argument() { Command = args[i].Substring(1).ToLower(), Value = "" });
                        else
                        {
                            this.args.Add(new Argument() { Command = args[i].Substring(1).ToLower(), Value = args[i + 1] });
                            i++;
                        }
                    }
                    else this.args.Add(new Argument() { Command = args[i].Substring(1).ToLower(), Value = "" });
                }
                if (args[i][0] == '/')
                {
                    if (i + 1 < args.Length)
                    {
                        if (args[i + 1][0] == '-' | args[i + 1][0] == '/') this.args.Last().Params.Add(new Argument.Parameter() { Param = args[i].Substring(1).ToLower(), Value = "" });
                        else
                        {
                            this.args.Last().Params.Add(new Argument.Parameter() { Param = args[i].Substring(1).ToLower(), Value = args[i + 1] });
                            i++;
                        }
                    }
                    else this.args.Last().Params.Add(new Argument.Parameter() { Param = args[i].Substring(1).ToLower(), Value = "" });
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

            FNT.Table = XDocument.Load(filename);
        }

        public static void ImportImg(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".BMP");

            FNT.Image = Imaging.OpenImage(filename);
        }

        public static void ImportWT(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, ".XML");

            FNT.Table.Save(filename);
        }

        public static void Save(PersonaEditorLib.FileStructure.FNT.FNT FNT, string fontname, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(fontname, "(NEW).FNT");

            FNT.This.SaveToFile(filename);
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
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportBMD) ImportBMD(PM1, SourceFile.Value, command.Value);
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

        public static void ImportBMD(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".BMD");

            using (var FS = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {

                MemoryStream MS = new MemoryStream();
                FS.CopyTo(MS);
                PM1.SetBMD(MS);
            }
        }

        public static void Save(PersonaEditorLib.FileStructure.PM1.PM1 PM1, string sourcefile, string filename, List<ArgumentWork.Argument.Parameter> ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).PM1");

            bool IsLittleEndian = true;

            foreach (var a in ParList)
            {
                if (a.Param == "be") IsLittleEndian = false;
            }

            PM1.Get(IsLittleEndian).SaveToFile(filename);
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

                PersonaEditorLib.FileStructure.BF.BF PM1 = new PersonaEditorLib.FileStructure.BF.BF(SourceFile.Value, true);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportBMD) ExportBMD(PM1, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.ImportBMD) ImportBMD(PM1, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(PM1, SourceFile.Value, command.Value, command.Parameters);

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

        public static void ImportBMD(PersonaEditorLib.FileStructure.BF.BF PM1, string sourcefile, string filename)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, ".BMD");

            using (var FS = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {

                MemoryStream MS = new MemoryStream();
                FS.CopyTo(MS);
                PM1.SetBMD(MS);
            }
        }

        public static void Save(PersonaEditorLib.FileStructure.BF.BF BF, string sourcefile, string filename, List<ArgumentWork.Argument.Parameter> ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BF");

            bool IsLittleEndian = true;

            foreach (var a in ParList)
            {
                if (a.Param == "be") IsLittleEndian = false;
            }

            BF.Get(IsLittleEndian).SaveToFile(filename);
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

                Text.BMDfactory BMD = new Text.BMDfactory(SourceFile.Value);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportPTP) ExportPTP(BMD, SourceFile.Value, command.Value);
                    else if (command.Action == ArgumentWork.ArgumentAction.ActType.Save) Save(BMD, SourceFile.Value, command.Value, command.Parameters);


                return true;
            }
            catch (Exception e)
            {
                PersonaEditorLib.Logging.Write("BMDWork.log", e);
                return false;
            }
        }

        public static void ExportPTP(Text.BMDfactory BMD, string sourcefile, string filename)
        {
            if (filename == "")
            {
                string fullpath = Path.GetFullPath(sourcefile);
                filename = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + ".PTP";
            }

            BMD.SavePTP(filename);
        }

        public static void Save(Text.BMDfactory BMD, string sourcefile, string filename, List<ArgumentWork.Argument.Parameter> ParList)
        {
            if (filename == "") filename = Util.GetNewPath(sourcefile, "(NEW).BMD");

            bool IsLittleEndian = true;

            foreach (var a in ParList)
                if (a.Param == "be") IsLittleEndian = false;

            BMD.Get(IsLittleEndian).SaveToFile(filename);
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

                string oldfont = "";
                string newfont = "";
                string oldmap = "";
                string newmap = "";

                foreach (var a in SourceFile.Parameters)
                {
                    if (a.Param == "nf") newfont = a.Value;
                    else if (a.Param == "of") oldfont = a.Value;
                    else if (a.Param == "nm") newmap = a.Value;
                    else if (a.Param == "om") oldmap = a.Value;
                }

                Text.PTPfactory PTP = new Text.PTPfactory(SourceFile.Value, oldfont, newfont, oldmap, newmap);

                foreach (var command in ActList)
                    if (command.Action == ArgumentWork.ArgumentAction.ActType.ExportTXT) ExportTXT(PTP, SourceFile.Value, command.Value);
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

        public static void ExportTXT(Text.PTPfactory PTP, string sourcefile, string filename)
        {

        }

        public static void ImportTXT(Text.PTPfactory PTP, string sourcefile, string filename, List<ArgumentWork.Argument.Parameter> ParList)
        {
            string map = "";
            bool auto = false;
            int width = 0;

            foreach (var a in ParList)
            {
                if (a.Param == "map") map = a.Value;
                else if (a.Param == "auto")
                {
                    auto = true;
                    width = Convert.ToInt32(a.Value);
                }
            }

            PTP.PTP.ImportTXT(filename, map, auto, width);
        }

        public static void ExportBMD(Text.PTPfactory PTP, string sourcefile, string filename, List<ArgumentWork.Argument.Parameter> ParList)
        {
            if (filename == "")
                Util.GetNewPath(sourcefile, "(NEW).BMD");

            bool IsLittleEndian = true;

            foreach (var a in ParList)
                if (a.Param == "be") IsLittleEndian = false;

            PersonaEditorLib.FileStructure.BMD BMD = new PersonaEditorLib.FileStructure.BMD();
            BMD.Load(PTP.PTP);
            BMD.Get(IsLittleEndian).SaveToFile(filename);
        }

        public static void Save(Text.PTPfactory PTP, string sourcefile, string filename)
        {
            if (filename == "")
            {
                string fullpath = Path.GetFullPath(sourcefile);
                filename = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + "_NEW.PTP";
            }

            PTP.Save(filename);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool complete = false;

            ArgumentWork argWRK = new ArgumentWork(args);
            var arg = argWRK.GetSourceFile();

            if (arg.SourceType != ArgumentWork.ArgumentSourceFile.FileType.Empty)
            {
                if (arg.SourceType == ArgumentWork.ArgumentSourceFile.FileType.FNT) complete = FNTWork.Work(argWRK);
                else if (arg.SourceType == ArgumentWork.ArgumentSourceFile.FileType.PM1) complete = PM1Work.Work(argWRK);
                else if (arg.SourceType == ArgumentWork.ArgumentSourceFile.FileType.BMD) complete = BMDWork.Work(argWRK);
                else if (arg.SourceType == ArgumentWork.ArgumentSourceFile.FileType.PTP) complete = PTPWork.Work(argWRK);
                else if (arg.SourceType == ArgumentWork.ArgumentSourceFile.FileType.BF) complete = BFWork.Work(argWRK);
            }

            if (complete) Console.WriteLine("Success");
            else Console.WriteLine("Failure");

            Console.ReadKey();
        }
    }
}