using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AuxiliaryLibraries.WPF;
using PersonaEditor.ViewModels.Tools;
using PersonaEditorLib.Text;

namespace PersonaEditor.ViewModels.Editors
{
    class PTPMsgVM : BindingObject
    {
        PTPMSG msg;
        private int BackgroundIndex;

        public string Name => msg.Name;

        public ObservableCollection<PTPMsgStrEditVM> Strings { get; } = new ObservableCollection<PTPMsgStrEditVM>();

        public void UpdateOldEncoding(string OldEncoding)
        {
            foreach (var a in Strings)
                a.UpdateOldEncoding(OldEncoding);
        }

        public void UpdateNewEncoding(string NewEncoding)
        {
            foreach (var a in Strings)
                a.UpdateNewEncoding(NewEncoding);
        }

        public void UpdateBackground()
        {
            //foreach (var a in Strings)
            //    a.UpdateBackground();
        }

        public async void UpdateView(bool isEnable)
        {
            await Task.Run((() => Update()));


        }

        void Update()
        {

            foreach (var a in Strings)
            {
                
                    a.UpdateView(true);

                
            }
        }

        public void UpdateBackground(int BackgroundIndex)
        {
            this.BackgroundIndex = BackgroundIndex;

            foreach (var a in Strings)
                a.UpdateBackground(BackgroundIndex);
        }

        public PTPMsgVM(PTPMSG msg, Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry> tuple, string OldEncoding, string NewEncoding, int backgroundIndex)
        {
            BackgroundIndex = backgroundIndex;
            this.msg = msg;

            foreach (var a in msg.Strings)
                Strings.Add(new PTPMsgStrEditVM(a, tuple, OldEncoding, NewEncoding, BackgroundIndex));
        }

       
    }
}