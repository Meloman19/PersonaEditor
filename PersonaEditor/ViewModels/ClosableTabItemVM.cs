using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System.Windows.Input;

namespace PersonaEditor.ViewModels
{
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
            if ((MouseButton)arg == MouseButton.Middle)
                Close();
        }

        private object dataContext = null;

        public string TabTitle { get; set; }
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
                if (DataContext is IEditor vm)
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