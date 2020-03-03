using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public abstract class ActionBase : Animatable, IAttachedObject
    {
        DependencyObject associatedObject = null;
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(TriggerAction), new FrameworkPropertyMetadata(true));

        public ActionBase()
        {
           
        }

        #region IAttachedObject

        public void Attach(DependencyObject dependencyObject)
        {
            associatedObject = dependencyObject;
            OnAttached();
        }

        public void Detach()
        {
            associatedObject = null;
            OnDetached();
        }

        protected virtual void OnAttached()
        {

        }

        protected virtual void OnDetached()
        {

        }

        #endregion

        public abstract void Invoke(object[] args);
        protected override Freezable CreateInstanceCore() => ((Freezable)Activator.CreateInstance(GetType()));

        #region Properties 

        public DependencyObject AssociatedObject => associatedObject;

        public bool IsEnabled
        {
            get => ((bool)GetValue(IsEnabledProperty));
            set => SetValue(IsEnabledProperty, value);
        }

        #endregion
    }
}