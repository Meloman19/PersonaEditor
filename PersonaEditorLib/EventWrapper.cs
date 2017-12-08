using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public interface IEventWrapper : INotifyPropertyChanged
    {
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
    }

    public class EventWrapper
    {
        INotifyPropertyChanged eventSource;
        WeakReference eventDestination;

        public EventWrapper(INotifyPropertyChanged eventSource, IEventWrapper eventDestination)
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