using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public interface IEventWrapper
    {
        event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
    }

    public class EventWrapper
    {
        IEventWrapper eventSource;
        WeakReference wr;

        public EventWrapper(IEventWrapper eventSource, IEventWrapper obj)
        {
            this.eventSource = eventSource;
            this.wr = new WeakReference(obj);
            eventSource.PropertyChanged += OnEvent;
        }

        void OnEvent(object sender, PropertyChangedEventArgs e)
        {
            IEventWrapper obj = (IEventWrapper)wr.Target;
            if (obj != null)
                obj.OnPropertyChanged(sender, e);
            else
                Deregister();
        }

        public void Deregister()
        {
            eventSource.PropertyChanged -= OnEvent;
            eventSource = null;
            wr = null;
        }
    }
}