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

        private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
        {
            var sen = sender as FrameworkElement;
            Rect temp;
            if (sen.DataContext is SPRTextureVM spr)
                temp = spr.Rect;
            if (sen.DataContext is SPDTextureVM spd)
                temp = spd.Rect;
            else
                return;

            var a = e.GetPosition(sender as IInputElement);

            var newX = Math.Round((a.X / sen.ActualWidth) * temp.Width);
            XCoo.Text = newX.ToString();
            var newY = Math.Round((a.Y / sen.ActualHeight) * temp.Height);
            YCoo.Text = newY.ToString();
        }
    }
}