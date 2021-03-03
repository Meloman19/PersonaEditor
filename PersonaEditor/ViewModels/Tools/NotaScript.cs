using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaEditor.ViewModels.Tools
{
    class NotaScript
    {
        
        public static List<string> Parse(string path, int rows)
        {
            try
            {


                string[] text = File.ReadAllLines(path);
                string[] names = File.ReadAllLines("names.txt");
                List<string> texts = new List<string>();

                foreach (var line in text)
                {

                    if (line.Length > 0 && line[0] != '=') //не название файла
                    {
                        if (line.Contains("Name №"))
                        {
                            string norm = "";
                            norm = line;


                            if (!line.Contains('\t'))
                            {
                                int twodots = line.IndexOf(@":");
                                norm = norm.Insert(twodots + 1, "\t");


                            }
                            texts.Add(norm);

                        }
                        else
                        {

                            int spaces = 0;
                            int nameStart = 0;
                            int nameLen = 0;

                            string norm = "";

                            foreach (var name in names)
                            {
                                if (line.Contains(name))
                                {
                                    nameLen = name.Length;
                                    nameStart = line.IndexOf(name);
                                    break;
                                }
                            }


                            int pos = 0;
                            foreach (var ch in line)
                            {
                                if (spaces <= rows)
                                {


                                    if (ch == ' ')
                                    {
                                        if (nameLen > 0)
                                        {


                                            if (pos < nameStart)
                                            {
                                                norm += "\t";
                                                spaces++;

                                            }
                                            else
                                            {
                                                if (pos == nameStart + nameLen)
                                                {
                                                    norm += "\t";
                                                }
                                                else
                                                    norm += ch;

                                            }
                                        }
                                        else
                                        {
                                            if (spaces < rows)
                                            {
                                                norm += "\t";

                                                spaces++;

                                            }
                                            else
                                            {
                                                norm += ch;

                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (ch == '\t')
                                        {
                                            spaces++;
                                        }
                                        norm += ch;
                                    }
                                }





                                pos++;
                            }



                            texts.Add(norm);
                        }


                    }


                }

                return texts;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }
    }
}
