using PersonaEditorGUI.Classes.Media.Visual;
using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using PersonaEditorLib.FileStructure.Text;
using PersonaEditorLib.Extension;

namespace PersonaEditorGUI.Tools
{
    class VisualizerVM : BindingObject
    {
        #region PrivateField

        private ImageSource nameImage = null;
        private Rect nameRect;

        private ImageSource textImage = null;
        private Rect textRect;

        #endregion PrivateField

        readonly Backgrounds backgrounds = new Backgrounds(Static.Paths.DirBackgrounds);

        PersonaEditorLib.PersonaEncoding.PersonaEncoding PersonaEncoding;
        PersonaEditorLib.PersonaEncoding.PersonaFont PersonaFont;
        EventWrapper BackgroundEW;

        TextVisual Text;
        TextVisual Name;

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
                Text.UpdateText(_TextTB.GetTextBaseList(PersonaEncoding));
                Name.UpdateText(_NameTB.GetTextBaseList(PersonaEncoding));
                Text.UpdateFont(PersonaFont);
                Name.UpdateFont(PersonaFont);
                Text2HEX();
            }
        }

        public ReadOnlyObservableCollection<string> BackgroundList => backgrounds.BackgroundList;
        public int BackgroundSelect
        {
            get { return backgrounds.SelectedIndex; }
            set { backgrounds.SetBackground(value); }
        }

        public BitmapSource BackgroundImage => backgrounds.CurrentBackground.Image;
        public Rect BackgroundRect => backgrounds.CurrentBackground.Rect;

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
                    Name.UpdateText(value.GetTextBaseList(PersonaEncoding));
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
                    Text.UpdateText(_TextTB.GetTextBaseList(PersonaEncoding));
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
            var temp = _TextTB.GetTextBaseList(PersonaEncoding).GetByteArray();
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

            BackgroundEW = new EventWrapper(backgrounds.CurrentBackground, this);

            SetBack(backgrounds.CurrentBackground);
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

        private void SetBack(BackgroundImage image)
        {
            Name.Start = image.NameStart;
            Text.Start = image.TextStart;

            Name.Color = image.ColorName;
            Text.Color = image.ColorText;

            Name.LineSpacing = image.LineSpacing;
            Text.LineSpacing = image.LineSpacing;

            Name.GlyphScale = image.GlyphScale;
            Text.GlyphScale = image.GlyphScale;
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is BackgroundImage image)
            {
                if (e.PropertyName == "Image")
                    Notify("BackgroundImage");
                else if (e.PropertyName == "Rect")
                    Notify("BackgroundRect");
                else if (e.PropertyName == "TextStart")
                    Text.Start = image.TextStart;
                else if (e.PropertyName == "ColorText")
                    Text.Color = image.ColorText;
                else if (e.PropertyName == "NameStart")
                    Name.Start = image.NameStart;
                else if (e.PropertyName == "ColorName")
                    Name.Color = image.ColorName;
                else if (e.PropertyName == "LineSpacing")
                {
                    Text.LineSpacing = image.LineSpacing;
                    Name.LineSpacing = image.LineSpacing;
                }
                else if (e.PropertyName == "GlyphScale")
                {
                    Text.GlyphScale = image.GlyphScale;
                    Name.GlyphScale = image.GlyphScale;
                }
            }
            else if (sender is PersonaEditorLib.PersonaEncoding.PersonaEncodingManager man)
            {
                if (e.PropertyName == man.GetPersonaEncodingName(FontSelect))
                    Text2HEX();
            }
        }
    }
}