using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System.Windows.Input;

namespace PersonaEditor.ViewModels
{
    class ClosableTabItemVM : BindingObject
    {
        private object _dataContext = null;

        public ClosableTabItemVM()
        {
            MouseUp = new RelayCommand(MouseButtonUp);
            FileClose = new RelayCommand(CloseFile);
        }

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

        public string TabTitle { get; set; }
        public object DataContext
        {
            get => _dataContext;
            set => SetProperty(ref _dataContext, value);
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

    }
}