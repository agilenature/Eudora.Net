using System.Windows.Input;


namespace Eudora.Net.GUI
{
    public class DelegateCommand : ICommand
    {
        public Action<object?> _execute;
        public Predicate<object?> _canExecute;

        public DelegateCommand()
        {
        }

        public DelegateCommand(Action<object?> execute, Predicate<object?> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute is not null && _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            if(_execute is not null)
            {
                _execute(parameter);
            }            
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
