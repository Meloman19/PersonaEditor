using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PersonaEditor.Controls
{
    public sealed class AtlasItemsContol : ItemsControl
    {
        private const string DragBorderCanvasName = "DragBorderCanvas";

        public static readonly DependencyProperty ObjectBorderThicknessProperty =
            DependencyProperty.Register(nameof(ObjectBorderThickness), typeof(Thickness), typeof(AtlasItemsContol),
                new FrameworkPropertyMetadata(new Thickness(1)));

        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(AtlasItemsContol),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedObjectPropertyChanged));

        internal static readonly DependencyPropertyKey CursorPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CursorPosition), typeof(Point?), typeof(AtlasItemsContol),
                new FrameworkPropertyMetadata(null, OnCursorPositionPropertyChanged));

        public static readonly DependencyProperty CursorPositionProperty
            = CursorPositionPropertyKey.DependencyProperty;

        public static readonly RoutedEvent CursorPositionChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(CursorPositionChanged), RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(AtlasItemsContol));

        private Canvas _dragBorderCanvas;
        private DragBorderControl _dragBorderControl;

        static AtlasItemsContol()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AtlasItemsContol),
                new FrameworkPropertyMetadata(typeof(AtlasItemsContol)));
        }

        public event RoutedEventHandler CursorPositionChanged
        {
            add { AddHandler(CursorPositionChangedEvent, value); }
            remove { RemoveHandler(CursorPositionChangedEvent, value); }
        }

        public Thickness ObjectBorderThickness
        {
            get { return (Thickness)GetValue(ObjectBorderThicknessProperty); }
            set { SetValue(ObjectBorderThicknessProperty, value); }
        }

        public Point? CursorPosition
        {
            get => (Point?)GetValue(CursorPositionProperty);
            private set => SetValue(CursorPositionPropertyKey, value);
        }

        public object SelectedObject
        {
            get { return GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragBorderCanvas = GetTemplateChild(DragBorderCanvasName) as Canvas;

            var dragBorderVisualize = new DragBorderVisualize();
            _dragBorderCanvas.Children.Add(dragBorderVisualize);

            _dragBorderControl = new DragBorderControl(dragBorderVisualize);
            _dragBorderControl.SetSelectedObject(SelectedObject);
            _dragBorderCanvas.Children.Add(_dragBorderControl);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AtlasObjectContainer();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AtlasObjectContainer;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var container = element as AtlasObjectContainer;
            container.DataContext = item;
            container.IsSelected = item == SelectedObject;
            container.SelectingRequested += Container_SelectingRequested;

            var borderBinding = new Binding();
            borderBinding.Source = this;
            borderBinding.Path = new PropertyPath(nameof(ObjectBorderThickness));
            container.SetBinding(Control.BorderThicknessProperty, borderBinding);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var container = element as AtlasObjectContainer;
            container.DataContext = null;
            container.IsSelected = false;
            container.SelectingRequested -= Container_SelectingRequested;

            BindingOperations.ClearBinding(container, Control.BorderThicknessProperty);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            CursorPosition = new Point(Math.Round(pos.X), Math.Round(pos.Y));
        }

        private void Container_SelectingRequested(object sender, RoutedEventArgs e)
        {
            SelectedObject = (sender as AtlasObjectContainer).Data;
        }

        private static void OnCursorPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AtlasItemsContol;
            var eventArgs = new RoutedEventArgs(CursorPositionChangedEvent);
            control.RaiseEvent(eventArgs);
        }

        private static void OnSelectedObjectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AtlasItemsContol;
            var oldSelected = control.ItemContainerGenerator.ContainerFromItem(e.OldValue);
            if (oldSelected is AtlasObjectContainer oldContainer)
                oldContainer.IsSelected = false;
            var newSelected = control.ItemContainerGenerator.ContainerFromItem(e.NewValue);
            if (newSelected is AtlasObjectContainer newContainer)
                newContainer.IsSelected = true;

            control._dragBorderControl?.SetSelectedObject(e.NewValue);
        }
    }
}