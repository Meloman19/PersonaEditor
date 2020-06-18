using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaEditor.Controls
{
    interface IScrollInfo2
    {
        void ZoomTo(Point point, double zoomFactor);
    }
}
