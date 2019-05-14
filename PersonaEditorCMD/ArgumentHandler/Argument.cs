using AuxiliaryLibraries.Extensions;
using PersonaEditorLib;
using System.Linq;

namespace PersonaEditorCMD.ArgumentHandler
{
    public class Argument
    {
        public CommandType Command { get; } = CommandType.Empty;
        public CommandSubType Type { get; } = CommandSubType.Empty;
        public FormatEnum FileType { get; } = FormatEnum.Unknown;

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

        static FormatEnum GetFileType(string type)
        {
            if (type == "bin")
                return FormatEnum.BIN;
            else if (type == "spr")
                return FormatEnum.SPR;
            else if (type == "tmx")
                return FormatEnum.TMX;
            else if (type == "bf")
                return FormatEnum.BF;
            else if (type == "pm1")
                return FormatEnum.PM1;
            else if (type == "bmd")
                return FormatEnum.BMD;
            else if (type == "fnt")
                return FormatEnum.FNT;
            else if (type == "bvp")
                return FormatEnum.BVP;
            else if (type == "hex")
                return FormatEnum.DAT;
            else
                return FormatEnum.Unknown;
        }
    }
}