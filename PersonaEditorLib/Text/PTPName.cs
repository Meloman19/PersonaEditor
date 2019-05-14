using AuxiliaryLibraries.Tools;

namespace PersonaEditorLib.Text
{
    public class PTPName
    {
        public PTPName(int index, string oldName, string newName)
        {
            Index = index;
            NewName = newName;
            OldName = StringTool.SplitString(oldName, '-');
        }

        public PTPName(int index, byte[] oldName, string newName)
        {
            Index = index;
            NewName = newName;
            OldName = oldName;
        }

        public PTPName() { }

        public int Index { get; set; }
        public byte[] OldName { get; set; }
        public string NewName { get; set; } = "";
    }
}