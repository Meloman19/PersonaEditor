using PersonaEditorLib;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PersonaEditorGUI.Controls
{
    class PreviewEditorTabControlVM : BindingObject
    {
        private ObservableCollection<ClosableTabItemVM> tabCollection = new ObservableCollection<ClosableTabItemVM>();
        private int selectedTabIndex = 0;

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

        public DragEventHandler Drop => SingleFileEdit_Drop;
        private void SingleFileEdit_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(ObjectFile));
            if (data is ObjectFile objF)
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

        public bool Open(ObjectFile sender)
        {
            if (sender.Object is IPersonaFile pf)
            {
                TabItemType DataContextType = TabItemType.Null;
                object DataContext;
                string Title = sender.Name;

                if (pf.Type == FileType.SPR)
                {
                    DataContext = new Editors.SPREditorVM(sender.Object as PersonaEditorLib.FileStructure.SPR.SPR);
                    DataContextType = TabItemType.SPR;
                }
                else if (pf.Type == FileType.PTP)
                {
                    DataContext = new Editors.PTPEditorVM(sender.Object as PersonaEditorLib.FileStructure.Text.PTP);
                    DataContextType = TabItemType.PTP;
                }
                else if (pf.Type == FileType.BMD)
                {
                    DataContext = new Editors.BMDEditorVM(sender);
                    DataContextType = TabItemType.BMD;
                }
                else if (pf.Type == FileType.FNT)
                {
                    DataContext = new Editors.FNTEditorVM(sender.Object as PersonaEditorLib.FileStructure.FNT.FNT);
                    DataContextType = TabItemType.FNT;
                }
                else if (pf.Type == FileType.DAT)
                {
                    DataContext = new Editors.HEXEditorVM(sender.Object as PersonaEditorLib.FileStructure.DAT);
                    DataContextType = TabItemType.HEX;
                }
                else
                {
                    return false;
                }

                ClosableTabItemVM closableTabItemVM = new ClosableTabItemVM();
                closableTabItemVM.DataContext = DataContext;
                closableTabItemVM.SetDataContextType(DataContextType);
                closableTabItemVM.TabTitle = Title;
                closableTabItemVM.PropertyChanged += ClosableTabItemVM_PropertyChanged;

                tabCollection.Add(closableTabItemVM);
                SelectedTabIndex = tabCollection.IndexOf(closableTabItemVM);
                return true;
            }

            return false;
        }

        public void SetPreview(ImageSource Preview)
        {
            if (Preview == null)
            {
                tabCollection[0].SetDataContextType(TabItemType.Null);
                tabCollection[0].DataContext = null;
            }
            else
            {
                tabCollection[0].SetDataContextType(TabItemType.Image);
                tabCollection[0].DataContext = Preview;
            }
        }

        private void ClosableTabItemVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            tabCollection.Remove(sender as ClosableTabItemVM);
        }

        public PreviewEditorTabControlVM()
        {
            string tabtitle = "";
            if (Application.Current.Resources.MergedDictionaries[0].Contains("fileedit_Preview"))
                tabtitle = Application.Current.Resources.MergedDictionaries[0]["fileedit_Preview"] as string;

            tabCollection.Add(new ClosableTabItemVM() { TabTitle = tabtitle, IsClosable = false });
            TabCollection = new ReadOnlyObservableCollection<ClosableTabItemVM>(tabCollection);
        }
    }
}