using PersonaEditorLib.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Interfaces
{
    public interface IQuantization
    {
        PixelFormat PixelFormat { get; set; }

        bool StartQuantization(BitmapSource bitmapSource);
        bool StartQuantization(Color[] image);

        Color[] QuantPalette { get; }
        byte[] QuantData { get; }
    }
}