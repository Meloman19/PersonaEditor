using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PersonaEditorGUI.Classes;
using PersonaEditorLib;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorGUI.Controls
{
    public delegate void ObjectChangedEventHandler(ObjectFile sender);
    class TreeViewPEVM : BindingObject
    {
        public event ObjectChangedEventHandler SelectedItemData;
        public event ObjectChangedEventHandler SelectedItemDataOpen;

        private ObservableCollection<UserTreeViewItem> tree = new ObservableCollection<UserTreeViewItem>();

        private List<EventWrapper> treeEW = new List<EventWrapper>();

        public ReadOnlyObservableCollection<UserTreeViewItem> Tree { get; }

        public void SetRoot(ObjectFile personaFile)
        {
            if (tree.Count > 0)
                if (!tree[0].Close())
                    return;

            tree.Clear();
            treeEW.Clear();
            UserTreeViewItem item = new UserTreeViewItem(personaFile);
            tree.Add(item);
            treeEW.Add(new EventWrapper(item, this));
        }

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
                    SelectedItemData?.Invoke(item.personaFile);
                else if (e.PropertyName == "Open")
                    SelectedItemDataOpen?.Invoke(item.personaFile);
            }
        }

        public TreeViewPEVM()
        {
            Tree = new ReadOnlyObservableCollection<UserTreeViewItem>(tree);
        }
    }
}