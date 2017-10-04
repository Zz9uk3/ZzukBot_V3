using System;
using System.Windows.Input;
using ZzukBot.WPF.Commands.Interfaces;

namespace ZzukBot.WPF.Commands
{
    internal class CommandWithParameter<T> : ICommand, ICommandAction where T : class
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        internal CommandWithParameter(Action<T> execute) : this(x => true, execute)
        {
        }

        internal CommandWithParameter(Predicate<T> canExecute, Action<T> execute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            var param = parameter as T;
            return param != null && _canExecute(param);
        }

        public void Execute(object parameter)
        {
            var param = parameter as T;
            if (param == null) return;
            _execute(param);
        }

        public event EventHandler CanExecuteChanged
        {
            remove { CommandManager.RequerySuggested -= value; }
            add { CommandManager.RequerySuggested += value; }
        }
    }
}