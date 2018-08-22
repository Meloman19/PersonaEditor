using AuxiliaryLibraries.WPF;
using System.Windows.Input;

namespace PersonaEditorGUI.Controls.SettingsWindow
{
    class SetSettingsVM : BindingObject
    {
        public DefaultBackgroundVM DefaultBackgroundVM { get; } = new DefaultBackgroundVM();
        public MainVM MainVM { get; } = new MainVM();

        public ICommand ClickOk { get; }
        public void Ok()
        {
            DefaultBackgroundVM.Save();
            MainVM.Save();
        }

        public SetSettingsVM()
        {
            ClickOk = new RelayCommand(Ok);
        }
    }
}