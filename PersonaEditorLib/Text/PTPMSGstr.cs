using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PersonaEditorLib.Text
{
    public class PTPMSGstr
    {
        public PTPMSGstr(int index, string newstring)
        {
            Index = index;
            NewString = newstring;
        }

        public PTPMSGstr(int index, string newstring, byte[] Prefix, byte[] OldString, byte[] Postfix) : this(index, newstring)
        {
            foreach (var a in Prefix.GetTextBases())
                this.Prefix.Add(a);

            foreach (var a in OldString.GetTextBases())
                this.OldString.Add(a);

            foreach (var a in Postfix.GetTextBases())
                this.Postfix.Add(a);
        }

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
            returned.AddRange(NewString.GetTextBases(New).GetByteArray().ToArray());
            returned.AddRange(Postfix.GetByteArray());
            return returned.ToArray();
        }

        public bool MovePrefixDown()
        {
            if (Prefix.Count == 0)
                return false;

            var temp = Prefix[Prefix.Count - 1];
            Prefix.RemoveAt(Prefix.Count - 1);
            OldString.Insert(0, temp);
            return true;
        }

        public bool MovePrefixUp()
        {
            if (OldString.Count == 0)
                return false;

            var temp = OldString[0];

            if (temp.IsText)
                return false;

            Prefix.Add(temp);
            OldString.RemoveAt(0);
            return true;
        }

        public bool MovePostfixDown()
        {
            if (OldString.Count == 0)
                return false;

            var temp = OldString[OldString.Count - 1];

            if (temp.IsText)
                return false;

            Postfix.Insert(0, temp);
            OldString.RemoveAt(OldString.Count - 1);
            return true;
        }

        public bool MovePostfixUp()
        {
            if (Postfix.Count == 0)
                return false;

            var temp = Postfix[0];
            Postfix.RemoveAt(0);

            OldString.Add(temp);
            return true;
        }

        public int Index { get; set; }
        public int CharacterIndex { get; set; }
        public BindingList<TextBaseElement> Prefix { get; } = new BindingList<TextBaseElement>();
        public BindingList<TextBaseElement> OldString { get; } = new BindingList<TextBaseElement>();
        public BindingList<TextBaseElement> Postfix { get; } = new BindingList<TextBaseElement>();
        public string NewString { get; set; } = "";
    }
}