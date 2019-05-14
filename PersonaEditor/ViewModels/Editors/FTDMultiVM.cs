using PersonaEditorLib.Other;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Controls.ToolBox;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditor.ViewModels.Editors
{
    class FTDMultiVM : FTDEntryVM
    {
        public string ButtonContent { get; set; } = "+";
        public ICommand Expand { get; }
        public ICommand Resize { get; }
        public Visibility Visibility { get; set; } = Visibility.Collapsed;
        public ObservableCollection<FTDSingleVM> MultiElements { get; } = new ObservableCollection<FTDSingleVM>();

        private void expand()
        {
            if (Visibility == Visibility.Collapsed)
            {
                ButtonContent = "-";
                Visibility = Visibility.Visible;
            }
            else
            {
                ButtonContent = "+";
                Visibility = Visibility.Collapsed;
            }

            Notify("Visibility");
            Notify("ButtonContent");
        }

        private void resize()
        {
            Resize resize = new Resize();
            resize.Size = ftd.Entries[index][0].Length;
            if (resize.ShowDialog() == true)
            {
                foreach (var a in MultiElements)
                    a.Resize(resize.Size);
            }
        }

        public FTDMultiVM(FTD ftd, int index, Encoding encoding) : base(ftd, index, encoding)
        {
            Expand = new RelayCommand(expand);
            Resize = new RelayCommand(resize);

            for (int i = 0; i < ftd.Entries[index].Length; i++)
                MultiElements.Add(new FTDSingleVM(ftd, index, i, encoding));
        }

        public override void OnSetEncoding()
        {
            foreach (var a in MultiElements)
                a.SetEncoding(encoding);
        }
    }
}