using AuxiliaryLibraries.WPF.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonaEditorGUI.ArgConverters
{
    class MouseWheelConverter : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            MouseWheelEventArgs arg = args[1] as MouseWheelEventArgs;
            if (arg.Delta > 0)
                return 1;
            else if (arg.Delta < 0)
                return -1;
            else
                return 0;
        }
    }
}
