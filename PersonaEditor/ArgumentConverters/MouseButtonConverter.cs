using AuxiliaryLibraries.WPF.Interactivity;
using System.Windows.Input;

namespace PersonaEditor.ArgumentConverters
{
    class MouseButtonConverter : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            return (args[1] as MouseButtonEventArgs).ChangedButton;
        }
    }
}