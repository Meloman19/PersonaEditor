using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditor.Common.Behaviors
{
    internal class ClosingWindowBehavior : Behavior<Window>
    {
        public readonly static DependencyProperty ClosingProperty =
            DependencyProperty.Register(nameof(Closing), typeof(ICommand), typeof(ClosingWindowBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.Closing += AssociatedObject_Closing;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Closing -= AssociatedObject_Closing;
        }

        public ICommand Closing
        {
            get => (ICommand)GetValue(ClosingProperty);
            set => SetValue(ClosingProperty, value);
        }

        private void AssociatedObject_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing?.Execute(e);
        }
    }
}