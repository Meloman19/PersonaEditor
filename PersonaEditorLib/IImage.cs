using AuxiliaryLibraries.Media;

namespace PersonaEditorLib
{
    public interface IImage
    {
        PixelMap GetBitmap();
        void SetBitmap(PixelMap bitmap);
    }
}