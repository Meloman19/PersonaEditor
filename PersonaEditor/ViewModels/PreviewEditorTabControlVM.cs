using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PersonaEditor.Common;
using PersonaEditor.ViewModels.Editors;
using PersonaEditorLib;
using PersonaEditorLib.Other;
using PersonaEditorLib.SpriteContainer;
using PersonaEditorLib.Text;

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

        private static readonly Dictionary<Type, Func<GameFile, object>> _editorFactory = new Dictionary<Type, Func<GameFile, object>>
        {
            { typeof(SPR),  gm => new SPRTextureAtlasEditor(gm) },
            { typeof(SPD),  gm => new SPDTextureAtlasEditor(gm) },
            { typeof(PTP),  gm => new PTPEditorVM(gm.GameData as PTP) },
            { typeof(BMD),  gm => new BMDEditorVM(gm) },
            { typeof(FTD),  gm => new FTDEditorVM(gm.GameData as FTD) },
            { typeof(FNT),  gm => new FNTEditorVM(gm.GameData as FNT) },
            { typeof(FNT0), gm => new FNT0EditorVM(gm.GameData as FNT0) },
            { typeof(DAT),  gm => new HEXEditorVM(gm.GameData as DAT) },
        };

        public bool Open(GameFileTreeItem sender)
        {
            if (!sender.CanEdit())
            {
                MessageBox.Show(String.Format("Can't open {0}", sender.Header));
                return false;
            }

            string Title = sender.PersonaFile.Name;

            if (!_editorFactory.TryGetValue(sender.PersonaFile.GameData.GetType(), out var factory))
                return false;

            var dataContext = factory(sender.PersonaFile);

            ClosableTabItemVM closableTabItemVM = new ClosableTabItemVM();
            closableTabItemVM.DataContext = dataContext;
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