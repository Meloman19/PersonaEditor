using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public class TriggerBase : Animatable, IAttachedObject
    {
        public TriggerBase() : base()
        {

        }

        #region IAttachedObject

        DependencyObject associatedObject = null;

        public DependencyObject AssociatedObject => associatedObject;

        public virtual void Attach(DependencyObject dependencyObject)
        {
            associatedObject = dependencyObject;
        }

        public virtual void Detach()
        {
            associatedObject = null;
        }

        #endregion IAttachedObject

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        protected override Freezable CreateInstanceCore()
        {
            return (Freezable)(Activator.CreateInstance(base.GetType()));
        }
    }
}