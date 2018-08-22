using AuxiliaryLibraries.WPF;
using PersonaEditorGUI.Classes;
using System.Windows.Input;

namespace PersonaEditorGUI.Controls
{
    enum TabItemType
    {
        Null,
        ImagePreview,
        SPR,
        PTP,
        BMD,
        FNT,
        HEX,
        FTD
    }

    class ClosableTabItemVM : BindingObject
    {
        public ICommand FileClose { get; }
        private void CloseFile()
        {
            Close();
        }

        public ICommand MouseUp { get; }

        private void MouseButtonUp(object arg)
        {
            if ((arg as MouseButtonEventArgs).ChangedButton == MouseButton.Middle)
                Close();
        }

        private TabItemType dataContextType = TabItemType.Null;
        private object dataContext = null;

        public string TabTitle { get; set; }

        public TabItemType DataContextType
        {
            get { return dataContextType; }
            set
            {
                if (dataContextType != value)
                {
                    dataContextType = value;
                    Notify("DataContextType");
                }
            }
        }
        public object DataContext
        {
            get { return dataContext; }
            set
            {
                if (dataContext != value)
                {
                    dataContext = value;
                    Notify("DataContext");
                }
            }
        }
        public bool IsClosable { get; set; } = true;
        public TreeViewItemVM PersonaFile { get; set; } = null;

        public bool Close()
        {
            if (IsClosable)
            {
                if (DataContext is IViewModel vm)
                    if (!vm.Close())
                        return false;

                Notify("Close");
            }

            //dataContextType = TabItemType.Null;
            //DataContext = null;

            PersonaFile?.Enable();
            return true;
        }

        public ClosableTabItemVM()
        {
            MouseUp = new RelayCommand(MouseButtonUp);
            FileClose = new RelayCommand(CloseFile);
        }
    }
}