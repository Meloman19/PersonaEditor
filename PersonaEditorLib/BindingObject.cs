using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public class BindingObject : INotifyPropertyChanged, IEventWrapper
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
    }
}