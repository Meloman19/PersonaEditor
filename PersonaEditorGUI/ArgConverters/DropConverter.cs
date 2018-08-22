using AuxiliaryLibraries.WPF.Interactivity;

namespace PersonaEditorGUI.ArgConverters
{
    class DropConverter : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {            
            return args[1];
        }
    }
}