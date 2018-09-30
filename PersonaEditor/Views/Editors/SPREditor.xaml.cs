using PersonaEditor.ViewModels.Editors;
using PersonaEditor.Views.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonaEditor.Views.Editors
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
                    ColorPickerTool tool = new ColorPickerTool(color);
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