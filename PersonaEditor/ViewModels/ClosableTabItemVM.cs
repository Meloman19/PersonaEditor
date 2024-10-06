using System;
using System.Windows.Input;
using PersonaEditor.Common;

namespace PersonaEditor.ViewModels
{
    public sealed class ClosableTabItemVM : BindingObject
    {
        public ClosableTabItemVM(GameFileTreeItem gameFile, BindingObject gameFileViewModel, string tabTitle)
        {
            PersonaFile = gameFile;
            DataContext = gameFileViewModel;
            TabTitle = tabTitle;
            FileCloseCommand = new RelayCommand(FileClose);
        }

        public ICommand FileCloseCommand { get; }

        public string TabTitle { get; set; }

        public GameFileTreeItem PersonaFile { get; } = null;

        public BindingObject DataContext { get; } = null;

        public bool IsClosable { get; set; } = true;

        private void FileClose()
        {
            Close();
        }

        public bool Close()
        {
            if (IsClosable)
            {
                if (DataContext is IEditor vm)
                    if (!vm.Close())
                        return false;
            }

            PersonaFile?.Enable();
            ItemClosed?.Invoke(this);
            return true;
        }

        public override void Release()
        {
            base.Release();

            DataContext?.Release();
        }

        public event Action<ClosableTabItemVM> ItemClosed;
    }
}