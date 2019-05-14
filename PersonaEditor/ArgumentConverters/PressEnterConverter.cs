using AuxiliaryLibraries.WPF.Interactivity;
using System.Windows.Input;

namespace PersonaEditor.ArgumentConverters
{
    class PressEnterConverter : ICommandArgConverter
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