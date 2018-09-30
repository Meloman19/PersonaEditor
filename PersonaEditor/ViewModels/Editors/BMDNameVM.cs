using PersonaEditor.Classes;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.GameFormat.Text;

namespace PersonaEditor.ViewModels.Editors
{
    class BMDNameVM : BindingObject
    {
        BMD.Names name;
        int sourceFont;

        public int Index => name.Index;

        public string Name { get; set; }

        public void Changes(bool save, int destFont)
        {
            if (save)
                name.NameBytes = Static.EncodingManager.GetPersonaEncoding(destFont).GetBytes(Name);
            else
            {
                Name = name.NameBytes.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
                Notify("Name");
            }
        }

        public void Update(int sourceFont)
        {
            this.sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            Notify("Name");
        }

        public BMDNameVM(BMD.Names name, int sourceFont)
        {
            this.name = name;
            this.sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBaseList().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }
    }
}