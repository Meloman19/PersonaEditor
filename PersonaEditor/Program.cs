using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using PersonaEditorLib.FileTypes;
using PersonaEditorLib.Extension;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PersonaEditor
{
    public static class FNTWork
    {
        public static bool Work(string[] args)
        {
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("-----Font decompressor/compressor by Meloman19-----");
            Console.WriteLine("-------------------Persona 3/4/5-------------------");
            Console.WriteLine("----------Based on RikuKH3's decompressor----------");
            Console.WriteLine("---------------------------------------------------");

            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    if (args.Length > 1)
                    {
                        if (args[1].ToLower() == "export")
                            if (args.Length > 3)
                                return Export(args[0], args[2], args[3]);
                            else return Export(args[0]);
                        else if (args[1].ToLower() == "import")
                            if (args.Length > 4)
                                return Import(args[0], args[2], args[3], args[4]);
                            else return Import(args[0]);

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Command not found.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("File " + args[0] + " not found.");
                    return false;
                }
            }
            else return false;
        }

        public static bool Export(string fontname)
        {
            string fullpath = Path.GetFullPath(fontname);

            string imagename = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + ".BMP";

            string wtname = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + ".XML";

            return Export(fontname, imagename, wtname);
        }

        public static bool Export(string fontname, string imagename, string wtname)
        {
            try
            {
                Text.FNTwork FONT0 = new Text.FNTwork(fontname);
                FONT0.WidthTable.WriteToFile(wtname);
                BitmapSource BS = FONT0.GetFontImage();
                Imaging.SaveBMP(BS, imagename);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static bool Import(string fontname)
        {
            string fullpath = Path.GetFullPath(fontname);

            string imagename = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + ".BMP";

            string wtname = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + ".XML";

            string newfontname = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + "_NEW.FNT";

            return Import(fontname, imagename, wtname, newfontname);
        }

        public static bool Import(string fontname, string imagename, string wtname, string newfontname)
        {
            try
            {
                Text.FNTwork FONT0 = new Text.FNTwork(fontname);
                FONT0.WidthTable.ReadFromFile(wtname);
                BitmapSource BS = Imaging.OpenImage(imagename);
                FONT0.SetFontImage(BS);
                FONT0.GetFont().SaveToFile(newfontname);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }

    public static class PMDWork
    {
        public static bool Work(string[] args)
        {
            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    if (args.Length > 2)
                    {
                        if (args[1].ToLower() == "export")
                            if (args[2].ToLower() == "msg")
                                if (args.Length > 3)
                                    return ExportMSG(args[0], args[3]);
                                else return ExportMSG(args[0]);
                            else return false;

                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Command not found.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("File " + args[0] + " not found.");
                    return false;
                }
            }
            else return false;
        }

        public static bool ExportMSG(string pmdname)
        {
            string fullpath = Path.GetFullPath(pmdname);

            string msgname = Path.GetDirectoryName(fullpath) + "\\" + Path.GetFileNameWithoutExtension(fullpath) + ".MSG";

            return ExportMSG(pmdname, msgname);
        }

        public static bool ExportMSG(string pmdname, string msgname)
        {
            try
            {
                Text.PMDwork PM1 = new Text.PMDwork(pmdname);
                PM1.GetMSG().SaveToFile(msgname);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool complete = false;

            if (args.Length > 0)
            {
                if (args[0].ToLower() == "fnt")
                    complete = FNTWork.Work(args.Skip(1).ToArray());

                else if (args[0].ToLower() == "pmd")
                {
                    complete = PMDWork.Work(args.Skip(1).ToArray());
                }
            }

            if (complete) Console.WriteLine("Success");
            else Console.WriteLine("Failure");

            Console.ReadKey();
        }
    }
}