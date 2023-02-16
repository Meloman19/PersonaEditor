using System.IO;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Other;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class HEXEditorVM : BindingObject, IEditor
    {
        public HEXEditorVM(DAT hex)
        {
            _memoryStream = new MemoryStream(hex.Data);
        }

        private Stream _memoryStream;
        public Stream Stream
        {
            get => _memoryStream;
            set
            {
                _memoryStream = value;
                Notify(nameof(Stream));
            }
        }

        public bool Close()
        {
            _memoryStream?.Close();
            return true;
        }
    }
}