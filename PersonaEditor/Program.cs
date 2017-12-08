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

namespace PersonaEditor
{
    public class FNTConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.FNT.FNT FNT;
        string source = "";

        public FNTConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("-----Font decompressor/compressor by Meloman19-----");
            Console.WriteLine("-------------------Persona 3/4/5-------------------");
            Console.WriteLine("----------Based on RikuKH3's decompressor----------");
            Console.WriteLine("---------------------------------------------------");

            source = Path.GetFullPath(filepath);
            FNT = new PersonaEditorLib.FileStructure.FNT.FNT(source);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.Image)
                ExportImg(dest);
            else if (com == CommandSubType.Table)
                ExportWT(dest);
        }

        #region ExportMethods

        private void ExportImg(string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".BMP");

            Imaging.SaveBMP(FNT.Image, dest);
        }

        private void ExportWT(string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".XML");

            FNT.Table.Save(dest);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.Image)
                ImportImg(src);
            if (com == CommandSubType.Table)
                ImportWT(src);
        }

        #region ImportMethods

        private void ImportImg(string src)
        {
            if (src == "") src = Util.GetNewPath(source, ".BMP");

            FNT.Image = Imaging.OpenImage(src);
        }

        private void ImportWT(string src)
        {
            if (src == "") src = Util.GetNewPath(source, ".XML");

            FNT.Table = XDocument.Load(src);
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).FNT");

            File.WriteAllBytes(dest, FNT.Get());
        }
    }

    public class TMXConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.TMX.TMX TMX;
        string source = "";

        public TMXConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            TMX = new PersonaEditorLib.FileStructure.TMX.TMX(filepath, parameters.IsLittleEndian);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.Image)
                ExportImg(dest);
        }

        #region ExportMethods

        private void ExportImg(string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PNG");

            Imaging.SaveBMP(TMX.Image, dest);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string source)
        {
        }

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            File.WriteAllBytes(dest, TMX.Get());
        }
    }

    public class PM1Console : IConsoleWork
    {
        PersonaEditorLib.FileStructure.PM1.PM1 PM1;
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public PM1Console(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            PM1 = new PersonaEditorLib.FileStructure.PM1.PM1(source, parameters.IsLittleEndian);
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

            File.WriteAllBytes(dest, PM1.GetBMD());
        }

        private void ExportPTP(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PM1.GetBMD(), Path.GetFileNameWithoutExtension(dest));
            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP();
            PTP.Open(BMD, ParList.CopyOld2New, Old, New);
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

            PM1.SetBMD(File.ReadAllBytes(src));
        }

        private void ImportPTP(string src, ArgumentsWork.Parameters ParList)
        {
            if (src == "") src = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP();
            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PTP, New);
            PM1.SetBMD(BMD.Get());
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).PM1");

            File.WriteAllBytes(dest, PM1.Get(parameters.IsLittleEndian));
        }
    }

    public class BFConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.BF.BF BF;
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public BFConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            BF = new PersonaEditorLib.FileStructure.BF.BF(source);
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

            File.WriteAllBytes(dest, BF.GetBMD());
        }

        private void ExportPTP(string dest, ArgumentsWork.Parameters ParList)
        {
            if (dest == "") dest = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(BF.GetBMD(), Path.GetFileNameWithoutExtension(dest));
            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP();
            PTP.Open(BMD, ParList.CopyOld2New, Old, New);
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

            BF.SetBMD(File.ReadAllBytes(src));
        }

        private void ImportPTP(string src, ArgumentsWork.Parameters ParList)
        {
            if (src == "") src = Util.GetNewPath(source, ".PTP");

            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP();
            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
            BMD.Open(PTP, New);
            BMD.IsLittleEndian = ParList.IsLittleEndian;
            BF.SetBMD(BMD.Get());
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
        PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public BMDConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            using (FileStream FS = File.OpenRead(source))
                BMD.Open(FS, source);
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

            PersonaEditorLib.FileStructure.PTP.PTP PTP = new PersonaEditorLib.FileStructure.PTP.PTP();
            PTP.Open(BMD, ParList.CopyOld2New, Old, New);
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
        PersonaEditorLib.FileStructure.PTP.PTP PTP;
        CharList Old = new CharList();
        CharList New = new CharList();
        string source = "";

        public PTPConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            PTP = new PersonaEditorLib.FileStructure.PTP.PTP();
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

            PersonaEditorLib.FileStructure.BMD.BMD BMD = new PersonaEditorLib.FileStructure.BMD.BMD();
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

    public class BINConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.BIN.BIN BIN;
        string source = "";

        public BINConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            BIN = new PersonaEditorLib.FileStructure.BIN.BIN(source);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.ALL)
                ExportAll(dest, parameters);
        }

        #region ExportMethods

        private void ExportAll(string dest, ArgumentsWork.Parameters ParList)
        {
            List<string> filelist = BIN.GetFileList.Select(x => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(source)), Path.GetFileName(source) + "+" + x.Replace('/', '+'))).ToList();

            for (int i = 0; i < filelist.Count; i++)
                using (BinaryWriter writer = new BinaryWriter(File.Create(filelist[i])))
                    writer.Write(BIN[i].ToString());
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.ALL)
                ImportAll(src, parameters);
        }

        #region ImportMethods

        private void ImportAll(string src, ArgumentsWork.Parameters ParList)
        {
            List<string> filelist = BIN.GetFileList.Select(x => Path.Combine(Path.GetDirectoryName(source), Path.GetFileName(source) + "+" + x.Replace('/', '+')))
                .Select(x => Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x) + "(NEW)" + Path.GetExtension(x))).ToList();

            List<FileInfo> Files = new List<FileInfo>(new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(source))).GetFiles());

            for (int i = 0; i < filelist.Count; i++)
            {
                var temp = Files.Find(x => x.Name == filelist[i]);
                if (temp != null)
                    using (BinaryReader reader = new BinaryReader(File.OpenRead(temp.FullName)))
                        BIN[i] = reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "")
            {
                string fullpath = Path.GetFullPath(source);
                dest = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + "(NEW)" + Path.GetExtension(fullpath);
            }

            File.WriteAllBytes(dest, BIN.Get());
        }
    }

    public class BVPConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.BVP.BVP BVP;
        string source = "";

        public BVPConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            BVP = new PersonaEditorLib.FileStructure.BVP.BVP(source, parameters.IsLittleEndian);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.ALL)
                ExportAll(parameters);
        }

        #region ExportMethods

        private void ExportAll(ArgumentsWork.Parameters ParList)
        {
            string temp = Path.GetDirectoryName(source);
            string temp2 = Path.GetFileNameWithoutExtension(source);

            for (int i = 0; i < BVP.Count; i++)
                File.WriteAllBytes(Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(3, '0') + ").BMD"), BVP[i]);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.ALL)
                ImportAll(parameters);
        }

        #region ImportMethods

        private void ImportAll(ArgumentsWork.Parameters ParList)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(source));
            string temp2 = Path.GetFileNameWithoutExtension(source);

            for (int i = 0; i < BVP.Count; i++)
            {
                string name = Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(3, '0') + ")(NEW).BMD");
                if (File.Exists(name))
                    BVP[i] = File.ReadAllBytes(name);
            }
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).BVP");

            File.WriteAllBytes(dest, BVP.Get(parameters.IsLittleEndian));
        }
    }

    public class TBLConsole : IConsoleWork
    {
        PersonaEditorLib.FileStructure.TBL.TBL TBL;
        string source = "";

        public TBLConsole(string filepath, ArgumentsWork.Parameters parameters)
        {
            source = Path.GetFullPath(filepath);
            TBL = new PersonaEditorLib.FileStructure.TBL.TBL(source, parameters.IsLittleEndian);
        }

        public void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest)
        {
            if (com == CommandSubType.ALL)
                ExportAll(parameters);
        }

        #region ExportMethods

        private void ExportAll(ArgumentsWork.Parameters ParList)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(source));
            string temp2 = Path.GetFileName(source);

            for (int i = 0; i < TBL.Count; i++)
                File.WriteAllBytes(Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(2, '0') + ").DAT"), TBL[i]);
        }

        #endregion ExportMethods

        public void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string src)
        {
            if (com == CommandSubType.ALL)
                ImportAll(parameters);
        }

        #region ImportMethods

        private void ImportAll(ArgumentsWork.Parameters ParList)
        {
            string temp = Path.GetDirectoryName(Path.GetFullPath(source));
            string temp2 = Path.GetFileName(source);

            for (int i = 0; i < TBL.Count; i++)
            {
                string name = Path.Combine(temp, temp2 + "(" + i.ToString().PadLeft(2, '0') + ")(NEW).DAT");
                if (File.Exists(name))
                    TBL[i] = File.ReadAllBytes(name);
            }
        }

        #endregion ImportMethods

        public void Save(ArgumentsWork.Parameters parameters, string dest)
        {
            if (dest == "") dest = Util.GetNewPath(source, "(NEW).TBL");

            File.WriteAllBytes(dest, TBL.Get(parameters.IsLittleEndian));
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
#if DEBUG
            Test();
#endif

            bool complete = Do(args);

            if (complete)
                Console.WriteLine(args[1] + " - Success");
            else
                Console.WriteLine("Failure");

#if DEBUG
            Console.ReadKey();
#endif
        }

        static bool Do(string[] args)
        {
            try
            {
                IConsoleWork work;

                ArgumentsWork argwrk = new ArgumentsWork(args);

                if (argwrk.FileType == "-FNT")
                    work = new FNTConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-TMX")
                    work = new TMXConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-PM1")
                    work = new PM1Console(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-BF")
                    work = new BFConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-BMD")
                    work = new BMDConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-PTP")
                    work = new PTPConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-BIN")
                    work = new BINConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-BVP")
                    work = new BVPConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-TBL")
                    work = new TBLConsole(argwrk.FileSource, argwrk.Param);
                else if (argwrk.FileType == "-TEXT")
                    work = new TEXTConsole(argwrk.FileSource, argwrk.Param);
                else
                    return false;

                foreach (var command in argwrk.ArgumentList)
                    if (command.Command == CommandType.Export)
                        work.Export(command.Type, command.Parameters, command.Value);
                    else if (command.Command == CommandType.Import)
                        work.Import(command.Type, command.Parameters, command.Value);
                    else if (command.Command == CommandType.Save)
                        work.Save(command.Parameters, command.Value);

                return true;
            }
            catch (Exception ex)
            {
                Logging.Write("D", ex);
                return false;
            }
        }

        static void Test()
        {
            string OldMap = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_OLD.TXT");
            string OldFont = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "font", "FONT_OLD.FNT");
            CharList charList = new CharList(OldMap, OldFont);
            PersonaEditorLib.FileStructure.StringList StringList = new PersonaEditorLib.FileStructure.StringList("MSG.TBL(03).DAT", charList);
            File.WriteAllBytes("111.DAT", StringList.Get(charList, 1));
        }
    }
}