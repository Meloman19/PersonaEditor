using PersonaEditor.Common;
using PersonaEditorLib.SpriteContainer;
using System;
using System.Windows;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDKeyVM : BindingObject
    {
        SPDKey Key;

        private bool _IsSelected = false;

        public string Name => Key.CommentString;

        public int X1
        {
            get { return Key.X0; }
            set
            {
                if (value != Key.X0)
                {
                    Key.X0 = value;
                    Notify(nameof(X1)); Notify(nameof(X2));
                    Notify(nameof(Rect));
                }
            }
        }
        public int X2
        {
            get { return Key.X0 + Key.Xdel; }
            set
            {
                if (value != Key.X0 + Key.Xdel)
                {
                    Key.Xdel = value - Key.X0;
                    Notify(nameof(X1)); Notify(nameof(X2));
                    Notify(nameof(Rect));
                }
            }
        }
        public int Y1
        {
            get { return Key.Y0; }
            set
            {
                if (value != Key.Y0)
                {
                    Key.Y0 = value;
                    Notify(nameof(Y1)); Notify(nameof(Y2));
                    Notify(nameof(Rect));
                }
            }
        }
        public int Y2
        {
            get { return Key.Y0 + Key.Ydel; }
            set
            {
                if (value != Key.Y0 + Key.Ydel)
                {
                    Key.Ydel = value - Key.Y0;
                    Notify(nameof(Y1)); Notify(nameof(Y2));
                    Notify(nameof(Rect));
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
                    Notify(nameof(IsSelected));
                }
            }
        }

        public SPDKeyVM(SPDKey key)
        {
            Key = key ?? throw new ArgumentNullException("key");
        }
    }
}