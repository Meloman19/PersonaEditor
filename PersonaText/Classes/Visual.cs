using PersonaEditorLib;
using PersonaEditorLib.FileStructure.PTP;
using PersonaEditorLib.FileTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonaText
{
    class Visual
    {
        public Visual(CharList CharLST, BackgroundImage background)
        {
            this.CharLST = CharLST;

            TextColor = background.ColorText;
            TextStart = background.TextStart;
            NameColor = background.ColorName;
            NameStart = background.NameStart;
            GlyphScale = background.GlyphScale;
            LineSpacing = background.LineSpacing;
        }

        ImageData CreateImageData(IList<TextBaseElement> array)
        {
            return ImageData.DrawText(array, CharLST, Static.FontMap.Shift, LineSpacing);
        }

        ImageData CreateImageData(string text)
        {
            return ImageData.DrawText(text.GetTextBaseList(CharLST), CharLST, Static.FontMap.Shift, LineSpacing);
        }

        ImageData CreateImageData(byte[] Name)
        {
            return ImageData.DrawText(Name.GetTextBaseList(), CharLST, Static.FontMap.Shift, LineSpacing);
        }

        List<TextBaseElement> TextTemp = new List<TextBaseElement>();
        List<TextBaseElement> NameTemp = new List<TextBaseElement>();

        public void Text_Update(IList<TextBaseElement> array)
        {
            TextTemp.Clear();
            TextTemp.AddRange(array);
            DataText = CreateImageData(TextTemp);
        }

        public void Name_Update(IList<TextBaseElement> array)
        {
            NameTemp.Clear();
            NameTemp.AddRange(array);
            DataName = CreateImageData(NameTemp);
        }

        Color TextColor;
        Point TextStart;
        Color NameColor;
        Point NameStart;
        int LineSpacing;
        double GlyphScale;

        public void Background_Update(BackgroundImage background)
        {
            TextColor = background.ColorText;
            TextStart = background.TextStart;
            NameColor = background.ColorName;
            NameStart = background.NameStart;
            GlyphScale = background.GlyphScale;
            LineSpacing = background.LineSpacing;

            DataText = CreateImageData(TextTemp);
            DataName = CreateImageData(NameTemp);
        }

        ImageData _ImageData;
        ImageData _DataName;
        CharList CharLST;

        ImageData DataText
        {
            get { return _ImageData; }
            set
            {
                _ImageData = value;
                DrawingText.ImageSource = _ImageData.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(TextColor, _ImageData.PixelFormat));
                DrawingText.Rect = GetSize(TextStart, _ImageData.PixelWidth, _ImageData.PixelHeight);
            }
        }
        ImageData DataName
        {
            get { return _DataName; }
            set
            {
                _DataName = value;
                DrawingName.ImageSource = _DataName.GetImageSource(PersonaEditorLib.Utilities.Utilities.CreatePallete(NameColor, _DataName.PixelFormat));
                DrawingName.Rect = GetSize(NameStart, _DataName.PixelWidth, _DataName.PixelHeight);
            }
        }

        public ImageDrawing DrawingText { get; private set; } = new ImageDrawing();
        public ImageDrawing DrawingName { get; private set; } = new ImageDrawing();

        Rect GetSize(Point start, double pixelWidth, double pixelHeight)
        {
            double Height = pixelHeight * GlyphScale;
            double Width = pixelWidth * GlyphScale * 0.9375;
            return new Rect(start, new Size(Width, Height));
        }
    }
}