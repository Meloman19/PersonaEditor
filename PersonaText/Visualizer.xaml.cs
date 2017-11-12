using System;
using System.Collections.Generic;
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
using PersonaEditorLib.FileStructure.PTP;
using PersonaEditorLib.Extension;

namespace PersonaText
{
    public partial class Visualizer : Window
    {
        ObservableVariable OV;

        Backgrounds backgrounds = new Backgrounds();

        DrawingGroup DG = new DrawingGroup();

        DrawingCollection OldCol = new DrawingCollection();
        DrawingCollection NewCol = new DrawingCollection();

        Visual Old;
        Visual New;
        
        string state = "Old";

        public Visualizer(ObservableVariable OV)
        {
            this.OV = OV;

            InitializeComponent();
            SelectBackground.DataContext = backgrounds.BackgroundList;
            SelectBackground.SelectedIndex = 0;

            Old = new Visual(OV.OldCharList, backgrounds.CurrentBackground);
            New = new Visual(OV.NewCharList, backgrounds.CurrentBackground);
            
            backgrounds.BackgroundChanged += Old.Background_Update;
            backgrounds.BackgroundChanged += New.Background_Update;

            backgrounds.CurrentBackground.PropertyChanged += CurrentBackground_PropertyChanged;
            InitAfter();
        }

        private void CurrentBackground_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Image")
                DG.ClipGeometry = backgrounds.CurrentBackground.Rect;
        }

        public void InitAfter()
        {
            DG.ClipGeometry = backgrounds.CurrentBackground.Rect;

            OldCol.Add(backgrounds.CurrentBackground.Drawing);
            OldCol.Add(Old.DrawingName);
            OldCol.Add(Old.DrawingText);

            NewCol.Add(backgrounds.CurrentBackground.Drawing);
            NewCol.Add(New.DrawingName);
            NewCol.Add(New.DrawingText);

            VisualText.Source = new DrawingImage(DG);
        }

        private void SelectBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            backgrounds.SetBackground(index);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string temp = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
            state = temp;
            Text2HEX();
            if (temp == "Old")
                DG.Children = OldCol;
            else
                DG.Children = NewCol;
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            Old.Name_Update(text.GetTextBaseList(OV.OldCharList));
            New.Name_Update(text.GetTextBaseList(OV.NewCharList));
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            Text2HEX();
            Old.Text_Update(text.GetTextBaseList(OV.OldCharList));
            New.Text_Update(text.GetTextBaseList(OV.NewCharList));
        }

        private void Text2HEX()
        {
            if (TextBoxText != null)
                if (state == "Old")
                {
                    string text = TextBoxText.Text;
                    var temp = text.GetTextBaseList(OV.OldCharList).GetByteArray();
                    HEX.Text = BitConverter.ToString(temp).Replace('-', ' ');
                }
                else
                {
                    string text = TextBoxText.Text;
                    var temp = text.GetTextBaseList(OV.NewCharList).GetByteArray();
                    HEX.Text = BitConverter.ToString(temp).Replace('-', ' ');
                }
        }
    }
}