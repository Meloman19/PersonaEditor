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
        readonly CharList CharList = new CharList();
        EventWrapper CharListEW;

        readonly Backgrounds backgrounds = new Backgrounds(Static.Paths.DirBackgrounds);

        EventWrapper BackgroundEW;

        string Dir;

        TextVisual Text;
        EventWrapper TextEW;
        TextVisual Name;
        EventWrapper NameEW;

        private int _FontSelect;
        public ObservableCollection<string> FontList { get; } = new ObservableCollection<string>();
        public int FontSelect
        {
            get { return _FontSelect; }
            set
            {
                _FontSelect = value;
                string font = Path.Combine(Dir, FontList[_FontSelect]);
                string fontmp = Path.Combine(Dir, Path.GetFileNameWithoutExtension(FontList[_FontSelect]) + ".txt");
                CharList.Open(font, fontmp);
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

        public ImageSource TextImage => Text.Image;
        public Rect TextRect => Text.Rect;

        public ImageSource NameImage => Name.Image;
        public Rect NameRect => Name.Rect;

        public string NameTB { set { Name.UpdateText(value); } }

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
                    Text.UpdateText(_TextTB);
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
            var temp = _TextTB.GetTextBaseList(CharList).GetByteArray();
            _HexTB = BitConverter.ToString(temp).Replace('-', ' ');
            Notify("HexTB");
        }

        private string HEX2Text(string hex)
        {
            return "";
        }

        public VisualizerVM()
        {
            Text = new TextVisual(CharList) { Tag = "Text" };
            TextEW = new EventWrapper(Text, this);
            Name = new TextVisual(CharList) { Tag = "Name" };
            NameEW = new EventWrapper(Name, this);

            CharListEW = new EventWrapper(CharList, this);

            BackgroundEW = new EventWrapper(backgrounds.CurrentBackground, this);

            LoadFontList(Static.Paths.DirFont);
            FontSelect = 0;

            SetBack(backgrounds.CurrentBackground);
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
            if (sender is TextVisual vis)
            {
                if (vis.Tag == "Text")
                {
                    if (e.PropertyName == "Image")
                        Notify("TextImage");
                    else if (e.PropertyName == "Rect")
                        Notify("TextRect");
                }
                else if (vis.Tag == "Name")
                {
                    if (e.PropertyName == "Image")
                        Notify("NameImage");
                    else if (e.PropertyName == "Rect")
                        Notify("NameRect");
                }
            }
            else if (sender is BackgroundImage image)
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
            else if (sender is CharList chr)
            {
                Text2HEX();
            }
        }

        private void LoadFontList(string dir = "")
        {
            Dir = Path.GetFullPath(dir);
            if (Directory.Exists(Dir))
            {
                DirectoryInfo DI = new DirectoryInfo(Dir);
                foreach (var file in DI.GetFiles(@"*.fnt"))
                    FontList.Add(file.Name);
            }
        }
    }
}