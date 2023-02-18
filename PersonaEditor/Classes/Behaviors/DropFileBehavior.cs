using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditor.Classes.Behaviors
{
    internal class DropFileBehavior : Behavior<UIElement>
    {
        public readonly static DependencyProperty OpenFileProperty =
            DependencyProperty.Register(nameof(OpenFile), typeof(ICommand), typeof(DropFileBehavior));

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
            var fileDrop = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileDrop == null || fileDrop.Length < 1)
                e.Effects = DragDropEffects.None;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            string[] temp = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (temp == null || temp.Length < 1)
                return;

            OpenFile?.Execute(temp[0]);
        }
    }
}