using AuxiliaryLibraries.WPF.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonaEditorGUI.ArgConverters
{
    class MouseButtonConverter : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            return (args[1] as MouseButtonEventArgs).ChangedButton;
        }
    }
}
