using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PersonaEditor.Classes.Visual;
using System.Windows.Media;
using System.Windows.Threading;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.Text;
using PersonaEditor.Classes;
using PersonaEditor.Classes.Managers;
using PersonaEditor.ViewModels.Tools;
using MessageBox = System.Windows.Forms.MessageBox;

namespace PersonaEditor.ViewModels.Editors
{
    class PTPEditorVM : BindingObject, IEditor
    {
        #region Private

        EventWrapperINPC EncodingManagerEW;

        #endregion Private

        private int Background;

        private Background selectBack = null;
        private Background SelectBack



        {
            get { return selectBack; }
            set
            {
                if (selectBack != value)
                {
                    if (selectBack != null)
                        selectBack.BackgroundChanged -= SelectBack_BackgroundChanged;
                    selectBack = value;
                    if (selectBack != null)
                        selectBack.BackgroundChanged += SelectBack_BackgroundChanged;
                }
            }
        }

        public ImageDrawing BackgroundDrawing { get; } = new ImageDrawing();
        public RectangleGeometry ClipGeometry { get; } = new RectangleGeometry();

        private void SelectBack_BackgroundChanged(Background background)
        {
            UpdateBackground(SelectedBackgroundIndex);
            BackgroundDrawing.ImageSource = SelectBack.Image;
            BackgroundDrawing.Rect = SelectBack.Rect;
            ClipGeometry.Rect = SelectBack.Rect;
        }

        public ReadOnlyObservableCollection<string> BackgroundList => Static.BackManager.BackgroundList;
        public int SelectedBackgroundIndex
        {
            get { return Background; }
            set
            {

                    if (SelectBack != Static.BackManager.GetBackground(value) && Static.BackManager.GetBackground(value) != null)
                    {
                        SelectBack = Static.BackManager.GetBackground(value);
                        Background = value;
                        ApplicationSettings.AppSetting.Default.PTPBackgroundDefault = Static.BackManager.GetBackgroundName(value);
                        UpdateBackground(value);
                        BackgroundDrawing.ImageSource = SelectBack.Image;
                        BackgroundDrawing.Rect = SelectBack.Rect;
                        ClipGeometry.Rect = SelectBack.Rect;
                    }
                    Notify("SelectedBackgroundIndex");
                
            }
        }

        private int OldEncoding;
        private int NewEncoding;
        private bool View;

        public ReadOnlyObservableCollection<string> FontList => Static.EncodingManager.EncodingList;

        public int SelectedOldFont
        {
            get { return OldEncoding; }
            set
            {
                OldEncoding = value;
                ApplicationSettings.AppSetting.Default.PTPOldDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateOldEncoding(ApplicationSettings.AppSetting.Default.PTPOldDefault);
                Notify("SelectedOldFont");
            }
        }

        public int SelectedNewFont
        {
            get { return NewEncoding; }
            set
            {
                NewEncoding = value;
                ApplicationSettings.AppSetting.Default.PTPNewDefault = Static.EncodingManager.GetPersonaEncodingName(value);
                UpdateNewEncoding(ApplicationSettings.AppSetting.Default.PTPNewDefault);
                Notify("SelectedNewFont");
            }
        }

        public bool ViewImage
        {
            get { return View; }
            set
            {
                if (View != value)
                {
                    View = value;
                    ApplicationSettings.AppSetting.Default.PTPImageView = value;
                    Notify("ViewImage");
                    UpdateView();
                }
            }
        }

        public ObservableCollection<PTPNameEditVM> Names { get; } = new ObservableCollection<PTPNameEditVM>();
        public ObservableCollection<PTPMsgVM> MSG { get; } = new ObservableCollection<PTPMsgVM>();

        public override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PersonaEncodingManager man)
            {
                if (e.PropertyName == man.GetPersonaEncodingName(OldEncoding))
                    UpdateOldEncoding(ApplicationSettings.AppSetting.Default.PTPOldDefault);
                if (e.PropertyName == man.GetPersonaEncodingName(NewEncoding))
                    UpdateNewEncoding(ApplicationSettings.AppSetting.Default.PTPNewDefault);
            }
        }

        private void UpdateOldEncoding(string oldEncoding)
        {
            foreach (var a in Names)
                a.UpdateOldEncoding(oldEncoding);

            foreach (var a in MSG)
                a.UpdateOldEncoding(oldEncoding);
        }

        private void UpdateNewEncoding(string newEncoding)
        {
            foreach (var a in Names)
                a.UpdateNewEncoding(newEncoding);

            foreach (var a in MSG)
                a.UpdateNewEncoding(newEncoding);
        }

        private void UpdateBackground(int backgroundIndex)
        {
            foreach (var a in Names)
                a.UpdateBackground(backgroundIndex);

            foreach (var a in MSG)
                a.UpdateBackground(backgroundIndex);
        }

        private void UpdateView()
        {

            foreach (var a in Names)
                a.UpdateView(View);

            foreach (var a in MSG)
                a.UpdateView(View);
        }

       

        public ICommand FromCommand { get; }
        public ICommand FindCommand { get; }

        public PTPEditorVM(PTP ptp)
        {

            FromCommand = new RelayCommand(Replace);
            BackgroundWorker.Status = "Загрузка...";

            int sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(ApplicationSettings.AppSetting.Default.PTPOldDefault);
            if (sourceInd >= 0)
                OldEncoding = sourceInd;
            else
                OldEncoding = 0;

            sourceInd = Static.EncodingManager.GetPersonaEncodingIndex(ApplicationSettings.AppSetting.Default.PTPNewDefault);
            if (sourceInd >= 0)
                NewEncoding = sourceInd;
            else
                NewEncoding = 0;

            sourceInd = Static.BackManager.GetBackgroundIndex(ApplicationSettings.AppSetting.Default.PTPBackgroundDefault);
            if (sourceInd >= 0)
                SelectedBackgroundIndex = sourceInd;
            else
                SelectedBackgroundIndex = 0;

            View = ApplicationSettings.AppSetting.Default.PTPImageView;
            EncodingManagerEW = new EventWrapperINPC(Static.EncodingManager, this);

            foreach (var a in ptp.Names)
                Names.Add(new PTPNameEditVM(a, OldEncoding, NewEncoding, SelectedBackgroundIndex));

            
            foreach (var a in ptp.Msg)
            {
                var name = Names.FirstOrDefault(x => x.Index == a.CharacterIndex);
                Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry> tuple;
                if (name == null)
                    tuple = new Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry>(null, null, BackgroundDrawing, ClipGeometry);
                else
                    tuple = new Tuple<ImageDrawing, ImageDrawing, ImageDrawing, RectangleGeometry>(name.OldNameVisual.TextDrawing, name.NewNameVisual.TextDrawing, BackgroundDrawing, ClipGeometry);

                MSG.Add(new PTPMsgVM(a, tuple, ApplicationSettings.AppSetting.Default.PTPOldDefault, ApplicationSettings.AppSetting.Default.PTPNewDefault, SelectedBackgroundIndex));
            }


            BackgroundWorker.Status = "Готово...";


        }




        delegate void ProgressBarProcess(int value);

        private int _RowsValue = 3;
        public int RowsValue
        {
            get
            {
                return _RowsValue;
            }
            set
            {
                _RowsValue = value;
                Notify("RowsValue");
            }
        }

        public void Replace()
        {
            BackgroundWorker.Status = "Дождитесь открытия файла...";
            BackgroundWorker.ProgressMaximum = MSG.Count;

            OpenFileDialog ofs = new OpenFileDialog();
            ofs.Filter = ".txt файл с notabenoid(*.txt)|*.txt|All files(*.*)|*.*";
            if (ofs.ShowDialog() == DialogResult.OK)
            {
                List<string> textlist = NotaScript.Parse(ofs.FileName, _RowsValue);

                foreach (var ms in MSG)
                {
                    foreach (var msg in ms.Strings)
                    {

                        for (int j = 0; j < textlist.Count; j++)
                        {
                            if (textlist[j].Contains(ms.Name))
                            {

                                string[] t = textlist[j].Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                msg.NewString = t[t.Length - 1];
                                
                                textlist.RemoveAt(j);
                                break;
                            }
                        }
                    }
                }
                BackgroundWorker.Status = "";

                //DialogResult dialogResult = MessageBox.Show("Sure", "Some Title", MessageBoxButtons.YesNo);
                //if (dialogResult == DialogResult.Yes)
                //{
                //    foreach (var ms in MSG)
                //    {
                //        foreach (var str in ms.Strings)
                //        {
                //            str.NewString = NewLine(str.NewString);
                //        }
                //    }
                //}
                //else if (dialogResult == DialogResult.No)
                //{
                //    //do something else
                //}
            }


        }

        public string NewLine(string line)
        {
            if (line.Length > 33)
            {
                return GetTextWithNewLines(line);
            }
            else
            {
                return line;
            }

            
        }

        public static string GetTextWithNewLines(string value = "", int charactersToWrapAt = 33, int maxLength = 250)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            value = value.Replace("  ", " ");
            var words = value.Split(' ');
            var sb = new StringBuilder();
            var currString = new StringBuilder();

            foreach (var word in words)
            {
                if (currString.Length + word.Length + 1 < charactersToWrapAt) // The + 1 accounts for spaces
                {
                    sb.AppendFormat(" {0}", word);
                    currString.AppendFormat(" {0}", word);
                }
                else
                {
                    currString.Clear();
                    sb.AppendFormat("{0}{1}", Environment.NewLine, word);
                    currString.AppendFormat(" {0}", word);
                }
            }

            if (sb.Length > maxLength)
            {
                return sb.ToString().Substring(0, maxLength) + " ...";
            }

            return sb.ToString().TrimStart().TrimEnd();
        }

        public bool Close()
        {
            SelectBack = null;
            return true;
        }
    }
}