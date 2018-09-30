using AuxiliaryLibraries.WPF;
using System.Windows.Media.Imaging;

namespace PersonaEditor.ViewModels.Editors
{
    class FNT0GlyphCut : BindingObject
    {
        public BitmapSource Image { get; } = null;
        public int Left { get; set; } = 0;
        public int Right { get; set; } = 0;
        public int Top { get; set; } = 0;
        public int Bottom { get; set; } = 0;

        public FNT0GlyphCut(BitmapSource bitmapSource, int left, int right, int top, int bottom)
        {
            Image = bitmapSource;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}
