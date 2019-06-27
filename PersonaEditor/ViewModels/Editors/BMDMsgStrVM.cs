using PersonaEditor.Classes;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    class BMDMsgStrVM : BindingObject
    {
        int sourceFont;

        public byte[] data { get; private set; }

        public string Text { get; set; }

        // public FlowDocument Document { get; } = new FlowDocument();

        public void Changes(bool save, int destFont)
        {
            if (save)
                data = Text.GetTextBases(Static.EncodingManager.GetPersonaEncoding(sourceFont)).GetByteArray();
            else
            {
                Text = data.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
                Notify("Text");
            }
        }

        public void Update(int sourceFont)
        {
            this.sourceFont = sourceFont;
            Text = data.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            Notify("Text");
        }

        public BMDMsgStrVM(byte[] array, int sourceFont)
        {
            data = array;
            this.sourceFont = sourceFont;

            Text = data.GetTextBases().GetString(Static.EncodingManager.GetPersonaEncoding(sourceFont));
            //  Style style = new Style(typeof(Paragraph));
            //  style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            //  Document.Resources.Add(typeof(Paragraph), style);
            //  Document.Blocks.Add(data.GetTextBaseList().GetDocument(TestClass.personaEncoding, false));
        }
    }
}