using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditorGUI.Classes
{
    class CommandSetter : EventSetter
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public delegate void EventStartHandler(object obj);

        private ICommand command;
        private RoutedEvent routedEvent;

        public ICommand Command
        {
            set
            {
                command = value;
            }
        }

        public CommandSetter()
        {
            Handler = Delegate.CreateDelegate(typeof(EventStartHandler), this, "EventStart");
        }

        private void EventStart(object obj)
        {

        }
    }
}