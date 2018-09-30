using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PersonaEditor.Views
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