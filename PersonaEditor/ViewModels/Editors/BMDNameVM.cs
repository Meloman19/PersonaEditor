using PersonaEditor.Common;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class BMDNameVM : BindingObject
    {
        private readonly BMDName _bmdName;

        private int _sourceFont;
        private string _name;

        public BMDNameVM(BMDName name, int sourceFont)
        {
            _bmdName = name;
            _sourceFont = sourceFont;
            Name = name.NameBytes.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }

        public int Index => _bmdName.Index;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public void Changes(bool save, int destFont)
        {
            if (save)
                _bmdName.NameBytes = Static.EncodingManager.GetPersonaEncoding(destFont).GetBytes(Name);
            else
            {
                Name = _bmdName.NameBytes.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(_sourceFont));
            }
        }

        public void Update(int sourceFont)
        {
            _sourceFont = sourceFont;
            Name = _bmdName.NameBytes.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }
    }
}