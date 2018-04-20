using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib.Extension;

namespace PersonaEditor
{
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
        PTP,
        Text,
        All
    }

    public class Parameters
    {
        public string Map { get; } = "";
        public int Width { get; } = 0;
        public bool RemoveSplit { get; } = false;
        public bool CopyOld2New { get; } = false;
        public int Length { get; } = 0;
        public bool Old { get; } = true;
        public bool SkipEmpty { get; } = false;
        public Encoding Encode { get; } = Encoding.UTF8;
        public bool Sub { get; } = false;
        public bool LineByLine { get; } = false;
        public int Size { get; } = -1;

        public Parameters()
        {

        }

        public Parameters(string[][] parameters)
        {
            foreach (var a in parameters)
            {
                if (a[0] == "/map")
                    Map = a[1];
                else if (a[0] == "/auto")
                    Width = Convert.ToInt32(a[1]);
                else if (a[0] == "/rmvspl")
                    RemoveSplit = true;
                else if (a[0] == "/co2n")
                    CopyOld2New = true;
                else if (a[0] == "/len")
                    Length = Convert.ToInt32(a[1]);
                else if (a[0] == "/new")
                    Old = false;
                else if (a[0] == "/skipempty")
                    SkipEmpty = true;
                else if (a[0] == "/enc")
                {
                    if (a[1] == "UTF-7")
                        Encode = Encoding.UTF7;
                    if (a[1] == "UTF-16")
                        Encode = Encoding.Unicode;
                    if (a[1] == "UTF-32")
                        Encode = Encoding.UTF32;
                }
                else if (a[0] == "/sub")
                    Sub = true;
                else if (a[0] == "/lbl")
                    LineByLine = true;
                else if (a[0] == "/size")
                    Size = int.Parse(a[1]);
            }
        }
    }

    public class Argument
    {
        public CommandType Command { get; } = CommandType.Empty;
        public CommandSubType Type { get; } = CommandSubType.Empty;
        public FileType FileType { get; } = FileType.Unknown;

        public string Value { get; } = "";
        public Parameters Parameters { get; } = new Parameters();

        public Argument(string[] args)
        {
            var split = args.SplitInclude(x => x.StartsWith("/"), true).ToArray();

            string com = split[0][0].ToLower();
            if (com == "-save")
                Command = CommandType.Save;
            else if (com.StartsWith("-exp"))
                Command = CommandType.Export;
            else if (com.StartsWith("-imp"))
                Command = CommandType.Import;
            else
                return;

            if ((Command == CommandType.Export | Command == CommandType.Import) && com.Length > 4)
            {
                string temp = com.Substring(4, com.Length - 4);
                if (temp == "image")
                    Type = CommandSubType.Image;
                else if (temp == "table")
                    Type = CommandSubType.Table;
                else if (temp == "ptp")
                    Type = CommandSubType.PTP;
                else if (temp == "text")
                    Type = CommandSubType.Text;
                else if (temp == "all")
                    Type = CommandSubType.All;
                else
                    FileType = GetFileType(temp);
            }

            if (split[0].Length > 1)
                Value = split[0][1];
            if (split.Length > 1)
                Parameters = new Parameters(split.Skip(1).ToArray());
            else
                Parameters = new Parameters();
        }

        static FileType GetFileType(string type)
        {
            if (type == "bin")
                return FileType.BIN;
            else if (type == "spr")
                return FileType.SPR;
            else if (type == "tmx")
                return FileType.TMX;
            else if (type == "bf")
                return FileType.BF;
            else if (type == "pm1")
                return FileType.PM1;
            else if (type == "bmd")
                return FileType.BMD;
            else if (type == "fnt")
                return FileType.FNT;
            else if (type == "bvp")
                return FileType.BVP;
            else if (type == "hex")
                return FileType.DAT;
            else
                return FileType.Unknown;
        }
    }

    public class ArgumentsWork
    {
        public List<Argument> ArgumentList { get; } = new List<Argument>();

        public string OpenedFile { get; } = "";

        public string OpenedFileDir { get; } = "";

        public string OpenedArgument { get; } = "";

        public ArgumentsWork(string[] args)
        {
            var split = args.SplitInclude(x => x.StartsWith("-"), true).ToArray();

            if (File.Exists(split[0][0]))
            {
                OpenedFile = Path.GetFullPath(args[0]);
                OpenedFileDir = Path.GetDirectoryName(args[0]);

                if (split[0].Length > 1)
                    OpenedArgument = split[0][1];
            }
            else
                return;

            for (int i = 1; i < split.Length; i++)
                ArgumentList.Add(new Argument(split[i]));
        }
    }
}