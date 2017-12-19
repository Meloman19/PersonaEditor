using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditor
{
    public interface IConsoleWork
    {
        void Export(CommandSubType com, ArgumentsWork.Parameters parameters, string dest);
        void Import(CommandSubType com, ArgumentsWork.Parameters parameters, string source);
        void Save(ArgumentsWork.Parameters parameters, string dest);
    }

    public enum CommandType
    {
        Empty,
        Export,
        Import,
        Save
    }

    public enum CommandSubType
    {
        Empty,
        Image,
        Table,
        BMD,
        PTP,
        TXT,
        ALL
    }

    public class ArgumentsWork
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
            public Encoding Encode { get; private set; } = Encoding.UTF8;

            public void Update(List<string[]> ParList)
            {
                foreach (var a in ParList)
                {
                    if (a[0] == "nf") NewFont = a[1];
                    else if (a[0] == "of") OldFont = a[1];
                    else if (a[0] == "nm") NewMap = a[1];
                    else if (a[0] == "om") OldMap = a[1];
                    else if (a[0] == "be") IsLittleEndian = false;
                    else if (a[0] == "map") Map = a[1];
                    else if (a[0] == "auto")
                    {
                        Auto = true;
                        Width = Convert.ToInt32(a[1]);
                    }
                    else if (a[0] == "rmvspl") RemoveSplit = true;
                    else if (a[0] == "co2n") CopyOld2New = true;
                    else if (a[0] == "len") Length = Convert.ToInt32(a[1]);
                    else if (a[0] == "new") Old = false;
                    else if (a[0] == "skipempty") SkipEmpty = true;
                    else if (a[0] == "enc")
                    {
                        if (a[1] == "UTF-7")
                            Encode = Encoding.UTF7;
                        if (a[1] == "UTF-16")
                            Encode = Encoding.Unicode;
                    }
                }
            }

            public Parameters()
            {

            }

            public Parameters(List<string[]> ParList)
            {
                Update(ParList);
            }
        }

        public class Argument
        {
            public CommandType Command { get; private set; } = CommandType.Empty;
            public CommandSubType Type { get; private set; } = CommandSubType.Empty;

            public string Com { get; } = "";

            public string Value { get; set; } = "";
            public Parameters Parameters { get; set; } = new Parameters();

            public Argument(string arg)
            {
                if (arg == "save")
                    Command = CommandType.Save;
                else
                {
                    if (arg.StartsWith("exp"))
                        Command = CommandType.Export;
                    else if (arg.StartsWith("imp"))
                        Command = CommandType.Import;

                    if (arg.Length > 4)
                    {

                        string temp = arg.Substring(3, arg.Length - 3);

                        Com = temp;
                        if (temp == "img")
                            Type = CommandSubType.Image;
                        else if (temp == "wt")
                            Type = CommandSubType.Table;
                        else if (temp == "bmd")
                            Type = CommandSubType.BMD;
                        else if (temp == "ptp")
                            Type = CommandSubType.PTP;
                        else if (temp == "txt")
                            Type = CommandSubType.TXT;
                        else if (temp == "all")
                            Type = CommandSubType.ALL;
                    }
                }
            }
        }

        public List<Argument> ArgumentList { get; private set; } = new List<Argument>();

        public string FType { get; private set; } = "";

        public FileType FileType { get; }

        public string FileSource { get; private set; } = "";

        public Parameters Param { get; private set; } = new Parameters();

        public ArgumentsWork(string[] args)
        {
            if (args.Length > 1)
            {
                FType = args[0];
                FileType = GetType(args[0]);
                FileSource = args[1];
            }
            else
                return;

            List<string[]> param = new List<string[]>();

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i][0] == '-')
                {
                    if (param.Count > 0)
                    {
                        var temp = ArgumentList.LastOrDefault();
                        if (temp == null)
                            Param.Update(param);
                        else
                            temp.Parameters.Update(param);
                        param.Clear();
                    }
                    ArgumentList.Add(new Argument(args[i].Substring(1).ToLower()));
                }
                else if (args[i][0] == '/')
                    param.Add(new string[] { args[i].Substring(1).ToLower(), "" });
                else
                {
                    var temp = param.LastOrDefault();
                    if (temp != null)
                        temp[1] = args[i];
                    else
                    {
                        var lastarg = ArgumentList.Last();
                        if (lastarg != null)
                            lastarg.Value = args[i];
                    }
                }
            }

            if (param.Count > 0)
            {
                var temp = ArgumentList.Last();
                if (temp == null)
                    Param.Update(param);
                else
                    temp.Parameters = new Parameters(param);
                param.Clear();
            }
        }

        private FileType GetType(string command)
        {
            if (command == "-FNT")
                return FileType.FNT;
            else if (command == "-TMX")
                return FileType.TMX;
            else if (command == "-PM1")
                return FileType.PM1;
            else if (command == "-BF")
                return FileType.BF;
            else if (command == "-BMD")
                return FileType.BMD;
            else if (command == "-PTP")
                return FileType.PTP;
            else if (command == "-BIN")
                return FileType.BIN;
            else if (command == "-BVP")
                return FileType.BVP;
            else if (command == "-SPR")
                return FileType.SPR;
            //else if (command == "-TBL")
            //    work = new TBLConsole(argwrk.FileSource, argwrk.Param);
            //else if (command == "-TEXT")
            //    work = new TEXTConsole(argwrk.FileSource, argwrk.Param);
            else
                throw new ArgumentException("Open file's type unknown", "command");
        }
    }
}