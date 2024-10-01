using PersonaEditor.ViewModels.Editors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonaEditor.Controls
{
    public sealed class AtlasObjectContainer : Control
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(object), typeof(AtlasObjectContainer),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static readonly RoutedEvent SelectingRequestedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectingRequested), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AtlasObjectContainer));

        public event RoutedEventHandler SelectingRequested
        {
            add { AddHandler(SelectingRequestedEvent, value); }
            remove { RemoveHandler(SelectingRequestedEvent, value); }
        }

        static AtlasObjectContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AtlasObjectContainer),
                new FrameworkPropertyMetadata(typeof(AtlasObjectContainer)));
        }

        public AtlasObjectContainer()
        {
            DataContextChanged += AtlasObjectContainer_DataContextChanged;
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public TextureObjectBase Data => DataContext as TextureObjectBase;

        private void AtlasObjectContainer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TextureObjectBase oldData)
                oldData.PropertyChanged -= Data_PropertyChanged;

            if (e.NewValue is TextureObjectBase newData)
                newData.PropertyChanged += Data_PropertyChanged;

            UpdateData();
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var eventArgs = new RoutedEventArgs(SelectingRequestedEvent);
            RaiseEvent(eventArgs);
        }

        private void Data_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            this.SetLocation(Data?.TextureObjectRect ?? new Rect());
        }
    }
}