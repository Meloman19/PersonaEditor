using AuxiliaryLibraries.WPF;
using System.IO;
using System.Windows.Input;

namespace PersonaEditor.Controls.HexEditor
{
    class HexEditorUserControlVM : BindingObject
    {
        private Stream stream;

        public ScrollBarVM ScrollBarVM { get; } = new ScrollBarVM();
        public HexViewVM HexViewVM { get; } = new HexViewVM();

        public double ActualHeight { set { HexViewVM.SetHeight(value); } }

        public void SetStream(Stream stream)
        {
            this.stream = stream;
            int linecount = (int)stream.Length / 0x10;
            ScrollBarVM.Maximum = linecount;

            HexViewVM.SetStream(stream);
        }

        public ICommand MouseWheel { get; }

        private void mouseWheel(object arg)
        {
            int delta = (int)arg;
            if (delta > 0)
                ScrollBarVM.SetValue(false);
            else if (delta < 0)
                ScrollBarVM.SetValue(true);
        }

        public HexEditorUserControlVM()
        {
            MouseWheel = new RelayCommand(mouseWheel);
            ScrollBarVM.ValueChanged += ScrollBarVM_ValueChanged;
        }

        private void ScrollBarVM_ValueChanged(double value)
        {
            long val = (long)value;
            HexViewVM.SetStartOffset(val * 0x10);
        }
    }
}