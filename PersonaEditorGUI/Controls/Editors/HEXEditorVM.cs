using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using System.IO;

namespace PersonaEditorGUI.Controls.Editors
{
    class HEXEditorVM : BindingObject
    {
        PersonaEditorLib.FileStructure.HEX hex;
        MemoryStream Worked;

        public HEXEditorVM(PersonaEditorLib.FileStructure.HEX hex)
        {
            this.hex = hex;
            Worked = new MemoryStream(hex.Data);
        }
    }
}