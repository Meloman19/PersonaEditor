using PersonaEditorLib;
using PersonaEditorLib.Other;
using PersonaEditorLib.SpriteContainer;
using PersonaEditorLib.Text;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Common;
using PersonaEditor.ViewModels.Editors;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditor.ViewModels
{
    class PreviewEditorTabControlVM : BindingObject
    {
        private ObservableCollection<ClosableTabItemVM> tabCollection = new ObservableCollection<ClosableTabItemVM>();
        private int _selectedTabIndex = 0;
        private ImagePreviewVM previewVM = new ImagePreviewVM();

        public PreviewEditorTabControlVM()
        {
            DropItemCommand = new RelayCommand(SingleFileEdit_Drop);
            tabCollection.Add(new ClosableTabItemVM() { TabTitle = "Preview", IsClosable = false, DataContext = previewVM });
            TabCollection = new ReadOnlyObservableCollection<ClosableTabItemVM>(tabCollection);
        }

        public ReadOnlyObservableCollection<ClosableTabItemVM> TabCollection { get; }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public ICommand DropItemCommand { get; }

        private void SingleFileEdit_Drop(object arg)
        {
            if (arg is GameFileTreeItem treeItem)
                Open(treeItem);
        }

        public bool CloseAll()
        {
            bool returned = true;

            var list = tabCollection.ToList();
            foreach (var a in list)
                returned = returned & a.Close();

            return returned;
        }

        public bool Open(GameFileTreeItem sender)
        {
            if (!sender.CanEdit())
            {
                MessageBox.Show(String.Format("Can't open {0}", sender.Header));
                return false;
            }

            object DataContext;
            string Title = sender.PersonaFile.Name;

            switch (sender.PersonaFile.GameData.Type)
            {
                case FormatEnum.SPR:
                    DataContext = new SPRTextureAtlasEditor(sender.PersonaFile);
                    break;
                case FormatEnum.SPD:
                    DataContext = new SPDTextureAtlasEditor(sender.PersonaFile);
                    break;
                case FormatEnum.PTP:
                    DataContext = new PTPEditorVM(sender.PersonaFile.GameData as PTP);
                    break;
                case FormatEnum.BMD:
                    DataContext = new BMDEditorVM(sender.PersonaFile);
                    break;
                case FormatEnum.FTD:
                    DataContext = new FTDEditorVM(sender.PersonaFile.GameData as FTD);
                    break;
                case FormatEnum.FNT:
                    DataContext = new FNTEditorVM(sender.PersonaFile.GameData as FNT);
                    break;
                case FormatEnum.FNT0:
                    DataContext = new FNT0EditorVM(sender.PersonaFile.GameData as FNT0);
                    break;
                case FormatEnum.DAT:
                    DataContext = new HEXEditorVM(sender.PersonaFile.GameData as DAT);
                    break;
                default:
                    return false;
            }

            ClosableTabItemVM closableTabItemVM = new ClosableTabItemVM();
            closableTabItemVM.DataContext = DataContext;
            closableTabItemVM.TabTitle = Title;
            closableTabItemVM.PropertyChanged += ClosableTabItemVM_PropertyChanged;
            closableTabItemVM.PersonaFile = sender;

            tabCollection.Add(closableTabItemVM);
            SelectedTabIndex = tabCollection.IndexOf(closableTabItemVM);

            sender.UnEnable();
            return true;
        }

        public void SetPreview(ImageSource Preview)
        {
            previewVM.SourceIMG = Preview;
        }

        private void ClosableTabItemVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            tabCollection.Remove(sender as ClosableTabItemVM);
        }
    }
}