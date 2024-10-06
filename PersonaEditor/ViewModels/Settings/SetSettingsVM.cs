using System.Windows.Input;
using PersonaEditor.Common;

namespace PersonaEditor.ViewModels.Settings
{
    class SetSettingsVM : BindingObject
    {
        public SetSettingsVM()
        {
            ClickOk = new RelayCommand(Ok);
        }

        public MainVM MainVM { get; } = new MainVM();

        public ICommand ClickOk { get; }

        public async void Ok()
        {
            MainVM.Save();

            var path = Static.Paths.AppSettings;
            Static.SettingsProvider.Save();
        }
    }
}