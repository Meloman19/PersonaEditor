using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public static class Interaction
    {
        private static readonly DependencyProperty TriggersProperty =
            DependencyProperty.RegisterAttached("ShadowTriggers",
                typeof(TriggerCollection),
                typeof(Interaction),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTriggersChanged)));

        public static TriggerCollection GetTriggers(DependencyObject obj)
        {
            TriggerCollection triggers = (TriggerCollection)obj.GetValue(TriggersProperty);
            if (triggers == null)
            {
                triggers = new TriggerCollection();
                obj.SetValue(TriggersProperty, triggers);
            }
            return triggers;
        }

        internal static bool IsElementLoaded(FrameworkElement element) => element.IsLoaded;

        private static void OnTriggersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TriggerCollection triggers = args.OldValue as TriggerCollection;
            TriggerCollection triggers2 = args.NewValue as TriggerCollection;
            if (triggers != triggers2)
            {
                if ((triggers != null) && (triggers.AssociatedObject != null))
                {
                    triggers.Detach();
                }
                if ((triggers2 != null) && (obj != null))
                {
                    if (triggers2.AssociatedObject != null)
                    {
                        //throw new InvalidOperationException(ExceptionStringTable.CannotHostTriggerCollectionMultipleTimesExceptionMessage);
                    }
                    triggers2.Attach(obj);
                }
            }
        }
        
        private static bool shouldRunInDesignMode;

        internal static bool ShouldRunInDesignMode
        {
            [CompilerGenerated]
            get => shouldRunInDesignMode;
            [CompilerGenerated]
            set => shouldRunInDesignMode = value;
        }

    }
}