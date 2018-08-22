using System.IO;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.GameFormat.Other;

namespace PersonaEditorGUI.Controls.Editors
{
    class HEXEditorVM : BindingObject, IViewModel
    {
        DAT hex;
        MemoryStream MemoryStream;

        public HexEditor.HexEditorUserControlVM HexEditorUserControlVM { get; } = new HexEditor.HexEditorUserControlVM();

        public HEXEditorVM(DAT hex)
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