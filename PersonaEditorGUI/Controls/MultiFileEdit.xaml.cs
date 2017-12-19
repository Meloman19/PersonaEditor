using PersonaEditorGUI.Classes;
using PersonaEditorLib.Interfaces;
using System;
using System.Collections.Generic;
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

namespace PersonaEditorGUI.Controls
{
    public partial class MultiFileEdit : UserControl
    {
        public MultiFileEdit()
        {
            InitializeComponent();
        }
        
        double temp = 0;
        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FirstCol.MaxWidth == Double.PositiveInfinity)
            {
                temp = FirstCol.Width.Value;
                FirstCol.MaxWidth = 0;
            }
            else
            {
                FirstCol.MaxWidth = Double.PositiveInfinity;
                FirstCol.Width = new GridLength(temp);
            }
        }
    }
}
