using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PersonaEditorLib.Extension;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PersonaEditorLib.FileStructure
{
    public class StringList
    {
        List<string> list = new List<string>();

        public StringList(string file, int length, CharList charlist)
        {
            using (BinaryReader reader = Utilities.IO.OpenReadFile(file, true))
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                    list.Add(charlist.Decode(reader.ReadBytes(length).Where(x => x != 0).ToArray()));
        }

        public int Count => list.Count;

        public string this[int i]
        {
            get { return list[i]; }
            set { list[i] = value; }
        }

        public void Import(string path)
        {
            List<string[]> templist = new List<string[]>();

            using (StreamReader SR = new StreamReader(File.OpenRead(path)))
                while (SR.EndOfStream == false)
                    templist.Add(Regex.Split(SR.ReadLine(), "\t"));

            for (int i = 0; i < list.Count; i++)
            {
                string[] temp = templist.Find(x => x[0] == list[i]);
                if (temp != null)
                    if (temp.Length > 1)
                        list[i] = temp[1];
            }
        }

        public byte[] Get(int length, CharList charlist)
        {
            byte[] returned = null;

            using (BinaryWriter writer = Utilities.IO.OpenWriteFile(new MemoryStream(), true))
            {
                foreach (var a in list)
                {
                    byte[] temp;
                    int index = 0;
                    do
                    {
                        temp = charlist.Encode(a.Substring(0, a.Length - index));
                        index++;
                    } while (temp.Length > length);

                    if (temp.Length == 0)
                        temp = new byte[] { 0x32 };

                    writer.Write(temp);
                    writer.Write(new byte[Utilities.Utilities.Alignment(temp.Length, length)]);
                }

                writer.BaseStream.Position = 0;
                returned = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(returned, 0, returned.Length);
            }

            return returned;
        }
    }
}
