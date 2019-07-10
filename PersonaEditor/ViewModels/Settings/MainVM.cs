using AuxiliaryLibraries.WPF;
using PersonaEditor.ApplicationSettings;
using System.Collections.ObjectModel;
using System.IO;

namespace PersonaEditor.ViewModels.Settings
{
    class MainVM : BindingObject
    {
        AppSetting AppSetting_Temp = new AppSetting();

        public ObservableCollection<string> LangList { get; } = new ObservableCollection<string>() { "Default" };
        public int SelectedLangIndex
        {
            get
            {
                if (LangList.Contains(AppSetting_Temp.DefaultLocalization))
                    return LangList.IndexOf(AppSetting_Temp.DefaultLocalization);
                else
                    return 0;
            }
            set
            {
                if (LangList.Count > value)
                    AppSetting_Temp.DefaultLocalization = LangList[value];
                Notify("SelectedLangIndex");
            }
        }

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;

        public bool CopyOld2New
        {
            get { return AppSetting_Temp.SaveAsPTP_CO2N; }
            set
            {
                AppSetting_Temp.SaveAsPTP_CO2N = value;
                Notify("CopyOld2New");
            }
        }

        public int SelectedFontSave
        {
            get
            {
                int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(AppSetting_Temp.SaveAsPTP_Font);
                if (sourceInd >= 0)
                    return sourceInd;
                else
                    return 0;
            }
            set
            {
                AppSetting_Temp.SaveAsPTP_Font = Static.EncodingManager.GetPersonaEncodingName(value);
                Notify("SelectedFontSave");
            }
        }

        public int SelectedFontOpen
        {
            get
            {
                int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(AppSetting_Temp.OpenPTP_Font);
                if (sourceInd >= 0)
                    return sourceInd;
                else
                    return 0;
            }
            set
            {
                AppSetting_Temp.OpenPTP_Font = Static.EncodingManager.GetPersonaEncodingName(value);
                Notify("SelectedFontOpen");
            }
        }

        public bool SingleInstanceApplication
        {
            get { return AppSetting_Temp.SingleInstanceApplication; }
            set
            {
                AppSetting_Temp.SingleInstanceApplication = value;
                Notify("SingleInstanceApplication");
            }
        }

        public MainVM()
        {
            LoadLangList();
        }

        public void Save()
        {
            AppSetting_Temp.Save();
            AppSetting.Default.Reload();
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