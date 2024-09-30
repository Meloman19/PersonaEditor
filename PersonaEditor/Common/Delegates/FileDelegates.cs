namespace PersonaEditor.Common.Delegates
{
    public delegate void OpenFilePathEventHandler(string path);

    public delegate void TreeViewItemEventHandler(ViewModels.GameFileTreeItem sender, UserTreeViewItemEventEnum action);
}