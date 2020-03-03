using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public abstract class AttachableCollection<T> : FreezableCollection<T>, IAttachedObject where T : DependencyObject, IAttachedObject
    {
        private Collection<T> snapshot;
        private DependencyObject associatedObject;

        internal AttachableCollection()
        {
            INotifyCollectionChanged changed = this;
            changed.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCollectionChanged);
            this.snapshot = new Collection<T>();
        }

        public void Attach(DependencyObject dependencyObject)
        {
            if (dependencyObject != this.AssociatedObject)
            {
                if (this.AssociatedObject != null)
                {
                    throw new InvalidOperationException();
                }
                if (Interaction.ShouldRunInDesignMode || !((bool)base.GetValue(DesignerProperties.IsInDesignModeProperty)))
                {
                    base.WritePreamble();
                    this.associatedObject = dependencyObject;
                    base.WritePostscript();
                }
                this.OnAttached();
            }
        }

        public void Detach()
        {
            this.OnDetaching();
            base.WritePreamble();
            this.associatedObject = null;
            base.WritePostscript();
        }

        internal abstract void ItemAdded(T item);
        internal abstract void ItemRemoved(T item);
        protected abstract void OnAttached();
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T local in e.NewItems)
                    {
                        try
                        {
                            this.VerifyAdd(local);
                            this.ItemAdded(local);
                        }
                        finally
                        {
                            this.snapshot.Insert(base.IndexOf(local), local);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (T local4 in e.OldItems)
                    {
                        this.ItemRemoved(local4);
                        this.snapshot.Remove(local4);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (T local2 in e.OldItems)
                    {
                        this.ItemRemoved(local2);
                        this.snapshot.Remove(local2);
                    }
                    foreach (T local3 in e.NewItems)
                    {
                        try
                        {
                            this.VerifyAdd(local3);
                            this.ItemAdded(local3);
                        }
                        finally
                        {
                            this.snapshot.Insert(base.IndexOf(local3), local3);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (T local5 in this.snapshot)
                    {
                        this.ItemRemoved(local5);
                    }
                    this.snapshot = new Collection<T>();
                    foreach (T local6 in this)
                    {
                        this.VerifyAdd(local6);
                        this.ItemAdded(local6);
                    }
                    break;

                default:
                    return;
            }
        }

        protected abstract void OnDetaching();
        private void VerifyAdd(T item)
        {
            if (this.snapshot.Contains(item))
            {
                //  throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.DuplicateItemInCollectionExceptionMessage, new object[] { typeof(T).Name, base.GetType().Name }));
            }
        }

        // Properties
        public DependencyObject AssociatedObject
        {
            get
            {
                base.ReadPreamble();
                return this.associatedObject;
            }
        }

        DependencyObject IAttachedObject.AssociatedObject =>
           this.AssociatedObject;

    }
}