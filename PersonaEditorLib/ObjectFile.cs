using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public class ObjectFile : BindingObject
    {
        EventWrapper eventWrapper;

        private object _obj = null;

        public object Object
        {
            get { return _obj; }
            set
            {
                if (_obj != value)
                {
                    _obj = value;

                    if (eventWrapper != null)
                    {
                        eventWrapper.Deregister();
                        eventWrapper = null;
                    }

                    if (value is INotifyPropertyChanged INPC)
                        eventWrapper = new EventWrapper(INPC, this);

                    Notify("Object");
                }
            }
        }

        public object Tag { get; set; } = null;

        public string Name { get; set; } = "";

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TunnelNotify(sender, e);
        }

        public ObjectFile(string name, object obj)
        {
            Name = name;
            Object = obj;
        }
    }
}