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
        MemoryStream MemoryStream;

        public HexEditor.HexEditorUserControlVM HexEditorUserControlVM { get; } = new HexEditor.HexEditorUserControlVM();

        public HEXEditorVM(PersonaEditorLib.FileStructure.DAT hex)
        {
            this.hex = hex;
            MemoryStream = new MemoryStream(hex.Data);
            HexEditorUserControlVM.SetStream(MemoryStream);
        }

        public bool Close()
        {
            MemoryStream.Close();
            return true;
        }
    }
}