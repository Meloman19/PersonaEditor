using System;
using System.Windows.Input;

namespace AuxiliaryLibraries.WPF
{
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