namespace PersonaEditor.Common.Delegates
{
    public delegate void OpenFilePathEventHandler(string path);

    public delegate void TreeViewItemEventHandler(ViewModels.TreeViewItemVM sender, UserTreeViewItemEventEnum action);
}