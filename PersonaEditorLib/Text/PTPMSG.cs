using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PersonaEditorLib.Text
{
    public class PTPMSG
    {
        public PTPMSG(int index, int type, string name, int charindex)
        {
            Index = index;
            Type = type;
            Name = name;
            CharacterIndex = charindex;
        }

        public PTPMSG() { }

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
        public int Type { get; set; }
        public string Name { get; set; }
        public int CharacterIndex { get; set; }
        public byte[] MsgBytes { get; set; }

        public BindingList<PTPMSGstr> Strings { get; } = new BindingList<PTPMSGstr>();
    }
}