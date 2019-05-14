using AuxiliaryLibraries.WPF.Interactivity;
using System.Windows.Input;

namespace PersonaEditor.ArgumentConverters
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