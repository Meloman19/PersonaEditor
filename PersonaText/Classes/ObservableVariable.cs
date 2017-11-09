using PersonaEditorLib;
using PersonaEditorLib.FileStructure.PTP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PersonaText
{
    public class ObservableVariable : INotifyPropertyChanged
    {
        public Backgrounds BackImage { get; } = new Backgrounds();

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

        public ObservableVariable()
        {
            PTP = new PTP(OldCharList, NewCharList);
        }

        public CharList OldCharList { get; set; } = new CharList();
        public CharList NewCharList { get; set; } = new CharList();

        private bool _OpenFile = false;
        public bool OpenFile
        {
            get { return _OpenFile; }
            set
            {
                if (value != _OpenFile)
                {
                    _OpenFile = value;
                    Notify("OpenFile");
                }
            }
        }

        public PTP PTP { get; private set; }

        private Visibility _ViewVisualizer = Visibility.Visible;
        public Visibility ViewVisualizer
        {
            get { return _ViewVisualizer; }
            set
            {
                if (value != _ViewVisualizer)
                {
                    _ViewVisualizer = value;
                    Notify("ViewVisualizer");
                }
            }
        }

        private Visibility _ViewPrefixPostfix = Visibility.Visible;
        public Visibility ViewPrefixPostfix
        {
            get { return _ViewPrefixPostfix; }
            set
            {
                if (value != _ViewPrefixPostfix)
                {
                    _ViewPrefixPostfix = value;
                    Notify("ViewPrefixPostfix");
                }
            }
        }
    }
}