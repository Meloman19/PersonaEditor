using AuxiliaryLibraries.Media;

namespace PersonaEditorLib
{
    public interface IImage
    {
        Bitmap GetBitmap();
        void SetBitmap(Bitmap bitmap);
    }
}