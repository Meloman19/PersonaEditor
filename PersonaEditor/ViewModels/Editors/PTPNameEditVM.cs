using System.Text;
using PersonaEditor.Classes.Visual;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class PTPNameEditVM : BindingObject
    {
        private PTPName name;

        private Encoding OldEncoding;
        private Encoding NewEncoding;

        public TextVisual OldNameVisual { get; } = new TextVisual();
        public TextVisual NewNameVisual { get; } = new TextVisual();

        public int Index => name.Index;
        public string OldName => name.OldName.GetTextBases().GetString(OldEncoding);
        public string NewName
        {
            get { return name.NewName; }
            set
            {
                if (name.NewName != value)
                {
                    name.NewName = value;
                    NewNameVisual.UpdateText(value.GetTextBases(NewEncoding));
                    Notify("NewName");
                }
            }
        }

        public void UpdateOldEncoding(string oldEncoding)
        {
            OldEncoding = Static.EncodingManager.GetPersonaEncoding(oldEncoding);
            OldNameVisual.UpdateFont(Static.FontManager.GetPersonaFont(oldEncoding));
            Notify("OldName");
        }

        public void UpdateNewEncoding(string newEncoding)
        {
            NewEncoding = Static.EncodingManager.GetPersonaEncoding(newEncoding);
            NewNameVisual.UpdateFont(Static.FontManager.GetPersonaFont(newEncoding));
        }

        public void UpdateBackground(int background)
        {
            if (Static.BackManager.GetBackground(background) is Background back)
            {
                OldNameVisual.Start = back.NameStart;
                NewNameVisual.Start = back.NameStart;

                OldNameVisual.Color = back.ColorName;
                NewNameVisual.Color = back.ColorName;

                OldNameVisual.LineSpacing = back.LineSpacing;
                NewNameVisual.LineSpacing = back.LineSpacing;

                OldNameVisual.GlyphScale = back.GlyphScale;
                NewNameVisual.GlyphScale = back.GlyphScale;
            }
        }

        public void UpdateView(bool view)
        {
            OldNameVisual.IsEnable = view;
            NewNameVisual.IsEnable = view;
        }

        public PTPNameEditVM(PTPName name, int oldEncoding, int newEncoding, int background)
        {
            this.name = name;
            UpdateOldEncoding(Static.EncodingManager.GetPersonaEncodingName(oldEncoding));
            UpdateNewEncoding(Static.EncodingManager.GetPersonaEncodingName(newEncoding));
            UpdateBackground(background);

            OldNameVisual.UpdateText(name.OldName);
            NewNameVisual.UpdateText(name.NewName.GetTextBases(NewEncoding));
        }
    }
}