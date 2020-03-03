using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public class ActionCollection : AttachableCollection<ActionBase>
    {
        public ActionCollection()
        { }

        protected override void OnAttached()
        { }

        protected override void OnDetaching()
        { }

        internal override void ItemAdded(ActionBase item)
        { }

        internal override void ItemRemoved(ActionBase item)
        { }
    }
}