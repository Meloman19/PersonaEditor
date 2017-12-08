using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using PersonaEditorLib.FileStructure.FNT;

namespace PersonaEditorGUI.Controls.Editors
{
    class FNTEditorVM : BindingObject
    {
        FNT fnt;

        public FNTEditorVM(FNT fnt)
        {
            this.fnt = fnt;
        }
    }
}