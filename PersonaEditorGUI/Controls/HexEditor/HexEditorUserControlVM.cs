using PersonaEditorLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PersonaEditorGUI.Controls.HexEditor
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

        public MouseWheelEventHandler MouseWheel => mouseWheel;

        private void mouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ScrollBarVM.SetValue(false);
            else if (e.Delta < 0)
                ScrollBarVM.SetValue(true);
        }

        public HexEditorUserControlVM()
        {
            ScrollBarVM.ValueChanged += ScrollBarVM_ValueChanged;
        }

        private void ScrollBarVM_ValueChanged(double value)
        {
            long val = (long)value;
            HexViewVM.SetStartOffset(val * 0x10);
        }
    }
}