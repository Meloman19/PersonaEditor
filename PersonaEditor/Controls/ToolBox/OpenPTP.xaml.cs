using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PersonaEditor.Controls.ToolBox
{
    /// <summary>
    /// Логика взаимодействия для OpenPTP.xaml
    /// </summary>
    public partial class OpenPTP : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged

        public ToolBoxResult Result { get; set; } = ToolBoxResult.Cancel;

        private int selectedFont = 0;
        private bool neverAskAgain = false;

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;

        public int SelectedFont
        {
            get { return selectedFont; }
            set
            {
                selectedFont = value;
                Notify("SelectedFont");
            }
        }

        public bool NeverAskAgain
        {
            get { return neverAskAgain; }
            set
            {
                neverAskAgain = value;
                Notify("NeverAskAgain");
            }
        }

        public OpenPTP()
        {
            InitializeComponent();
            DataContext = this;

            int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(ApplicationSettings.AppSetting.Default.OpenPTP_Font);
            if (sourceInd >= 0)
                selectedFont = sourceInd;
            else
                selectedFont = 0;

            NeverAskAgain = ApplicationSettings.AppSetting.Default.OpenPTP_NeverAskAgain;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.AppSetting.Default.OpenPTP_Font = Static.EncodingManager.GetPersonaEncodingName(SelectedFont);
            ApplicationSettings.AppSetting.Default.OpenPTP_NeverAskAgain = NeverAskAgain;
            Result = ToolBoxResult.Ok;
            Close();
        }
    }
}
