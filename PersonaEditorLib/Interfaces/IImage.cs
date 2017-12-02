using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PersonaEditorLib.Interfaces
{
    public interface IImage
    {
        BitmapSource Image { get; set; }
    }
}