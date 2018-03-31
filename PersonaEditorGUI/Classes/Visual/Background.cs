using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorGUI.Classes.Visual
{
    class Background
    {
        private Point textStart;
        private Point nameStart;
        private double glyphScale;
        private BitmapSource image;
        private Color colorText;
        private Color colorName;
        private int lineSpacing;

        public Point TextStart => textStart;
        public Point NameStart => nameStart;

        public double GlyphScale => glyphScale;
        public BitmapSource Image => image;
        public Color ColorText => colorText;
        public Color ColorName => colorName;
        public int LineSpacing => lineSpacing;

        public Rect Rect => new Rect(0, 0, image.Width, image.Height);

        public Background()
        {

        }

        public Background(string imgPath, string xmlPath)
        {

        }
    }
}