using PersonaEditorLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PersonaEditorLib.FileStructure.PTP
{
    public static class Extension
    {
        public static List<TextBaseElement> GetTextBaseList(this string String, CharList FontMap)
        {
            List<TextBaseElement> MyByteArrayList = new List<TextBaseElement>();

            foreach (var a in Regex.Split(String, "(\r\n|\r|\n)"))
                if (Regex.IsMatch(a, "\r\n|\r|\n"))
                    MyByteArrayList.Add(new TextBaseElement("System", new byte[] { 0x0A }));
                else
                    foreach (var b in Regex.Split(a, @"({[^}]+})"))
                        if (Regex.IsMatch(b, @"{.+}"))
                            MyByteArrayList.Add(new TextBaseElement("System", Utilities.String.SplitString(b.Substring(1, b.Length - 2), ' ')));
                        else
                            MyByteArrayList.Add(new TextBaseElement("Text", FontMap.Encode(b, CharList.EncodeOptions.Bracket)));

            return MyByteArrayList;
        }

        public static List<TextBaseElement> GetTextBaseList(this byte[] array)
        {
            List<TextBaseElement> returned = new List<TextBaseElement>();

            string type = "Text";
            List<byte> temp = new List<byte>();

            for (int i = 0; i < array.Length; i++)
            {
                if (0x20 <= array[i] & array[i] < 0x80)
                {
                    temp.Add(array[i]);
                }
                else if (0x80 <= array[i] & array[i] < 0xF0)
                {
                    temp.Add(array[i]);
                    i = i + 1;
                    temp.Add(array[i]);
                }
                else
                {
                    if (0x00 <= array[i] & array[i] < 0x20)
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(new TextBaseElement(type, temp.ToArray()));
                            temp.Clear();
                        }

                        type = "System";
                        temp.Add(array[i]);

                        returned.Add(new TextBaseElement(type, temp.ToArray()));
                        type = "Text";
                        temp.Clear();
                    }
                    else
                    {
                        if (temp.Count != 0)
                        {
                            returned.Add(new TextBaseElement(type, temp.ToArray()));
                            type = "Text";
                            temp.Clear();
                        }


                        type = "System";
                        temp.Add(array[i]);
                        int count = (array[i] - 0xF0) * 2 - 1;
                        for (int k = 0; k < count; k++)
                        {
                            i++;
                            temp.Add(array[i]);
                        }

                        returned.Add(new TextBaseElement(type, temp.ToArray()));
                        type = "Text";
                        temp.Clear();
                    }
                }
            }

            if (temp.Count != 0)
            {
                returned.Add(new TextBaseElement(type, temp.ToArray()));
                temp.Clear();
            }


            return returned;
        }

        public static void ParseStrings(this IList<MSG.MSGstr> Strings, byte[] SourceBytes, CharList New)
        {
            Strings.Clear();

            int Index = 0;
            foreach (var Bytes in SplitSourceBytes(SourceBytes))
            {
                MSG.MSGstr MSG = new MSG.MSGstr(Index, "");

                List<TextBaseElement> temp = Bytes.GetTextBaseList();

                int tempdown = 0;
                int temptop = temp.Count;

                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i].Type == "System")
                        MSG.Prefix.Add(temp[i]);
                    else
                    {
                        tempdown = i;
                        i = temp.Count;
                    }
                }

                if (MSG.Prefix.Count < temp.Count)
                {
                    for (int i = temp.Count - 1; i >= tempdown; i--)
                    {
                        if (temp[i].Type == "System")
                            MSG.Postfix.Add(temp[i]);
                        else
                        {
                            temptop = i;
                            i = 0;
                        }
                    }

                    var temparray = MSG.Postfix.Reverse().ToList();

                    MSG.Postfix.Clear();
                    foreach (var a in temparray)
                        MSG.Postfix.Add(a);


                    for (int i = tempdown; i <= temptop; i++)
                        MSG.OldString.Add(temp[i]);
                }

                Strings.Add(MSG);
                Index++;
            }
        }

        public static string SplitByWidth(this string String, CharList FontMap, int width)
        {
            string returned = String.Join(" ", Regex.Split(String, @"\\n"));

            List<TextBaseElement> temp = returned.GetTextBaseList(FontMap);
            List<int> widthlist = new List<int>();

            foreach (var a in temp)
            {
                if (a.Type == "Text")
                    for (int i = 0; i < a.Array.Length; i++)
                    {
                        CharList.FnMpData fnmp = null;
                        if (a.Array[i] == 0x20)
                        {
                            widthlist.Add(9);
                            continue;
                        }
                        else if (0x20 < a.Array[i] & a.Array[i] < 0x80) fnmp = FontMap.List.FirstOrDefault(x => x.Index == a.Array[i]);
                        else if (0x80 <= a.Array[i] & a.Array[i] < 0xF0)
                        {
                            int newindex = (a.Array[i] - 0x81) * 0x80 + a.Array[i + 1] + 0x20;
                            i++;
                            fnmp = FontMap.List.FirstOrDefault(x => x.Index == newindex);
                        }

                        if (fnmp != null)
                        {
                            if (fnmp.Cut.Right - fnmp.Cut.Left > 0)
                                widthlist.Add(fnmp.Cut.Right - fnmp.Cut.Left - 1);
                            else
                                widthlist.Add(fnmp.Cut.Right - fnmp.Cut.Left);

                            if (fnmp.Char.Length > 1)
                            {
                                widthlist.AddRange(new int[2] { 0, 0 });
                                for (int k = 1; k < fnmp.Char.Length; k++)
                                    widthlist.Add(0);
                            }
                        }
                        else
                            widthlist.Add(0);
                    }
                else if (a.Type == "System")
                    widthlist.AddRange(new int[a.GetSystem().Length]);
            }

            int index = 0;
            int widthsum = 0;
            while (index < widthlist.Count)
            {
                if (widthsum + widthlist[index] <= width)
                {
                    widthsum += widthlist[index];
                    index++;
                }
                else
                {
                    bool te = true;
                    while (index != 0 & te)
                    {
                        if (widthlist[index - 1] != 0 & returned[index - 1] == ' ')
                        {
                            returned = returned.Insert(index, "\n");
                            widthlist.Insert(index, 0);
                            te = false;
                        }
                        index--;
                    }
                    widthsum = 0;
                }
            }

            return returned;
        }

        public static List<byte[]> SplitSourceBytes(this byte[] B)
        {
            List<byte[]> returned = new List<byte[]>();

            byte[] LineSplit = B.ToArray().Take((B[0] - 0xF0) * 2).ToArray();

            List<byte> String = new List<byte>();
            for (int i = 0; i < B.Length; i++)
            {
                if (B.CheckEntrance(LineSplit, i))
                {
                    if (String.Count != 0)
                    {
                        returned.Add(String.ToArray());
                        String.Clear();
                    }
                }

                String.Add(B[i]);
            }

            if (String.Count != 0)
            {
                returned.Add(String.ToArray());
                String.Clear();
            }

            return returned;
        }
    }
}