using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media;
using AuxiliaryLibraries.WPF.Wrapper;
using PersonaEditorLib;
using PersonaEditorLib.Text;

namespace PersonaEditor.Common.Visual
{
    public sealed class TextVisual
    {
        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private PersonaEncoding _encoding;
        private PersonaFont _font;
        private string _text;
        private BitmapSource _textBitmap;

        public BitmapSource Image => _textBitmap;

        public void UpdateText(string text)
        {
            _text = text;
            UpdateText();
        }

        public void UpdateFont(PersonaEncoding encoding, PersonaFont font)
        {
            _encoding = encoding;
            _font = font;
            UpdateText();
        }

        public async void UpdateText()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
                CancellationTokenSource.Cancel();

            CancellationTokenSource.Dispose();
            CancellationTokenSource = new CancellationTokenSource();

            try
            {
                var text = _text ?? string.Empty;
                var encoding = _encoding;
                var font = _font;

                _textBitmap = await Task.Run(() =>
                {
                    var textBases = text.GetTextBases(encoding);
                    var imageData = DrawText(textBases, font);
                    if (imageData.IsEmpty)
                        return null;
                    var img = imageData.GetBitmapSource();
                    img.Freeze();
                    return img;
                }, CancellationTokenSource.Token);
                ImageChanged?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
            }
        }

        public static PixelMap DrawText(IEnumerable<TextBaseElement> text, PersonaFont personaFont)
        {
            if (text != null && personaFont != null)
            {
                PixelMap returned = new PixelMap();
                PixelMap line = new PixelMap();
                foreach (var a in text)
                {
                    if (a.IsText)
                    {
                        for (int i = 0; i < a.Data.Length; i++)
                        {
                            int index = 0;

                            if (0x20 <= a.Data[i] & a.Data[i] < 0x80)
                                index = a.Data[i];
                            else if (0x80 <= a.Data[i] & a.Data[i] < 0xF0)
                            {
                                index = (a.Data[i] - 0x81) * 0x80 + a.Data[i + 1] + 0x20;
                                i++;
                            }

                            var Glyph = personaFont.GetGlyph(index);

                            if (Glyph.Item1 != null)
                            {
                                PixelMap glyph = new PixelMap(personaFont.Width, personaFont.Height, Glyph.Item1);
                                byte Left = Glyph.Item2.Left;
                                byte Right = Glyph.Item2.Right;
                                glyph = Crop(glyph, new Rectangle(Left, 0, Right - Left, glyph.Height));
                                line = MergeLeftRight(line, glyph, 1);
                            }
                        }
                    }
                    else
                    {
                        if (a.Data.ArrayEquals(new byte[] { 0x0A }))
                        {
                            returned = MergeUpDown(returned, line);
                            line = new PixelMap();
                        }
                    }
                }
                returned = MergeUpDown(returned, line);
                return returned;
            }
            return new PixelMap();
        }

        public static PixelMap MergeLeftRight(PixelMap left, PixelMap right, int horizontalshift)
        {
            if (left.IsEmpty)
                return right;
            if (right.IsEmpty)
                return left;

            var leftRect = new Rectangle(0, 0, left.Width, left.Height);
            var rightRect = new Rectangle(left.Width + horizontalshift, 0, right.Width, right.Height);

            var union = Rectangle.Union(leftRect, rightRect);
            var newPixels = new Pixel[union.Width * union.Height];
            Array.Fill(newPixels, Pixel.FromArgb(255, 0, 0, 0));
            CopyPixels(newPixels, union, left, leftRect);
            CopyPixels(newPixels, union, right, rightRect);

            return new PixelMap(union.Width, union.Height, newPixels);
        }

        public static PixelMap MergeUpDown(PixelMap up, PixelMap down)
        {
            if (up.IsEmpty)
                return down;
            if (down.IsEmpty)
                return up;

            var upRect = new Rectangle(0, 0, up.Width, up.Height);
            var downRect = new Rectangle(0, up.Height, down.Width, down.Height);

            var union = Rectangle.Union(upRect, downRect);
            var newPixels = new Pixel[union.Width * union.Height];
            Array.Fill(newPixels, Pixel.FromArgb(255, 0, 0, 0));
            CopyPixels(newPixels, union, up, upRect);
            CopyPixels(newPixels, union, down, downRect);

            return new PixelMap(union.Width, union.Height, newPixels);
        }

        private static void CopyPixels(Pixel[] newPixels, Rectangle newRect, PixelMap pixelMap, Rectangle rect)
        {
            for (int y = rect.Top; y < rect.Bottom; y++)
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    var newIndex = y * newRect.Width + x;
                    var oldIndex = (y - rect.Top) * rect.Width + (x - rect.Left);

                    newPixels[newIndex] = pixelMap.Pixels[oldIndex];
                }
        }

        private static PixelMap Crop(PixelMap image, Rectangle rect)
        {
            if (image.IsEmpty)
                return image;

            var intersect = Rectangle.Intersect(new Rectangle(0, 0, image.Width, image.Height), rect);
            if (intersect.IsEmpty)
                return new PixelMap();

            var newPixels = new Pixel[intersect.Width * intersect.Height];
            Array.Fill(newPixels, Pixel.FromArgb(255, 0, 0, 0));
            for (int y = intersect.Y; y < intersect.Bottom; y++)
                for (int x = intersect.X; x < intersect.Right; x++)
                {
                    var newIndex = (y - intersect.Y) * intersect.Width + (x - intersect.X);
                    var oldIndex = y * image.Width + x;

                    newPixels[newIndex] = image.Pixels[oldIndex];
                }

            return new PixelMap(intersect.Width, intersect.Height, newPixels);
        }

        public event Action ImageChanged;
    }
}