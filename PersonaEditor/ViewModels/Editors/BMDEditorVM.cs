using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using PersonaEditor.Classes;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib;
using PersonaEditorLib.Text;
using PersonaEditor.Classes.Managers;
using PersonaEditor.ViewModels.Tools;
using PersonaEditor.Views.Tools;

namespace PersonaEditor.ViewModels.Editors
{
    class FindClass
    {
        public string Text { get; set; }
    }

    class BMDEditorVM : BindingObject, IEditor
    {
        EventWrapperINPC EncodingEW;

        private List<FindClass> findListString = new List<FindClass>();

        public List<FindClass> FindStringList
        {
            get
            {
                return findListString;
            }
            set
            {
                findListString = value;
                Notify("FindStringList");
            }
        }

        public ICommand FromCommand { get; }
        public ICommand TestCommand { get; }
       

        private string _SearchText = "";
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                Notify("SearchText");
            }
        }

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

        public void Replace()
        {
            MessageBox.Show("Use PTP Editor");
            //using (StreamWriter sw = new StreamWriter("out.txt"))
            //{
            //    List<string> textlist = NotaScript.Parse("text.txt");

            //    foreach (var ms in MsgList)
            //    {
            //        for (int i = 0; i < ms.StringList.Count; i++)
            //        {
            //            for (int j = 0; j < textlist.Count; j++)
            //            {
            //                if (textlist[j].Contains(ms.Name))
            //                {
            //                    string start_bytes = "";
            //                    string end_bytes = "";

            //                    for (int k = 0; k < ms.StringList[i].Text.Length; k++)
            //                    {
            //                        start_bytes += ms.StringList[i].Text[k];
            //                        if (ms.StringList[i].Text[k] == '}' && ms.StringList[i].Text[k + 1] != '{') break;
            //                    }

            //                    for (int k = ms.StringList[i].Text.Length-1; k >= 0; k--)
            //                    {
            //                        end_bytes += ms.StringList[i].Text[k];
            //                        if (ms.StringList[i].Text[k] == '{' && ms.StringList[i].Text[k - 1] != '}')
            //                        {
            //                            end_bytes = Reverse(end_bytes);
            //                            break;
            //                        }
            //                    }

            //                    string[] t = textlist[j].Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            //                    sw.WriteLine("[From editor]: " + ms.Name + " ; " + ms.StringList[i].Text + " | [From file]: " + t[t.Length - 1]);
            //                    ms.StringList[i].Text = start_bytes + t[t.Length - 1] + end_bytes;
            //                    Notify("Text");
            //                    Notify("StringList");
            //                    Notify("MsgList");
            //                    sw.WriteLine("[From editor]: " + ms.Name + " ; " + ms.StringList[i].Text + " | [From file]: " + t[t.Length - 1]);

            //                    textlist.RemoveAt(j);

            //                    break;
            //                }
            //            }
            //        }
            //    }

            //}
        }

        void Test()
        {

        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public BMDEditorVM(GameFile objbmd)
        {
            FromCommand = new RelayCommand(Replace);
            TestCommand = new RelayCommand(Test);

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