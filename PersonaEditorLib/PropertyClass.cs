using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonaEditorLib
{
    public class PropertyClass : BindingObject
    {
        private string stringValue;
        private ObservableCollection<string> comboValue = new ObservableCollection<string>();
        private int comboIndex = 0;

        public bool ReadOnly { get; }
        public string Name { get; }
        public string Type { get; }

        public string StringValue
        {
            get { return stringValue; }
            set
            {
                if (stringValue != value)
                {
                    stringValue = value;
                    Notify("StringValue");
                }
            }
        }

        public ReadOnlyObservableCollection<string> ComboValue { get; } = null;
        public int ComboIndex
        {
            get { return comboIndex; }
            set
            {
                if (comboIndex != value)
                {
                    comboIndex = value;
                    Notify("ComboIndex");
                }
            }
        }



        public PropertyClass(string name, string value, bool readOnly) : this(name, readOnly)
        {
            stringValue = value;
            Type = "TextBox";
        }

        public PropertyClass(string name, string[] values, int select) : this(name, false)
        {
            foreach (var a in values)
                comboValue.Add(a);
            ComboValue = new ReadOnlyObservableCollection<string>(comboValue);
            Type = "ComboBox";
        }

        private PropertyClass(string name, bool readOnly)
        {
            Name = name;
            ReadOnly = readOnly;
        }
    }
}
