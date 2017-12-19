using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PersonaEditorLib
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

    public class RelayCommand : ICommand
    {
        object action;

        public RelayCommand(Action<object> action)
        {
            this.action = action;
        }

        public RelayCommand(Action action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (action is Action act)
                act();
            else if (action is Action<object> actobj)
                actobj(parameter);
        }
    }
}