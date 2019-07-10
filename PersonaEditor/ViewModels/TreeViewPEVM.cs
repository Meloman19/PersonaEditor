using System.Collections.ObjectModel;
using System.Linq;
using PersonaEditorLib;
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

        public void SetRoot(GameFile personaFile)
        {
            if (tree.Count > 0)
                if (!tree[0].Close())
                    return;

            tree.Clear();

            TreeViewItemVM item = new TreeViewItemVM(personaFile);
            item.ItemAction += Item_Action;
            tree.Add(item);

            if (personaFile.GameData.Type == FormatEnum.PTP)
                ItemAction(item, UserTreeViewItemEventEnum.Open);
        }

        private void Item_Action(TreeViewItemVM sender, UserTreeViewItemEventEnum action) => ItemAction?.Invoke(sender, action);

        public GameFile GetRoot()
        {
            if (tree.Count > 0)
                return tree[0].PersonaFile;
            else
                return null;
        }

        public TreeViewPEVM()
        {
            Tree = new ReadOnlyObservableCollection<TreeViewItemVM>(tree);
        }
    }
}