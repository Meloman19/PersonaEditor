using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorGUI.Settings
{
    class MainVM : BindingObject, ISetting
    {
        App AppSetting = new App();

        public ObservableCollection<string> LangList { get; } = new ObservableCollection<string>() { "Default" };
        public int SelectedLangIndex
        {
            get
            {
                if (LangList.Contains(AppSetting.DefaultLocalization))
                    return LangList.IndexOf(AppSetting.DefaultLocalization);
                else
                    return 0;
            }
            set
            {
                if (LangList.Count > value)
                    AppSetting.DefaultLocalization = LangList[value];
                Notify("SelectedLangIndex");
            }
        }

        public MainVM()
        {
            AppSetting.PropertyChanged += AppSetting_PropertyChanged;
            LoadLangList();
        }

        private void AppSetting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Notify("IsChanged");
        }

        public void Save()
        {
            AppSetting.Save();
            Notify("IsChanged");
        }

        public void Reset()
        {
            AppSetting.Reload();
            Notify("IsChanged");
        }

        private void LoadLangList()
        {
            if (Directory.Exists(Static.Paths.DirLang))
            {
                var list = Directory.GetFiles(Static.Paths.DirLang);
                foreach (var file in list)
                    if (Path.GetExtension(file).ToLower() == ".xml")
                        LangList.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
    }
}