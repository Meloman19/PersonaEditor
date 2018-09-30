using AuxiliaryLibraries.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonaEditorCMD.ArgumentHandler
{
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
                OpenedFileDir = Path.GetDirectoryName(OpenedFile);

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