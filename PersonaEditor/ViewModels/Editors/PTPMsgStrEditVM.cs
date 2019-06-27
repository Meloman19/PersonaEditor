using System;
using System.Text;
using PersonaEditor.Classes.Visual;
using System.Windows.Media;
using System.Windows.Input;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;
using PersonaEditor.Classes;

namespace PersonaEditor.ViewModels.Editors
{
    class PTPMsgStrEditVM : BindingObject
    {
        PTPMSGstr str;

        private Encoding OldEncoding;
        private Encoding NewEncoding;

        TextVisual OldText = new TextVisual();
        TextVisual NewText = new TextVisual();

        public string Prefix => str.Prefix.MSGListToSystem();
        public string Postfix => str.Postfix.MSGListToSystem();
        public string OldString => str.OldString.GetString((OldEncoding), true);
        public string NewString
        {
            get { return str.NewString; }
            set
            {
                str.NewString = value;
                NewText.UpdateText(str.NewString.GetTextBases(NewEncoding));
            }
        }

        public DrawingImage OldTextImage { get; } = new DrawingImage();
        public DrawingImage NewTextImage { get; } = new DrawingImage();

        #region Command

        public ICommand MovePrefixDown { get; }
        public ICommand MovePrefixUp { get; }
        public ICommand MovePostfixDown { get; }
        public ICommand MovePostfixUp { get; }

        private void movePrefixDown()
        {
            if (str.MovePrefixDown())
            {
                Notify("Prefix");
                Notify("OldString");
            }
        }
        private void movePrefixUp()
        {
            if (str.MovePrefixUp())
            {
                Notify("Prefix");
                Notify("OldString");
            }
        }
        private void movePostfixDown()
        {
            if (str.MovePostfixDown())
            {
                Notify("Postfix");
                Notify("OldString");
            }
        }
        private void movePostfixUp()
        {
            if (str.MovePostfixUp())
            {
                Notify("Postfix");
                Notify("OldString");
            }
        }

        #endregion

        public void UpdateOldEncoding(string oldEncoding)
        {
            OldEncoding = Static.EncodingManager.GetPersonaEncoding(oldEncoding);
            OldText.UpdateFont(Static.FontManager.GetPersonaFont(oldEncoding));
            Notify("OldString");
        }

        public void UpdateNewEncoding(string newEncoding)
        {
            NewEncoding = Static.EncodingManager.GetPersonaEncoding(newEncoding);
            NewText.UpdateText(str.NewString.GetTextBases(NewEncoding), Static.FontManager.GetPersonaFont(newEncoding));
        }

        public void UpdateBackground(int backgroundIndex)
        {
            if (Static.BackManager.GetBackground(backgroundIndex) is Background back)
            {
                OldText.Start = back.TextStart;
                NewText.Start = back.TextStart;

                OldText.Color = back.ColorText;
                NewText.Color = back.ColorText;

                OldText.LineSpacing = back.LineSpacing;
                NewText.LineSpacing = back.LineSpacing;

                OldText.GlyphScale = back.GlyphScale;
                NewText.GlyphScale = back.GlyphScale;
            }
        }

        public void UpdateView(bool view)
        {
            OldText.IsEnable = view;
            NewText.IsEnable = view;
        }

        public PTPMsgStrEditVM(PTPMSGstr str, Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry> tuple, string oldEncoding, string newEncoding, int backgroundIndex)
        {
            this.str = str;
            
            OldEncoding = Static.EncodingManager.GetPersonaEncoding(oldEncoding);
            NewEncoding = Static.EncodingManager.GetPersonaEncoding(newEncoding);
            OldText = new TextVisual(Static.FontManager.GetPersonaFont(oldEncoding)) { Tag = "Old" };
            NewText = new TextVisual(Static.FontManager.GetPersonaFont(newEncoding)) { Tag = "New" };
            OldText.IsEnable = ApplicationSettings.AppSetting.Default.PTPImageView;
            NewText.IsEnable = ApplicationSettings.AppSetting.Default.PTPImageView;

            UpdateBackground(backgroundIndex);

            OldText.UpdateText(str.OldString);
            NewText.UpdateText(str.NewString.GetTextBases(NewEncoding));

            DrawingGroup oldDrawingGroup = new DrawingGroup();
            oldDrawingGroup.Children.Add(tuple.Item3);
            if (tuple.Item1 != null)
                oldDrawingGroup.Children.Add(tuple.Item1);
            oldDrawingGroup.Children.Add(OldText.TextDrawing);
            oldDrawingGroup.ClipGeometry = tuple.Item4;
            OldTextImage.Drawing = oldDrawingGroup;

            DrawingGroup newDrawingGroup = new DrawingGroup();
            newDrawingGroup.Children.Add(tuple.Item3);
            if (tuple.Item2 != null)
                newDrawingGroup.Children.Add(tuple.Item2);
            newDrawingGroup.Children.Add(NewText.TextDrawing);
            newDrawingGroup.ClipGeometry = tuple.Item4;
            NewTextImage.Drawing = newDrawingGroup;

            MovePrefixDown = new RelayCommand(movePrefixDown);
            MovePrefixUp = new RelayCommand(movePrefixUp);
            MovePostfixDown = new RelayCommand(movePostfixDown);
            MovePostfixUp = new RelayCommand(movePostfixUp);
        }
    }
}