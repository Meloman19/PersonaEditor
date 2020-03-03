using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public class TriggerCollection : AttachableCollection<TriggerBase>
    {
        internal TriggerCollection()
        {
        }

        internal override void ItemAdded(TriggerBase item)
        {
            if (base.AssociatedObject != null)
            {
                item.Attach(base.AssociatedObject);
            }
        }

        internal override void ItemRemoved(TriggerBase item)
        {
            if (((IAttachedObject)item).AssociatedObject != null)
            {
                item.Detach();
            }
        }

        protected override void OnAttached()
        {
            foreach (TriggerBase base2 in this)
            {
                base2.Attach(base.AssociatedObject);
            }
        }

        protected override void OnDetaching()
        {
            foreach (TriggerBase base2 in this)
            {
                base2.Detach();
            }
        }


        protected override Freezable CreateInstanceCore() => new TriggerCollection();
    }
}