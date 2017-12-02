using PersonaEditorGUI.Files;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PersonaEditorGUI.Controls
{
    public delegate void ObjectChangedEventHandler(object sender);
    public partial class TreeViewPE : UserControl
    {
        public event ObjectChangedEventHandler SelectedItemData;
        public event ObjectChangedEventHandler SelectedItemDataOpen;

        public static readonly DependencyProperty HasOneItemProperty = DependencyProperty.Register("HasOneItem", typeof(bool), typeof(TreeViewPE),
           new FrameworkPropertyMetadata(false));

        [Bindable(true)]
        public bool HasOneItem
        {
            get { return (bool)GetValue(HasOneItemProperty); }
            set { SetValue(HasOneItemProperty, value); }
        }
        
        public TreeViewPE()
        {

            InitializeComponent();

        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Files.UserTreeViewItem item)
                SelectedItemData?.Invoke(item.personaFile);

            // SelectedItemData?.Invoke()
            // SelectedItemChanged?.Invoke(sender, e);
        }

        private void TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is Control control)
                if (control.DataContext is Files.UserTreeViewItem item)
                    item.KeyDown(e.Key);
        }

        private void TreeView_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is Control control)
                if (control.DataContext is Files.UserTreeViewItem item)
                    item.KeyUp(e.Key);
        }

        private void TreeViewItem_Drop(object sender, DragEventArgs e)
        {
            if ((sender as TreeViewItem)?.DataContext is UserTreeViewItem treeitem)
                if (treeitem.IsMouseOver)
                    if (treeitem.personaFile is IPersonaFile userTreeViewItem)
                    {
                        string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];

                        if (temp.Length > 0)
                            if (MessageBox.Show("Replace " + userTreeViewItem.Name + "?", "Replace?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                            {
                                var personaFile = PersonaEditorLib.Utilities.PersonaFile.OpenFile(userTreeViewItem.Name, File.ReadAllBytes(temp[0]), userTreeViewItem.Type, true);
                                if (personaFile != null)
                                {
                                    treeitem.Replace(personaFile);
                                }
                                else
                                {
                                    MessageBox.Show("Error. " + Path.GetFileName(temp[0]) + " is not a " + userTreeViewItem.Type + " type");
                                }
                            }
                    }
        }

        private void TreeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ObservableCollection<UserTreeViewItem> list)
            {

                int count = 0;

                foreach (var a in list)
                {
                    count++;
                    foreach (var b in a.SubItems)
                        count++;
                }

                if (count == 1)
                {
                    HasOneItem = true;
                    return;
                }
            }
            HasOneItem = false;
        }

        private void TreeViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock control)
                if (control.DataContext is UserTreeViewItem item)
                    if (item.SubItems.Count == 0)
                        SelectedItemDataOpen?.Invoke(item);
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textblock)
                if (textblock.DataContext is UserTreeViewItem item)
                {
                    item.IsMouseOver = false;
                    if (e.LeftButton == MouseButtonState.Pressed)
                        DragDrop.DoDragDrop(this, item.GetDragObject(), DragDropEffects.Copy);
                }
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textblock)
                if (textblock.DataContext is UserTreeViewItem item)
                    item.IsMouseOver = true;
        }
    }
}