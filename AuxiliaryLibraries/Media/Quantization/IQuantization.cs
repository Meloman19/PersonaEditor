using System.Drawing;

namespace AuxiliaryLibraries.Media.Quantization
{
    public interface IQuantization
    {
        PixelFormat PixelFormat { get; set; }

        bool StartQuantization(Color[] image, int colorMax);

        Color[] QuantPalette { get; }
        byte[] QuantData { get; }
    }
}