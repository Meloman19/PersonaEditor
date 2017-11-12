using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonaText.SettingsWindow
{
    public partial class Main : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged implementation

        private bool _IsLittleEndian;
        public bool IsLIttleEndian
        {
            get { return _IsLittleEndian; }
            set
            {
                if (value != _IsLittleEndian)
                    _IsLittleEndian = value;

                Notify("IsLIttleEndian");
            }
        }

        public Main()
        {
            InitializeComponent();
            IsLIttleEndian = Current.Default.IsLittleEndian;
        }


        public void Save()
        {
            Current.Default.IsLittleEndian = IsLIttleEndian;
        }
    }
}
