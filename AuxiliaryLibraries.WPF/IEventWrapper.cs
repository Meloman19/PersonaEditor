using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryLibraries.WPF
{
    public interface IEventWrapper : INotifyPropertyChanged
    {
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
    }
}