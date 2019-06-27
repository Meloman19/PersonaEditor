using PersonaEditor.Classes;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    class BMDNameVM : BindingObject
    {
        BMDName name;
        int sourceFont;

        public int Index => name.Index;

        public string Name { get; set; }

        public void Changes(bool save, int destFont)
        {
            if (save)
                name.NameBytes = Static.EncodingManager.GetPersonaEncoding(destFont).GetBytes(Name);
            else
            {
                Name = name.NameBytes.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
                Notify("Name");
            }
        }

        public void Update(int sourceFont)
        {
            this.sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            Notify("Name");
        }

        public BMDNameVM(BMDName name, int sourceFont)
        {
            this.name = name;
            this.sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }
    }
}