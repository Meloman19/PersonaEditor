using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonaEditorLib;
using PersonaEditorLib.FileStructure.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace PersonaEditorGUI.Classes.Media.Visual
{
    public delegate void VisualChangedEventHandler(ImageSource imageSource, Rect rect);

    public class TextVisual : BindingObject
    {
        public event VisualChangedEventHandler VisualChanged;

        PersonaEditorLib.PersonaEncoding.PersonaFont Font;

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
                _Image = _Data.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(Color, _Data.PixelFormat));
                _Rect = GetSize(Start, _Data.PixelWidth, _Data.PixelHeight);
                VisualChanged?.Invoke(_Image, _Rect);
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
                    _Rect = GetSize(Start, _Data.PixelWidth, _Data.PixelHeight);
                    VisualChanged?.Invoke(_Image, _Rect);
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
                    _Image = _Data.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(Color, _Data.PixelFormat));
                    VisualChanged?.Invoke(_Image, _Rect);
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
                    _Rect = GetSize(Start, _Data.PixelWidth, _Data.PixelHeight);
                    VisualChanged?.Invoke(_Image, _Rect);
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

        ImageData CreateImageData(object Text)
        {
            if (Text is IList<TextBaseElement> list)
                return ImageData.DrawText(list, Font, Static.FontMap.Shift, LineSpacing);
            else if (Text is byte[] array)
                return ImageData.DrawText(array.GetTextBaseList(), Font, Static.FontMap.Shift, LineSpacing);
            else return new ImageData();
        }

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * GlyphScale;
            double Width = pixelWidth * GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }

        public void UpdateText(IList<TextBaseElement> List, PersonaEditorLib.PersonaEncoding.PersonaFont Font = null)
        {
            Text = List.ToArray();
            if (Font != null)
                this.Font = Font;
            Data = CreateImageData(Text);
        }

        public void UpdateText(byte[] array)
        {
            Text = array;
            Data = CreateImageData(Text);
        }

        public void UpdateFont(PersonaEditorLib.PersonaEncoding.PersonaFont Font)
        {
            this.Font = Font;
            Data = CreateImageData(Text);
        }
        
        public TextVisual(PersonaEditorLib.PersonaEncoding.PersonaFont Font)
        {
            this.Font = Font;
        }
    }
}