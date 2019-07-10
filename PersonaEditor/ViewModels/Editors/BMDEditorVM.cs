using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using PersonaEditor.Classes;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib;
using PersonaEditorLib.Text;
using PersonaEditor.Classes.Managers;

namespace PersonaEditor.ViewModels.Editors
{
    class BMDEditorVM : BindingObject, IEditor
    {
        EventWrapperINPC EncodingEW;

        private bool _IsEdit = false;
        public bool IsEdit
        {
            get { return _IsEdit; }
            set
            {
                if (value)
                {
                    _IsEdit = value;
                    IsSelectCharList = false;
                    Notify("IsSelectCharList");
                }
                else
                {
                    if (Save())
                    {
                        _IsEdit = value;
                        IsSelectCharList = true;
                        Notify("IsSelectCharList");
                    }
                }

                Notify("IsEdit");
            }
        }

        private int sourceFont;
        private int destFont;

        public int SelectedSourceFont
        {
            get { return sourceFont; }
            set
            {
                sourceFont = value;
                ApplicationSettings.AppSetting.Default.BMDFontDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                Update();
                Notify("SelectedSourceFont");
            }
        }

        public int SelectedDestFont
        {
            get { return destFont; }
            set
            {
                destFont = value;
                ApplicationSettings.AppSetting.Default.BMDFontDestDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                Notify("SelectedDestFont");
            }
        }

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;
        public bool IsSelectCharList { get; set; } = true;

        public ObservableCollection<BMDNameVM> NameList { get; } = new ObservableCollection<BMDNameVM>();

        public ObservableCollection<BMDMsgVM> MsgList { get; } = new ObservableCollection<BMDMsgVM>();

        private string Name = "";

        private bool Save()
        {
            var result = MessageBox.Show("Save changes?", Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes | result == MessageBoxResult.No)
            {
                bool save = result == MessageBoxResult.Yes ? true : false;

                foreach (var a in NameList)
                    a.Changes(save, destFont);
                foreach (var a in MsgList)
                    a.Changes(save, destFont);

                return true;
            }
            else return false;
        }

        private void Update()
        {
            foreach (var a in NameList)
                a.Update(sourceFont);
            foreach (var a in MsgList)
                a.Update(sourceFont);
        }

        public bool Close()
        {
            if (IsEdit)
                if (!Save())
                    return false;

            NameList.Clear();
            MsgList.Clear();
            return true;
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PersonaEncodingManager man)
            {
                if (IsSelectCharList && e.PropertyName == man.GetPersonaEncodingName(sourceFont))
                    Update();
            }
        }

        public BMDEditorVM(GameFile objbmd)
        {
            if (objbmd.GameData is BMD bmd)
            {
                EncodingEW = new EventWrapperINPC(Static.EncodingManager, this);

                int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(ApplicationSettings.AppSetting.Default.BMDFontDefault);
                if (sourceInd >= 0)
                    sourceFont = sourceInd;
                else
                    sourceFont = 0;

                sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(ApplicationSettings.AppSetting.Default.BMDFontDestDefault);
                if (sourceInd >= 0)
                    destFont = sourceInd;
                else
                    destFont = 0;


                foreach (var a in bmd.Name)
                    NameList.Add(new BMDNameVM(a, sourceFont));
                foreach (var a in bmd.Msg)
                    MsgList.Add(new BMDMsgVM(a, sourceFont));

                Name = objbmd.Name;
            }
        }
    }
}