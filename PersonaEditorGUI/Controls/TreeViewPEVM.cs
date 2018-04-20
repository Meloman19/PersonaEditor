using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PersonaEditorGUI.Classes;
using PersonaEditorLib;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorGUI.Controls
{
    public delegate void TreeViewItemEventHandler(UserTreeViewItem sender);
    class TreeViewPEVM : BindingObject
    {
        public event TreeViewItemEventHandler SelectedItemChanged;
        public event TreeViewItemEventHandler SelectedItemOpen;

        private ObservableCollection<UserTreeViewItem> tree = new ObservableCollection<UserTreeViewItem>();

        public ReadOnlyObservableCollection<UserTreeViewItem> Tree { get; }

        public MouseEventHandler Leave => MouseLeave;
        private void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private object _sel = 10;
        public object Sel => _sel;

        public void SetRoot(ObjectFile personaFile)
        {
            if (tree.Count > 0)
                if (!tree[0].Close())
                    return;

            tree.Clear();

            UserTreeViewItem item = new UserTreeViewItem(personaFile);
            item.SelectedItemChanged += Item_SelectedItem;
            item.SelectedItemOpen += Item_SelectedItemOpen;
            tree.Add(item);

            if (personaFile.Object is IPersonaFile pfile && pfile.SubFiles.Count == 0)
                Item_SelectedItemOpen(item);
        }

        private void Item_SelectedItem(UserTreeViewItem sender) => SelectedItemChanged?.Invoke(sender);

        private void Item_SelectedItemOpen(UserTreeViewItem sender) => SelectedItemOpen?.Invoke(sender);

        public ObjectFile GetRoot()
        {
            if (tree.Count > 0)
                return tree[0].PersonaFile;
            else
                return null;
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is UserTreeViewItem item)
            {
                if (e.PropertyName == "IsSelected")
                    SelectedItemChanged?.Invoke(item);
                else if (e.PropertyName == "Open")
                    SelectedItemOpen?.Invoke(item);
            }
        }

        public TreeViewPEVM()
        {
            Tree = new ReadOnlyObservableCollection<UserTreeViewItem>(tree);
        }
    }
}