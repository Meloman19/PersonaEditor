using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public class EventTrigger : TriggerBase
    {
        #region EventNameProperty

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName",
                typeof(string),
                typeof(EventTrigger),
                new PropertyMetadata("", new PropertyChangedCallback(OnEventNameChanged)));

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        #endregion EventNameProperty

        #region Actions

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions",
                typeof(ActionCollection),
                typeof(EventTrigger),
                new PropertyMetadata(null));

        public ActionCollection Actions
        {
            get { return (ActionCollection)GetValue(ActionsProperty); }
            set
            {
                if (Actions is ActionCollection oldObj)
                    oldObj.Detach();
                SetValue(ActionsProperty, value);
                if (Actions is ActionCollection newObj)
                    newObj.Attach(AssociatedObject);
            }
        }

        public static ActionCollection GetTriggers(DependencyObject element)
        {
            ActionCollection actionCollection = (ActionCollection)element.GetValue(ActionsProperty);
            if (actionCollection == null)
                element.SetValue(ActionsProperty, new ActionCollection());
            return actionCollection;
        }

        #endregion Actions

        public EventTrigger() : base()
        {
            Actions = new ActionCollection();
        }

        AnyEventHandler anyEventHandler = null;

        public override void Attach(DependencyObject dependencyObject)
        {
            Unsubscribe();
            base.Attach(dependencyObject);
            Subscribe();
        }

        public override void Detach()
        {
            Unsubscribe();
            base.Detach();
        }

        private void Subscribe()
        {
            if (AssociatedObject == null || string.IsNullOrWhiteSpace(EventName))
                return;
            Unsubscribe();

            anyEventHandler = new AnyEventHandler(AssociatedObject, EventName);
            anyEventHandler.EventRaise += AnyEventHandler_EventRaise;
        }

        private void Unsubscribe()
        {
            if (AssociatedObject == null)
                return;
            if (anyEventHandler != null)
            {
                anyEventHandler.EventRaise -= AnyEventHandler_EventRaise;
                anyEventHandler.Dispose();
                anyEventHandler = null;
            }
        }

        private void AnyEventHandler_EventRaise(object[] obj)
        {
            foreach (var a in Actions)
                (a as ActionBase).Invoke(obj);
        }

        public void OnEventNameChanged(string oldName, string newName)
        {
            Unsubscribe();
            Subscribe();
        }

        private static void OnEventNameChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ((EventTrigger)sender).OnEventNameChanged((string)args.OldValue, (string)args.NewValue);
        }
    }
}