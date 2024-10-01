using System.Collections.ObjectModel;
using PersonaEditor.Common;
using PersonaEditor.Common.Delegates;
using PersonaEditorLib;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels
{
    public sealed class TreeViewPEVM : BindingObject
    {
        private ObservableCollection<GameFileTreeItem> tree = new ObservableCollection<GameFileTreeItem>();

        public event TreeViewItemEventHandler ItemAction;

        public ReadOnlyObservableCollection<GameFileTreeItem> Tree { get; }

        private object _sel = 10;
        public object Sel => _sel;

        public void SetRoot(GameFile personaFile)
        {
            if (tree.Count > 0)
                if (!tree[0].Close())
                    return;

            tree.Clear();

            GameFileTreeItem item = new GameFileTreeItem(personaFile);
            item.ItemAction += Item_Action;
            tree.Add(item);

            if (personaFile.GameData is PTP)
                ItemAction(item, UserTreeViewItemEventEnum.Open);
        }

        private void Item_Action(GameFileTreeItem sender, UserTreeViewItemEventEnum action) => ItemAction?.Invoke(sender, action);

        public GameFile GetRoot()
        {
            if (tree.Count > 0)
                return tree[0].PersonaFile;
            else
                return null;
        }

        public TreeViewPEVM()
        {
            Tree = new ReadOnlyObservableCollection<GameFileTreeItem>(tree);
        }
    }
}