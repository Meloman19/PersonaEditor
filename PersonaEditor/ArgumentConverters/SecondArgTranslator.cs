using AuxiliaryLibraries.WPF.Interactivity;

namespace PersonaEditor.ArgumentConverters
{
    class SecondArgTranslator : ICommandArgConverter
    {
        public object GetArguments(object[] args)
        {
            return args[1];
        }
    }
}