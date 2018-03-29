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

namespace PersonaEditorGUI.Controls.Editors
{
    public partial class SPREditor : UserControl
    {
        public SPREditor()
        {
            InitializeComponent();
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
                if (item.Tag is Color color)
                {
                    ColorPicker.ColorPickerTool tool = new ColorPicker.ColorPickerTool(color);
                    if (tool.ShowDialog() == true)
                        item.Tag = tool.Color;
                }
        }
    }
}