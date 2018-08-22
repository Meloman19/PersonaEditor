using AuxiliaryLibraries.GameFormat.Other.FNT;
using AuxiliaryLibraries.WPF;

namespace PersonaEditorGUI.Controls.Editors
{
    class FNTEditorVM : BindingObject, IViewModel
    {
        FNT fnt;

        public FNTEditorVM(FNT fnt)
        {
            this.fnt = fnt;
        }

        public bool Close()
        {
            return true;
        }
    }
}