using AuxiliaryLibraries.Extensions;
using AuxiliaryLibraries.Media;
using PersonaEditorLib.Text;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PersonaEditorLib
{
    public struct ImageData
    {
        public PixelMap PixelMap { get; }

        public bool IsEmpty => PixelMap == null || PixelMap.Pixels.Length == 0;

        public int PixelWidth => PixelMap == null ? 0 : PixelMap.Width;

        public int PixelHeight => PixelMap == null ? 0 : PixelMap.Height;

        public PixelMap GetImageSource()
        {
            if (IsEmpty)
                return null;
            else
                return PixelMap;
        }

        public ImageData()
        {
            PixelMap = null;
        }

        public ImageData(int pixelwidth, int pixelheight, Pixel[] pixels)
        {
            PixelMap = new PixelMap(pixelwidth, pixelheight, pixels);
        }

        public static ImageData DrawText(IEnumerable<TextBaseElement> text, PersonaFont personaFont, int LineSpacing)
        {
            if (text != null && personaFont != null)
            {
                ImageData returned = new ImageData();
                ImageData line = new ImageData();
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
                                ImageData glyph = new ImageData(personaFont.Width, personaFont.Height, Glyph.Item1);
                                byte Left = Glyph.Item2.Left;
                                byte Right = Glyph.Item2.Right;
                                glyph = Crop(glyph, new Rectangle(Left, 0, Right - Left, glyph.PixelMap.Height));
                                line = MergeLeftRight(line, glyph, 1);
                            }
                        }
                    }
                    else
                    {
                        if (a.Data.ArrayEquals(new byte[] { 0x0A }))
                        {
                            returned = MergeUpDown(returned, line, LineSpacing);
                            line = new ImageData();
                        }
                    }
                }
                returned = ImageData.MergeUpDown(returned, line, LineSpacing);
                return returned;
            }
            return new ImageData();
        }

        public static ImageData MergeLeftRight(ImageData left, ImageData right, int horizontalshift)
        {
            if (left.IsEmpty)
                return right;
            if (right.IsEmpty)
                return left;

            var leftRect = new Rectangle(0, 0, left.PixelMap.Width, left.PixelMap.Height);
            var rightRect = new Rectangle(left.PixelMap.Width + horizontalshift, 0, right.PixelMap.Width, right.PixelMap.Height);

            var union = Rectangle.Union(leftRect, rightRect);
            var newPixels = new Pixel[union.Width * union.Height];
            Array.Fill(newPixels, Pixel.FromArgb(255, 0, 0, 0));
            CopyPixels(newPixels, union, left.PixelMap, leftRect);
            CopyPixels(newPixels, union, right.PixelMap, rightRect);

            return new ImageData(union.Width, union.Height, newPixels);
        }

        public static ImageData MergeUpDown(ImageData up, ImageData down, int h)
        {
            if (up.IsEmpty)
                return down;
            if (down.IsEmpty)
                return up;

            var upRect = new Rectangle(0, 0, up.PixelMap.Width, up.PixelMap.Height);
            var downRect = new Rectangle(0, up.PixelMap.Height + h, down.PixelMap.Width, down.PixelMap.Height);

            var union = Rectangle.Union(upRect, downRect);
            var newPixels = new Pixel[union.Width * union.Height];
            Array.Fill(newPixels, Pixel.FromArgb(255, 0, 0, 0));
            CopyPixels(newPixels, union, up.PixelMap, upRect);
            CopyPixels(newPixels, union, down.PixelMap, downRect);

            return new ImageData(union.Width, union.Height, newPixels);
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

        public static ImageData Crop(ImageData image, Rectangle rect)
        {
            if (image.IsEmpty)
                return new ImageData();

            var intersect = Rectangle.Intersect(new Rectangle(0, 0, image.PixelMap.Width, image.PixelMap.Height), rect);
            if (intersect.IsEmpty)
                return new ImageData();

            var newPixels = new Pixel[intersect.Width * intersect.Height];
            Array.Fill(newPixels, Pixel.FromArgb(255, 0, 0, 0));
            for (int y = intersect.Y; y < intersect.Bottom; y++)
                for (int x = intersect.X; x < intersect.Right; x++)
                {
                    var newIndex = (y - intersect.Y) * intersect.Width + (x - intersect.X);
                    var oldIndex = y * image.PixelMap.Width + x;

                    newPixels[newIndex] = image.PixelMap.Pixels[oldIndex];
                }

            return new ImageData(intersect.Width, intersect.Height, newPixels);
        }
    }
}