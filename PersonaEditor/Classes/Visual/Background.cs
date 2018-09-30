using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace PersonaEditor.Classes.Visual
{
    delegate void BackgroundChanged(Background background);

    class Background
    {
        public event BackgroundChanged BackgroundChanged;

        private bool Changed = false;

        private Point textStart;
        private Point nameStart;
        private double glyphScale;
        private BitmapSource image;
        private Color colorText;
        private Color colorName;
        private int lineSpacing;
        private Rect rect;

        private int width;
        private int height;
        private Color emptyBackgroundColor;

        public Point TextStart
        {
            get { return textStart; }
            private set
            {
                if (textStart != value)
                {
                    textStart = value;
                    Changed = true;
                }
            }
        }
        public Point NameStart
        {
            get { return nameStart; }
            private set
            {
                if (nameStart != value)
                {
                    nameStart = value;
                    Changed = true;
                }
            }
        }

        public double GlyphScale
        {
            get { return glyphScale; }
            private set
            {
                if (glyphScale != value)
                {
                    glyphScale = value;
                    Changed = true;
                }
            }
        }
        public BitmapSource Image => image;
        public Color ColorText
        {
            get { return colorText; }
            private set
            {
                if (colorText != value)
                {
                    colorText = value;
                    Changed = true;
                }
            }
        }
        public Color ColorName
        {
            get { return colorName; }
            private set
            {
                if (colorName != value)
                {
                    colorName = value;
                    Changed = true;
                }
            }
        }
        public int LineSpacing
        {
            get { return lineSpacing; }
            private set
            {
                if (lineSpacing != value)
                {
                    lineSpacing = value;
                    Changed = true;
                }
            }
        }

        private int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    Changed = true;
                }
            }
        }
        private int Height
        {
            get { return height; }
            set
            {
                if (height != value)
                {
                    height = value;
                    Changed = true;
                }
            }
        }
        private Color EmptyBackgroundColor
        {
            get { return emptyBackgroundColor; }
            set
            {
                if (emptyBackgroundColor != value)
                {
                    emptyBackgroundColor = value;
                    Changed = true;
                }
            }
        }

        public Rect Rect => rect;

        public Background()
        {
            SetEmpty();
        }

        public Background(string imgPath, string xmlPath)
        {
            try
            {
                image = new BitmapImage(new Uri(imgPath));
                rect = new Rect(0, 0, image.PixelWidth, image.PixelHeight);
                ParseDescription(xmlPath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        void ParseDescription(string FileName)
        {
            var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
            culture.NumberFormat.NumberDecimalSeparator = ".";
            XDocument XDoc = XDocument.Load(FileName);
            XElement Background = XDoc.Element("Background");

            LineSpacing = Convert.ToInt32(Background.Element("LineSpacing").Value, culture);
            GlyphScale = Convert.ToDouble(Background.Element("glyphScale").Value, culture);

            TextStart = new Point(Convert.ToInt32(Background.Element("textStartX").Value, culture), Convert.ToInt32(Background.Element("textStartY").Value, culture));
            NameStart = new Point(Convert.ToInt32(Background.Element("nameStartX").Value, culture), Convert.ToInt32(Background.Element("nameStartY").Value, culture));

            ColorText = (Color)ColorConverter.ConvertFromString(Background.Element("ColorText").Value);
            ColorName = (Color)ColorConverter.ConvertFromString(Background.Element("ColorName").Value);
        }

        public void SetEmpty()
        {
            Changed = false;

            Width = ApplicationSettings.BackgroundDefault.Default.EmptyWidth;
            Height = ApplicationSettings.BackgroundDefault.Default.EmptyHeight;
            EmptyBackgroundColor = ApplicationSettings.BackgroundDefault.Default.EmptyBackgroundColor;

            TextStart = ApplicationSettings.BackgroundDefault.Default.EmptyTextPos;
            NameStart = ApplicationSettings.BackgroundDefault.Default.EmptyNamePos;

            GlyphScale = ApplicationSettings.BackgroundDefault.Default.EmptyGlyphScale;
            ColorName = ApplicationSettings.BackgroundDefault.Default.EmptyNameColor;
            ColorText = ApplicationSettings.BackgroundDefault.Default.EmptyTextColor;
            LineSpacing = ApplicationSettings.BackgroundDefault.Default.EmptyLineSpacing;

            rect = new Rect(0, 0, Width, Height);

            image = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Indexed1,
                new BitmapPalette(new List<Color> { EmptyBackgroundColor }), new byte[Width * Height], Width);

            if (Changed)
                BackgroundChanged?.Invoke(this);
        }
    }
}