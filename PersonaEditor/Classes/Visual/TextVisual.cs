using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;
using AuxiliaryLibraries;
using PersonaEditorLib;
using AuxiliaryLibraries.WPF.Wrapper;

namespace PersonaEditor.Classes.Visual
{
    public delegate void VisualChangedEventHandler(ImageSource imageSource, Rect rect);

    class TextVisual : BindingObject
    {
        public static System.Drawing.Color[] CreatePallete(Color color, AuxiliaryLibraries.Media.PixelFormat pixelformat)
        {
            int colorcount = 0;
            byte step = 0;
            if (pixelformat == AuxiliaryLibraries.Media.PixelFormats.Indexed4)
            {
                colorcount = 16;
                step = 0x10;
            }
            else if (pixelformat == AuxiliaryLibraries.Media.PixelFormats.Indexed8)
            {
                colorcount = 256;
                step = 1;
            }


            List<System.Drawing.Color> ColorBMP = new List<System.Drawing.Color>();
            ColorBMP.Add(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            for (int i = 1; i < colorcount; i++)
                ColorBMP.Add(System.Drawing.Color.FromArgb(
                    ByteTruncate(i * step),
                    color.R,
                    color.G,
                    color.B));

            return ColorBMP.ToArray();
        }

        public static byte ByteTruncate(int value)
        {
            if (value < 0) { return 0; }
            else if (value > 255) { return 255; }
            else { return (byte)value; }
        }

        public event VisualChangedEventHandler VisualChanged;

        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        Func<ImageData> GetData;

        PersonaFont font;

        #region PrivateField

        private bool isEnable = true;
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
                _Image = _Data.GetImageSource(CreatePallete(Color, _Data.PixelFormat)).GetBitmapSource();
                TextDrawing.ImageSource = _Image;
                _Rect = GetSize(Start, _Data.PixelWidth, _Data.PixelHeight);
                TextDrawing.Rect = _Rect;
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
                    _Image = _Data.GetImageSource(CreatePallete(Color, _Data.PixelFormat))?.GetBitmapSource();
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
                    UpdateText();
                }
            }
        }

        public Rect Rect => _Rect;
        public ImageSource Image => _Image;

        public ImageDrawing TextDrawing { get; } = new ImageDrawing();

        public bool IsEnable
        {
            get { return isEnable; }
            set
            {
                isEnable = value;
                if (value)
                    UpdateText();
            }
        }

        ImageData CreateData()
        {
            if (Text is IEnumerable<TextBaseElement> list)
                return ImageData.DrawText(list, font, Static.FontMap.Shift, LineSpacing);
            else if (Text is byte[] array)
                return ImageData.DrawText(array.GetTextBases(), font, Static.FontMap.Shift, LineSpacing);
            else return new ImageData();
        }

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * GlyphScale;
            double Width = pixelWidth * GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }

        public void UpdateText(IEnumerable<TextBaseElement> textBases, PersonaFont font = null)
        {
            Text = textBases;
            if (font != null)
                this.font = font;
            UpdateText();
        }

        public void UpdateText(byte[] array)
        {
            Text = array;
            UpdateText();
        }

        public async void UpdateText()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
                CancellationTokenSource.Cancel();

            CancellationTokenSource.Dispose();
            CancellationTokenSource = new CancellationTokenSource();

            if (IsEnable)
                try
                {
                    Data = await Task.Run(GetData, CancellationTokenSource.Token);
                }
                catch (OperationCanceledException ex)
                {
                }
                catch (Exception e)
                {
                }
        }

        public void UpdateFont(PersonaFont Font)
        {
            this.font = Font;
            UpdateText();
        }

        public TextVisual()
        {
            GetData = CreateData;
        }

        public TextVisual(PersonaFont font) : this()
        {
            this.font = font;
        }
    }
}