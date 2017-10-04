namespace ZzukBot.WPF.Commands.Interfaces
{
    internal interface ICommandAction
    {
        bool CanExecute(object parameter);
        void Execute(object parameter);
    }
}