using PersonaEditorLib;
using PersonaEditorLib.FileStructure.PTP;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using PersonaEditorLib.Extension;
using System.ComponentModel;

namespace PersonaEditorGUI.Controls.Editors
{
    public class ObservableVariable : INotifyPropertyChanged
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

        public Classes.Media.Visual.Backgrounds BackImage { get; } = new Classes.Media.Visual.Backgrounds();
        
        public CharList OldCharList { get; set; }
        public CharList NewCharList { get; set; }
    }

    public class BytesToString : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] array = (byte[])values[0];
            CharList charlist = (CharList)values[1];
            return array.GetTextBaseList().GetString(charlist, true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MSGListToText : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IList<TextBaseElement> array = values[0] as IList<TextBaseElement>;
            CharList charlist = (CharList)values[1];
            return array.GetString(charlist, true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MSGListToSystem : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string returned = "";

            IList<TextBaseElement> list = (IList<TextBaseElement>)value;
            foreach (var Bytes in list)
            {
                byte[] temp = Bytes.Array.ToArray();
                if (temp.Length > 0)
                {
                    returned += "{" + System.Convert.ToString(temp[0], 16).PadLeft(2, '0').ToUpper();
                    for (int i = 1; i < temp.Length; i++)
                    {
                        returned += "\u00A0" + System.Convert.ToString(temp[i], 16).PadLeft(2, '0').ToUpper();
                    }
                    returned += "} ";
                }
            }

            return returned;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class PTPEditor : UserControl
    {
        public PTPEditor()
        {
            InitializeComponent();
        }        
    }
}