using System.IO;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Other;
using PersonaEditor.Common;

namespace PersonaEditor.ViewModels.Editors
{
    class HEXEditorVM : BindingObject, IEditor
    {
        private Stream _memoryStream;

        public HEXEditorVM(DAT hex)
        {
            _memoryStream = new MemoryStream(hex.Data);
        }

        public Stream Stream
        {
            get => _memoryStream;
            set => SetProperty(ref _memoryStream, value);
        }

        public bool Close()
        {
            _memoryStream?.Close();
            return true;
        }
    }
}