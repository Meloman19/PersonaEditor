
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;

namespace TXTtoFNTMAP
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".txt")
                {
                   // List<PersonaEncoding.FnMpData> List = new List<PersonaEncoding.FnMpData>();


                }
            }
        }
    }
}
