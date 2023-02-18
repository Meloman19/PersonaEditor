using System.Windows.Input;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using PersonaEditor.ViewModels;

namespace PersonaEditor.Common.Behaviors
{
    internal class DropTreeItemBehavior : Behavior<UIElement>
    {
        public readonly static DependencyProperty OpenFileProperty =
            DependencyProperty.Register(nameof(OpenFile), typeof(ICommand), typeof(DropTreeItemBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragOver -= AssociatedObject_DragOver;
            AssociatedObject.Drop -= AssociatedObject_Drop;
        }

        public ICommand OpenFile
        {
            get => (ICommand)GetValue(OpenFileProperty);
            set => SetValue(OpenFileProperty, value);
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            var itemDrop = e.Data.GetData(typeof(TreeViewItemVM)) as TreeViewItemVM;
            if (itemDrop == null)
                e.Effects = DragDropEffects.None;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            var itemDrop = e.Data.GetData(typeof(TreeViewItemVM)) as TreeViewItemVM;
            if (itemDrop == null)
                return;

            OpenFile?.Execute(itemDrop);
        }
    }
}