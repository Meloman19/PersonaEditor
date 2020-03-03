using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryLibraries.WPF
{
    public class BindingObject : IEventWrapper
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
        }

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void TunnelNotify(object sender, PropertyChangedEventArgs property)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(sender, property);
            }
        }
    }
}
