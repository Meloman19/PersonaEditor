using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using PersonaEditor.Classes.Visual;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib;
using PersonaEditorLib.Text;
using PersonaEditor.Classes;
using PersonaEditor.Classes.Managers;

namespace PersonaEditor.ViewModels.Tools
{
    class VisualizerVM : BindingObject
    {
        #region PrivateField

        private ImageSource nameImage = null;
        private Rect nameRect;

        private ImageSource textImage = null;
        private Rect textRect;

        #endregion PrivateField

        PersonaEncoding PersonaEncoding;
        PersonaFont PersonaFont;

        TextVisual Text;
        TextVisual Name;

        private Background selectBack = null;
        private Background SelectBack
        {
            get { return selectBack; }
            set
            {
                if (selectBack != value)
                {
                    if (selectBack != null)
                        selectBack.BackgroundChanged -= SelectBack_BackgroundChanged;
                    selectBack = value;
                    if (selectBack != null)
                        selectBack.BackgroundChanged += SelectBack_BackgroundChanged;
                    SetBack();
                }
            }
        }

        private void SelectBack_BackgroundChanged(Classes.Visual.Background background)
        {
            SetBack();
        }

        private int backgroundSelect;
        private int _FontSelect;

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;
        public int FontSelect
        {
            get { return _FontSelect; }
            set
            {
                _FontSelect = value;
                PersonaEncoding = Static.EncodingManager.GetPersonaEncoding(_FontSelect);
                PersonaFont = Static.FontManager.GetPersonaFont(Static.EncodingManager.GetPersonaEncodingName(_FontSelect));
                Notify("FontSelect");
                Text.UpdateText(_TextTB.GetTextBases(PersonaEncoding));
                Name.UpdateText(_NameTB.GetTextBases(PersonaEncoding));
                Text.UpdateFont(PersonaFont);
                Name.UpdateFont(PersonaFont);
                Text2HEX();
            }
        }

        public ReadOnlyObservableCollection<string> BackgroundList => Static.BackManager.BackgroundList;
        public int BackgroundSelect
        {
            get { return backgroundSelect; }
            set
            {
                if (SelectBack != Static.BackManager.GetBackground(value) && Static.BackManager.GetBackground(value) != null)
                {
                    SelectBack = Static.BackManager.GetBackground(value);
                    backgroundSelect = value;
                    Notify("BackgroundImage");
                    Notify("BackgroundRect");
                }
                Notify("BackgroundSelect");
            }
        }

        public BitmapSource BackgroundImage => SelectBack.Image;
        public Rect BackgroundRect => SelectBack.Rect;

        public ImageSource NameImage
        {
            get { return nameImage; }
            set
            {
                if (nameImage != value)
                {
                    nameImage = value;
                    Notify("NameImage");
                }
            }
        }
        public Rect NameRect
        {
            get { return nameRect; }
            set
            {
                if (nameRect != value)
                {
                    nameRect = value;
                    Notify("NameRect");
                }
            }
        }

        public ImageSource TextImage
        {
            get { return textImage; }
            set
            {
                if (textImage != value)
                {
                    textImage = value;
                    Notify("TextImage");
                }
            }
        }
        public Rect TextRect
        {
            get { return textRect; }
            set
            {
                if (textRect != value)
                {
                    textRect = value;
                    Notify("TextRect");
                }
            }
        }

        private string _NameTB = "";
        public string NameTB
        {
            get { return _NameTB; }
            set
            {
                if (_NameTB != value)
                {
                    _NameTB = value;
                    Name.UpdateText(value.GetTextBases(PersonaEncoding));
                }
            }
        }

        private string _TextTB = "";
        public string TextTB
        {
            get { return _TextTB; }
            set
            {
                if (_TextTB != value)
                {
                    _TextTB = value;
                    Text2HEX();
                    Text.UpdateText(_TextTB.GetTextBases(PersonaEncoding));
                }
            }
        }

        private string _HexTB = "";
        public string HexTB
        {
            get { return _HexTB; }
            set
            {
                if (_HexTB != value)
                {
                    _HexTB = value;

                }
            }
        }

        private void Text2HEX()
        {
            var temp = _TextTB.GetTextBases(PersonaEncoding).GetByteArray();
            _HexTB = BitConverter.ToString(temp).Replace('-', ' ');
            Notify("HexTB");
        }

        private string HEX2Text(string hex)
        {
            return "";
        }

        public VisualizerVM()
        {
            Text = new TextVisual(PersonaFont) { Tag = "Text" };
            Name = new TextVisual(PersonaFont) { Tag = "Name" };
            Text.VisualChanged += Text_VisualChanged;
            Name.VisualChanged += Name_VisualChanged;

            FontSelect = 0;
            BackgroundSelect = 0;
            

            SetBack();
        }

        private void Name_VisualChanged(ImageSource imageSource, Rect rect)
        {
            NameImage = imageSource;
            NameRect = rect;
        }

        private void Text_VisualChanged(ImageSource imageSource, Rect rect)
        {
            TextImage = imageSource;
            TextRect = rect;
        }

        private void SetBack()
        {
            if (SelectBack != null)
            {
                Name.Start = SelectBack.NameStart;
                Text.Start = SelectBack.TextStart;

                Name.Color = SelectBack.ColorName;
                Text.Color = SelectBack.ColorText;

                Name.LineSpacing = SelectBack.LineSpacing;
                Text.LineSpacing = SelectBack.LineSpacing;

                Name.GlyphScale = SelectBack.GlyphScale;
                Text.GlyphScale = SelectBack.GlyphScale;
            }
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PersonaEncodingManager man)
            {
                if (e.PropertyName == man.GetPersonaEncodingName(FontSelect))
                    Text2HEX();
            }
        }
    }
}