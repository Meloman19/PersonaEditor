using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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