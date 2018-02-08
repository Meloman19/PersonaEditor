using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        //public ObservableCollection<ISetting> SettingList { get; } = new ObservableCollection<ISetting>()
        //{
        //    new DefaultBackgroundVM(),
        //    new MainVM()
        //};

        public DefaultBackgroundVM DefaultBackgroundVM { get; } = new DefaultBackgroundVM();
        public MainVM MainVM { get; } = new MainVM();

        public void Reset()
        {
            DefaultBackgroundVM.Reset();
            MainVM.Reset();
            SettingChange = false;
        }

        public ICommand ClickOk { get; }
        public void Ok()
        {
            DefaultBackgroundVM.Save();
            MainVM.Save();
        }

        public ICommand ClickApply { get; }
        public void Apply()
        {
            DefaultBackgroundVM.Save();
            MainVM.Save();
            SettingChange = false;
        }

        public SetSettingsVM()
        {
            ClickOk = new RelayCommand(Ok);
            ClickApply = new RelayCommand(Apply);

            DefaultBackgroundVM.PropertyChanged += Setting_PropertyChanged;
            MainVM.PropertyChanged += Setting_PropertyChanged;
        }
        
        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChanged")
                SettingChange = true;
        }
    }
}