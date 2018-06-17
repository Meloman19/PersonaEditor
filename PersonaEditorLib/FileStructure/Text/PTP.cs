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
using PersonaEditorLib.Interfaces;
using System.Collections.ObjectModel;

namespace PersonaEditorLib.FileStructure.Text
{
    public struct TextBaseElement
    {
        public TextBaseElement(bool isText, byte[] array)
        {
            Array = array;
            IsText = isText;
        }

        public string GetText(Encoding encoding, bool linesplit = false)
        {
            if (IsText)
                return String.Concat(encoding.GetChars(Array));
            else
            {
                if (Array[0] == 0x0A)
                    if (linesplit)
                        return "\n";
                    else
                        return GetSystem();
                else
                    return GetSystem();
            }
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

        public bool IsText { get; set; }
        public byte[] Array { get; set; }
    }

    public class PTPName : BindingObject
    {
        public PTPName(int index, string oldName, string newName)
        {
            Index = index;
            NewName = newName;
            OldName = Utilities.String.SplitString(oldName, '-');
        }

        public PTPName(int index, byte[] oldName, string newName)
        {
            Index = index;
            NewName = newName;
            OldName = oldName;
        }

        public PTPName() { }

        private byte[] _OldName;
        private string _NewName;

        public int Index { get; set; }
        public byte[] OldName
        {
            get { return _OldName; }
            set
            {
                _OldName = value;
                Notify("OldName");
            }
        }
        public string NewName
        {
            get { return _NewName; }
            set
            {
                _NewName = value;
                Notify("NewName");
            }
        }
    }

    public class MSGstr : BindingObject
    {
        public MSGstr(int index, string newstring)
        {
            Index = index;
            NewString = newstring;
        }

        public MSGstr(int index, string newstring, byte[] Prefix, byte[] OldString, byte[] Postfix) : this(index, newstring)
        {
            List<TextBaseElement> temp = Prefix.GetTextBaseList();
            foreach (var a in temp)
                this.Prefix.Add(a);

            temp = OldString.GetTextBaseList();
            foreach (var a in temp)
                this.OldString.Add(a);

            temp = Postfix.GetTextBaseList();
            foreach (var a in temp)
                this.Postfix.Add(a);
        }

        private string _NewString = "";

        public byte[] GetOld()
        {
            List<byte> returned = new List<byte>();
            returned.AddRange(Prefix.GetByteArray());
            returned.AddRange(OldString.GetByteArray());
            returned.AddRange(Postfix.GetByteArray());
            return returned.ToArray();
        }

        public byte[] GetNew(Encoding New)
        {
            List<byte> returned = new List<byte>();
            returned.AddRange(Prefix.GetByteArray());
            returned.AddRange(NewString.GetTextBaseList(New).GetByteArray().ToArray());
            returned.AddRange(Postfix.GetByteArray());
            return returned.ToArray();
        }

        public void MovePrefixDown()
        {
            if (Prefix.Count == 0)
                return;

            var temp = Prefix[Prefix.Count - 1];
            Prefix.RemoveAt(Prefix.Count - 1);
            Notify("Prefix");

            OldString.Insert(0, temp);
            Notify("OldString");
        }

        public void MovePrefixUp()
        {
            if (OldString.Count == 0)
                return;

            var temp = OldString[0];

            if (temp.IsText)
                return;

            Prefix.Add(temp);
            Notify("Prefix");

            OldString.RemoveAt(0);
            Notify("OldString");
        }

        public void MovePostfixDown()
        {
            if (OldString.Count == 0)
                return;

            var temp = OldString[OldString.Count - 1];

            if (temp.IsText)
                return;

            Postfix.Insert(0, temp);
            Notify("Postfix");

            OldString.RemoveAt(OldString.Count - 1);
            Notify("OldString");
        }

        public void MovePostfixUp()
        {
            if (Postfix.Count == 0)
                return;

            var temp = Postfix[0];
            Postfix.RemoveAt(0);
            Notify("Postfix");

            OldString.Add(temp);
            Notify("OldString");
        }

        public int Index { get; set; }
        public int CharacterIndex { get; set; }
        public BindingList<TextBaseElement> Prefix { get; } = new BindingList<TextBaseElement>();
        public BindingList<TextBaseElement> OldString { get; } = new BindingList<TextBaseElement>();
        public BindingList<TextBaseElement> Postfix { get; } = new BindingList<TextBaseElement>();
        public string NewString
        {
            get { return _NewString; }
            set
            {
                if (_NewString != value)
                {
                    _NewString = value;
                    Notify("NewString");
                }
            }
        }
    }

    public class MSG : BindingObject
    {
        public MSG(int index, string type, string name, int charindex)
        {
            Index = index;
            Type = type;
            Name = name;
            CharacterIndex = charindex;
        }

        public byte[] GetOld()
        {
            List<byte> returned = new List<byte>();
            foreach (var a in Strings)
                returned.AddRange(a.GetOld());
            return returned.ToArray();
        }

        public byte[] GetNew(Encoding New)
        {
            List<byte> returned = new List<byte>();
            foreach (var a in Strings)
                returned.AddRange(a.GetNew(New));
            return returned.ToArray();
        }

        public int Index { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int CharacterIndex { get; set; }
        public byte[] MsgBytes { get; set; }

        public BindingList<MSGstr> Strings { get; } = new BindingList<MSGstr>();
    }

    public class PTP : IPersonaFile
    {
        public PTP(byte[] data)
        {
            using (MemoryStream MS = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(MS))
            {
                int ver = GetVersion(reader);
                if (ver == 0)
                    OpenPTP0(reader);
                else
                    OpenXmlPTP(MS);
            }
        }

        public PTP(BMD bmd)
        {
            foreach (var NAME in bmd.Name)
            {
                int Index = NAME.Index;
                byte[] OldNameSource = NAME.NameBytes;
                string NewName = "";

                names.Add(new PTPName(Index, OldNameSource, NewName));
            }

            foreach (var Message in bmd.Msg)
            {
                int Index = Message.Index;
                string Type = Message.Type.ToString();
                string Name = Message.Name;
                int CharacterNameIndex = Message.CharacterIndex;
                MSG temp = new MSG(Index, Type, Name, CharacterNameIndex);
                temp.Strings.ParseStrings(Message.MsgBytes);
                msg.Add(temp);
            }
        }

        public ObservableCollection<PTPName> names { get; } = new ObservableCollection<PTPName>();
        public ObservableCollection<MSG> msg { get; } = new ObservableCollection<MSG>();

        bool Open(Stream stream)
        {
            try
            {
                BinaryReader reader = new BinaryReader(stream);
                stream.Position = 0;
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) == "PTP0")
                {
                    names.Clear();
                    msg.Clear();

                    int MSGPos = reader.ReadInt32();
                    int MSGCount = reader.ReadInt32();
                    int NamesPos = reader.ReadInt32();
                    int NamesCount = reader.ReadInt32();

                    stream.Position = NamesPos;
                    for (int i = 0; i < NamesCount; i++)
                    {
                        int size = reader.ReadInt32();
                        byte[] OldName = reader.ReadBytes(size);
                        stream.Position += Utilities.UtilitiesTool.Alignment(stream.Position, 4);

                        size = reader.ReadInt32();
                        string NewName = Encoding.UTF8.GetString(reader.ReadBytes(size));
                        stream.Position += Utilities.UtilitiesTool.Alignment(stream.Position, 4);

                        names.Add(new PTPName(i, OldName, NewName));
                    }

                    stream.Position = MSGPos;
                    for (int i = 0; i < MSGCount; i++)
                    {
                        int type = reader.ReadInt32();
                        string Type = type == 0 ? "MSG" : "SEL";
                        byte[] buffer = reader.ReadBytes(24);
                        string Name = Encoding.ASCII.GetString(buffer.Where(x => x != 0).ToArray());
                        int StringCount = reader.ReadInt16();
                        int CharacterIndex = reader.ReadInt16();
                        MSG mSG = new MSG(i, Type, Name, CharacterIndex);

                        for (int k = 0; k < StringCount; k++)
                        {
                            int size = reader.ReadInt32();
                            byte[] Prefix = reader.ReadBytes(size);
                            stream.Position += Utilities.UtilitiesTool.Alignment(stream.Position, 4);

                            size = reader.ReadInt32();
                            byte[] OldString = reader.ReadBytes(size);
                            stream.Position += Utilities.UtilitiesTool.Alignment(stream.Position, 4);

                            size = reader.ReadInt32();
                            byte[] Postfix = reader.ReadBytes(size);
                            stream.Position += Utilities.UtilitiesTool.Alignment(stream.Position, 4);

                            size = reader.ReadInt32();
                            string NewString = Encoding.UTF8.GetString(reader.ReadBytes(size));
                            stream.Position += Utilities.UtilitiesTool.Alignment(stream.Position, 16);

                            mSG.Strings.Add(new MSGstr(k, NewString, Prefix, OldString, Postfix) { CharacterIndex = CharacterIndex });
                        }

                        msg.Add(mSG);
                    }

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                names.Clear();
                msg.Clear();
                Logging.Write("PTPfactory", e.ToString());
                return false;
            }
        }

        public void CopyOld2New(Encoding Old)
        {
            foreach (var Name in names)
                Name.NewName = Name.OldName.GetTextBaseList().GetString(Old, true);

            foreach (var Msg in msg)
                foreach (var Str in Msg.Strings)
                    Str.NewString = Str.OldString.GetString(Old, true);
        }

        public void OpenPTP0(BinaryReader reader)
        {
            reader.ReadInt32();
            int MSGPos = reader.ReadInt32();
            int MSGCount = reader.ReadInt32();
            int NamesPos = reader.ReadInt32();
            int NamesCount = reader.ReadInt32();

            reader.BaseStream.Position = NamesPos;
            for (int i = 0; i < NamesCount; i++)
            {
                int size = reader.ReadInt32();
                byte[] OldName = reader.ReadBytes(size);
                reader.BaseStream.Position += Utilities.UtilitiesTool.Alignment(reader.BaseStream.Position, 4);

                size = reader.ReadInt32();
                string NewName = Encoding.UTF8.GetString(reader.ReadBytes(size));
                reader.BaseStream.Position += Utilities.UtilitiesTool.Alignment(reader.BaseStream.Position, 4);

                names.Add(new PTPName(i, OldName, NewName));
            }

            reader.BaseStream.Position = MSGPos;
            for (int i = 0; i < MSGCount; i++)
            {
                int type = reader.ReadInt32();
                string Type = type == 0 ? "MSG" : "SEL";
                byte[] buffer = reader.ReadBytes(24);
                string Name = Encoding.ASCII.GetString(buffer.Where(x => x != 0).ToArray());
                int StringCount = reader.ReadInt16();
                int CharacterIndex = reader.ReadInt16();
                MSG mSG = new MSG(i, Type, Name, CharacterIndex);

                for (int k = 0; k < StringCount; k++)
                {
                    int size = reader.ReadInt32();
                    byte[] Prefix = reader.ReadBytes(size);
                    reader.BaseStream.Position += Utilities.UtilitiesTool.Alignment(reader.BaseStream.Position, 4);

                    size = reader.ReadInt32();
                    byte[] OldString = reader.ReadBytes(size);
                    reader.BaseStream.Position += Utilities.UtilitiesTool.Alignment(reader.BaseStream.Position, 4);

                    size = reader.ReadInt32();
                    byte[] Postfix = reader.ReadBytes(size);
                    reader.BaseStream.Position += Utilities.UtilitiesTool.Alignment(reader.BaseStream.Position, 4);

                    size = reader.ReadInt32();
                    string NewString = Encoding.UTF8.GetString(reader.ReadBytes(size));
                    reader.BaseStream.Position += Utilities.UtilitiesTool.Alignment(reader.BaseStream.Position, 16);

                    mSG.Strings.Add(new MSGstr(k, NewString, Prefix, OldString, Postfix) { CharacterIndex = CharacterIndex });
                }

                msg.Add(mSG);
            }
        }

        public void OpenXmlPTP(Stream stream)
        {
            XDocument xDoc = XDocument.Load(stream, LoadOptions.PreserveWhitespace);

            XElement MSG1Doc = xDoc.Element("MSG1");

            foreach (var NAME in MSG1Doc.Element("CharacterNames").Elements())
            {
                int Index = Convert.ToInt32(NAME.Attribute("Index").Value);
                string OldNameSource = NAME.Element("OldNameSource").Value;
                string NewName = NAME.Element("NewName").Value;

                names.Add(new PTPName(Index, OldNameSource, NewName));
            }

            foreach (var Message in MSG1Doc.Element("MSG").Elements())
            {
                int Index = Convert.ToInt32(Message.Attribute("Index").Value);
                string Type = Message.Element("Type").Value;
                string Name = Message.Element("Name").Value;
                int CharacterNameIndex = Convert.ToInt32(Message.Element("CharacterNameIndex").Value);

                MSG temp = new MSG(Index, Type, Name, CharacterNameIndex);
                msg.Add(temp);

                foreach (var Strings in Message.Element("MessageStrings").Elements())
                {
                    int StringIndex = Convert.ToInt32(Strings.Attribute("Index").Value);
                    string NewString = Strings.Element("NewString").Value;

                    MSGstr temp2 = new MSGstr(StringIndex, NewString) { CharacterIndex = CharacterNameIndex };
                    temp.Strings.Add(temp2);

                    foreach (var Prefix in Strings.Elements("PrefixBytes"))
                    {
                        int PrefixIndex = Convert.ToInt32(Prefix.Attribute("Index").Value);
                        string PrefixType = Prefix.Attribute("Type").Value;
                        string PrefixBytes = Prefix.Value;

                        temp2.Prefix.Add(new TextBaseElement(PrefixType == "Text" ? true : false, PersonaEditorLib.Utilities.String.SplitString(PrefixBytes, '-')));
                    }

                    foreach (var Old in Strings.Elements("OldStringBytes"))
                    {
                        int OldIndex = Convert.ToInt32(Old.Attribute("Index").Value);
                        string OldType = Old.Attribute("Type").Value;
                        string OldBytes = Old.Value;

                        temp2.OldString.Add(new TextBaseElement(OldType == "Text" ? true : false, PersonaEditorLib.Utilities.String.SplitString(OldBytes, '-')));
                    }

                    foreach (var Postfix in Strings.Elements("PostfixBytes"))
                    {
                        int PostfixIndex = Convert.ToInt32(Postfix.Attribute("Index").Value);
                        string PostfixType = Postfix.Attribute("Type").Value;
                        string PostfixBytes = Postfix.Value;

                        temp2.Postfix.Add(new TextBaseElement(PostfixType == "Text" ? true : false, PersonaEditorLib.Utilities.String.SplitString(PostfixBytes, '-')));
                    }
                }
            }
        }

        public static int GetVersion(BinaryReader reader)
        {
            long temp = reader.BaseStream.Position;
            reader.BaseStream.Position = 0;
            byte[] buffer = reader.ReadBytes(4);
            reader.BaseStream.Position = temp;
            if (buffer.SequenceEqual(new byte[4] { 0x50, 0x54, 0x50, 0x30 }))
                return 0;
            else
                return -1;
        }

        public static void SaveOldPTP(PTP ptp, string path)
        {
            XDocument xDoc = new XDocument();
            XElement Document = new XElement("MSG1");
            xDoc.Add(Document);
            XElement CharName = new XElement("CharacterNames");
            Document.Add(CharName);

            foreach (var NAME in ptp.names)
            {
                XElement Name = new XElement("Name");
                Name.Add(new XAttribute("Index", NAME.Index));
                Name.Add(new XElement("OldNameSource", BitConverter.ToString(NAME.OldName)));
                Name.Add(new XElement("NewName", NAME.NewName));
                CharName.Add(Name);
            }

            XElement MES = new XElement("MSG");
            Document.Add(MES);

            foreach (var MSG in ptp.msg)
            {
                XElement Msg = new XElement("Message");
                Msg.Add(new XAttribute("Index", MSG.Index));
                Msg.Add(new XElement("Type", MSG.Type));
                Msg.Add(new XElement("Name", MSG.Name));
                Msg.Add(new XElement("CharacterNameIndex", MSG.CharacterIndex));

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
                        PrefixBytes.Add(new XAttribute("Type", STR.Prefix[i].IsText ? "Text" : "System"));
                        String.Add(PrefixBytes);
                    }

                    for (int i = 0; i < STR.OldString.Count; i++)
                    {
                        XElement OldStringBytes = new XElement("OldStringBytes", BitConverter.ToString(STR.OldString[i].Array));
                        OldStringBytes.Add(new XAttribute("Index", i));
                        OldStringBytes.Add(new XAttribute("Type", STR.OldString[i].IsText ? "Text" : "System"));
                        String.Add(OldStringBytes);
                    }

                    String.Add(new XElement("NewString", STR.NewString));

                    for (int i = 0; i < STR.Postfix.Count; i++)
                    {
                        XElement PostfixBytes = new XElement("PostfixBytes", BitConverter.ToString(STR.Postfix[i].Array));
                        PostfixBytes.Add(new XAttribute("Index", i));
                        PostfixBytes.Add(new XAttribute("Type", STR.Postfix[i].IsText ? "Text" : "System"));
                        String.Add(PostfixBytes);
                    }
                }

                MES.Add(Msg);
            }

            xDoc.Save(path);
        }

        public string[] ExportTXT(string map, string PTPname, bool removesplit, Encoding Old, Encoding New)
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
                        if (type == LineMap.Type.FileName) returned += PTPname + "\t";
                        else if (type == LineMap.Type.MSGindex) returned += a.Index + "\t";
                        else if (type == LineMap.Type.MSGname) returned += a.Name + "\t";
                        else if (type == LineMap.Type.StringIndex) returned += b.Index + "\t";
                        else if (type == LineMap.Type.OldText) returned += removesplit ? b.OldString.GetString(Old, removesplit).Replace("\n", " ") + "\t" : b.OldString.GetString(Old, removesplit).Replace("\n", "\\n") + "\t";
                        else if (type == LineMap.Type.NewText) returned += removesplit ? b.NewString.Replace("\n", " ") + "\t" : b.NewString.Replace("\n", "\\n") + "\t";
                        else if (type == LineMap.Type.OldName)
                        {
                            if (a.Type == "SEL")
                                returned += "<SELECT>\t";
                            else
                            {
                                var name = names.FirstOrDefault(x => x.Index == a.CharacterIndex);
                                returned += name == null ? " \t" : name.OldName.GetTextBaseList().GetString(Old, false).Replace("\n", " ") + "\t";
                            }
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

            return list.ToArray();
        }

        public void ImportTXT(string[] text, string PTPname, string map, int width, bool skipEmpty, Encoding oldEncoding, Encoding newEncoding, PersonaEncoding.PersonaFont newFont)
        {
            int Width = (int)Math.Round((double)width / 0.9375);
            LineMap MAP = new LineMap(map);

            if (MAP.CanGetText | MAP.CanGetName)
            {
                foreach (var line in text)
                {
                    string[] linespl = Regex.Split(line, "\t");

                    if (MAP.CanGetText)
                    {
                        if (PTPname.Equals(linespl[MAP[LineMap.Type.FileName]], StringComparison.OrdinalIgnoreCase))
                        {
                            string NewText = linespl[MAP[LineMap.Type.NewText]];

                            if (!(NewText == "" & skipEmpty))
                            {
                                if (Width > 0)
                                    NewText = NewText.SplitByWidth(newEncoding, newFont, Width);
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
                        ImportNameByName(linespl[MAP[LineMap.Type.OldName]], linespl[MAP[LineMap.Type.NewName]], oldEncoding);
                }

            }
        }

        public void ImportTXT_LBL(string[] text, string map, int width, bool skipEmpty, Encoding oldEncoding, Encoding newEncoding, PersonaEncoding.PersonaFont newFont)
        {
            int Width = (int)Math.Round((double)width / 0.9375);
            LineMap MAP = new LineMap(map);

            int index = 0;

            foreach (var a in msg)
                foreach (var b in a.Strings)
                {
                    if (index >= text.Length)
                        return;

                    string[] linespl = Regex.Split(text[index], "\t");
                    index++;

                    if (MAP[LineMap.Type.NewText] < linespl.Length)
                    {
                        string NewText = linespl[MAP[LineMap.Type.NewText]];

                        if (!(NewText == "" & skipEmpty))
                        {
                            if (Width > 0)
                                NewText = NewText.SplitByWidth(newEncoding, newFont, Width);
                            else
                                NewText = NewText.Replace("\\n", "\n");

                            b.NewString = NewText;
                        }
                    }

                    if (MAP[LineMap.Type.NewName] < linespl.Length)
                    {
                        string NewName = linespl[MAP[LineMap.Type.NewName]];

                        var name = names.FirstOrDefault(x => x.Index == a.CharacterIndex);
                        if (name != null)
                        {
                            name.NewName = NewName;
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

        private void ImportNameByName(string OldName, string NewName, Encoding Old)
        {
            ImportName(names.FirstOrDefault(x => x.OldName.GetTextBaseList().GetString(Old, true) == OldName), NewName);
        }

        private void ImportName(PTPName Name, string NewName)
        {
            if (Name != null)
                Name.NewName = NewName;
        }

        #region IPersonaFile

        public FileType Type => FileType.PTP;

        public List<ObjectFile> SubFiles { get; } = new List<ObjectFile>();

        public ReadOnlyObservableCollection<PropertyClass> GetProperties => null;

        #endregion IPersonaFile

        #region IFile

        public int Size() => Get().Length;

        public byte[] Get()
        {
            using (MemoryStream MS = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(MS))
                {
                    byte[] buffer;
                    writer.Write(Encoding.ASCII.GetBytes("PTP0"));
                    long MSGLinkPos = MS.Position;
                    writer.Write(0);
                    writer.Write(msg.Count);
                    long NamesLinkPos = MS.Position;
                    writer.Write(0);
                    writer.Write(names.Count);
                    writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 0x10)]);

                    long MSGPos = MS.Position;
                    foreach (var a in msg)
                    {
                        writer.Write(a.Type == "MSG" ? 0 : 1);
                        buffer = new byte[24];
                        Encoding.ASCII.GetBytes(a.Name, 0, a.Name.Length, buffer, 0);
                        writer.Write(buffer);
                        writer.Write((ushort)a.Strings.Count);
                        writer.Write((ushort)a.CharacterIndex);

                        foreach (var b in a.Strings)
                        {
                            buffer = b.Prefix.GetByteArray();
                            writer.Write(buffer.Length);
                            writer.Write(buffer);
                            writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 4)]);

                            buffer = b.OldString.GetByteArray();
                            writer.Write(buffer.Length);
                            writer.Write(buffer);
                            writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 4)]);

                            buffer = b.Postfix.GetByteArray();
                            writer.Write(buffer.Length);
                            writer.Write(buffer);
                            writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 4)]);

                            buffer = Encoding.UTF8.GetBytes(b.NewString);
                            writer.Write(buffer.Length);
                            writer.Write(buffer);
                            writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 16)]);
                        }

                        writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 0x10)]);
                    }

                    long NamesPos = MS.Position;
                    foreach (var a in names)
                    {
                        buffer = a.OldName;
                        writer.Write(buffer.Length);
                        writer.Write(buffer);
                        writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 4)]);

                        buffer = Encoding.UTF8.GetBytes(a.NewName);
                        writer.Write(buffer.Length);
                        writer.Write(buffer);
                        writer.Write(new byte[Utilities.UtilitiesTool.Alignment(MS.Position, 4)]);
                    }

                    MS.Position = MSGLinkPos;
                    writer.Write((int)MSGPos);
                    MS.Position = NamesLinkPos;
                    writer.Write((int)NamesPos);
                }
                return MS.ToArray();
            }
        }

        #endregion IFile        
    }
}