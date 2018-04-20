using PersonaEditorGUI.Classes;
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

        public DragEventHandler Drop => SingleFileEdit_Drop;
        private void SingleFileEdit_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(typeof(UserTreeViewItem));
            if (data is UserTreeViewItem objF)
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

        public bool Open(UserTreeViewItem sender)
        {
            if (!sender.CanEdit())
            {
                MessageBox.Show(String.Format("Can't open {0}", sender.Header));
                return false;
            }

            if (sender.PersonaFile.Object is IPersonaFile pf)
            {
                TabItemType DataContextType = TabItemType.Null;
                object DataContext;
                string Title = sender.PersonaFile.Name;

                if (pf.Type == FileType.SPR)
                {
                    DataContext = new Editors.SPREditorVM(sender.PersonaFile.Object as PersonaEditorLib.FileStructure.SPR.SPR);
                    DataContextType = TabItemType.SPR;
                }
                else if (pf.Type == FileType.SPD)
                {
                    DataContext = new Editors.SPREditorVM(sender.PersonaFile.Object as PersonaEditorLib.FileStructure.SPR.SPD);
                    DataContextType = TabItemType.SPR;
                }
                else if (pf.Type == FileType.PTP)
                {
                    DataContext = new Editors.PTPEditorVM(sender.PersonaFile.Object as PersonaEditorLib.FileStructure.Text.PTP);
                    DataContextType = TabItemType.PTP;
                }
                else if (pf.Type == FileType.BMD)
                {
                    DataContext = new Editors.BMDEditorVM(sender.PersonaFile);
                    DataContextType = TabItemType.BMD;
                }
                else if (pf.Type == FileType.FTD)
                {
                    DataContext = new Editors.FTDEditorVM(sender.PersonaFile.Object as PersonaEditorLib.FileStructure.Text.FTD);
                    DataContextType = TabItemType.FTD;
                }
                //else if (pf.Type == FileType.FNT)
                //{
                //    DataContext = new Editors.FNTEditorVM(sender.personaFile.Object as PersonaEditorLib.FileStructure.FNT.FNT);
                //    DataContextType = TabItemType.FNT;
                //}
                else if (pf.Type == FileType.DAT)
                {
                    DataContext = new Editors.HEXEditorVM(sender.PersonaFile.Object as PersonaEditorLib.FileStructure.DAT);
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

            tabCollection.Add(new ClosableTabItemVM() { TabTitle = tabtitle, IsClosable = false, DataContext = previewVM, DataContextType = TabItemType.ImagePreview });
            TabCollection = new ReadOnlyObservableCollection<ClosableTabItemVM>(tabCollection);
        }
    }
}