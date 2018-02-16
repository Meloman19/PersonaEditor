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
    public delegate void ObjectChangedEventHandler(ObjectFile sender);
    class TreeViewPEVM : BindingObject
    {
        public event ObjectChangedEventHandler SelectedItemChanged;
        public event ObjectChangedEventHandler SelectedItemOpen;

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
        }

        private void Item_SelectedItem(ObjectFile sender) => SelectedItemChanged?.Invoke(sender);

        private void Item_SelectedItemOpen(ObjectFile sender) => SelectedItemOpen?.Invoke(sender);

        public ObjectFile GetRoot()
        {
            if (tree.Count > 0)
                return tree[0].personaFile;
            else
                return null;
        }

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is UserTreeViewItem item)
            {
                if (e.PropertyName == "IsSelected")
                    SelectedItemChanged?.Invoke(item.personaFile);
                else if (e.PropertyName == "Open")
                    SelectedItemOpen?.Invoke(item.personaFile);
            }
        }

        public TreeViewPEVM()
        {
            Tree = new ReadOnlyObservableCollection<UserTreeViewItem>(tree);
        }
    }
}