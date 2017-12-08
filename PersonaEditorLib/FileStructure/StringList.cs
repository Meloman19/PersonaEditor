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
        public List<Tuple<string, int>> list = new List<Tuple<string, int>>();

        public StringList(string file, CharList charlist)
        {
            List<byte[]> splited = SplitByNull(File.ReadAllBytes(file));
            foreach (var a in splited)
                list.Add(new Tuple<string, int>(charlist.Decode(a.Where(x => x != 0).ToArray()), a.Length));
        }

        public List<byte[]> SplitByNull(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            List<byte[]> returned = new List<byte[]>();

            bool Null = false;
            int start = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (Null)
                {
                    if (array[i] != 0)
                    {
                        Null = false;
                        returned.Add(array.SubArray(start, i - start));
                        start = i;
                    }
                }
                else
                {
                    if (array[i] == 0)
                        Null = true;
                }
            }

            returned.Add(array.SubArray(start, array.Length - start));

            return returned;
        }

        public int Count => list.Count;

        public Tuple<string, int> this[int i]
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
                string[] temp = templist.Find(x => x[0] == list[i].Item1);
                if (temp != null)
                    if (temp.Length > 1)
                        if (temp[1].Length > 0)
                            list[i] = new Tuple<string, int>(temp[1], list[i].Item2);
            }
        }

        public byte[] Get(CharList charlist, int lengthoffset = 0)
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
                        temp = charlist.Encode(a.Item1.Substring(0, a.Item1.Length - index), CharList.EncodeOptions.OneChar);
                        index++;
                    } while (temp.Length > a.Item2 + lengthoffset);

                    if (temp.Length == 0)
                        temp = new byte[] { 0x32 };

                    if (index > 1)
                        Logging.Write("", "StringList: Max length reach for \"" + a.Item1 + "\"");

                    writer.Write(temp);
                    writer.Write(new byte[Utilities.Utilities.Alignment(temp.Length, a.Item2 + lengthoffset)]);
                }

                writer.BaseStream.Position = 0;
                returned = new byte[writer.BaseStream.Length];
                writer.BaseStream.Read(returned, 0, returned.Length);
            }

            return returned;
        }
    }
}