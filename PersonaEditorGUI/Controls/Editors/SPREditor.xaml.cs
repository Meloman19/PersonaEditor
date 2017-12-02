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

        private void RelativePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Classes.Controls.RelativePanel item && item.DesiredSize.Height > 0 & item.DesiredSize.Width > 0)
                if (item.DataContext is TMXVM tmx)
                {
                    var point = e.GetPosition(item);
                    tmx.Point = new Point(point.X / item.DesiredSize.Width, point.Y / item.DesiredSize.Height);
                }
        }

        private void RelativePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Classes.Controls.RelativePanel item && item.DesiredSize.Height > 0 & item.DesiredSize.Width > 0)
                if (item.DataContext is TMXVM tmx)
                    tmx.Point = new Point();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Border input)
            {
                var pos = e.GetPosition(input);

                Mouse.OverrideCursor = Math.Min(pos.X, Math.Abs(pos.X - input.DesiredSize.Width)) > Math.Min(pos.Y, Math.Abs(pos.Y - input.DesiredSize.Height)) ? Cursors.SizeNS : Cursors.SizeWE;
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Border input)
            {
                var pos = new Point(-1, -1);
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void UserControl_MouseMove_1(object sender, MouseEventArgs e)
        {
            CONTR.Text = e.OriginalSource.GetType().ToString();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (SPRKey a in e.RemovedItems)
                a.IsSelected = false;

            foreach (SPRKey a in e.AddedItems)
                a.IsSelected = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
                if (item.Tag is Color color)
                {
                    PersonaEditorLib.ColorPicker.ColorPickerTool tool = new PersonaEditorLib.ColorPicker.ColorPickerTool(color);
                    if (tool.ShowDialog() == true)
                        item.Tag = tool.Color;
                }
        }
    }
}