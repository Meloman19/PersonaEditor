using PersonaEditorLib.Other;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.WPF.Extensions;
using PersonaEditor.Classes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Editors
{
    class FNT0EditorVM : BindingObject, IEditor
    {
        private bool edited = false;
        private FNT0 fnt;
        private GeometryDrawing left = new GeometryDrawing(null, new Pen(new SolidColorBrush(Colors.SkyBlue), 0.3), null);
        private GeometryDrawing rigth = new GeometryDrawing(null, new Pen(new SolidColorBrush(Colors.IndianRed), 0.3), null);
        private GeometryDrawing top = new GeometryDrawing(null, new Pen(new SolidColorBrush(Colors.SkyBlue), 0.3), null);
        private GeometryDrawing bottom = new GeometryDrawing(null, new Pen(new SolidColorBrush(Colors.IndianRed), 0.3), null);
        private FNT0GlyphCut selectedGlyph = null;

        private void DrawImage()
        {
            Glyph.Children.Clear();
            if (selectedGlyph == null)
                return;

            BitmapSource bitmapSource = selectedGlyph.Image;
            var colors = bitmapSource.GetColors();
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;

            Glyph.Children.Add(Utilities.DrawBitmap(width, height, colors));

            left.Geometry = new LineGeometry(new Point(LeftCut, 0), new Point(LeftCut, MaxWidth));
            rigth.Geometry = new LineGeometry(new Point(RightCut, 0), new Point(RightCut, MaxWidth));
            top.Geometry = new LineGeometry(new Point(0, TopCut), new Point(MaxHeight, TopCut));
            bottom.Geometry = new LineGeometry(new Point(0, BottomCut), new Point(MaxHeight, BottomCut));

            Glyph.Children.Add(left);
            Glyph.Children.Add(rigth);
            Glyph.Children.Add(top);
            Glyph.Children.Add(bottom);
        }

        private void OpenFont()
        {
            var GlyphList = fnt.Compressed.GetDecompressedData();
            var pallete = new BitmapPalette(AuxiliaryLibraries.Media.ImageHelper.GetGrayPalette(8).Select(x => Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray());
            var pixelFormat = PixelFormats.Indexed8;

            foreach (var glyph in GlyphList)
            {
                var image = BitmapSource.Create(fnt.Width, fnt.Height, 96, 96,
                    pixelFormat, pallete, glyph.Skip(6).ToArray(),
                    (pixelFormat.BitsPerPixel * fnt.Width + 7) / 8);
                image.Freeze();
                GlyphCuts.Add(new FNT0GlyphCut(image, glyph[0], glyph[0] + glyph[2], glyph[1], glyph[1] + glyph[3]));
            }
        }

        public ObservableCollection<FNT0GlyphCut> GlyphCuts { get; } = new ObservableCollection<FNT0GlyphCut>();
        public FNT0GlyphCut SelectedItem
        {
            set
            {
                if (value != selectedGlyph)
                {
                    selectedGlyph = value;
                    DrawImage();
                    Notify("LeftCut");
                    Notify("RightCut");
                    Notify("TopCut");
                    Notify("BottomCut");
                }
            }
        }
        public DrawingGroup Glyph { get; } = new DrawingGroup();
        public Visibility VerticalSliderVisible { get; } = Visibility.Visible;

        public int MaxWidth { get; }
        public int MaxHeight { get; }

        public int LeftCut
        {
            get { return selectedGlyph?.Left ?? 0; }
            set
            {
                if (selectedGlyph != null && selectedGlyph.Left != value)
                {
                    selectedGlyph.Left = value;
                    left.Geometry = new LineGeometry(new Point(value, 0), new Point(value, MaxWidth));
                    edited = true;
                }

                Notify("LeftCut");
            }
        }
        public int RightCut
        {
            get { return selectedGlyph?.Right ?? 0; }
            set
            {
                if (selectedGlyph != null && selectedGlyph.Right != value)
                {
                    selectedGlyph.Right = value;
                    rigth.Geometry = new LineGeometry(new Point(value, 0), new Point(value, MaxWidth));
                    edited = true;
                }

                Notify("RightCut");
            }
        }
        public int TopCut
        {
            get { return selectedGlyph?.Top ?? 0; }
            set
            {
                if (selectedGlyph != null && selectedGlyph.Top != value)
                {
                    selectedGlyph.Top = value;
                    top.Geometry = new LineGeometry(new Point(0, value), new Point(MaxHeight, value));
                    edited = true;
                }

                Notify("TopCut");
            }
        }
        public int BottomCut
        {
            get { return selectedGlyph?.Bottom ?? 0; }
            set
            {
                if (selectedGlyph != null && selectedGlyph.Bottom != value)
                {
                    selectedGlyph.Bottom = value;
                    bottom.Geometry = new LineGeometry(new Point(0, value), new Point(MaxHeight, value));
                    edited = true;
                }

                Notify("BottomCut");
            }
        }

        public FNT0EditorVM(FNT0 fnt)
        {
            this.fnt = fnt;
            MaxWidth = fnt.Width;
            MaxHeight = fnt.Height;
            Glyph.ClipGeometry = new RectangleGeometry(new Rect(0, 0, MaxWidth, MaxWidth));
            OpenFont();
        }

        public bool Close()
        {
            if (edited)
            {
                var result = MessageBox.Show("Save changes?", "Saving", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    fnt.SetGlyphRect(GlyphCuts.Select(x => new System.Drawing.Rectangle(x.Left, x.Top, x.Right - x.Left, x.Bottom - x.Top)).ToArray());
                    return true;
                }
                else if (result == MessageBoxResult.No)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }
    }
}