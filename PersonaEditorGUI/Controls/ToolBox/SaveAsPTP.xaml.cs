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

namespace PersonaEditorGUI.Controls.ToolBox
{
    partial class SaveAsPTP : Window, INotifyPropertyChanged
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
        private bool copyOld2New = false;
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

        public bool CopyOld2New
        {
            get { return copyOld2New; }
            set
            {
                copyOld2New = value;
                Notify("CopyOld2New");
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

        public SaveAsPTP()
        {
            InitializeComponent();
            DataContext = this;

            int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(Settings.AppSetting.Default.SaveAsPTP_Font);
            if (sourceInd >= 0)
                selectedFont = sourceInd;
            else
                selectedFont = 0;

            CopyOld2New = Settings.AppSetting.Default.SaveAsPTP_CO2N;
            NeverAskAgain = Settings.AppSetting.Default.SaveAsPTP_NeverAskAgain;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Settings.AppSetting.Default.SaveAsPTP_Font = Static.EncodingManager.GetPersonaEncodingName(SelectedFont);
            Settings.AppSetting.Default.SaveAsPTP_CO2N = CopyOld2New;
            Settings.AppSetting.Default.SaveAsPTP_NeverAskAgain = NeverAskAgain;
            Result = ToolBoxResult.Ok;
            Close();
        }
    }
}
