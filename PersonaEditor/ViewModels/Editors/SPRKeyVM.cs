using System;
using System.Windows;
using AuxiliaryLibraries.WPF;
using PersonaEditorLib.SpriteContainer;

namespace PersonaEditor.ViewModels.Editors
{
    class SPRKeyVM : BindingObject
    {
        SPRKey Key;

        private bool _IsSelected = false;

        public string Name
        {
            get { return Key.mComment; }
        }

        public int X1
        {
            get { return Key.X1; }
            set
            {
                if (value != Key.X1)
                {
                    Key.X1 = value;
                    Notify("X1");
                    Notify("Rect");
                }
            }
        }

        public int X2
        {
            get { return Key.X2; }
            set
            {
                if (value != Key.X2)
                {
                    Key.X2 = value;
                    Notify("X2");
                    Notify("Rect");
                }
            }
        }

        public int Y1
        {
            get { return Key.Y1; }
            set
            {
                if (value != Key.Y1)
                {
                    Key.Y1 = value;
                    Notify("Y1");
                    Notify("Rect");
                }
            }
        }
        public int Y2
        {
            get { return Key.Y2; }
            set
            {
                if (value != Key.Y2)
                {
                    Key.Y2 = value;
                    Notify("Y2");
                    Notify("Rect");
                }
            }
        }

        public Rect Rect
        {
            get { return new Rect(new Point(X1, Y1), new Point(X2, Y2)); }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    Notify("IsSelected");
                }
            }
        }

        public SPRKeyVM(SPRKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
        }
    }
}