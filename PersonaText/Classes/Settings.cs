using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaText.Classes
{
    class Settings
    {
        public delegate void PropertyChangedEventHandler(SettingName Setting);
        public event PropertyChangedEventHandler PropertyChanged;

        class EmptyClass
        {
            class Text
            {
                PropertyChangedEventHandler PropertyChanged;
                Dictionary<SettingName, object> SettingList;

                public Text(PropertyChangedEventHandler PropertyChanged, Dictionary<SettingName, object> SettingList)
                {
                    this.PropertyChanged = PropertyChanged;
                    this.SettingList = SettingList;
                }

                public int X
                {
                    get { return (int)SettingList[SettingName.EmptyGlyphScale]; }
                    set
                    {
                        if ((int)SettingList[SettingName.EmptyGlyphScale] != value)
                        {
                            SettingList[SettingName.EmptyGlyphScale] = value;
                            PropertyChanged?.Invoke(SettingName.EmptyGlyphScale);
                        }
                    }
                }
            }

            PropertyChangedEventHandler PropertyChanged;
            Dictionary<SettingName, object> SettingList;

            public EmptyClass(PropertyChangedEventHandler PropertyChanged, Dictionary<SettingName, object> SettingList)
            {
                this.PropertyChanged = PropertyChanged;
                this.SettingList = SettingList;
            }

            public double GlyphScale
            {
                get { return (double)SettingList[SettingName.EmptyGlyphScale]; }
                set
                {
                    if ((double)SettingList[SettingName.EmptyGlyphScale] != value)
                    {
                        SettingList[SettingName.EmptyGlyphScale] = value;
                        PropertyChanged?.Invoke(SettingName.EmptyGlyphScale);
                    }
                }
            }

            public int Width
            {
                get { return (int)SettingList[SettingName.EmptyWidth]; }
                set
                {
                    if ((int)SettingList[SettingName.EmptyWidth] != value)
                    {
                        SettingList[SettingName.EmptyWidth] = value;
                        PropertyChanged?.Invoke(SettingName.EmptyWidth);
                    }
                }
            }

            public int Height
            {
                get { return (int)SettingList[SettingName.EmptyHeight]; }
                set
                {
                    if ((int)SettingList[SettingName.EmptyHeight] != value)
                    {
                        SettingList[SettingName.EmptyHeight] = value;
                        PropertyChanged?.Invoke(SettingName.EmptyHeight);
                    }
                }
            }
        }

        public enum SettingName
        {
            ViewVisualizer,
            ViewPrefixPostfix,
            SelectedBackground,
            SelectedBackgroundVisual,
            EmptyGlyphScale,
            EmptyWidth,
            EmptyHeight,
            EmptyBackgroundColor,
            EmptyTextX,
            EmptyTextY,
            EmptyTextColor,
            EmptyNameX,
            EmptyNameY,
            EmptyNameColor
        }

        Dictionary<SettingName, object> SettingList = new Dictionary<SettingName, object>();

        public Settings()
        {
            SettingList.Add(SettingName.ViewVisualizer, true);
            SettingList.Add(SettingName.ViewPrefixPostfix, true);
            SettingList.Add(SettingName.SelectedBackground, "Empty");
            SettingList.Add(SettingName.SelectedBackgroundVisual, "Empty");
            SettingList.Add(SettingName.EmptyGlyphScale, (double)1);
            SettingList.Add(SettingName.EmptyWidth, 640);
            SettingList.Add(SettingName.EmptyHeight, 200);
            SettingList.Add(SettingName.EmptyBackgroundColor, System.Windows.Media.Colors.White);
            Empty = new EmptyClass(PropertyChanged, SettingList);
        }

        EmptyClass Empty;

        public bool ViewVisualizer
        {
            get { return (bool)SettingList[SettingName.ViewVisualizer]; }
            set
            {
                if ((bool)SettingList[SettingName.ViewVisualizer] != value)
                {
                    SettingList[SettingName.ViewVisualizer] = value;
                    PropertyChanged?.Invoke(SettingName.ViewVisualizer);
                }
            }
        }

        public bool ViewPrefixPostfix
        {
            get { return (bool)SettingList[SettingName.ViewPrefixPostfix]; }
            set
            {
                if ((bool)SettingList[SettingName.ViewPrefixPostfix] != value)
                {
                    SettingList[SettingName.ViewPrefixPostfix] = value;
                    PropertyChanged?.Invoke(SettingName.ViewPrefixPostfix);
                }
            }
        }

        public string SelectedBackground
        {
            get { return (string)SettingList[SettingName.SelectedBackground]; }
            set
            {
                if ((string)SettingList[SettingName.SelectedBackground] != value)
                {
                    SettingList[SettingName.SelectedBackground] = value;
                    PropertyChanged?.Invoke(SettingName.SelectedBackground);
                }
            }
        }

        public string SelectedBackgroundVisual
        {
            get { return (string)SettingList[SettingName.SelectedBackgroundVisual]; }
            set
            {
                if ((string)SettingList[SettingName.SelectedBackgroundVisual] != value)
                {
                    SettingList[SettingName.SelectedBackgroundVisual] = value;
                    PropertyChanged?.Invoke(SettingName.SelectedBackgroundVisual);
                }
            }
        }
    }
}
