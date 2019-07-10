using AuxiliaryLibraries.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.Text
{
    public class StringList : IGameData
    {
        public class Element
        {
            public int Length { get; set; } = 0;
            public string OldString { get; set; } = "";
            public string NewString { get; set; } = "";

            public Element(int length, string old)
            {
                Length = length;
                OldString = old;
            }
        }

        public List<Element> List { get; } = new List<Element>();

        public Encoding DestEncoding { get; set; } = Encoding.ASCII;

        public StringList(string file, Encoding srcEncoding) : this(File.ReadAllBytes(file), srcEncoding)
        {
        }

        public StringList(byte[] data, Encoding srcEncoding)
        {
            List<byte[]> List = SplitByNull(data);
            foreach (var a in List)
                this.List.Add(new Element(a.Length, srcEncoding.GetString(a.Where(x => x != 0).ToArray())));
        }

        private static List<byte[]> SplitByNull(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            List<byte[]> list = new List<byte[]>();

            bool Null = false;
            int start = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (Null)
                {
                    if (array[i] != 0)
                    {
                        Null = false;
                        list.Add(array.SubArray(start, i - start));
                        start = i;
                    }
                }
                else
                {
                    if (array[i] == 0)
                        Null = true;
                }
            }

            list.Add(array.SubArray(start, array.Length - start));
            return list;
        }

        public void ImportText(string[][] text)
        {
            if (text == null)
                return;

            foreach (var a in text)
            {
                var find = List.Find(x => x.OldString == a[0]);
                if (find != null)
                    find.NewString = a[1];
            }
        }

        public string[] ExportText()
        {
            return List.Select(x => x.OldString).ToArray();
        }

        #region IGameFile

        public FormatEnum Type => FormatEnum.StringList;

        public List<GameFile> SubFiles { get; } = new List<GameFile>();

        public int GetSize() => List.Sum(x => x.Length);

        public byte[] GetData()
        {
            using (MemoryStream MS = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(MS))
            {
                foreach (var a in List)
                {
                    string SelStr = a.NewString == "" ? a.OldString : a.NewString;

                    int length = 0;
                    int index = -1;
                    do
                    {
                        index++;
                        length = DestEncoding.GetByteCount(SelStr.Substring(0, SelStr.Length - index));
                    } while (length >= a.Length);

                    // if (index > 1)
                    //     Logging.Write("", "StringList: Max length reach for \"" + SelStr + "\"");

                    if (length == 0)
                    {
                        writer.Write((byte)0x32);
                        writer.Write(new byte[a.Length - 1]);
                    }
                    else
                    {
                        writer.Write(DestEncoding.GetBytes(SelStr.Substring(0, SelStr.Length - index)));
                        writer.Write(new byte[a.Length - length]);
                    }
                }

                return MS.ToArray();
            }
        }

        #endregion
    }
}