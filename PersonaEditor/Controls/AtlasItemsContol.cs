using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditor.Controls
{
    public sealed class AtlasItemsContol : ItemsControl
    {
        private const string DragBorderCanvasName = "DragBorderCanvas";

        public static readonly DependencyProperty ItemBorderThicknessProperty =
            DependencyProperty.Register(nameof(ItemBorderThickness), typeof(Thickness), typeof(AtlasItemsContol),
                new FrameworkPropertyMetadata(new Thickness(1)));

        public static readonly DependencyProperty ItemBorderBrushProperty =
           DependencyProperty.Register(nameof(ItemBorderBrush), typeof(Brush), typeof(AtlasItemsContol),
               new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ItemSelectionBrushProperty =
           DependencyProperty.Register(nameof(ItemSelectionBrush), typeof(Brush), typeof(AtlasItemsContol),
               new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(AtlasItemsContol),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemPropertyChanged));

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

        public Thickness ItemBorderThickness
        {
            get { return (Thickness)GetValue(ItemBorderThicknessProperty); }
            set { SetValue(ItemBorderThicknessProperty, value); }
        }

        public Brush ItemBorderBrush
        {
            get { return (Brush)GetValue(ItemBorderBrushProperty); }
            set { SetValue(ItemBorderBrushProperty, value); }
        }

        public Brush ItemSelectionBrush
        {
            get { return (Brush)GetValue(ItemSelectionBrushProperty); }
            set { SetValue(ItemSelectionBrushProperty, value); }
        }

        public Point? CursorPosition
        {
            get => (Point?)GetValue(CursorPositionProperty);
            private set => SetValue(CursorPositionPropertyKey, value);
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dragBorderCanvas = GetTemplateChild(DragBorderCanvasName) as Canvas;

            var dragBorderVisualize = new DragBorderVisualize();
            _dragBorderCanvas.Children.Add(dragBorderVisualize);

            _dragBorderControl = new DragBorderControl(dragBorderVisualize);
            _dragBorderControl.SetSelectedObject(SelectedItem);
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
            container.IsSelected = item == SelectedItem;
            container.SelectingRequested += Container_SelectingRequested;

            var borderThicknessBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(ItemBorderThickness)),
                Mode = BindingMode.OneWay,
            };
            container.SetBinding(Control.BorderThicknessProperty, borderThicknessBinding);

            var borderBrushBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(ItemBorderBrush)),
                Mode = BindingMode.OneWay,
            };
            container.SetBinding(Control.BorderBrushProperty, borderBrushBinding);

            var foregroundBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(ItemSelectionBrush)),
                Mode = BindingMode.OneWay,
            };
            container.SetBinding(Control.ForegroundProperty, foregroundBinding);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var container = element as AtlasObjectContainer;
            container.DataContext = null;
            container.IsSelected = false;
            container.SelectingRequested -= Container_SelectingRequested;

            BindingOperations.ClearBinding(container, Control.BorderThicknessProperty);
            BindingOperations.ClearBinding(container, Control.BorderBrushProperty);
            BindingOperations.ClearBinding(container, Control.ForegroundProperty);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            CursorPosition = new Point(Math.Round(pos.X), Math.Round(pos.Y));
        }

        private void Container_SelectingRequested(object sender, RoutedEventArgs e)
        {
            SelectedItem = (sender as AtlasObjectContainer).Data;
        }

        private static void OnCursorPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AtlasItemsContol;
            var eventArgs = new RoutedEventArgs(CursorPositionChangedEvent);
            control.RaiseEvent(eventArgs);
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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