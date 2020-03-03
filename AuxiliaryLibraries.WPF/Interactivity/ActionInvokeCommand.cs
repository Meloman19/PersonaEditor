using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AuxiliaryLibraries.WPF.Interactivity
{
    public class ActionInvokeCommand : ActionBase
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command",
                typeof(ICommand),
                typeof(ActionInvokeCommand));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter",
                typeof(object),
                typeof(ActionInvokeCommand));

        public static readonly DependencyProperty CommandArgConverterProperty =
           DependencyProperty.Register("CommandArgConverter",
               typeof(ICommandArgConverter),
               typeof(ActionInvokeCommand));

        public override void Invoke(object[] args)
        {
            if (IsEnabled)
            {
                var converter = CommandArgConverter;
                if (converter == null)
                    Command?.Execute(CommandParameter);
                else
                    Command?.Execute(converter.GetArguments(args));
            }
        }

        #region Properties

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ICommandArgConverter CommandArgConverter
        {
            get => (ICommandArgConverter)GetValue(CommandArgConverterProperty);
            set => SetValue(CommandArgConverterProperty, value);
        }

        #endregion
    }
}