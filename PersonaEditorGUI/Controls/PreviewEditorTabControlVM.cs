using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.GameFormat.Other;
using AuxiliaryLibraries.GameFormat.SpriteContainer;
using AuxiliaryLibraries.GameFormat.Text;
using AuxiliaryLibraries.WPF;
using PersonaEditorGUI.Classes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditorGUI.Controls
{
    class PreviewEditorTabControlVM : BindingObject
    {
        private ObservableCollection<ClosableTabItemVM> tabCollection = new ObservableCollection<ClosableTabItemVM>();
        private int selectedTabIndex = 0;
        private ImagePreviewVM previewVM = new ImagePreviewVM();

        public ReadOnlyObservableCollection<ClosableTabItemVM> TabCollection { get; }
        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set
            {
                if (selectedTabIndex != value)
                {
                    selectedTabIndex = value;
                    Notify("SelectedTabIndex");
                }
            }
        }

        public ICommand Drop { get; }
        private void SingleFileEdit_Drop(object arg)
        {
            var data = (arg as DragEventArgs).Data.GetData(typeof(TreeViewItemVM));
            if (data is TreeViewItemVM objF)
                Open(objF);
        }

        public bool CloseAll()
        {
            bool returned = true;

            var list = tabCollection.ToList();
            foreach (var a in list)
                returned = returned & a.Close();

            return returned;
        }

        public bool Open(TreeViewItemVM sender)
        {
            if (!sender.CanEdit())
            {
                MessageBox.Show(String.Format("Can't open {0}", sender.Header));
                return false;
            }

            if (sender.PersonaFile.Object is IGameFile pf)
            {
                TabItemType DataContextType = TabItemType.Null;
                object DataContext;
                string Title = sender.PersonaFile.Name;

                if (pf.Type == FormatEnum.SPR)
                {
                    DataContext = new Editors.SPREditorVM(sender.PersonaFile.Object as SPR);
                    DataContextType = TabItemType.SPR;
                }
                else if (pf.Type == FormatEnum.SPD)
                {
                    DataContext = new Editors.SPDEditorVM(sender.PersonaFile.Object as SPD);
                    DataContextType = TabItemType.SPR;
                }
                else if (pf.Type == FormatEnum.PTP)
                {
                    DataContext = new Editors.PTPEditorVM(sender.PersonaFile.Object as PTP);
                    DataContextType = TabItemType.PTP;
                }
                else if (pf.Type == FormatEnum.BMD)
                {
                    DataContext = new Editors.BMDEditorVM(sender.PersonaFile);
                    DataContextType = TabItemType.BMD;
                }
                else if (pf.Type == FormatEnum.FTD)
                {
                    DataContext = new Editors.FTDEditorVM(sender.PersonaFile.Object as FTD);
                    DataContextType = TabItemType.FTD;
                }
                //else if (pf.Type == FileType.FNT)
                //{
                //    DataContext = new Editors.FNTEditorVM(sender.personaFile.Object as PersonaEditorLib.FileStructure.FNT.FNT);
                //    DataContextType = TabItemType.FNT;
                //}
                else if (pf.Type == FormatEnum.DAT)
                {
                    DataContext = new Editors.HEXEditorVM(sender.PersonaFile.Object as DAT);
                    DataContextType = TabItemType.HEX;
                }
                else
                {
                    return false;
                }

                ClosableTabItemVM closableTabItemVM = new ClosableTabItemVM();
                closableTabItemVM.DataContext = DataContext;
                closableTabItemVM.DataContextType = DataContextType;
                closableTabItemVM.TabTitle = Title;
                closableTabItemVM.PropertyChanged += ClosableTabItemVM_PropertyChanged;
                closableTabItemVM.PersonaFile = sender;

                tabCollection.Add(closableTabItemVM);
                SelectedTabIndex = tabCollection.IndexOf(closableTabItemVM);

                sender.UnEnable();
                return true;
            }

            return false;
        }

        public void SetPreview(ImageSource Preview)
        {
            previewVM.SourceIMG = Preview;
        }

        private void ClosableTabItemVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            tabCollection.Remove(sender as ClosableTabItemVM);
        }

        public PreviewEditorTabControlVM()
        {
            string tabtitle = "";
            tabtitle = Application.Current.Resources.MergedDictionaries.GetString("main_Preview");
            Drop = new RelayCommand(SingleFileEdit_Drop);
            tabCollection.Add(new ClosableTabItemVM() { TabTitle = tabtitle, IsClosable = false, DataContext = previewVM, DataContextType = TabItemType.ImagePreview });
            TabCollection = new ReadOnlyObservableCollection<ClosableTabItemVM>(tabCollection);
        }
    }
}