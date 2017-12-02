using Microsoft.Win32;
using PersonaEditorLib.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace SPRAtlasEditor
{
    class Key : INotifyPropertyChanged
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

        PersonaEditorLib.FileStructure.SPR.SPRKey KeyOut;

        public string Name
        {
            get { return KeyOut.mComment; }
        }
        public int X1
        {
            get { return KeyOut.X1; }
            set
            {
                if (value != KeyOut.X1)
                {
                    KeyOut.X1 = value;
                    Notify("X1");
                }
            }
        }
        public int X2
        {
            get { return KeyOut.X2; }
            set
            {
                if (value != KeyOut.X2)
                {
                    KeyOut.X2 = value;
                    Notify("X2");
                }
            }
        }
        public int Y1
        {
            get { return KeyOut.Y1; }
            set
            {
                if (value != KeyOut.Y1)
                {
                    KeyOut.Y1 = value;
                    Notify("Y1");
                }
            }
        }
        public int Y2
        {
            get { return KeyOut.Y2; }
            set
            {
                if (value != KeyOut.Y2)
                {
                    KeyOut.Y2 = value;
                    Notify("Y2");
                }
            }
        }

        public Key(PersonaEditorLib.FileStructure.SPR.SPRKey key)
        {
            KeyOut = key;
        }
    }

    class Visual
    {
        public DrawingGroup GD = new DrawingGroup();

        GeometryDrawing Brush = new GeometryDrawing();
        GeometryDrawing Boarder = new GeometryDrawing();

        public Key Key { get; private set; }

        public Visual(PersonaEditorLib.FileStructure.SPR.SPRKey Key)
        {
            this.Key = new Key(Key);
            this.Key.PropertyChanged += Key_PropertyChanged;

            GD.Children.Add(Boarder);
            GD.Children.Add(Brush);
            Boarder.Geometry = new RectangleGeometry(new Rect(new Point(Key.X1, Key.Y1), new Point(Key.X2, Key.Y2)));
            Brush.Geometry = Boarder.Geometry;
            Boarder.Pen = new Pen(new SolidColorBrush(Current.Default.LineColor), 0.5);
            Boarder.Pen.DashStyle = DashStyles.Dash;
            Current.Default.PropertyChanged += Default_PropertyChanged;
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LineColor")
            {
                Boarder.Pen.Brush = new SolidColorBrush(Current.Default.LineColor);
            }
            else if (e.PropertyName == "SelectColor")
            {
                if (Brush.Brush != null) Pick();
            }
        }

        private void Key_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Boarder.Geometry = new RectangleGeometry(new Rect(new Point(Key.X1, Key.Y1), new Point(Key.X2, Key.Y2)));
            Brush.Geometry = Boarder.Geometry;
        }

        public void Pick()
        {
            Color color = new Color()
            {
                A = 0x80,
                R = Current.Default.SelectColor.R,
                G = Current.Default.SelectColor.G,
                B = Current.Default.SelectColor.B
            };

            Brush.Brush = new SolidColorBrush(color);
        }

        public void UnPick()
        {
            Brush.Brush = null;
        }
    }

    class DRAW
    {
        public DrawingImage DI { get; private set; }
        public List<Visual> VisualList { get; private set; } = new List<Visual>();

        public DRAW(List<PersonaEditorLib.FileStructure.SPR.SPRKey> KeyList, DrawingImage DI)
        {
            this.DI = DI;

            foreach (var a in KeyList)
            {
                Visual D = new Visual(a);
                VisualList.Add(D);
                (DI.Drawing as DrawingGroup).Children.Add(D.GD);
            }
        }
    }

    public partial class MainWindow : Window
    {
        BindingList<string> Names = new BindingList<string>();
        List<DRAW> Images = new List<DRAW>();

        PersonaEditorLib.FileStructure.SPR.SPR SPR;

        public MainWindow()
        {
            Current.Default.Reload();
            InitializeComponent();
            ListNames.DataContext = Names;
            Board.DataContext = Current.Default;
        }

        string OpenFile = "";

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Persona Text Project (*.SPR)|*.SPR";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                OpenFile = ofd.FileName;
                Open(ofd.FileName);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Persona Text Project (*.SPR)|*.SPR";
            sfd.OverwritePrompt = true;
            sfd.InitialDirectory = Path.GetDirectoryName(OpenFile);
            sfd.FileName = Path.GetFileNameWithoutExtension(OpenFile);

            if (sfd.ShowDialog() == true)
            {
                Save(sfd.FileName);
            }
        }

        private void Open(string filename)
        {
            Names.Clear();
            Images.Clear();
            SPR = new PersonaEditorLib.FileStructure.SPR.SPR(filename, true);

            List<object> list = SPR.TextureList;

            for (int i = 0; i < list.Count; i++)
            {
                var img = list[i] as PersonaEditorLib.FileStructure.TMX.TMX;
                Names.Add(img.TMXname);
                var image = img.Image;

                var temp = new DrawingImage(new DrawingGroup());

                ImageDrawing ID = new ImageDrawing(image, new Rect(new Size(image.Width, image.Height)));
                (temp.Drawing as DrawingGroup).Children.Add(ID);
                (temp.Drawing as DrawingGroup).ClipGeometry = new RectangleGeometry(ID.Rect);

                Images.Add(new DRAW(SPR.KeyList.List.Where(x => x.mTextureIndex == i).ToList(), temp));
            }
        }

        private void Save(string filename)
        {
            File.WriteAllBytes(filename, SPR.Get(true));
        }

        private void ListNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var temp = sender as ListView;
            MainImage.Source = temp.SelectedIndex == -1 ? null : Images[temp.SelectedIndex].DI;
            KeyList.DataContext = temp.SelectedIndex == -1 ? null : Images[temp.SelectedIndex].VisualList;
        }

        private void SetBackground_Click(object sender, RoutedEventArgs e)
        {
            PersonaEditorLib.ColorPicker.ColorPickerTool colorPickerTool = new PersonaEditorLib.ColorPicker.ColorPickerTool(Current.Default.BackgroundColor);
            if (colorPickerTool.ShowDialog() == true)
                Current.Default.BackgroundColor = colorPickerTool.Color;
        }

        private void SetLine_Click(object sender, RoutedEventArgs e)
        {
            PersonaEditorLib.ColorPicker.ColorPickerTool colorPickerTool = new PersonaEditorLib.ColorPicker.ColorPickerTool(Current.Default.LineColor);
            if (colorPickerTool.ShowDialog() == true)
                Current.Default.LineColor = colorPickerTool.Color;
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e)
        {
            PersonaEditorLib.ColorPicker.ColorPickerTool colorPickerTool = new PersonaEditorLib.ColorPicker.ColorPickerTool(Current.Default.SelectColor);
            if (colorPickerTool.ShowDialog() == true)
                Current.Default.SelectColor = colorPickerTool.Color;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Current.Default.Save();
        }

        Visual temp;

        private void KeyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (temp != null) temp.UnPick();
            if (e.AddedItems.Count > 0)
            {
                temp = (e.AddedItems[0] as Visual);
                temp.Pick();
            }
        }
    }
}