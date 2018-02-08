﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using PersonaEditorLib;
using PersonaEditorLib.Interfaces;

namespace PersonaEditorGUI.Settings
{
    class DefaultBackgroundVM : BindingObject, ISetting
    {
        BackgroundDefault tempSetting = new BackgroundDefault();

        public string TextXpos
        {
            get { return tempSetting.EmptyTextPos.X.ToString(); }
            set
            {
                double temp;
                if (double.TryParse(value, out temp))
                    tempSetting.EmptyTextPos = new System.Windows.Point(temp, tempSetting.EmptyTextPos.Y);

                Notify("TextXpos");
            }
        }
        public string TextYpos
        {
            get { return tempSetting.EmptyTextPos.Y.ToString(); }
            set
            {
                double temp;
                if (double.TryParse(value, out temp))
                    tempSetting.EmptyTextPos = new System.Windows.Point(tempSetting.EmptyTextPos.X, temp);

                Notify("TextYpos");
            }
        }
        public string TextColor
        {
            get { return tempSetting.EmptyTextColor.ToString(); }
            set
            {
                try
                {
                    Color temp = (Color)ColorConverter.ConvertFromString(value);
                    tempSetting.EmptyTextColor = temp;
                }
                catch { }

                Notify("TextColor");
            }
        }

        public string NameXpos
        {
            get { return tempSetting.EmptyNamePos.X.ToString(); }
            set
            {
                double temp;
                if (double.TryParse(value, out temp))
                    tempSetting.EmptyNamePos = new System.Windows.Point(temp, tempSetting.EmptyNamePos.Y);

                Notify("TextXpos");
            }
        }
        public string NameYpos
        {
            get { return tempSetting.EmptyNamePos.Y.ToString(); }
            set
            {
                double temp;
                if (double.TryParse(value, out temp))
                    tempSetting.EmptyNamePos = new System.Windows.Point(tempSetting.EmptyNamePos.X, temp);

                Notify("TextYpos");
            }
        }
        public string NameColor
        {
            get { return tempSetting.EmptyNameColor.ToString(); }
            set
            {
                try
                {
                    Color temp = (Color)ColorConverter.ConvertFromString(value);
                    tempSetting.EmptyNameColor = temp;
                }
                catch { }

                Notify("TextColor");
            }
        }

        public string BackgroundWidth
        {
            get { return tempSetting.EmptyWidth.ToString(); }
            set
            {
                int temp;
                if (int.TryParse(value, out temp))
                    tempSetting.EmptyWidth = temp;

                Notify("BackgroundWidth");
            }
        }
        public string BackgroundHeight
        {
            get { return tempSetting.EmptyHeight.ToString(); }
            set
            {
                int temp;
                if (int.TryParse(value, out temp))
                    tempSetting.EmptyHeight = temp;

                Notify("BackgroundHeight");
            }
        }
        public string BackgroundColor
        {
            get { return tempSetting.EmptyBackgroundColor.ToString(); }
            set
            {
                try
                {
                    Color temp = (Color)ColorConverter.ConvertFromString(value);
                    tempSetting.EmptyBackgroundColor = temp;
                }
                catch { }

                Notify("BackgroundColor");
            }
        }

        public string GlyphScale
        {
            get { return tempSetting.EmptyGlyphScale.ToString(); }
            set
            {
                double temp;
                if (double.TryParse(value, out temp))
                    tempSetting.EmptyGlyphScale = temp;

                Notify("GlyphScale");
            }
        }
        public string LineSpacing
        {
            get { return tempSetting.EmptyLineSpacing.ToString(); }
            set
            {
                int temp;
                if (int.TryParse(value, out temp))
                    tempSetting.EmptyLineSpacing = temp;

                Notify("LineSpacing");
            }
        }

        private bool _IsChanged = false;
        public bool IsChanged => _IsChanged;

        public void Save()
        {
            _IsChanged = false;
            Notify("IsChanged");
            tempSetting.Save();
            BackgroundDefault.Default.Reload();
        }

        public void Reset()
        {
            tempSetting.Reload();
            _IsChanged = false;
            Notify("IsChanged");
        }

        public DefaultBackgroundVM()
        {
            tempSetting.PropertyChanged += TempSetting_PropertyChanged;
            SetColor = new RelayCommandWeak(x => SetColot_Click(x));
        }

        private void TempSetting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _IsChanged = true;
            Notify("IsChanged");
        }

        public ICommand SetColor { get; private set; }

        private void SetColot_Click(object param)
        {
            if ((string)param == "Text")
            {
                var a = ColorConverter.ConvertFromString(TextColor);
                Controls.ColorPicker.ColorPickerTool colorPicker = new Controls.ColorPicker.ColorPickerTool((Color)a);
                if (colorPicker.ShowDialog() == true)
                    TextColor = colorPicker.Color.ToString();
            }
            else if ((string)param == "Name")
            {
                var a = ColorConverter.ConvertFromString(NameColor);
                Controls.ColorPicker.ColorPickerTool colorPicker = new Controls.ColorPicker.ColorPickerTool((Color)a);
                if (colorPicker.ShowDialog() == true)
                    NameColor = colorPicker.Color.ToString();
            }
            else if ((string)param == "Back")
            {
                var a = ColorConverter.ConvertFromString(BackgroundColor);
                Controls.ColorPicker.ColorPickerTool colorPicker = new Controls.ColorPicker.ColorPickerTool((Color)a);
                if (colorPicker.ShowDialog() == true)
                    BackgroundColor = colorPicker.Color.ToString();
            }
        }
    }
}