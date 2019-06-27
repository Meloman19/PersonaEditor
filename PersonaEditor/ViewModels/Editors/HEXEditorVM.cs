using System.IO;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Other;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class HEXEditorVM : BindingObject, IEditor
    {
        DAT hex;
        MemoryStream MemoryStream;

        public Controls.HexEditor.HexEditorUserControlVM HexEditorUserControlVM { get; } = new Controls.HexEditor.HexEditorUserControlVM();

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