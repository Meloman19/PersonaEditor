using AuxiliaryLibraries.WPF.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorGUI.ArgConverters
{
    class MouseButtonConverter : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            return args[1];
        }
    }
}
