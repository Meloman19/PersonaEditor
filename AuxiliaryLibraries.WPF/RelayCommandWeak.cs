using System;
using System.Windows.Input;

namespace AuxiliaryLibraries.WPF
{
    public class RelayCommandWeak : ICommand
    {
        WeakReference action;

        public RelayCommandWeak(Action<object> action)
        {
            this.action = new WeakReference(action);
        }

        public RelayCommandWeak(Action action)
        {
            this.action = new WeakReference(action);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (action.IsAlive)
                if (action.Target is Action act)
                    act();
                else if (action.Target is Action<object> actobj)
                    actobj(parameter);
        }
    }
}