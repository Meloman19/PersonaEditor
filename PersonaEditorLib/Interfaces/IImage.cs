using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Interfaces
{
    public interface IImage
    {
        BitmapSource GetImage();
        void SetImage(BitmapSource bitmapSource);
    }
}