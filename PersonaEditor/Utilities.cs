using AuxiliaryLibraries.GameFormat.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PersonaEditor
{
    static class Utilities
    {
        public static void FindIncorrectString(string PTPdir, string txtFile)
        {
            string temp = PTPdir;
            byte[] searchArray = new byte[] { 0xF1, 0x25 };

            string[] Files = Directory.GetFiles(temp, "*.ptp", SearchOption.AllDirectories);

            List<string> returned = new List<string>();

            string DIRtemp = "";
            foreach (var file in Files)
            {
                PTP PTP = null;

                try { PTP = new PTP(File.ReadAllBytes(file)); }
                catch { continue; }

                foreach (var msg in PTP.msg)
                {
                    foreach (var str in msg.Strings)
                    {
                        var tempPrefix = str.Prefix.FirstOrDefault(x => x.Array.SequenceEqual(searchArray));
                        if (tempPrefix.Array != null)
                        {
                            string DIR = AuxiliaryLibraries.IO.IOTools.RelativePath(Path.GetDirectoryName(file), temp);
                            if (DIRtemp != DIR)
                            {
                                DIRtemp = DIR;
                                returned.Add("");
                            }
                            string FILE = Path.GetFileName(file);
                            string MSGINDEX = msg.Index.ToString();
                            string STRINGINDEX = str.Index.ToString();

                            returned.Add($"{DIR}\t{FILE}\t{MSGINDEX}\t{STRINGINDEX}\t{str.OldString.GetString(Program.Static.OldEncoding(), false).Replace('\n', ' ')}");
                        }
                    }
                }
            }

            File.WriteAllLines(txtFile, returned);
        }
    }
}