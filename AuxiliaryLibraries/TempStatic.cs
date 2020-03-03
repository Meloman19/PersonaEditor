using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AuxiliaryLibraries
{
    internal static class TempStatic
    {
        public static void ReadShift(this Dictionary<int, byte> list)
        {
            list.Add(81, 2);
            list.Add(103, 2);
            list.Add(106, 2);
            list.Add(112, 2);
            list.Add(113, 2);
            list.Add(121, 2);
        }

        public static byte[] GetBytes(this string str, int size)
        {
            List<byte> returned = new List<byte>();
            returned.AddRange(str.Select(x => Convert.ToByte(x)).ToArray());
            for (int i = returned.Count; i < size; i++)
                returned.Add(0);

            return returned.ToArray();
        }

        public static byte[] GetBytes(this string str)
        {
            return str.GetBytes(str.Length);
        }

        public static void Replace(string WRONG, string source, string newsource)
        {
            var wrong = File.ReadAllLines(WRONG).Select(x => x.Split('\t')).ToList();

            var src = File.ReadAllLines(source).Select(x => x.Split('\t')).ToList();

            List<string> newsrc = new List<string>();

            foreach (var a in src)
            {
                var temp = wrong.Find(x => x.Length > 1 && x[1].Equals(a[0]) && x[2].Equals(a[1]) && x[3].Equals(a[2]));
                if (temp != null)
                {
                    string wrongstring = temp[4];
                    string start = wrongstring.Split(new string[] { "{0A}" }, StringSplitOptions.None)[0];
                    newsrc.Add($"€{start}{{0A}}{a[5]}");
                }
                else
                {
                    newsrc.Add(a[5]);
                }
            }

            File.WriteAllLines(newsource, newsrc);
        }
    }
}