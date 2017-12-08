using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using PersonaEditorLib.FileStructure.PTP;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace PersonaEditorGUI.Classes.Media.Visual
{
    public class MSGVisual : BindingObject
    {
        #region PrivateField

        BackgroundImage Background;
        EventWrapper background;

        CharList CharList;
        EventWrapper charlist;

        ImageData _TextData;
        ImageSource _TextImage;
        Rect _TextRect;

        ImageData _NameData;
        ImageSource _NameImage;
        Rect _NameRect;

        IList<TextBaseElement> Text = new List<TextBaseElement>();
        IList<TextBaseElement> Name = new List<TextBaseElement>();

        #endregion PrivateField

        #region PrivateMethods

        ImageData CreateImageData(IList<TextBaseElement> array)
        {
            return ImageData.DrawText(array, CharList, Static.FontMap.Shift, Background.LineSpacing);
        }

        ImageData CreateImageData(string text)
        {
            return ImageData.DrawText(text.GetTextBaseList(CharList), CharList, Static.FontMap.Shift, Background.LineSpacing);
        }

        ImageData CreateImageData(byte[] Name)
        {
            return ImageData.DrawText(Name.GetTextBaseList(), CharList, Static.FontMap.Shift, Background.LineSpacing);
        }

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * Background.GlyphScale;
            double Width = pixelWidth * Background.GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }

        #endregion PrivateMethods

        ImageData TextData
        {
            get { return _TextData; }
            set
            {
                _TextData = value;
                TextImage = _TextData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(Background.ColorText, _TextData.PixelFormat));
                TextRect = GetSize(Background.TextStart, _TextData.PixelWidth, _TextData.PixelHeight);
            }
        }

        ImageData NameData
        {
            get { return _NameData; }
            set
            {
                _NameData = value;
                NameImage = _NameData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(Background.ColorName, _NameData.PixelFormat));
                NameRect = GetSize(Background.NameStart, _NameData.PixelWidth, _NameData.PixelHeight);
            }
        }

        public string Tag { get; set; } = "";

        public ImageSource TextImage
        {
            get { return _TextImage; }
            set
            {
                _TextImage = value;
                Notify("TextImage");
            }
        }
        public Rect TextRect
        {
            get { return _TextRect; }
            set
            {
                if (_TextRect != value)
                {
                    _TextRect = value;
                    Notify("TextRect");
                }
            }
        }

        public ImageSource NameImage
        {
            get { return _NameImage; }
            set
            {
                _NameImage = value;
                Notify("NameImage");
            }
        }
        public Rect NameRect
        {
            get { return _NameRect; }
            set
            {
                if (_NameRect != value)
                {
                    _NameRect = value;
                    Notify("NameRect");
                }
            }
        }

        public void UpdateText(IList<TextBaseElement> List)
        {
            Text = List;
            TextData = CreateImageData(Text);
        }

        public void UpdateName(IList<TextBaseElement> List)
        {
            Name = List;
            NameData = CreateImageData(Name);
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is BackgroundImage image)
            {
                if (e.PropertyName == "TextStart")
                    TextRect = GetSize(image.TextStart, _TextData.PixelWidth, _TextData.PixelHeight);
                else if (e.PropertyName == "ColorText")
                    TextImage = _TextData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(image.ColorText, _TextData.PixelFormat));
                else if (e.PropertyName == "NameStart")
                    NameRect = GetSize(image.NameStart, _NameData.PixelWidth, _NameData.PixelHeight);
                else if (e.PropertyName == "ColorName")
                    NameImage = _NameData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(image.ColorName, _NameData.PixelFormat));
                else if (e.PropertyName == "LineSpacing")
                    TextData = CreateImageData(Text);
                else if (e.PropertyName == "GlyphScale")
                {
                    TextRect = GetSize(Background.TextStart, _TextData.PixelWidth, _TextData.PixelHeight);
                    NameRect = GetSize(Background.NameStart, _NameData.PixelWidth, _NameData.PixelHeight);
                }
            }
            if (sender is MSG.MSGstr msg)
            {

            }
            if (sender is CharList charlist)
            {
                TextData = CreateImageData(Text);
                NameData = CreateImageData(Name);
            }
        }

        public MSGVisual(BackgroundImage background, CharList charlist)
        {
            CharList = charlist != null ? charlist : throw new ArgumentNullException("charlist");
            Background = background != null ? background : throw new ArgumentNullException("background");

            this.charlist = new EventWrapper(charlist, this);
            this.background = new EventWrapper(background, this);
        }
    }

    public class TextVisual : BindingObject
    {
        CharList CharList;
        EventWrapper charlist;

        #region PrivateField

        private object Text;

        private ImageSource _Image;
        private Rect _Rect;
        private Point _Start;
        private Color _Color;
        private double _GlyphScale;
        private int _LineSpacing;
        private ImageData _Data;
        private ImageData Data
        {
            get { return _Data; }
            set
            {
                _Data = value;
                CreateSource();
                CreateRect();
            }
        }

        #endregion PrivateField

        public string Tag { get; set; }

        public Point Start
        {
            get { return _Start; }
            set
            {
                if (_Start != value)
                {
                    _Start = value;
                    CreateRect();
                }
            }
        }
        public Color Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    CreateSource();
                }
            }
        }
        public double GlyphScale
        {
            get { return _GlyphScale; }
            set
            {
                if (_GlyphScale != value)
                {
                    _GlyphScale = value;
                    CreateRect();
                }
            }
        }
        public int LineSpacing
        {
            get { return _LineSpacing; }
            set
            {
                if (_LineSpacing != value)
                {
                    _LineSpacing = value;
                    Data = CreateImageData(Text);
                }
            }
        }

        public Rect Rect => _Rect;
        public ImageSource Image => _Image;

        #region PrivateMethods

        void CreateSource()
        {
            _Image = _Data.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(Color, _Data.PixelFormat));
            Notify("Image");
        }

        void CreateRect()
        {
            _Rect = GetSize(Start, _Data.PixelWidth, _Data.PixelHeight);
            Notify("Rect");
        }

        ImageData CreateImageData(object Text)
        {
            if (Text is string text)
                return ImageData.DrawText(text.GetTextBaseList(CharList), CharList, Static.FontMap.Shift, LineSpacing);
            else if (Text is IList<TextBaseElement> list)
                return ImageData.DrawText(list, CharList, Static.FontMap.Shift, LineSpacing);
            else if (Text is byte[] array)
                return ImageData.DrawText(array.GetTextBaseList(), CharList, Static.FontMap.Shift, LineSpacing);
            else return new ImageData();
        }

        ImageData CreateImageData(IList<TextBaseElement> array)
        {
            return ImageData.DrawText(array, CharList, Static.FontMap.Shift, LineSpacing);
        }

        ImageData CreateImageData(string text)
        {
            return ImageData.DrawText(text.GetTextBaseList(CharList), CharList, Static.FontMap.Shift, LineSpacing);
        }

        ImageData CreateImageData(byte[] Name)
        {
            return ImageData.DrawText(Name.GetTextBaseList(), CharList, Static.FontMap.Shift, LineSpacing);
        }

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * GlyphScale;
            double Width = pixelWidth * GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }

        #endregion PrivateMethods

        public void UpdateText(IList<TextBaseElement> List)
        {
            Text = List.ToArray();
            Data = CreateImageData(Text);
        }

        public void UpdateText(byte[] array)
        {
            Text = array;
            Data = CreateImageData(Text);
        }

        public void UpdateText(string str)
        {
            Text = str;
            Data = CreateImageData(Text);
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CharList charlist)
            {
                Data = CreateImageData(Text);
            }
        }

        public TextVisual(CharList charlist)
        {
            CharList = charlist != null ? charlist : throw new ArgumentNullException("charlist");

            this.charlist = new EventWrapper(charlist, this);
        }
    }
}