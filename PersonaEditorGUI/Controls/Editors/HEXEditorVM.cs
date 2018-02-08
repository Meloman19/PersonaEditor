using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using System.IO;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorGUI.Controls.Editors
{
    class HEXEditorVM : BindingObject, IViewModel
    {
        PersonaEditorLib.FileStructure.DAT hex;
        public MemoryStream WorkStream { get; set; }

        public HEXEditorVM(PersonaEditorLib.FileStructure.DAT hex)
        {
            this.hex = hex;
            WorkStream = new MemoryStream(hex.Data);
        }

        public bool Close()
        {
            return true;
        }
    }
}