using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PersonaEditorLib.Extension;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorLib.FileStructure.Text
{
    public class StringList : IPersonaFile
    {
        private class Element
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

        List<Element> List = new List<Element>();

        public List<Tuple<string, int>> list = new List<Tuple<string, int>>();

        public Encoding DestEncoding { get; set; } = Encoding.ASCII;

        List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public StringList(string file, Encoding srcEncoding)
        {
            List<byte[]> List = SplitByNull(File.ReadAllBytes(file));
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

        public void ImportText(string[] text, string map, bool skipEmpty)
        {
            LineMap lineMap = new LineMap(map);

            if (lineMap.CanGetString)
            {
                List<string[]> list = new List<string[]>(text.Select(x => x.Split('\t')));

                foreach (var a in List)
                    //foreach (var b in list)
                    //    if (b.Length >= lineMap.MinLength)
                    //        if (b[lineMap[LineMap.Type.OldText]] == a.OldString)
                    //            if (b[lineMap[LineMap.Type.NewText]] == "")
                    //            {
                    //                if (!skipEmpty)
                    //                    a.NewString = b[lineMap[LineMap.Type.NewText]];
                    //            }
                    //            else
                    //            {
                    //                a.NewString = b[lineMap[LineMap.Type.NewText]];
                    //            }
                    a.NewString = list.Find(x =>
                    x.Length >= lineMap.MinLength &&
                    x[lineMap[LineMap.Type.OldText]] == a.OldString &&
                    (x[lineMap[LineMap.Type.NewText]] == "" ? (skipEmpty ? false : true) : true))?[lineMap[LineMap.Type.NewText]] ?? a.NewString;
            }
        }

        #region IPersonaFile

        public FileType Type => FileType.StringList;

        public List<ObjectFile> GetSubFiles()
        {
            return SubFiles;
        }

        public List<ContextMenuItems> ContextMenuList
        {
            get
            {
                List<ContextMenuItems> returned = new List<ContextMenuItems>();

                returned.Add(ContextMenuItems.SaveAs);
                returned.Add(ContextMenuItems.Replace);

                return returned;
            }
        }

        public Dictionary<string, object> GetProperties
        {
            get
            {
                Dictionary<string, object> returned = new Dictionary<string, object>();

                returned.Add("Size", Size);
                returned.Add("Type", Type);

                return returned;
            }
        }

        #endregion IPersonaFile

        #region IFile

        public int Size => List.Sum(x => x.Length);

        public byte[] Get()
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

                    if (index > 1)
                        Logging.Write("", "StringList: Max length reach for \"" + SelStr + "\"");

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

        #endregion IFile
    }
}