using System;
using System.Text;

namespace PersonaEditorCMD.ArgumentHandler
{
    public class Parameters
    {
        public string Map { get; } = "%FN %MSGIND %STRIND %I %I %NEWSTR";
        public int Width { get; } = 0;
        public bool RemoveSplit { get; } = false;
        public bool CopyOld2New { get; } = false;
        public int Length { get; } = 0;
        public bool Old { get; } = true;
        public bool SkipEmpty { get; } = false;
        public Encoding FileEncoding { get; } = Encoding.UTF8;
        public bool Sub { get; } = false;
        public int Size { get; } = -1;
        public bool AsBMD { get; } = false;
        public bool LineByLine { get; } = false;

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
                        FileEncoding = Encoding.UTF7;
                    if (a[1] == "UTF-16")
                        FileEncoding = Encoding.Unicode;
                    if (a[1] == "UTF-32")
                        FileEncoding = Encoding.UTF32;
                }
                else if (a[0] == "/sub")
                    Sub = true;
                else if (a[0] == "/size")
                    Size = int.Parse(a[1]);
                else if (a[0] == "/bmd")
                    AsBMD = true;
                else if (a[0] == "/lbl")
                    LineByLine = true;
            }
        }
    }
}