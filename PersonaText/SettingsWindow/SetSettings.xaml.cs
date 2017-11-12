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
using System.Windows.Shapes;

namespace PersonaText
{
    public partial class SetSettings : Window, INotifyPropertyChanged
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

        private bool _SettingChange = false;
        public bool SettingChange
        {
            get { return _SettingChange; }
            set
            {
                if (value != _SettingChange)
                {
                    _SettingChange = value;
                    Notify("SettingChange");
                }
            }
        }

        public SetSettings()
        {
            InitializeComponent();
            DefaultBack.PropertyChanged += DefaultBack_PropertyChanged;
        }

        private void DefaultBack_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SettingChange = true;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Main.Save();
            DefaultBack.Save();
            (Owner as MainWindow).OV.BackImage.CurrentUpdate();
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main.Save();
            DefaultBack.Save();
            (Owner as MainWindow).OV.BackImage.CurrentUpdate();
            SettingChange = false;
        }
    }
}
