using PersonaEditor.Common;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    public sealed class BMDMsgStrVM : BindingObject
    {
        private int sourceFont;
        private string _text;

        public byte[] Data { get; private set; }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public void Changes(bool save, int destFont)
        {
            if (save)
                Data = Text.GetTextBases(Static.EncodingManager.GetPersonaEncoding(sourceFont)).GetByteArray();
            else
            {
                Text = Data.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            }
        }

        public void Update(int sourceFont)
        {
            this.sourceFont = sourceFont;
            Text = Data.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }

        public BMDMsgStrVM(byte[] array, int sourceFont)
        {
            Data = array;
            this.sourceFont = sourceFont;

            Text = Data.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
        }
    }
}