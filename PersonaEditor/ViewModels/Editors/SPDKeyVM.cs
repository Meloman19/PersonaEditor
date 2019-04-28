using PersonaEditorLib.SpriteContainer;
using AuxiliaryLibraries.WPF;
using PersonaEditor.Classes;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace PersonaEditor.ViewModels.Editors
{
    class SPDKeyVM : BindingObject
    {
        SPDKey Key;

        private bool _IsSelected = false;

        public string Name => Encoding.GetEncoding("shift-jis").GetString(Key.Comment.Where(x => x != 0x00).ToArray());

        public int X1
        {
            get { return Key.X0; }
            set
            {
                if (value != Key.X0)
                {
                    Key.X0 = value;
                    Notify("X1"); Notify("X2");
                    Notify("Rect");
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
                    Notify("X1"); Notify("X2");
                    Notify("Rect");
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
                    Notify("Y1"); Notify("Y2");
                    Notify("Rect");
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
                    Notify("Y1"); Notify("Y2");
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

        public SPDKeyVM(SPDKey key)
        {
            Key = key ?? throw new ArgumentNullException("key");
        }
    }
}