using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PersonaText
{
    public class BackgroundImage : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged implementation

        double _glyphScale = 1;
        BitmapSource _Image;
        Color _ColorText;
        Color _ColorName;
        Point _TextStart;
        Point _NameStart;
        int _LineSpacing;

        public Point TextStart
        {
            get { return _TextStart; }
            set
            {
                if (value != _TextStart)
                {
                    _TextStart = value;
                    Notify("TextStart");
                }
            }
        }
        public Point NameStart
        {
            get { return _NameStart; }
            set
            {
                if (value != _NameStart)
                {
                    _NameStart = value;
                    Notify("NameStart");
                }
            }
        }

        public double GlyphScale
        {
            get { return _glyphScale; }
            set
            {
                if (value != _glyphScale)
                {
                    _glyphScale = value;
                    Notify("GlyphScale");
                }
            }
        }
        public BitmapSource Image
        {
            get { return _Image; }
            set
            {
                if (value != _Image)
                {
                    _Image = value;
                    Drawing.ImageSource = _Image;
                    Drawing.Rect = new Rect(0, 0, _Image.Width, _Image.Height);
                    Rect.Rect = Drawing.Rect;
                    Notify("Image");
                }
            }
        }
        public Color ColorText
        {
            get { return _ColorText; }
            set
            {
                if (_ColorText != value)
                {
                    _ColorText = value;
                    Notify("ColorText");
                }
            }
        }
        public Color ColorName
        {
            get { return _ColorName; }
            set
            {
                if (_ColorName != value)
                {
                    _ColorName = value;
                    Notify("ColorName");
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
                    Notify("LineSpacing");
                }
            }
        }

        public ImageDrawing Drawing { get; private set; } = new ImageDrawing();
        public RectangleGeometry Rect { get; private set; } = new RectangleGeometry();

        public void SetDefault()
        {
            int Width = Current.Default.EmptyWidth;
            int Height = Current.Default.EmptyHeight;
            TextStart = new Point(Current.Default.EmptyTextX, Current.Default.EmptyTextY);
            NameStart = new Point(Current.Default.EmptyNameX, Current.Default.EmptyNameY);

            GlyphScale = Current.Default.EmptyGlyphScale;
            ColorName = Current.Default.EmptyNameColor;
            ColorText = Current.Default.EmptyTextColor;
            LineSpacing = Current.Default.EmptyLineSpacing ;

            Image = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Indexed1, new BitmapPalette(new List<Color> { Current.Default.EmptyBackgroundColor }), new byte[Width * Height], Width);
        }
    }

    public class Backgrounds // : INotifyPropertyChanged
    {
        public delegate void BackgroundImageChanged(BackgroundImage background);

        public event BackgroundImageChanged BackgroundChanged;

        public BackgroundImage CurrentBackground { get; } = new BackgroundImage();

        public List<string> BackgroundList { get; } = new List<string>();

        //#region INotifyPropertyChanged implementation
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void Notify(string propertyName)
        //{
        //    if (this.PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}
        //#endregion INotifyPropertyChanged implementation

        //double _glyphScale = 1;
        //BitmapSource _Image;
        //Color _ColorText;
        //Color _ColorName;
        //Point _TextStart;
        //Point _NameStart;
        //int _LineSpacing;

        //public Point TextStart
        //{
        //    get { return _TextStart; }
        //    private set
        //    {
        //        if (value != _TextStart)
        //        {
        //            _TextStart = value;
        //            Notify("TextStart");
        //        }
        //    }
        //}
        //public Point NameStart
        //{
        //    get { return _NameStart; }
        //    private set
        //    {
        //        if (value != _NameStart)
        //        {
        //            _NameStart = value;
        //            Notify("NameStart");
        //        }
        //    }
        //}

        //public double GlyphScale
        //{
        //    get { return _glyphScale; }
        //    private set
        //    {
        //        if (value != _glyphScale)
        //        {
        //            _glyphScale = value;
        //            Notify("GlyphScale");
        //        }
        //    }
        //}
        //public BitmapSource Image
        //{
        //    get { return _Image; }
        //    private set
        //    {
        //        if (value != _Image)
        //        {
        //            _Image = value;
        //            Drawing.ImageSource = _Image;
        //            Drawing.Rect = new Rect(0, 0, _Image.Width, _Image.Height);
        //            Rect.Rect = Drawing.Rect;
        //            Notify("Image");
        //        }
        //    }
        //}
        //public Color ColorText
        //{
        //    get { return _ColorText; }
        //    set
        //    {
        //        if (_ColorText != value)
        //        {
        //            _ColorText = value;
        //            Notify("ColorText");
        //        }
        //    }
        //}
        //public Color ColorName
        //{
        //    get { return _ColorName; }
        //    set
        //    {
        //        if (_ColorName != value)
        //        {
        //            _ColorName = value;
        //            Notify("ColorName");
        //        }
        //    }
        //}
        //public int LineSpacing
        //{
        //    get { return _LineSpacing; }
        //    set
        //    {
        //        if (_LineSpacing != value)
        //        {
        //            _LineSpacing = value;
        //            Notify("LineSpacing");
        //        }
        //    }
        //}

        //public ImageDrawing Drawing { get; private set; } = new ImageDrawing();
        //public RectangleGeometry Rect { get; private set; } = new RectangleGeometry();

        public Backgrounds()
        {
            BackgroundList.Add("Default");
            CurrentBackground.SetDefault();
            GetBackgroundList();
        }

        private void GetBackgroundList()
        {
            if (Directory.Exists(Static.Paths.DirBackgrounds))
            {
                DirectoryInfo DI = new DirectoryInfo(Static.Paths.DirBackgrounds);
                foreach (var file in DI.GetFiles(@"*.png"))
                    BackgroundList.Add(file.Name);
            }
        }

        private int CurrentIndex = 0;

        public bool SetBackground(int index)
        {
            CurrentIndex = index;
            return Update(BackgroundList[index]);
        }

        public bool CurrentUpdate()
        {
            return Update(BackgroundList[CurrentIndex]);
        }

        bool Update(string FileName)
        {
            if (Equals(FileName, "Default"))
            {
                CurrentBackground.SetDefault();
                BackgroundChanged?.Invoke(CurrentBackground);
                return true;
            }
            else
            {
                try
                {
                    CurrentBackground.Image = new BitmapImage(new Uri(Path.Combine(Static.Paths.DirBackgrounds, FileName)));
                    string xml = Path.Combine(Static.Paths.DirBackgrounds, Path.GetFileNameWithoutExtension(FileName) + ".xml");
                    ParseDescription(xml);
                    BackgroundChanged?.Invoke(CurrentBackground);
                    return true;
                }
                catch (FormatException)
                {
                    MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that the numeric values(except for GlyphScale) are Integer.");
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Background load error:\nThere is no description file.");
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("Background load error:\nAn error occurred while reading data from the description file.\nCheck that all the required values are present.");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.GetType().ToString());
                    MessageBox.Show(e.ToString());
                }
                CurrentBackground.SetDefault();
                return false;
            }
        }

        void ParseDescription(string FileName)
        {
            var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
            culture.NumberFormat.NumberDecimalSeparator = ".";
            XDocument XDoc = XDocument.Load(FileName);
            XElement Background = XDoc.Element("Background");

            CurrentBackground.LineSpacing = Convert.ToInt32(Background.Element("LineSpacing").Value, culture);
            CurrentBackground.GlyphScale = Convert.ToDouble(Background.Element("glyphScale").Value, culture);

            CurrentBackground.TextStart = new Point(Convert.ToInt32(Background.Element("textStartX").Value, culture), Convert.ToInt32(Background.Element("textStartY").Value, culture));
            CurrentBackground.NameStart = new Point(Convert.ToInt32(Background.Element("nameStartX").Value, culture), Convert.ToInt32(Background.Element("nameStartY").Value, culture));

            CurrentBackground.ColorText = (Color)ColorConverter.ConvertFromString(Background.Element("ColorText").Value);
            CurrentBackground.ColorName = (Color)ColorConverter.ConvertFromString(Background.Element("ColorName").Value);
        }
    }
}