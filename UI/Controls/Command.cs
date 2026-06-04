using System;
using System.Windows.Input;
namespace UI.Controls
{
    public class Command : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;
        public Command(Action<object> action) : this(action, null) { }
        public Command(Action<object> action, Predicate<object> canExecute)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
    public class Command<T> : ICommand
    {
        private readonly Action<T> _action;
        private readonly Predicate<T> _canExecute;
        public Command(Action<T> action) : this(action, null) { }
        public Command(Action<T> action, Predicate<T> canExecute)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || (parameter is T t && _canExecute(t));
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            if (parameter is T t)
                _action(t);
        }
    }
}