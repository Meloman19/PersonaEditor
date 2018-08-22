using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonaEditorGUI.ArgConverters
{
    class PressEnterConverter : AuxiliaryLibraries.WPF.Interactivity.ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            if ((args[1] as KeyEventArgs).Key == Key.Enter)
                return true;
            else
                return false;
        }
    }
}
