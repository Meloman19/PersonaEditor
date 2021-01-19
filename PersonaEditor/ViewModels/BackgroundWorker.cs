using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PersonaEditor.ViewModels
{
    class BackgroundWorker
    {
        public static int ProgressMaximum { get; set; }
        public static int ProgressValue { get; set; }

        public static UserControl Control { get; set; }
        public static string Status { get; set; }
    }
}
