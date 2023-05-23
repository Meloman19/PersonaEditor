﻿using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib;
using PersonaEditorLib.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PersonaEditor.Common.Visual
{
    public delegate void VisualChangedEventHandler(ImageSource imageSource, Rect rect);

    class TextVisual : BindingObject
    {
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
                _Image = _Data.GetImageSource()?.GetBitmapSource();
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
                    _Image = _Data.GetImageSource()?.GetBitmapSource();
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
                return ImageData.DrawText(list, font, LineSpacing);
            else if (Text is byte[] array)
                return ImageData.DrawText(array.GetTextBases(), font, LineSpacing);
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