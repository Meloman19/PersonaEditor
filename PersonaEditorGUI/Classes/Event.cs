using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace PersonaEditorGUI.Classes
{
    public class Event
    {
        #region MouseDoubleClick

        private static DependencyProperty MouseDoubleClickPropertyEW =
            DependencyProperty.RegisterAttached("MouseDoubleClickEW",
                typeof(ControlEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty MouseDoubleClickProperty =
            DependencyProperty.RegisterAttached("MouseDoubleClick",
                typeof(MouseButtonEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_Control));

        public static void SetMouseDoubleClick(Control element, MouseButtonEventHandler value)
        {
            element.SetValue(MouseDoubleClickProperty, value);
        }
        public static MouseButtonEventHandler GetMouseDoubleClick(Control element)
        {
            return (MouseButtonEventHandler)element.GetValue(MouseDoubleClickProperty);
        }

        #endregion MouseDoubleClick

        #region MouseEnter

        private static DependencyProperty MouseEnterPropertyEW =
            DependencyProperty.RegisterAttached("MouseEnterEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty MouseEnterProperty =
            DependencyProperty.RegisterAttached("MouseEnter",
                typeof(MouseEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetMouseEnter(UIElement element, MouseEventHandler value)
        {
            element.SetValue(MouseEnterProperty, value);
        }
        public static MouseEventHandler GetMouseEnter(UIElement element)
        {
            return (MouseEventHandler)element.GetValue(MouseEnterProperty);
        }

        #endregion MouseEnter

        #region MouseLeave

        private static DependencyProperty MouseLeavePropertyEW =
            DependencyProperty.RegisterAttached("MouseLeaveEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty MouseLeaveProperty =
            DependencyProperty.RegisterAttached("MouseLeave",
                typeof(MouseEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetMouseLeave(UIElement element, MouseEventHandler value)
        {
            element.SetValue(MouseLeaveProperty, value);
        }
        public static MouseEventHandler GetMouseLeave(UIElement element)
        {
            return (MouseEventHandler)element.GetValue(MouseLeaveProperty);
        }

        #endregion MouseLeave

        #region KeyUp

        private static DependencyProperty KeyUpPropertyEW =
            DependencyProperty.RegisterAttached("KeyUpEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty KeyUpProperty =
            DependencyProperty.RegisterAttached("KeyUp",
                typeof(KeyEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetKeyUp(UIElement element, KeyEventHandler value)
        {
            element.SetValue(KeyUpProperty, value);
        }
        public static KeyEventHandler GetKeyUp(UIElement element)
        {
            return (KeyEventHandler)element.GetValue(KeyUpProperty);
        }

        #endregion KeyUp

        #region KeyDown

        private static DependencyProperty KeyDownPropertyEW =
            DependencyProperty.RegisterAttached("KeyDownEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty KeyDownProperty =
            DependencyProperty.RegisterAttached("KeyDown",
                typeof(KeyEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetKeyDown(UIElement element, KeyEventHandler value)
        {
            element.SetValue(KeyDownProperty, value);
        }
        public static KeyEventHandler GetKeyDown(UIElement element)
        {
            return (KeyEventHandler)element.GetValue(KeyDownProperty);
        }

        #endregion KeyDown

        #region PreviewKeyUp

        private static DependencyProperty PreviewKeyUpPropertyEW =
            DependencyProperty.RegisterAttached("PreviewKeyUpEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty PreviewKeyUpProperty =
            DependencyProperty.RegisterAttached("PreviewKeyUp",
                typeof(KeyEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetPreviewKeyUp(UIElement element, KeyEventHandler value)
        {
            element.SetValue(PreviewKeyUpProperty, value);
        }
        public static KeyEventHandler GetPreviewKeyUp(UIElement element)
        {
            return (KeyEventHandler)element.GetValue(PreviewKeyUpProperty);
        }

        #endregion PreviewKeyUp

        #region PreviewKeyDown

        private static DependencyProperty PreviewKeyDownPropertyEW =
            DependencyProperty.RegisterAttached("PreviewKeyDownEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty PreviewKeyDownProperty =
            DependencyProperty.RegisterAttached("PreviewKeyDown",
                typeof(KeyEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetPreviewKeyDown(UIElement element, KeyEventHandler value)
        {
            element.SetValue(PreviewKeyDownProperty, value);
        }
        public static KeyEventHandler GetPreviewKeyDown(UIElement element)
        {
            return (KeyEventHandler)element.GetValue(PreviewKeyDownProperty);
        }

        #endregion PreviewKeyDown

        #region Drop

        private static DependencyProperty DropPropertyEW =
            DependencyProperty.RegisterAttached("DropEW",
                typeof(UIElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty DropProperty =
            DependencyProperty.RegisterAttached("Drop",
                typeof(DragEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_UIElement));

        public static void SetDrop(UIElement element, DragEventHandler value)
        {
            element.SetValue(DropProperty, value);
        }
        public static DragEventHandler GetDrop(UIElement element)
        {
            return (DragEventHandler)element.GetValue(DropProperty);
        }

        #endregion Drop

        #region Closing

        private static DependencyProperty ClosingPropertyEW =
            DependencyProperty.RegisterAttached("ClosingEW",
                typeof(WindowEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty ClosingProperty =
            DependencyProperty.RegisterAttached("Closing",
                typeof(CancelEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_Window));

        public static void SetClosing(Window element, CancelEventHandler value)
        {
            element.SetValue(ClosingProperty, value);
        }
        public static CancelEventHandler GetClosing(Window element)
        {
            return (CancelEventHandler)element.GetValue(ClosingProperty);
        }

        #endregion Closing

        #region Loaded

        private static DependencyProperty LoadedPropertyEW =
            DependencyProperty.RegisterAttached("LoadedEW",
                typeof(FrameworkElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty LoadedProperty =
            DependencyProperty.RegisterAttached("Loaded",
                typeof(RoutedEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_FrameworkElement));

        public static void SetLoaded(FrameworkElement element, RoutedEventHandler value)
        {
            element.SetValue(LoadedProperty, value);
        }
        public static RoutedEventHandler GetLoaded(FrameworkElement element)
        {
            return (RoutedEventHandler)element.GetValue(LoadedProperty);
        }

        #endregion Loaded

        #region Unloaded

        private static DependencyProperty UnloadedPropertyEW =
            DependencyProperty.RegisterAttached("UnloadedEW",
                typeof(FrameworkElementEventWrapper),
                typeof(Event),
                new PropertyMetadata(null, CallBackEW));

        public static DependencyProperty UnloadedProperty =
            DependencyProperty.RegisterAttached("Unloaded",
                typeof(RoutedEventHandler),
                typeof(Event),
                new PropertyMetadata(null, CallBack_FrameworkElement));

        public static void SetUnloaded(FrameworkElement element, RoutedEventHandler value)
        {
            element.SetValue(UnloadedProperty, value);
        }
        public static RoutedEventHandler GetUnloaded(FrameworkElement element)
        {
            return (RoutedEventHandler)element.GetValue(UnloadedProperty);
        }

        #endregion Unloaded

        private static void CallBackEW(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is UIElementEventWrapper uiWrapper)
                uiWrapper.Deregister();
            else if (e.OldValue is ControlEventWrapper wrapper)
                wrapper.Deregister();
            else if (e.OldValue is WindowEventWrapper winWrapper)
                winWrapper.Deregister();
            else if (e.OldValue is FrameworkElementEventWrapper feWrapper)
                feWrapper.Deregister();
        }

        private static void CallBack_Control(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string Name = e.Property.Name;
            Control control = d as Control;

            if (Name == "MouseDoubleClick")
                if (e.NewValue is MouseButtonEventHandler buthandler)
                    d.SetValue(MouseDoubleClickPropertyEW, new ControlEventWrapper(control, EventType.MouseDoubleClick, buthandler));
        }

        private static void CallBack_Window(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string Name = e.Property.Name;
            Window window = d as Window;

            if (Name == "Closing")
                if (e.NewValue is CancelEventHandler cancelHandler)
                    d.SetValue(ClosingPropertyEW, new WindowEventWrapper(window, EventType.Closing, cancelHandler));
        }

        private static void CallBack_FrameworkElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string Name = e.Property.Name;
            FrameworkElement frameworkElement = d as FrameworkElement;

            if (e.NewValue is RoutedEventHandler routedHandler)
                if (Name == "Loaded")
                    d.SetValue(LoadedPropertyEW, new FrameworkElementEventWrapper(frameworkElement, EventType.Loaded, routedHandler));
                else if (Name == "Unloaded")
                    d.SetValue(UnloadedPropertyEW, new FrameworkElementEventWrapper(frameworkElement, EventType.Unloaded, routedHandler));
        }

        private static void CallBack_UIElement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string Name = e.Property.Name;
            UIElement uiElement = d as UIElement;

            if (e.NewValue is MouseEventHandler mouseHandler)
            {
                if (Name == "MouseEnter")
                    d.SetValue(MouseEnterPropertyEW, new UIElementEventWrapper(uiElement, EventType.MouseEnter, mouseHandler));
                else if (Name == "MouseLeave")
                    d.SetValue(MouseLeavePropertyEW, new UIElementEventWrapper(uiElement, EventType.MouseLeave, mouseHandler));
            }
            else if (e.NewValue is KeyEventHandler keyHandler)
            {
                if (Name == "KeyUp")
                    d.SetValue(KeyUpPropertyEW, new UIElementEventWrapper(uiElement, EventType.KeyUp, keyHandler));
                else if (Name == "KeyDown")
                    d.SetValue(KeyDownPropertyEW, new UIElementEventWrapper(uiElement, EventType.KeyDown, keyHandler));
                else if (Name == "PreviewKeyUp")
                    d.SetValue(PreviewKeyUpPropertyEW, new UIElementEventWrapper(uiElement, EventType.PreviewKeyUp, keyHandler));
                else if (Name == "PreviewKeyDown")
                    d.SetValue(PreviewKeyDownPropertyEW, new UIElementEventWrapper(uiElement, EventType.PreviewKeyDown, keyHandler));
            }
            else if (e.NewValue is DragEventHandler dragHandler)
            {
                if (Name == "Drop")
                    d.SetValue(DropPropertyEW, new UIElementEventWrapper(uiElement, EventType.Drop, dragHandler));
            }
        }
    }

    enum EventType
    {
        MouseDoubleClick,
        MouseEnter,
        MouseLeave,
        KeyDown,
        KeyUp,
        PreviewKeyDown,
        PreviewKeyUp,
        Drop,
        Closing,
        Loaded,
        Unloaded
    }

    class ControlEventWrapper
    {
        Control eventSource;
        WeakReference eventDestination;

        public ControlEventWrapper(Control source, EventType eventType, MouseButtonEventHandler handler)
        {
            eventSource = source;
            eventDestination = new WeakReference(handler);

            if (eventType == EventType.MouseDoubleClick)
                eventSource.MouseDoubleClick += OnButtonEvent;
            else
            {
                eventSource = null;
                eventDestination = null;
            }
        }

        private void OnButtonEvent(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = (MouseButtonEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        private void EventSource_Unloaded(object sender, RoutedEventArgs e)
        {
            Deregister();
        }

        public void Deregister()
        {
            eventSource.MouseDoubleClick -= OnButtonEvent;
            eventSource = null;
            eventDestination = null;
        }
    }

    class UIElementEventWrapper
    {
        UIElement eventSource;
        WeakReference eventDestination;

        public UIElementEventWrapper(UIElement source, EventType eventType, MouseEventHandler handler)
        {
            eventSource = source;
            eventDestination = new WeakReference(handler);

            if (eventType == EventType.MouseEnter)
                eventSource.MouseEnter += OnEvent;
            else if (eventType == EventType.MouseLeave)
                eventSource.MouseLeave += OnEvent;
            else
            {
                eventSource = null;
                eventDestination = null;
            }
        }

        public UIElementEventWrapper(UIElement source, EventType eventType, KeyEventHandler handler)
        {
            eventSource = source;
            eventDestination = new WeakReference(handler);

            if (eventType == EventType.KeyDown)
                eventSource.KeyDown += OnKeyEvent;
            else if (eventType == EventType.KeyUp)
                eventSource.KeyUp += OnKeyEvent;
            else if (eventType == EventType.PreviewKeyDown)
                eventSource.PreviewKeyDown += OnKeyEvent;
            else if (eventType == EventType.PreviewKeyUp)
                eventSource.PreviewKeyUp += OnKeyEvent;
            else
            {
                eventSource = null;
                eventDestination = null;
            }


        }

        public UIElementEventWrapper(UIElement source, EventType eventType, DragEventHandler handler)
        {
            eventSource = source;
            eventDestination = new WeakReference(handler);

            if (eventType == EventType.Drop)
                eventSource.Drop += OnDropEvent;
            else
            {
                eventSource = null;
                eventDestination = null;
            }
        }

        private void OnDropEvent(object sender, DragEventArgs e)
        {
            DragEventHandler handler = (DragEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        private void OnEvent(object sender, MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        private void OnButtonEvent(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = (MouseButtonEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        private void OnKeyEvent(object sender, KeyEventArgs e)
        {
            KeyEventHandler handler = (KeyEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        private void EventSource_Unloaded(object sender, RoutedEventArgs e)
        {
            Deregister();
        }

        public void Deregister()
        {
            eventSource.MouseEnter -= OnEvent;
            eventSource.MouseLeave -= OnEvent;

            eventSource.KeyDown -= OnKeyEvent;
            eventSource.KeyUp -= OnKeyEvent;
            eventSource.PreviewKeyDown -= OnKeyEvent;
            eventSource.PreviewKeyUp -= OnKeyEvent;

            eventSource.Drop -= OnDropEvent;

            eventSource = null;
            eventDestination = null;
        }
    }

    class WindowEventWrapper
    {
        Window eventSource;
        WeakReference eventDestination;

        public WindowEventWrapper(Window source, EventType eventType, System.ComponentModel.CancelEventHandler handler)
        {
            eventSource = source;
            eventDestination = new WeakReference(handler);

            if (eventType == EventType.Closing)
                eventSource.Closing += OnWindowEvent;
            else
            {
                eventSource = null;
                eventDestination = null;
            }
        }

        private void OnWindowEvent(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.ComponentModel.CancelEventHandler handler = (System.ComponentModel.CancelEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        public void Deregister()
        {
            eventSource.Closing -= OnWindowEvent;
            eventSource = null;
            eventDestination = null;
        }
    }

    class FrameworkElementEventWrapper
    {
        FrameworkElement eventSource;
        WeakReference eventDestination;

        public FrameworkElementEventWrapper(FrameworkElement source, EventType eventType, RoutedEventHandler handler)
        {
            eventSource = source;
            eventDestination = new WeakReference(handler);

            if (eventType == EventType.Loaded)
                eventSource.Loaded += OnFEEvent;
            else if (eventType == EventType.Unloaded)
                eventSource.Unloaded += OnFEEvent;
            else
            {
                eventSource = null;
                eventDestination = null;
            }
        }

        private void OnFEEvent(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler handler = (RoutedEventHandler)eventDestination.Target;
            if (handler != null)
                handler.Invoke(sender, e);
            else
                Deregister();
        }

        public void Deregister()
        {
            eventSource.Loaded -= OnFEEvent;
            eventSource.Unloaded -= OnFEEvent;
            eventSource = null;
            eventDestination = null;
        }
    }
}