using System.Drawing;

namespace AuxiliaryLibraries.Media.Quantization
{
    public interface IQuantization
    {
        Color[] QuantPalette { get; }

        byte[] QuantData { get; }

        bool StartQuantization(Color[] image, int colorMax);
    }
}