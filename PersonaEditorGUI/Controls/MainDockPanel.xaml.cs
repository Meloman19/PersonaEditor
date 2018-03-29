using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PersonaEditorGUI.Controls
{
    public partial class MainDockPanel : UserControl, INotifyPropertyChanged
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

        private Dock _Dock = (Dock)(-1);
        public Dock Dock
        {
            get { return _Dock; }
            set
            {
                if (value != _Dock)
                {
                    _Dock = value;
                    Notify("Dock");
                }
            }
        }

        public MainDockPanel()
        {
            InitializeComponent();
        }

        public MainDockPanel(Dock dock) : this()
        {
            Dock = dock;
        }

        private void Add(object UIElement, string name)
        {
            TabItem item = new TabItem
            {
                Header = name,
                Content = UIElement
            };
            DockMain.Items.Add(item);
            DockMain.SelectedIndex = DockMain.Items.Count - 1;
        }

        public void Remove(object UIElement)
        {
            //foreach (var a in MainTabControl.Items)
            //    if (((TabItem)a).Content == UIElement)
            //    {
            //        MainTabControl.Items.Remove(a);
            //        break;
            //    }

            //if (DockLeft.Content != null)
            //    (DockLeft.Content as MainDockPanel).Remove(UIElement);

            //if (DockRight.Content != null)
            //    (DockRight.Content as MainDockPanel).Remove(UIElement);
        }

        private void Add(object UIElement, string name, Dock dock)
        {


            //if ((int)dock == -1)
            //{
            //    Add(UIElement, name);
            //}
            //else if (dock == Dock.Left)
            //{
            //    if (DockLeft.Content == null)
            //        DockLeft.Content = new MainDockPanel(dock);

            //    (DockLeft.Content as MainDockPanel).Add(UIElement, name);
            //}
            //else if (dock == Dock.Right)
            //{
            //    if (DockRight.Content == null)
            //        DockRight.Content = new MainDockPanel(dock);

            //    (DockRight.Content as MainDockPanel).Add(UIElement, name);
            //}
        }

        public void Add(object UIElement, string name, Dock[] dock = null)
        {
            if (dock == null)
                Add(UIElement, name);
            else if (dock.Length == 1)
                Add(UIElement, name, dock[0]);
            else if (dock.Length > 1)
            {

            }
            else
            {
            }
        }
    }
}