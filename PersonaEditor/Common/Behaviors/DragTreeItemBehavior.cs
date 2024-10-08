﻿using Microsoft.Xaml.Behaviors;
using PersonaEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditor.Common.Behaviors
{
    internal class DragTreeItemBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (e.Source is DependencyObject obj && AssociatedObject.DataContext is GameFileTreeItem treeItem)
                {
                    DataObject data = new DataObject();
                    data.SetData(typeof(GameFileTreeItem), treeItem);
                    DragDrop.DoDragDrop(obj, data, DragDropEffects.Link);
                }
        }
    }
}