using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonaEditorGUI.Settings
{
    class SetSettingsVM : BindingObject
    {
        private bool _SettingChange = false;
        public bool SettingChange
        {
            get { return _SettingChange; }
            set
            {
                if (value != _SettingChange)
                {
                    _SettingChange = value;
                    Notify("SettingChange");
                }
            }
        }

        public DefaultBackgroundVM DefaultBackgroundVM { get; } = new DefaultBackgroundVM();

        public void Reset()
        {
            DefaultBackgroundVM.Reset();
            SettingChange = false;
        }

        public ICommand ClickOk { get; }
        public void Ok()
        {
            DefaultBackgroundVM.Save();
        }

        public ICommand ClickApply { get; }
        public void Apply()
        {
            DefaultBackgroundVM.Save();
            SettingChange = false;
        }

        public SetSettingsVM()
        {
            ClickOk = new RelayCommand(Ok);
            ClickApply = new RelayCommand(Apply);

            DefaultBackgroundVM.PropertyChanged += DefaultBackgroundVM_PropertyChanged;
        }

        private void DefaultBackgroundVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChanged")
                SettingChange = true;
        }
    }
}