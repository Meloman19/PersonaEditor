using PersonaEditorLib.Other;
using AuxiliaryLibraries.Tools;
using AuxiliaryLibraries.WPF;
using AuxiliaryLibraries.WPF.Extensions;
using PersonaEditor.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Editors
{
    class GlyphCut : BindingObject
    {
        public BitmapSource Image { get; } = null;
        public int Left { get; set; } = 0;
        public int Right { get; set; } = 0;
        public int Index { get; }

        public GlyphCut(BitmapSource bitmapSource, VerticalCut verticalCut, int index)
        {
            Image = bitmapSource;
            Left = verticalCut.Left;
            Right = verticalCut.Right;
            Index = index;
        }
    }

    class FNTEditorVM : BindingObject, IEditor
    {
        private bool edited = false;
        private FNT fnt;
        private GeometryDrawing Left = new GeometryDrawing(null, new Pen(new SolidColorBrush(Colors.SkyBlue), 0.3), null);
        private GeometryDrawing Rigth = new GeometryDrawing(null, new Pen(new SolidColorBrush(Colors.IndianRed), 0.3), null);
        private GlyphCut selectedGlyph = null;

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

            Left.Geometry = new LineGeometry(new Point(LeftCut, 0), new Point(LeftCut, MaxWidth));
            Rigth.Geometry = new LineGeometry(new Point(RightCut, 0), new Point(RightCut, MaxWidth));

            Glyph.Children.Add(Left);
            Glyph.Children.Add(Rigth);
        }

        public ObservableCollection<GlyphCut> GlyphCuts { get; } = new ObservableCollection<GlyphCut>();
        public GlyphCut SelectedItem
        {
            set
            {
                if (value != selectedGlyph)
                {
                    selectedGlyph = value;
                    DrawImage();
                    Notify("LeftCut");
                    Notify("RightCut");
                }
            }
        }
        public DrawingGroup Glyph { get; } = new DrawingGroup();
        public int MaxWidth { get; }
        public int LeftCut
        {
            get { return selectedGlyph?.Left ?? 0; }
            set
            {
                if (selectedGlyph != null && selectedGlyph.Left != value)
                {
                    selectedGlyph.Left = value;
                    Left.Geometry = new LineGeometry(new Point(value, 0), new Point(value, MaxWidth));
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
                    Rigth.Geometry = new LineGeometry(new Point(value, 0), new Point(value, MaxWidth));
                    edited = true;
                }

                Notify("RightCut");
            }
        }
        public Visibility VerticalSliderVisible { get; } = Visibility.Collapsed;

        public FNTEditorVM(FNT fnt)
        {
            this.fnt = fnt;
            MaxWidth = fnt.Header.Glyphs.Size1;
            Glyph.ClipGeometry = new RectangleGeometry(new Rect(0, 0, MaxWidth, MaxWidth));
            OpenFont();
        }

        private void OpenFont()
        {
            var GlyphList = fnt.Compressed.GetDecompressedData();
            var CutList = fnt.WidthTable.WidthTable;

            PixelFormat pixelFormat;
            if (fnt.Header.Glyphs.BitsPerPixel == 4)
                pixelFormat = PixelFormats.Indexed4;
            else if (fnt.Header.Glyphs.BitsPerPixel == 8)
                pixelFormat = PixelFormats.Indexed8;
            else
                pixelFormat = PixelFormats.Default;

            if (pixelFormat == PixelFormats.Indexed4)
                ArrayTool.ReverseByteInList(GlyphList);

            var pallete = new BitmapPalette(fnt.Palette.GetImagePalette().Select(x => Color.FromArgb(x.A, x.R, x.G, x.B)).ToArray());

            if (GlyphList.Count <= CutList.Count)
            {
                for (int i = 0; i < GlyphList.Count; i++)
                {
                    var image = BitmapSource.Create(fnt.Header.Glyphs.Size1,
                        fnt.Header.Glyphs.Size2,
                        96, 96, pixelFormat, pallete,
                        GlyphList[i],
                        (pixelFormat.BitsPerPixel * fnt.Header.Glyphs.Size2 + 7) / 8);
                    image.Freeze();
                    GlyphCuts.Add(new GlyphCut(image, CutList[i], i));
                }
            }
        }

        public bool Close()
        {
            if (edited)
            {
                var result = MessageBox.Show("Save changes?", "Saving", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var CutList = fnt.WidthTable.WidthTable;
                    for (int i = 0; i < GlyphCuts.Count; i++)
                        CutList[i] = new VerticalCut((byte)GlyphCuts[i].Left, (byte)GlyphCuts[i].Right);
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