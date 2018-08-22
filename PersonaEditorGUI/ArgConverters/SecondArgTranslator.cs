using AuxiliaryLibraries.WPF.Interactivity;

namespace PersonaEditorGUI.ArgConverters
{
    class SecondArgTranslator : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            return args[1];
        }
    }
}