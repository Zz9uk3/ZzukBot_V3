using System;
using System.Windows.Input;
using ZzukBot.WPF.Commands.Interfaces;

namespace ZzukBot.WPF.Commands
{
    internal class CommandWithoutParameter : ICommand, ICommandAction
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        internal CommandWithoutParameter(Action execute) : this(() => true, execute)
        {
        }

        internal CommandWithoutParameter(Func<bool> canExecute, Action execute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            remove { CommandManager.RequerySuggested -= value; }
            add { CommandManager.RequerySuggested += value; }
        }
    }
}