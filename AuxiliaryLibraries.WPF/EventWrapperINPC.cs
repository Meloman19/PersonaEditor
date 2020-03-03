using System;
using System.ComponentModel;

namespace AuxiliaryLibraries.WPF
{
    public class EventWrapperINPC
    {
        INotifyPropertyChanged eventSource;
        WeakReference eventDestination;

        public EventWrapperINPC(INotifyPropertyChanged eventSource, IEventWrapper eventDestination)
        {
            this.eventSource = eventSource;
            this.eventDestination = new WeakReference(eventDestination);
            eventSource.PropertyChanged += OnEvent;
        }

        void OnEvent(object sender, PropertyChangedEventArgs e)
        {
            IEventWrapper obj = (IEventWrapper)eventDestination.Target;
            if (obj != null)
                obj.OnPropertyChanged(sender, e);
            else
                Deregister();
        }

        public void Deregister()
        {
            eventSource.PropertyChanged -= OnEvent;
            eventSource = null;
            eventDestination = null;
        }
    }
}