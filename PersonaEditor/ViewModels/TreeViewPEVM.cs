using System.Collections.ObjectModel;
using System.Linq;
using AuxiliaryLibraries.GameFormat;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using PersonaEditor.Classes.Delegates;

namespace PersonaEditor.ViewModels
{
    class TreeViewPEVM : BindingObject
    {
        private ObservableCollection<TreeViewItemVM> tree = new ObservableCollection<TreeViewItemVM>();

        public event TreeViewItemEventHandler ItemAction;

        public ReadOnlyObservableCollection<TreeViewItemVM> Tree { get; }

        private object _sel = 10;
        public object Sel => _sel;

        public void SetRoot(ObjectContainer personaFile)
        {
            if (tree.Count > 0)
                if (!tree[0].Close())
                    return;

            tree.Clear();

            TreeViewItemVM item = new TreeViewItemVM(personaFile);
            item.ItemAction += Item_Action;
            tree.Add(item);

            if (personaFile.Object is IGameFile pfile && pfile.Type == FormatEnum.PTP)
                ItemAction(item, UserTreeViewItemEventEnum.Open);
        }

        private void Item_Action(TreeViewItemVM sender, UserTreeViewItemEventEnum action) => ItemAction?.Invoke(sender, action);

        public ObjectContainer GetRoot()
        {
            if (tree.Count > 0)
                return tree[0].PersonaFile;
            else
                return null;
        }

        //public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (sender is UserTreeViewItem item)
        //    {
        //        if (e.PropertyName == "IsSelected")
        //            ItemAction?.Invoke(item, UserTreeViewItemEventEnum.Selected);
        //        else if (e.PropertyName == "Open")
        //            ItemAction?.Invoke(item, UserTreeViewItemEventEnum.Open);
        //    }
        //}

        public TreeViewPEVM()
        {
            Tree = new ReadOnlyObservableCollection<TreeViewItemVM>(tree);
        }
    }
}