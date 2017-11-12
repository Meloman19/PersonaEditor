using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PersonaEditorLib.Extension;
using System.Text.RegularExpressions;

namespace PersonaEditorLib.FileStructure.PTP
{
    public delegate void MsgElementListChanged(IList<TextBaseElement> array);

    public interface ListMsg
    {
        event MsgElementListChanged Old;
        event MsgElementListChanged New;
    }

    public struct TextBaseElement
    {
        public TextBaseElement(string type, byte[] array)
        {
            Type = type;
            Array = array;
        }

        public string GetText(CharList CharList)
        {
            if (Type == "System")
                if (Array[0] == 0x0A)
                    return "\n";
                else
                    return GetSystem();
            else
                return CharList.Decode(Array);
        }

        public string GetSystem()
        {
            string returned = "";

            if (Array.Length > 0)
            {
                returned += "{" + Convert.ToString(Array[0], 16).PadLeft(2, '0').ToUpper();
                for (int i = 1; i < Array.Length; i++)
                    returned += " " + Convert.ToString(Array[i], 16).PadLeft(2, '0').ToUpper();

                returned += "}";
            }

            return returned;
        }

        public string Type { get; set; }
        public byte[] Array { get; set; }
    }

    public class PTP
    {
        public PTP(string OldFont, string OldMap, string NewFont, string NewMap)
        {
            OldCharList = new CharList(OldMap, new FNT.FNT(File.OpenRead(OldFont), 0));
            NewCharList = new CharList(NewMap, new FNT.FNT(File.OpenRead(NewFont), 0));
        }

        public PTP(CharList OldCharList, CharList NewCharList)
        {
            this.OldCharList = OldCharList;
            this.NewCharList = NewCharList;
        }

        public CharList OldCharList { get; set; }
        public CharList NewCharList { get; set; }

        public string OpenFileName = "";

        public class Names : ListMsg
        {
            public event MsgElementListChanged Old;
            public event MsgElementListChanged New;

            CharList NewChar;

            public Names(int Index, string OldName, string NewName, CharList New)
            {
                NewChar = New;
                this.Index = Index;
                this.NewName = NewName;
                this.OldName = Utilities.String.SplitString(OldName, '-');
            }

            public Names(int Index, byte[] OldName, string NewName, CharList New)
            {
                NewChar = New;
                this.Index = Index;
                this.NewName = NewName;
                this.OldName = OldName;
            }

            private byte[] _OldName;
            private string _NewName;

            public int Index { get; set; }
            public byte[] OldName
            {
                get { return _OldName; }
                set
                {
                    _OldName = value;
                    Old?.Invoke(_OldName.GetTextBaseList());
                }
            }
            public string NewName
            {
                get { return _NewName; }
                set
                {
                    _NewName = value;
                    New?.Invoke(_NewName.GetTextBaseList(NewChar));
                }
            }
        }

        public class MSG
        {
            public class MSGstr : INotifyPropertyChanged, ListMsg
            {
                public event MsgElementListChanged Old;
                public event MsgElementListChanged New;

                CharList NewChar;

                public MSGstr(int index, string newstring, CharList New)
                {
                    NewChar = New;
                    Index = index;
                    NewString = newstring;
                    OldString.ListChanged += OldString_ListChanged;

                }

                private void OldString_ListChanged(object sender, ListChangedEventArgs e)
                {
                    Old?.Invoke(OldString);
                    Notify("OldString");
                }

                #region INotifyPropertyChanged implementation
                public event PropertyChangedEventHandler PropertyChanged;

                protected void Notify(string propertyName)
                {
                    if (this.PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                }
                #endregion INotifyPropertyChanged implementation

                private string _NewString = "";

                public int Index { get; set; }
                public int CharacterIndex { get; set; }
                public BindingList<TextBaseElement> Prefix { get; set; } = new BindingList<TextBaseElement>();
                public BindingList<TextBaseElement> OldString { get; set; } = new BindingList<TextBaseElement>();
                public BindingList<TextBaseElement> Postfix { get; set; } = new BindingList<TextBaseElement>();
                public string NewString
                {
                    get { return _NewString; }
                    set
                    {
                        if (_NewString != value)
                        {
                            _NewString = value;
                            New?.Invoke(_NewString.GetTextBaseList(NewChar));
                            Notify("NewString");
                        }
                    }
                }
            }

            public MSG(int index, string type, string name, int charindex, string array)
            {
                Index = index;
                Type = type;
                Name = name;
                CharacterIndex = charindex;
                MsgBytes = Utilities.String.SplitString(array, '-');
            }

            public MSG(int index, string type, string name, int charindex, byte[] array)
            {
                Index = index;
                Type = type;
                Name = name;
                CharacterIndex = charindex;
                MsgBytes = array;
            }

            public int Index { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public int CharacterIndex { get; set; }
            public byte[] MsgBytes { get; set; }

            public BindingList<MSGstr> Strings { get; set; } = new BindingList<MSGstr>();
        }

        public BindingList<Names> names { get; set; } = new BindingList<Names>();
        public BindingList<MSG> msg { get; set; } = new BindingList<MSG>();

        public bool Open(string path)
        {
            try
            {
                names.Clear();
                msg.Clear();

                XDocument xDoc = XDocument.Load(path, LoadOptions.PreserveWhitespace);

                XElement MSG1Doc = xDoc.Element("MSG1");

                foreach (var NAME in MSG1Doc.Element("CharacterNames").Elements())
                {
                    int Index = Convert.ToInt32(NAME.Attribute("Index").Value);
                    string OldNameSource = NAME.Element("OldNameSource").Value;
                    string NewName = NAME.Element("NewName").Value;

                    names.Add(new Names(Index, OldNameSource, NewName, NewCharList));
                }

                foreach (var Message in MSG1Doc.Element("MSG").Elements())
                {
                    int Index = Convert.ToInt32(Message.Attribute("Index").Value);
                    string Type = Message.Element("Type").Value;
                    string Name = Message.Element("Name").Value;
                    int CharacterNameIndex = Convert.ToInt32(Message.Element("CharacterNameIndex").Value);

                    string SourceBytes_str = Message.Element("SourceBytes").Value;

                    MSG temp = new MSG(Index, Type, Name, CharacterNameIndex, SourceBytes_str);
                    msg.Add(temp);

                    foreach (var Strings in Message.Element("MessageStrings").Elements())
                    {
                        int StringIndex = Convert.ToInt32(Strings.Attribute("Index").Value);
                        string NewString = Strings.Element("NewString").Value;

                        MSG.MSGstr temp2 = new MSG.MSGstr(StringIndex, NewString, NewCharList) { CharacterIndex = CharacterNameIndex };
                        temp.Strings.Add(temp2);

                        foreach (var Prefix in Strings.Elements("PrefixBytes"))
                        {
                            int PrefixIndex = Convert.ToInt32(Prefix.Attribute("Index").Value);
                            string PrefixType = Prefix.Attribute("Type").Value;
                            string PrefixBytes = Prefix.Value;

                            temp2.Prefix.Add(new TextBaseElement(PrefixType, Utilities.String.SplitString(PrefixBytes, '-')));
                        }

                        foreach (var Old in Strings.Elements("OldStringBytes"))
                        {
                            int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
                            string OldType = Old.Attribute("Type").Value;
                            string OldBytes = Old.Value;

                            temp2.OldString.Add(new TextBaseElement(OldType, Utilities.String.SplitString(OldBytes, '-')));
                        }

                        foreach (var Postfix in Strings.Elements("PostfixBytes"))
                        {
                            int PostfixIndex = Convert.ToInt32(Postfix.Attribute("Index").Value);
                            string PostfixType = Postfix.Attribute("Type").Value;
                            string PostfixBytes = Postfix.Value;

                            temp2.Postfix.Add(new TextBaseElement(PostfixType, Utilities.String.SplitString(PostfixBytes, '-')));
                        }
                    }
                }

                OpenFileName = Path.GetFileName(Path.GetFullPath(path));
                return true;
            }
            catch (Exception e)
            {
                names.Clear();
                msg.Clear();
                OpenFileName = "";
                Logging.Write("PTPfactory", e.ToString());
                return false;
            }
        }

        public bool Open(BMD.BMD BMD, bool CopyOld2New)
        {
            try
            {
                names.Clear();
                msg.Clear();

                foreach (var NAME in BMD.name)
                {
                    int Index = NAME.Index;
                    byte[] OldNameSource = NAME.NameBytes;
                    string NewName = "";

                    names.Add(new Names(Index, OldNameSource, NewName, NewCharList));
                }

                foreach (var Message in BMD.msg)
                {
                    int Index = Message.Index;
                    string Type = Message.Type.ToString();
                    string Name = Message.Name;
                    int CharacterNameIndex = Message.CharacterIndex;
                    byte[] SourceBytes_str = Message.MsgBytes;
                    MSG temp = new MSG(Index, Type, Name, CharacterNameIndex, SourceBytes_str) { CharacterIndex = CharacterNameIndex };
                    temp.Strings.ParseStrings(SourceBytes_str, NewCharList);
                    msg.Add(temp);
                }

                if (CopyOld2New)
                    this.CopyOld2New();

                OpenFileName = Path.GetFileNameWithoutExtension(Path.GetFullPath(BMD.OpenFileName)) + ".PTP";
                return true;
            }
            catch (Exception e)
            {
                names.Clear();
                msg.Clear();
                OpenFileName = "";
                Logging.Write("PTPfactory", e.ToString());
                return false;
            }
        }

        public void CopyOld2New()
        {
            foreach (var Name in names)
                Name.NewName = Name.OldName.GetTextBaseList().GetString(OldCharList, false);

            foreach (var Msg in msg)
                foreach (var Str in Msg.Strings)
                    Str.NewString = Str.OldString.GetString(OldCharList, false);
        }

        private class LineMap
        {
            public enum Type
            {
                FileName,
                MSGindex,
                MSGname,
                StringIndex,
                OldText,
                NewText,
                OldName,
                NewName
            }

            Dictionary<Type, int> dic = new Dictionary<Type, int>();

            public LineMap(string map)
            {
                string[] temp = Regex.Split(map, " ");

                dic.Add(Type.FileName, Array.IndexOf(temp, "%FN"));
                dic.Add(Type.MSGindex, Array.IndexOf(temp, "%MSGIND"));
                dic.Add(Type.MSGname, Array.IndexOf(temp, "%MSGNM"));
                dic.Add(Type.StringIndex, Array.IndexOf(temp, "%STRIND"));
                dic.Add(Type.OldText, Array.IndexOf(temp, "%OLDSTR"));
                dic.Add(Type.NewText, Array.IndexOf(temp, "%NEWSTR"));
                dic.Add(Type.OldName, Array.IndexOf(temp, "%OLDNM"));
                dic.Add(Type.NewName, Array.IndexOf(temp, "%NEWNM"));
            }

            public List<Type> GetList()
            {
                return dic.Where(x => x.Value >= 0).OrderBy(x => x.Value).Select(x => x.Key).ToList();
            }

            public int this[Type type] { get { return dic[type]; } }

            public bool CanGetText
            {
                get { return dic[Type.FileName] >= 0 & (dic[Type.MSGindex] >= 0 | dic[Type.MSGname] >= 0) & dic[Type.StringIndex] >= 0 & dic[Type.NewText] >= 0; }
            }

            public bool CanGetName
            {
                get { return dic[Type.OldName] >= 0 & dic[Type.NewName] >= 0; }
            }

            public bool CanGetNameMSG
            {
                get { return dic[Type.FileName] >= 0 & (dic[Type.MSGindex] >= 0 | dic[Type.MSGname] >= 0) & dic[Type.NewName] >= 0; }
            }
        }

        public bool ExportTXT(string output, string map, bool removesplit)
        {
            var temp = new LineMap(map).GetList();

            List<string> list = new List<string>();
            foreach (var a in msg)
            {
                foreach (var b in a.Strings)
                {
                    string returned = "";

                    foreach (var type in temp)
                    {
                        if (type == LineMap.Type.FileName) returned += OpenFileName + "\t";
                        else if (type == LineMap.Type.MSGindex) returned += a.Index + "\t";
                        else if (type == LineMap.Type.MSGname) returned += a.Name + "\t";
                        else if (type == LineMap.Type.StringIndex) returned += b.Index + "\t";
                        else if (type == LineMap.Type.OldText) returned += removesplit ? b.OldString.GetString(OldCharList, false).Replace("\n", " ") + "\t" : b.OldString.GetString(OldCharList, false).Replace("\n", "\\n") + "\t";
                        else if (type == LineMap.Type.NewText) returned += removesplit ? b.NewString.Replace("\n", " ") + "\t" : b.NewString.Replace("\n", "\\n") + "\t";
                        else if (type == LineMap.Type.OldName)
                        {
                            var name = names.FirstOrDefault(x => x.Index == a.CharacterIndex);
                            returned += name == null ? " \t" : name.OldName.GetTextBaseList().GetString(OldCharList, false).Replace("\n", " ") + "\t";
                        }
                        else if (type == LineMap.Type.NewName)
                        {
                            var name = names.FirstOrDefault(x => x.Index == a.CharacterIndex);
                            returned += name == null ? " \t" : name.NewName.Replace("\n", " ") + "\t";
                        }
                    }

                    list.Add(returned);
                }
            }

            File.AppendAllLines(output, list);
            return true;
        }

        public void ImportTXT(string txtfile, string map, bool auto, int width, bool skip, Encoding encoding)
        {
            int Width = (int)Math.Round((double)width / 0.9375);
            LineMap MAP = new LineMap(map);

            if (MAP.CanGetText | MAP.CanGetName)
            {
                using (StreamReader SR = new StreamReader(File.OpenRead(txtfile), encoding))
                {
                    while (SR.EndOfStream == false)
                    {
                        string[] linespl = Regex.Split(SR.ReadLine(), "\t");

                        if (MAP.CanGetText)
                        {
                            if (OpenFileName.Equals(linespl[MAP[LineMap.Type.FileName]], StringComparison.OrdinalIgnoreCase))
                            {
                                string NewText = linespl[MAP[LineMap.Type.NewText]];

                                if (!(NewText == "" & skip))
                                {
                                    if (auto)
                                        NewText = NewText.SplitByWidth(NewCharList, Width);
                                    else
                                        NewText = NewText.Replace("\\n", "\n");

                                    if (MAP[LineMap.Type.MSGindex] >= 0)
                                    {
                                        ImportText(Convert.ToInt32(linespl[MAP[LineMap.Type.MSGindex]]), Convert.ToInt32(linespl[MAP[LineMap.Type.StringIndex]]), NewText);
                                        if (MAP.CanGetNameMSG)
                                            ImportNameByMSG(Convert.ToInt32(linespl[MAP[LineMap.Type.MSGindex]]), linespl[MAP[LineMap.Type.NewName]]);
                                    }
                                    else
                                    {
                                        ImportText(linespl[MAP[LineMap.Type.MSGname]], Convert.ToInt32(linespl[MAP[LineMap.Type.StringIndex]]), NewText);
                                        if (MAP.CanGetNameMSG)
                                            ImportNameByMSG(linespl[MAP[LineMap.Type.MSGname]], linespl[MAP[LineMap.Type.NewName]]);
                                    }
                                }
                            }
                        }
                        else if (MAP.CanGetName)
                            ImportNameByName(linespl[MAP[LineMap.Type.OldName]], linespl[MAP[LineMap.Type.NewName]]);
                    }
                }
            }
        }

        private void ImportText(string MSGName, int StringIndex, string Text)
        {
            ImportText(msg.FirstOrDefault(x => x.Name == MSGName), StringIndex, Text);
        }

        private void ImportText(int MSGIndex, int StringIndex, string Text)
        {
            ImportText(msg.FirstOrDefault(x => x.Index == MSGIndex), StringIndex, Text);
        }

        private void ImportText(MSG MSG, int StringIndex, string Text)
        {
            if (MSG != null)
            {
                var temp = MSG.Strings.FirstOrDefault(x => x.Index == StringIndex);
                if (temp != null)
                {
                    temp.NewString = Text;
                }
            }
        }

        private void ImportNameByMSG(int MSGIndex, string NewName)
        {
            MSG MSG = msg.FirstOrDefault(x => x.Index == MSGIndex);
            if (MSG != null)
                ImportName(names.FirstOrDefault(x => x.Index == MSG.CharacterIndex), NewName);
        }

        private void ImportNameByMSG(string MSGName, string NewName)
        {
            MSG MSG = msg.FirstOrDefault(x => x.Name == MSGName);
            if (MSG != null)
                ImportName(names.FirstOrDefault(x => x.Index == MSG.CharacterIndex), NewName);
        }

        private void ImportNameByName(string OldName, string NewName)
        {
            ImportName(names.FirstOrDefault(x => x.OldName.GetTextBaseList().GetString(OldCharList, true) == OldName), NewName);
        }

        private void ImportName(Names Name, string NewName)
        {
            if (Name != null)
                Name.NewName = NewName;
        }

        public void SaveProject(string path)
        {
            try
            {
                XDocument xDoc = new XDocument();
                XElement Document = new XElement("MSG1");
                xDoc.Add(Document);
                XElement CharName = new XElement("CharacterNames");
                Document.Add(CharName);

                foreach (var NAME in names)
                {
                    XElement Name = new XElement("Name");
                    Name.Add(new XAttribute("Index", NAME.Index));
                    Name.Add(new XElement("OldNameSource", BitConverter.ToString(NAME.OldName)));
                    Name.Add(new XElement("NewName", NAME.NewName));
                    CharName.Add(Name);
                }

                XElement MES = new XElement("MSG");
                Document.Add(MES);

                foreach (var MSG in msg)
                {
                    XElement Msg = new XElement("Message");
                    Msg.Add(new XAttribute("Index", MSG.Index));
                    Msg.Add(new XElement("Type", MSG.Type));
                    Msg.Add(new XElement("Name", MSG.Name));
                    Msg.Add(new XElement("CharacterNameIndex", MSG.CharacterIndex));
                    Msg.Add(new XElement("SourceBytes", BitConverter.ToString(MSG.MsgBytes)));

                    XElement Strings = new XElement("MessageStrings");
                    Msg.Add(Strings);
                    foreach (var STR in MSG.Strings)
                    {
                        XElement String = new XElement("String");
                        String.Add(new XAttribute("Index", STR.Index));
                        Strings.Add(String);

                        for (int i = 0; i < STR.Prefix.Count; i++)
                        {
                            XElement PrefixBytes = new XElement("PrefixBytes", BitConverter.ToString(STR.Prefix[i].Array));
                            PrefixBytes.Add(new XAttribute("Index", i));
                            PrefixBytes.Add(new XAttribute("Type", STR.Prefix[i].Type));
                            String.Add(PrefixBytes);
                        }

                        for (int i = 0; i < STR.OldString.Count; i++)
                        {
                            XElement OldStringBytes = new XElement("OldStringBytes", BitConverter.ToString(STR.OldString[i].Array));
                            OldStringBytes.Add(new XAttribute("Index", i));
                            OldStringBytes.Add(new XAttribute("Type", STR.OldString[i].Type));
                            String.Add(OldStringBytes);
                        }

                        String.Add(new XElement("NewString", STR.NewString));

                        for (int i = 0; i < STR.Postfix.Count; i++)
                        {
                            XElement PostfixBytes = new XElement("PostfixBytes", BitConverter.ToString(STR.Postfix[i].Array));
                            PostfixBytes.Add(new XAttribute("Index", i));
                            PostfixBytes.Add(new XAttribute("Type", STR.Postfix[i].Type));
                            String.Add(PostfixBytes);
                        }
                    }

                    MES.Add(Msg);
                }

                xDoc.Save(path);
            }
            catch (Exception e)
            {
                Logging.Write("PTPfactory", e.ToString());
            }
        }
    }
}